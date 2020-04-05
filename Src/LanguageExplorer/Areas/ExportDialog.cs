// Copyright (c) 2005-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using LanguageExplorer.Controls;
using LanguageExplorer.Controls.XMLViews;
using LanguageExplorer.DictionaryConfiguration;
using LanguageExplorer.LIFT;
using Microsoft.Win32;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FwCoreDlgs;
using SIL.FieldWorks.FwCoreDlgs.FileDialog;
using SIL.FieldWorks.Resources;
using SIL.LCModel;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.LCModel.DomainImpl;
using SIL.LCModel.Utils;
using SIL.Lift;
using SIL.Lift.Validation;
using SIL.Reporting;
using SIL.Windows.Forms;
using SIL.Xml;
using ReflectionHelper = SIL.LCModel.Utils.ReflectionHelper;

namespace LanguageExplorer.Areas
{
	/// <summary>
	/// ExportDialog implements an XML-configurable set of export choices.
	/// The base class here implements the main lexicon export (and thus has some specific
	/// behavior we may want to refactor into a subclass one day).
	/// Override ConfigurationFilePath to give the path to a file (like the one in the example here)
	/// from the FW root directory. This file specifies what will appear in the export options.
	/// You will typically also need to override the actual Export process, unless it is
	/// a standard FXT export.
	/// </summary>
	internal class ExportDialog : Form, IFlexComponent
	{
		protected LcmCache m_cache;
		private Label label1;
		protected ColumnHeader columnHeader1;
		protected ColumnHeader columnHeader2;
		private Button btnExport;
		private Button btnCancel;
		protected ListView m_exportList;
		private RichTextBox m_description;
		private XDumper m_dumper;
		protected IThreadedProgress m_progressDlg;
		private Button buttonHelp;
		protected string m_helpTopic;
		private HelpProvider helpProvider;
		protected List<FxtType> m_rgFxtTypes = new List<FxtType>(8);
		protected ConfiguredExport m_ce;
		protected XmlSeqView m_seqView;
		private XmlVc m_xvc;
		private int m_hvoRootObj;
		private int m_clidRootObj;
		private CheckBox m_chkExportPictures;
		/// <summary>Flag whether to include picture and media files in the export.</summary>
		private bool m_fExportPicturesAndMedia;
		/// <summary>
		/// This is set true whenever the check value for m_chkExportPictures is retrieved
		/// for a LIFT export.
		/// </summary>
		private bool m_fLiftExportPicturesSet;
		/// <summary>
		/// The data access is needed if we're doing a filtered FXT export.  (See FWR-1223.)
		/// </summary>
		private ISilDataAccess m_sda;
		/// <summary>
		/// The record list is needed if we're doing a filtered FXT export.  (See FWR-1223.)
		/// </summary>
		private IRecordList m_recordList;
		private const string ksLiftExportPicturesPropertyName = "LIFT-ExportPictures";
		/// <summary>
		/// Store the active area from which this dialog was called.
		/// </summary>
		private string m_areaOrig;
		private CheckBox m_chkShowInFolder;
		private StatusBar _mainWindowStatusBar;
		private readonly MajorFlexComponentParameters _majorFlexComponentParameters;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;
		private List<ListViewItem> m_exportItems;

		/// <summary />
		public ExportDialog()
		{
			InitializeComponent();
		}

		/// <summary />
		public ExportDialog(MajorFlexComponentParameters majorFlexComponentParameters)
			: this()
		{
			_majorFlexComponentParameters = majorFlexComponentParameters;
			_mainWindowStatusBar = _majorFlexComponentParameters.StatusBar;
		}

		private void InitFromMainControl(object objCurrentControl)
		{
			var docView = FindXmlDocView(objCurrentControl as Control);
			if (docView != null)
			{
				m_seqView = docView.Controls[0] as XmlSeqView;
			}
			if (m_seqView != null)
			{
				m_xvc = m_seqView.Vc;
				m_sda = m_seqView.RootBox.DataAccess;
			}
			var cmo = PropertyTable.GetValue<ICmObject>(LanguageExplorerConstants.ActiveListSelectedObject, null);
			if (cmo != null)
			{
				var newHvoRoot = SetRoot(cmo, out var clidRoot);
				if (newHvoRoot > 0)
				{
					m_hvoRootObj = newHvoRoot;
					m_clidRootObj = clidRoot;
				}
			}
			var browseView = FindXmlBrowseView(objCurrentControl as Control);
			if (browseView != null)
			{
				m_sda = browseView.RootBox.DataAccess;
			}
		}

		/// <summary>
		/// Allows process to find an appropriate root hvo and change the current root.
		/// Subclasses (e.g. NotebookExportDialog) can override.
		/// </summary>
		/// <returns>Returns -1 if root hvo doesn't need changing.</returns>
		protected virtual int SetRoot(ICmObject cmo, out int clidRoot)
		{
			clidRoot = -1;
			var hvoRoot = -1;
			switch (cmo)
			{
				// Handle LexEntries that no longer have owners.
				case ILexEntry _:
					hvoRoot = m_cache.LanguageProject.LexDbOA.Hvo;
					clidRoot = m_cache.ServiceLocator.GetInstance<Virtuals>().LexDbEntries;
					break;
				case ICmSemanticDomain _:
					hvoRoot = cmo.OwnerOfClass<ICmPossibilityList>().Hvo;
					clidRoot = CmPossibilityListTags.kClassId;
					break;
				default:
				{
					if (cmo.Owner != null)
					{
						hvoRoot = cmo.Owner.Hvo;
						clidRoot = cmo.Owner.ClassID;
					}
					break;
				}
			}
			return hvoRoot;
		}

		/// <summary>
		/// If one exists, find an XmlDocView control no matter how deeply it's embedded.
		/// </summary>
		private static XmlDocView FindXmlDocView(Control control)
		{
			switch (control)
			{
				case null:
					return null;
				case XmlDocView view:
					return view;
			}
			return control.Controls.Cast<Control>().Select(FindXmlDocView).FirstOrDefault(xdv => xdv != null);
		}

		/// <summary>
		/// If one exists, find an XmlBrowseView control no matter how deeply it's embedded.
		/// </summary>
		private static XmlBrowseView FindXmlBrowseView(Control control)
		{
			switch (control)
			{
				case null:
					return null;
				case XmlBrowseView view:
					return view;
			}
			return control.Controls.Cast<Control>().Select(FindXmlBrowseView).FirstOrDefault(xbv => xbv != null);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			if (IsDisposed)
			{
				// No need to run it more than once.
				return;
			}

			if (disposing)
			{
				components?.Dispose();
			}

			PropertyTable = null;
			Publisher = null;
			Subscriber = null;

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ComponentResourceManager resources = new ComponentResourceManager(typeof(ExportDialog));
			this.btnExport = new Button();
			this.btnCancel = new Button();
			this.m_exportList = new ListView();
			this.columnHeader1 = new ColumnHeader();
			this.columnHeader2 = new ColumnHeader();
			this.m_description = new RichTextBox();
			this.label1 = new Label();
			this.buttonHelp = new Button();
			this.m_chkExportPictures = new CheckBox();
			this.m_chkShowInFolder = new CheckBox();
			this.SuspendLayout();
			//
			// btnExport
			//
			resources.ApplyResources(this.btnExport, "btnExport");
			this.btnExport.Name = "btnExport";
			this.btnExport.Click += new EventHandler(this.btnExport_Click);
			//
			// btnCancel
			//
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			//
			// m_exportList
			//
			resources.ApplyResources(this.m_exportList, "m_exportList");
			this.m_exportList.Columns.AddRange(new ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this.m_exportList.FullRowSelect = true;
			this.m_exportList.HideSelection = false;
			this.m_exportList.MinimumSize = new Size(256, 183);
			this.m_exportList.MultiSelect = false;
			this.m_exportList.Name = "m_exportList";
			this.m_exportList.Sorting = SortOrder.Ascending;
			this.m_exportList.UseCompatibleStateImageBehavior = false;
			this.m_exportList.View = View.Details;
			this.m_exportList.SelectedIndexChanged += new EventHandler(this.m_exportList_SelectedIndexChanged);
			//
			// columnHeader1
			//
			resources.ApplyResources(this.columnHeader1, "columnHeader1");
			//
			// columnHeader2
			//
			resources.ApplyResources(this.columnHeader2, "columnHeader2");
			//
			// m_description
			//
			resources.ApplyResources(this.m_description, "m_description");
			this.m_description.Name = "m_description";
			this.m_description.ReadOnly = true;
			this.m_description.LinkClicked += new LinkClickedEventHandler(this.m_description_LinkClicked);
			//
			// label1
			//
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			//
			// buttonHelp
			//
			resources.ApplyResources(this.buttonHelp, "buttonHelp");
			this.buttonHelp.Name = "buttonHelp";
			this.buttonHelp.Click += new EventHandler(this.buttonHelp_Click);
			//
			// m_chkExportPictures
			//
			resources.ApplyResources(this.m_chkExportPictures, "m_chkExportPictures");
			this.m_chkExportPictures.Name = "m_chkExportPictures";
			this.m_chkExportPictures.UseVisualStyleBackColor = true;
			this.m_chkExportPictures.CheckedChanged += new EventHandler(this.m_chkExportPictures_CheckedChanged);
			//
			// m_chkShowInFolder
			//
			resources.ApplyResources(this.m_chkShowInFolder, "m_chkShowInFolder");
			this.m_chkShowInFolder.Checked = true;
			this.m_chkShowInFolder.CheckState = CheckState.Checked;
			this.m_chkShowInFolder.Name = "m_chkShowInFolder";
			this.m_chkShowInFolder.UseVisualStyleBackColor = true;
			//
			// ExportDialog
			//
			this.AcceptButton = this.btnExport;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.m_chkShowInFolder);
			this.Controls.Add(this.m_chkExportPictures);
			this.Controls.Add(this.buttonHelp);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_description);
			this.Controls.Add(this.m_exportList);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnExport);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportDialog";
			this.ShowIcon = false;
			this.Load += new EventHandler(this.ExportDialog_Load);
			this.Closed += new EventHandler(this.ExportDialog_Closed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// In case the requested export requires a particular view and we aren't showing it, create a temporary one
		/// and update the appropriate variables from it.
		/// If this returns a non-null value, it is a newly created object which must be disposed. (See LT-11066.)
		/// </summary>
		private Control EnsureViewInfo()
		{
			m_areaOrig = PropertyTable.GetValue<string>(LanguageExplorerConstants.AreaChoice);
			if (m_rgFxtTypes.Count == 0)
			{
				return null; // only non-Fxt exports available (like Discourse chart?)
			}
			Control mainControl;
			var ft = m_rgFxtTypes[FxtIndex((string)m_exportItems[0].Tag)].m_ft;
			if (m_areaOrig == LanguageExplorerConstants.NotebookAreaMachineName)
			{
				if (ft != FxtTypes.kftConfigured) // Different from Configured Dictionary; Notebook uses a subclass of ExportDialog
				{
					return null;    // nothing to do.
				}
				mainControl = new XmlDocView(XElement.Parse(AreaResources.NotebookDocumentParameters), m_cache, m_recordList, _majorFlexComponentParameters.UiWidgetController);
			}
			else
			{
				switch (ft)
				{
					case FxtTypes.kftClassifiedDict:
						// Should match the tool in DistFiles/Language Explorer/Configuration/RDE/toolConfiguration.xml, the value attribute in
						// <tool label="Classified Dictionary" value="lexiconClassifiedDictionary" icon="DocumentView">.
						// We use this to create that tool and base this export on its objects and saved configuration.
						mainControl = new XmlDocView(XElement.Parse(AreaResources.LexiconClassifiedDictionaryParameters), m_cache, m_recordList, _majorFlexComponentParameters.UiWidgetController);
						break;
					case FxtTypes.kftGrammarSketch:
						mainControl = new GrammarSketchHtmlViewer();
						break;
					default:
						return null; // nothing to do.
				}
			}
			((IFlexComponent)mainControl).InitializeFlexComponent(_majorFlexComponentParameters.FlexComponentParameters);
			InitFromMainControl(mainControl);
			return mainControl;
		}

		private List<int> m_translationWritingSystems;
		private List<ICmPossibilityList> m_translatedLists;
		private bool m_allQuestions; // For semantic domains, export missing translations as English?

		private void btnExport_Click(object sender, EventArgs e)
		{
			if (m_exportList.SelectedItems.Count == 0)
			{
				return;
			}
			m_exportItems.Clear();
			foreach (ListViewItem sel in m_exportList.SelectedItems)
			{
				m_exportItems.Add(sel);
			}
			using (EnsureViewInfo())
			{
				if (!PrepareForExport())
				{
					return;
				}
				var fLiftExport = m_exportItems[0].SubItems[2].Text == @"lift";
				string sFileName;
				string sDirectory;
				if (fLiftExport)
				{
					using (var dlg = new FolderBrowserDialogAdapter())
					{
						dlg.Tag = AreaResources.ksChooseLIFTFolderTitle; // can't set title !!??
						dlg.Description = string.Format(AreaResources.ksChooseLIFTExportFolder, m_exportItems[0].SubItems[1].Text);
						dlg.ShowNewFolderButton = true;
						dlg.RootFolder = Environment.SpecialFolder.Desktop;
						dlg.SelectedPath = PropertyTable.GetValue("ExportDir", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
						if (dlg.ShowDialog(this) != DialogResult.OK)
						{
							return;
						}
						sDirectory = dlg.SelectedPath;
					}
					var sFile = Path.GetFileName(sDirectory);
					sFileName = Path.Combine(sDirectory, sFile + FwFileExtensions.ksLexiconInterchangeFormat);
					string sMsg = null;
					var btns = MessageBoxButtons.OKCancel;
					if (File.Exists(sFileName))
					{
						sMsg = AreaResources.ksLIFTAlreadyExists;
						btns = MessageBoxButtons.OKCancel;
					}
					else
					{
						var rgfiles = Directory.GetFiles(sDirectory);
						if (rgfiles.Length > 0)
						{
							sMsg = AreaResources.ksLIFTFolderNotEmpty;
							btns = MessageBoxButtons.YesNo;
						}
					}
					if (!string.IsNullOrEmpty(sMsg))
					{
						using (var dlg = new LiftExportMessageDlg(sMsg, btns))
						{
							if (dlg.ShowDialog(this) != DialogResult.OK)
							{
								return;
							}
						}
					}
				}
				else
				{
					FxtType ft;
					// Note that DiscourseExportDialog doesn't add anything to m_rgFxtTypes.
					// See FWR-2506.
					if (m_rgFxtTypes.Count > 0)
					{
						var fxtPath = (string)m_exportItems[0].Tag;
						ft = m_rgFxtTypes[FxtIndex(fxtPath)];
					}
					else
					{
						// Choose a dummy value that will take the default branch of merely choosing
						// an output file.
						ft.m_ft = FxtTypes.kftConfigured;
					}
					switch (ft.m_ft)
					{
						case FxtTypes.kftTranslatedLists:
							using (var dlg = new ExportTranslatedListsDlg())
							{
								dlg.Initialize(PropertyTable, m_cache,
									m_exportItems[0].SubItems[1].Text,
									m_exportItems[0].SubItems[2].Text,
									m_exportItems[0].SubItems[3].Text);
								if (dlg.ShowDialog(this) != DialogResult.OK)
								{
									return;
								}
								sFileName = dlg.FileName;
								sDirectory = Path.GetDirectoryName(sFileName);
								m_translationWritingSystems = dlg.SelectedWritingSystems;
								m_translatedLists = dlg.SelectedLists;
							}
							break;
						case FxtTypes.kftSemanticDomains:
							using (var dlg = new ExportSemanticDomainsDlg())
							{
								dlg.Initialize(m_cache);
								if (dlg.ShowDialog(this) != DialogResult.OK)
								{
									return;
								}
								m_translationWritingSystems = new List<int>
								{
									dlg.SelectedWs
								};
								m_allQuestions = dlg.AllQuestions;
							}
							goto default;
						case FxtTypes.kftPathway:
							ProcessPathwayExport();
							return;
						case FxtTypes.kftWebonary:
							ProcessWebonaryExport();
							return;
						default:
							using (var dlg = new SaveFileDialogAdapter())
							{
								var dlgAsISaveFileDialog = (ISaveFileDialog)dlg;
								dlgAsISaveFileDialog.AddExtension = true;
								dlgAsISaveFileDialog.DefaultExt = m_exportItems[0].SubItems[2].Text;
								dlgAsISaveFileDialog.Filter = m_exportItems[0].SubItems[3].Text;
								dlgAsISaveFileDialog.Title = string.Format(AreaResources.ExportTo0, m_exportItems[0].SubItems[1].Text);
								dlgAsISaveFileDialog.InitialDirectory = PropertyTable.GetValue("ExportDir", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
								if (dlgAsISaveFileDialog.ShowDialog(this) != DialogResult.OK)
								{
									return;
								}
								sFileName = dlgAsISaveFileDialog.FileName;
								sDirectory = Path.GetDirectoryName(sFileName);
							}
							break;
					}
				}
				if (sDirectory != null)
				{
					PropertyTable.SetProperty("ExportDir", sDirectory, true, true);
				}
				if (fLiftExport) // Fixes LT-9437 Crash exporting a discourse chart (or interlinear too!)
				{
					DoExport(sFileName, true);
				}
				else
				{
					DoExport(sFileName); // Musn't use the 2 parameter version here or overrides get messed up.
				}
				if (m_chkShowInFolder.Checked)
				{
					OpenExportFolder(sDirectory, sFileName);
					PropertyTable.SetProperty("ExportDlgShowInFolder", "true", true, true);
				}
				else
				{
					PropertyTable.SetProperty("ExportDlgShowInFolder", "false", true, true);
				}
			}
		}

		private static void OpenExportFolder(string sDirectory, string sFileName)
		{
			ProcessStartInfo processInfo = null;
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// if it exists, xdg-open uses the user's preference for opening directories
				if (File.Exists("/usr/bin/xdg-open"))
				{
					processInfo = new ProcessStartInfo("/usr/bin/xdg-open", $"\"{sDirectory}\"");
				}
				else if (File.Exists("/usr/bin/nautilus"))
				{
					processInfo = new ProcessStartInfo("/usr/bin/nautilus", $"\"{sDirectory}\"");
				}
				else if (File.Exists("/usr/bin/krusader"))
				{
					processInfo = new ProcessStartInfo("/usr/bin/krusader", $"\"{sDirectory}\"");
				}
				else if (File.Exists("/usr/bin/pcmanfm"))
				{
					processInfo = new ProcessStartInfo("/usr/bin/pcmanfm", $"\"{sDirectory}\"");
				}
				else if (File.Exists("/usr/bin/gnome-commander"))
				{
					processInfo = new ProcessStartInfo("/usr/bin/gnome-commander", $"-l \"{Path.GetDirectoryName(sDirectory)}\" -r \"{sDirectory}\"");
				}
				// If the user doesn't have one of these programs installed, I give up!
			}
			else
			{
				// REVIEW: What happens if directory or filename contain spaces?
				var program = Environment.ExpandEnvironmentVariables(@"%WINDIR%\explorer.exe");
				if (program == @"\explorer.exe")
				{
					program = @"C:\windows\explorer.exe";
				}
				processInfo = string.IsNullOrEmpty(sFileName) ? new ProcessStartInfo(program, $" /select,{sDirectory}") : new ProcessStartInfo(program, $" /select,{sFileName}");
			}
			if (processInfo != null)
			{
				using (Process.Start(processInfo))
				{
				}
			}
		}

		/// <summary>
		/// Allow custom final preparation before asking for file.  See LT-8403.
		/// </summary>
		/// <returns>true iff export can proceed further</returns>
		protected virtual bool PrepareForExport()
		{
			return true;
		}

		/// <summary>
		/// This version is overridden by (currently) Interlinear and Discourse Chart exports.
		/// </summary>
		protected virtual void DoExport(string outPath)
		{
			DoExport(outPath, false);
		}

		protected void DoExport(string outPath, bool fLiftOutput)
		{
			var fxtPath = (string)m_exportItems[0].Tag;
			var ft = m_rgFxtTypes[FxtIndex(fxtPath)];
			using (new WaitCursor(this))
			using (var progressDlg = new ProgressDialogWithTask(this))
			{
				try
				{
					UsageReporter.SendEvent(m_areaOrig + @"Export", @"Export", ft.m_ft.ToString(), $"{ft.m_sDataType} {ft.m_sFormat} {(ft.m_filtered ? "filtered" : "unfiltered")}", 0);
					switch (ft.m_ft)
					{
						case FxtTypes.kftFxt:
							m_dumper = new XDumper(m_cache);
							m_dumper.UpdateProgress += OnDumperUpdateProgress;
							m_dumper.SetProgressMessage += OnDumperSetProgressMessage;
							progressDlg.Minimum = 0;
							progressDlg.Maximum = m_dumper.GetProgressMaximum();
							progressDlg.AllowCancel = true;
							progressDlg.Restartable = true;
							progressDlg.RunTask(true, ExportFxt, outPath, fxtPath, fLiftOutput);
							break;
						case FxtTypes.kftConfigured:
						case FxtTypes.kftReversal:
							progressDlg.Minimum = 0;
							progressDlg.Maximum = 1; // max will be set by the task, since only it knows how many entries it will export
							progressDlg.AllowCancel = true;
							progressDlg.RunTask(true, ExportConfiguredXhtml, outPath);
							break;
						case FxtTypes.kftClassifiedDict:
							progressDlg.Minimum = 0;
							progressDlg.Maximum = m_seqView.ObjectCount;
							progressDlg.AllowCancel = true;
							var vss = m_seqView.RootBox?.Stylesheet;
							progressDlg.RunTask(true, ExportConfiguredDocView, outPath, fxtPath, ft, vss);
							break;
						case FxtTypes.kftTranslatedLists:
							progressDlg.Minimum = 0;
							progressDlg.Maximum = m_translatedLists.Count;
							progressDlg.AllowCancel = true;
							progressDlg.RunTask(true, ExportTranslatedLists, outPath);
							break;
						case FxtTypes.kftSemanticDomains:
							// Potentially, we could count semantic domains and try to make the export update for each.
							// In practice this only takes a second or two on a typical modern computer
							// an the main export routine is borrowed from kftTranslatedLists and set up to count each
							// list as one step. For now, claiming this export just has one step seems good enough.
							progressDlg.Minimum = 0;
							progressDlg.Maximum = 1;
							progressDlg.AllowCancel = true;
							progressDlg.RunTask(true, ExportSemanticDomains, outPath, ft, fxtPath, m_allQuestions);
							break;
						case FxtTypes.kftLift:
							progressDlg.Minimum = 0;
							progressDlg.Maximum = 1000;
							progressDlg.AllowCancel = true;
							progressDlg.Restartable = true;
							progressDlg.RunTask(true, ExportLift, outPath, ft.m_filtered);
							break;
						case FxtTypes.kftGrammarSketch:
							progressDlg.Minimum = 0;
							progressDlg.Maximum = 1000;
							progressDlg.AllowCancel = true;
							progressDlg.Restartable = true;
							progressDlg.RunTask(true, ExportGrammarSketch, outPath, ft.m_sDataType, ft.m_sXsltFiles);
							break;
					}
				}
				catch (WorkerThreadException e)
				{
					if (e.InnerException is CancelException)
					{
						MessageBox.Show(this, e.InnerException.Message);
						m_ce = null;
					}
					else if (e.InnerException is LiftFormatException)
					{
						// Show the pretty yellow semi-crash dialog box, with instructions for the
						// user to report the bug.
						var app = PropertyTable.GetValue<IApp>(LanguageExplorerConstants.App);
						ErrorReporter.ReportException(new Exception(AreaResources.ksLiftExportBugReport, e.InnerException), app.SettingsKey, app.SupportEmailAddress, this, false);
					}
					else
					{
						var msg = AreaResources.ErrorExporting_ProbablyBug + Environment.NewLine + e.InnerException.Message;
						MessageBox.Show(this, msg);
					}
				}
				finally
				{
					m_progressDlg = null;
					m_dumper = null;
					Close();
				}
			}
		}

		private object ExportConfiguredXhtml(IThreadedProgress progress, object[] args)
		{
			if (args.Length < 1)
			{
				return null;
			}
			var xhtmlPath = (string)args[0];
			switch (m_rgFxtTypes[FxtIndex((string)m_exportItems[0].Tag)].m_ft)
			{
				case FxtTypes.kftConfigured:
					new DictionaryExportService(m_cache, PropertyTable.GetValue<IRecordListRepository>(LanguageExplorerConstants.RecordListRepository).ActiveRecordList, PropertyTable, _mainWindowStatusBar).ExportDictionaryContent(xhtmlPath, progress: progress);
					break;
				case FxtTypes.kftReversal:
					new DictionaryExportService(m_cache, PropertyTable.GetValue<IRecordListRepository>(LanguageExplorerConstants.RecordListRepository).ActiveRecordList, PropertyTable, _mainWindowStatusBar).ExportReversalContent(xhtmlPath, progress: progress);
					break;
			}
			return null;
		}

		private object ExportGrammarSketch(IThreadedProgress progress, object[] args)
		{
			var outPath = (string)args[0];
			var sDataType = (string)args[1];
			var sXslts = (string)args[2];
			m_progressDlg = progress;
			var parameter = new Tuple<string, string, string>(sDataType, outPath, sXslts);
			Publisher.Publish(new PublisherParameterObject("SaveAsWebpage", parameter));
			m_progressDlg.Step(1000);
			return null;
		}

		/// <summary>
		/// Exports as a LIFT file (possibly with one or more range files.
		/// </summary>
		private object ExportLift(IThreadedProgress progress, object[] args)
		{
			var outPath = (string)args[0];
			var filtered = (bool)args[1];
			m_progressDlg = progress;
			var exporter = new LiftExporter(m_cache);
			exporter.UpdateProgress += OnDumperUpdateProgress;
			exporter.SetProgressMessage += OnDumperSetProgressMessage;
			exporter.ExportPicturesAndMedia = m_fExportPicturesAndMedia;
#if DEBUG
			var dtStart = DateTime.Now;
#endif
			using (TextWriter w = new StreamWriter(outPath))
			{
				if (filtered)
				{
					exporter.ExportLift(w, Path.GetDirectoryName(outPath), m_recordList.VirtualListPublisher, m_recordList.VirtualFlid);
				}
				else
				{
					exporter.ExportLift(w, Path.GetDirectoryName(outPath));
				}
			}
			var outPathRanges = Path.ChangeExtension(outPath, ".lift-ranges");
			using (var w = new StringWriter())
			{
				exporter.ExportLiftRanges(w);
				using (var sw = new StreamWriter(outPathRanges))
				{
					//actually write out to file
					sw.Write(w.GetStringBuilder().ToString());
					sw.Close();
				}
			}
#if DEBUG
			var dtExport = DateTime.Now;
#endif
			progress.Message = String.Format(AreaResources.ksValidatingOutputFile, Path.GetFileName(outPath));
			Validator.CheckLiftWithPossibleThrow(outPath);
#if DEBUG
			var dtValidate = DateTime.Now;
			var exportDelta = new TimeSpan(dtExport.Ticks - dtStart.Ticks);
			var validateDelta = new TimeSpan(dtValidate.Ticks - dtExport.Ticks);
			Debug.WriteLine($"Export time = {exportDelta}, Validation time = {validateDelta}");
#endif
			return null;
		}

		/// <summary>
		/// Exports as FXT.
		/// </summary>
		private object ExportFxt(IThreadedProgress progressDialog, object[] parameters)
		{
			Debug.Assert(parameters.Length == 3);
			m_progressDlg = progressDialog;
			var outPath = (string)parameters[0];
			var fxtPath = (string)parameters[1];
			var fLiftOutput = (bool)parameters[2];
#if DEBUG
			var dtStart = DateTime.Now;
#endif
			using (TextWriter w = new StreamWriter(outPath))
			{
				m_dumper.ExportPicturesAndMedia = m_fExportPicturesAndMedia;
				if (m_sda != null && m_recordList != null)
				{
					m_dumper.VirtualDataAccess = m_sda;
					m_dumper.VirtualFlid = m_recordList.VirtualFlid;
				}
				m_dumper.Go(m_cache.LangProject, fxtPath, w);
			}
#if DEBUG
			var dtExport = DateTime.Now;
#endif
			if (fLiftOutput)
			{
				progressDialog.Message = String.Format(AreaResources.ksValidatingOutputFile, Path.GetFileName(outPath));
				Validator.CheckLiftWithPossibleThrow(outPath);
			}
#if DEBUG
			var dtValidate = DateTime.Now;
			var exportDelta = new TimeSpan(dtExport.Ticks - dtStart.Ticks);
			var validateDelta = new TimeSpan(dtValidate.Ticks - dtExport.Ticks);
			Debug.WriteLine("Export time = {0}, Validation time = {1}", exportDelta, validateDelta);
#endif
			return null;
		}

		/// <summary>
		/// Exports the configured doc view.
		/// </summary>
		protected object ExportConfiguredDocView(IThreadedProgress progressDlg, object[] parameters)
		{
			Debug.Assert(parameters.Length == 4);
			m_progressDlg = progressDlg;
			if (m_xvc == null)
			{
				return null;
			}
			var outPath = (string)parameters[0];
			var fxtPath = (string)parameters[1];
			var ft = (FxtType)parameters[2];
			var vss = (IVwStylesheet)parameters[3];
			using (TextWriter w = new StreamWriter(outPath))
			{
#if DEBUG
				var dirPath = Path.GetTempPath();
				var copyCount = 1;
				var s = $"Starting Configured Export at {DateTime.Now.ToLongTimeString()}";
				Debug.WriteLine(s);
#endif
				m_ce = new ConfiguredExport(null, m_xvc.DataAccess, m_hvoRootObj);
				var sBodyClass = m_areaOrig == LanguageExplorerConstants.NotebookAreaMachineName ? "notebookBody" : "dicBody";
				m_ce.Initialize(m_cache, PropertyTable, w, ft.m_sDataType, ft.m_sFormat, outPath, sBodyClass);
				m_ce.UpdateProgress += ce_UpdateProgress;
				m_xvc.Display(m_ce, m_hvoRootObj, m_seqView.RootFrag);
				m_ce.Finish(ft.m_sDataType);
				w.Close();
#if DEBUG
				s = $"Finished Configured Export Dump at {DateTime.Now.ToLongTimeString()}";
				Debug.WriteLine(s);
#endif
				if (!string.IsNullOrEmpty(ft.m_sXsltFiles))
				{
					var rgsXslts = ft.m_sXsltFiles.Split(';');
					var cXslts = rgsXslts.GetLength(0);
					progressDlg.Position = 0;
					progressDlg.Minimum = 0;
					progressDlg.Maximum = cXslts;
					progressDlg.Message = AreaResources.ProcessingIntoFinalForm;
					var idx = fxtPath.LastIndexOfAny(new[] { '/', '\\' });
					if (idx < 0)
					{
						idx = 0;
					}
					else
					{
						++idx;
					}
					var basePath = fxtPath.Substring(0, idx);
					for (var ix = 0; ix <= cXslts; ++ix)
					{
#if DEBUG
						File.Copy(outPath, Path.Combine(dirPath, "DebugOnlyExportStage" + copyCount + ".txt"), true);
						copyCount++;
						s = ix < cXslts ? $"Starting Configured Export XSLT file {rgsXslts[ix]} at {DateTime.Now.ToLongTimeString()}" : $"Starting final postprocess phase at {DateTime.Now.ToLongTimeString()}";
						Debug.WriteLine(s);
#endif
						if (ix < cXslts)
						{
							var sXsltPath = basePath + rgsXslts[ix];
							m_ce.PostProcess(sXsltPath, outPath, ix + 1);
						}
						else
						{
							m_ce.PostProcess(null, outPath, ix + 1);
						}
						progressDlg.Step(0);
					}
				}
				if (ft.m_sFormat.ToLowerInvariant() == "xhtml")
				{
					m_ce.WriteCssFile(Path.ChangeExtension(outPath, ".css"), vss, AllowDictionaryParagraphIndent(ft));
				}
				m_ce = null;
#if DEBUG
				File.Copy(outPath, Path.Combine(dirPath, "DebugOnlyExportStage" + copyCount + ".txt"), true);
				s = $"Totally Finished Configured Export at {DateTime.Now.ToLongTimeString()}";
				Debug.WriteLine(s);
#endif
			}
			return null;
		}

		// Currently we allow indented paragraph styles only for classified dictionary.
		// See the comment on XhtmlHelper.AllowDictionaryParagraphIndent for more info.
		private static bool AllowDictionaryParagraphIndent(FxtType ft)
		{
			return ft.m_ft == FxtTypes.kftClassifiedDict;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Registry key for settings for this Dialog.
		/// </summary>
		public RegistryKey SettingsKey
		{
			get
			{
				using (var regKey = FwRegistryHelper.FieldWorksRegistryKey)
				{
					return regKey.CreateSubKey("ExportInterlinearDialog");
				}
			}
		}

		private void ExportDialog_Closed(object sender, EventArgs e)
		{
			// Save location.
			SettingsKey.SetValue("InsertX", Location.X);
			SettingsKey.SetValue("InsertY", Location.Y);
			SettingsKey.SetValue("InsertWidth", Width);
			SettingsKey.SetValue("InsertHeight", Height);
		}

		/// <summary>
		/// Overridden to defeat the standard .NET behavior of adjusting size by
		/// screen resolution. That is bad for this dialog because we remember the size,
		/// and if we remember the enlarged size, it just keeps growing.
		/// If we defeat it, it may look a bit small the first time at high resolution,
		/// but at least it will stay the size the user sets.
		/// </summary>
		protected override void OnLoad(EventArgs e)
		{
			var size = Size;
			base.OnLoad(e);
			if (Size != size)
			{
				Size = size;
			}
		}

		private void ExportDialog_Load(object sender, EventArgs e)
		{
			var p = FxtDirectory;
			if (Directory.Exists(p))
			{
				AddFxts(Directory.GetFiles(p, "*.xml"));
			}
		}

		internal string FxtDirectory => Path.Combine(FwDirectoryFinder.CodeDirectory, ConfigurationFilePath);

		protected virtual string ConfigurationFilePath => $"Language Explorer{Path.DirectorySeparatorChar}Export Templates";

		protected void AddFxts(string[] filePaths)
		{
			Debug.Assert(filePaths != null);

			foreach (var path in filePaths)
			{
				if (path.EndsWith(".xml~"))
				{
					continue;   // ignore editor backup files.
				}
				var document = new XmlDocument();
				// If we have an xml file that can't be loaded, ignore it.
				try
				{
					document.Load(path);
				}
				catch
				{
					continue;
				}
				var node = document.SelectSingleNode("//FxtDocumentDescription");
				if (node == null)
				{
					continue;
				}
				var dataLabel = XmlUtils.GetOptionalAttributeValue(node, "dataLabel", "unknown");
				var formatLabel = XmlUtils.GetOptionalAttributeValue(node, "formatLabel", "unknown");
				var defaultExtension = XmlUtils.GetOptionalAttributeValue(node, "defaultExtension", "txt");
				var sDefaultFilter = ResourceHelper.FileFilter(FileFilterType.AllFiles);
				var filter = XmlUtils.GetOptionalAttributeValue(node, "filter", sDefaultFilter);
				var description = node.InnerText;
				description = description.Trim();
				if (string.IsNullOrEmpty(description))
				{
					description = AreaResources.NoDescriptionForItem;
				}
				var item = new ListViewItem(new[] { dataLabel, formatLabel, defaultExtension, filter, description }) { Tag = path };
				m_exportList.Items.Add(item);
				ConfigureItem(document, item, node);
			}
			// Select the first available one
			foreach (ListViewItem lvi in m_exportList.Items)
			{
				if (!ItemDisabled((string)lvi.Tag))
				{
					lvi.Selected = true;
					m_exportItems.Add(lvi);
					break;
				}
			}
		}

		/// <summary>
		/// Store the attributes of the "template" element.
		/// Override (often to do nothing) if not configuring an FXT export process.
		/// </summary>
		protected virtual void ConfigureItem(XmlDocument document, ListViewItem item, XmlNode ddNode)
		{
			var templateRootNode = document.SelectSingleNode("//template");
			Debug.Assert(templateRootNode != null, "FXT files must always have a <template> node somewhere.");
			FxtType ft;
			ft.m_sFormat = XmlUtils.GetOptionalAttributeValue(templateRootNode, "format", "xml");
			var sType = XmlUtils.GetOptionalAttributeValue(templateRootNode, "type", "fxt");
			switch (sType)
			{
				case "fxt":
					ft.m_ft = FxtTypes.kftFxt;
					break;
				case "configured":
					ft.m_ft = FxtTypes.kftConfigured;
					break;
				case "classified":
					ft.m_ft = FxtTypes.kftClassifiedDict;
					break;
				case "reversal":
					ft.m_ft = FxtTypes.kftReversal;
					break;
				case "translatedList":
					ft.m_ft = FxtTypes.kftTranslatedLists;
					break;
				case "pathway":
					ft.m_ft = FxtTypes.kftPathway;
					break;
				case "webonary":
					ft.m_ft = FxtTypes.kftWebonary;
					break;
				case "LIFT":
					ft.m_ft = FxtTypes.kftLift;
					break;
				case LanguageExplorerConstants.GrammarSketchMachineName:
					ft.m_ft = FxtTypes.kftGrammarSketch;
					break;
				case "semanticDomains":
					ft.m_ft = FxtTypes.kftSemanticDomains;
					break;
				default:
					Debug.Fail("Invalid type attribute value for the template element");
					ft.m_ft = FxtTypes.kftFxt;
					break;
			}
			ft.m_sDataType = XmlUtils.GetOptionalAttributeValue(templateRootNode, "datatype", "UNKNOWN");
			ft.m_sXsltFiles = XmlUtils.GetOptionalAttributeValue(templateRootNode, "xslt", null);
			ft.m_filtered = XmlUtils.GetOptionalBooleanAttributeValue(templateRootNode, "filtered", false);
			ft.m_path = (string)item.Tag;
			m_rgFxtTypes.Add(ft);
			// We can't actually disable a list item, but we can make it look and act like it's
			// disabled.
			if (ItemDisabled(ft.m_ft, ft.m_filtered, ft.m_sFormat))
			{
				item.ForeColor = SystemColors.GrayText;
			}
		}

		private void m_exportList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_exportList.SelectedItems.Count == 0)
			{
				return;
			}
			m_fExportPicturesAndMedia = false;
			m_description.Text = m_exportList.SelectedItems[0].SubItems[4].Text;
			if (ItemDisabled((string)m_exportList.SelectedItems[0].Tag))
			{
				m_description.ForeColor = SystemColors.GrayText;
				btnExport.Enabled = false;
				m_chkExportPictures.Visible = false;
				m_chkExportPictures.Enabled = false;
			}
			else
			{
				m_description.ForeColor = SystemColors.ControlText;
				btnExport.Enabled = true;
				if (m_exportList.SelectedItems[0].SubItems[2].Text == "lift")
				{
					m_chkExportPictures.Visible = true;
					m_chkExportPictures.Enabled = true;
					if (!m_fLiftExportPicturesSet)
					{
						m_chkExportPictures.Checked = PropertyTable.GetValue(ksLiftExportPicturesPropertyName, true);
						m_fLiftExportPicturesSet = true;
					}
					m_fExportPicturesAndMedia = m_chkExportPictures.Checked;
				}
				else
				{
					m_chkExportPictures.Visible = false;
					m_chkExportPictures.Enabled = false;
				}
			}
		}

		protected int FxtIndex(string tag)
		{
			for (var i = 0; i < m_rgFxtTypes.Count; i++)
			{
				if (m_rgFxtTypes[i].m_path == tag)
				{
					return i;
				}
			}
			return 0;
		}

		protected virtual bool ItemDisabled(string tag)
		{
			return ItemDisabled(m_rgFxtTypes[FxtIndex(tag)].m_ft, m_rgFxtTypes[FxtIndex(tag)].m_filtered, m_rgFxtTypes[FxtIndex(tag)].m_sFormat);
		}

		private bool ItemDisabled(FxtTypes ft, bool isFiltered, string formatType)
		{
			//enable unless the type is pathway & pathway is not installed, or if the type is lift and it is filtered, but there is no filter available, or if the filter excludes all items
			var fFilterAvailable = DetermineIfFilterIsAvailable();
			return ft == FxtTypes.kftPathway && !IsPathwayInstalled || ft == FxtTypes.kftLift && isFiltered && fFilterAvailable || ft == FxtTypes.kftConfigured
			       && (formatType == "htm" || formatType == "sfm") || ft == FxtTypes.kftReversal && formatType == "sfm";
		}

		private bool DetermineIfFilterIsAvailable()
		{
			return m_recordList?.VirtualListPublisher.get_VecSize(m_cache.LangProject.LexDbOA.Hvo, m_recordList.VirtualFlid) < 1;
		}

		private void OnDumperUpdateProgress(object sender)
		{
			Debug.Assert(m_progressDlg != null);
			m_progressDlg.Step(0);
			if (m_progressDlg.Canceled)
			{
				m_dumper.Cancel();
			}
		}

		private void OnDumperSetProgressMessage(object sender, ProgressMessageArgs e)
		{
			Debug.Assert(m_progressDlg != null);
			var sMsg = DictionaryConfigurationStrings.ResourceManager.GetString(e.MessageId, DictionaryConfigurationStrings.Culture);
			if (!string.IsNullOrEmpty(sMsg))
			{
				m_progressDlg.Message = sMsg;
			}
			m_progressDlg.Position = 0;
			m_progressDlg.Minimum = 0;
			m_progressDlg.Maximum = e.Max;
			if (m_progressDlg.Canceled)
			{
				m_dumper.Cancel();
			}
		}

		private void buttonHelp_Click(object sender, EventArgs e)
		{
			ShowHelp.ShowHelpTopic(PropertyTable.GetValue<IHelpTopicProvider>(LanguageExplorerConstants.HelpTopicProvider), m_helpTopic);
		}

		private void ce_UpdateProgress(object sender)
		{
			Debug.Assert(m_progressDlg != null);
			m_progressDlg.Step(0);
			if (m_progressDlg.Canceled)
			{
				m_ce.Cancel();
			}
		}

		protected object ExportTranslatedLists(IThreadedProgress progressDlg, object[] parameters)
		{
			var outPath = (string)parameters[0];
			m_progressDlg = progressDlg;
			if (m_translatedLists.Count == 0 || m_translationWritingSystems.Count == 0)
			{
				return null;
			}
			var exporter = new TranslatedListsExporter(m_translatedLists, m_translationWritingSystems, progressDlg);
			exporter.ExportLists(outPath);
			return null;
		}

		/// <summary>
		/// For testing.
		/// </summary>
		internal void SetTranslationWritingSystems(List<int> wss)
		{
			m_translationWritingSystems = wss;
		}

		/// <summary>
		/// for testing
		/// </summary>
		internal void SetCache(LcmCache cache)
		{
			m_cache = cache;
		}

		/// <summary>
		/// Do the export of the semantic domains list to an HTML document (which is given extension .doc
		/// since it is mainly intended to be opened as a Word document, since Word understands the
		/// 'page break before' concept).
		/// The signature of this method is required by the way it is used as the task of the ProgressDialog.
		/// See the first few lines for the required parameters.
		/// </summary>
		internal object ExportSemanticDomains(IThreadedProgress progressDlg, object[] parameters)
		{
			var outPath = (string)parameters[0];
			var ft = (FxtType)parameters[1];
			var fxtPath = (string)parameters[2];
			var allQuestions = (bool)parameters[3];
			m_progressDlg = progressDlg;
			var lists = new List<ICmPossibilityList>
			{
				m_cache.LangProject.SemanticDomainListOA
			};
			var exporter = new TranslatedListsExporter(lists, m_translationWritingSystems, progressDlg);
			exporter.ExportLists(outPath);
			var ft1 = ft;
#if DEBUG
			var dirPath = Path.GetTempPath();
#endif
			var xslt = ft1.m_sXsltFiles;
			progressDlg.Position = 0;
			progressDlg.Minimum = 0;
			progressDlg.Maximum = 1;
			progressDlg.Message = AreaResources.ProcessingIntoFinalForm;
			var idx = fxtPath.LastIndexOfAny(new[] { '/', '\\' });
			if (idx < 0)
			{
				idx = 0;
			}
			else
			{
				++idx;
			}
			var basePath = fxtPath.Substring(0, idx);
			var sXsltPath = basePath + xslt;
			var sIntermediateFile = CollectorEnv.RenameOutputToPassN(outPath, 0);
			// The semantic domain xslt uses document('folderStart.xml') to retrieve the list of H1 topics.
			// This is not allowed by default so we must use a settings object to enable it.
			var settings = new XsltSettings(enableDocumentFunction: true, enableScript: false);
			var xsl = new XslCompiledTransform();
			xsl.Load(sXsltPath, settings, new XmlUrlResolver());
			var arguments = new XsltArgumentList();
			// If we aren't outputting english we need to ignore it (except possibly for missing items)
			var ignoreEn = m_translationWritingSystems[0] != m_cache.LanguageWritingSystemFactoryAccessor.GetWsFromStr("en");
			arguments.AddParam(@"ignoreLang", @"", (ignoreEn ? @"en" : @""));
			arguments.AddParam(@"allQuestions", @"", (allQuestions ? @"1" : @"0"));
			using (var writer = FileUtils.OpenFileForWrite(outPath, Encoding.UTF8))
			{
				xsl.Transform(sIntermediateFile, arguments, writer);
			}
			// Deleting them deals with LT-6345,
			// which asked that they be put in the temp folder.
			// But moving them to the temp directory is better for debugging errors.
			FileUtils.MoveFileToTempDirectory(sIntermediateFile, "FieldWorks-Export");
			progressDlg.Step(0);
#if DEBUG
			var s = $"Totally Finished Export Semantic domains at {DateTime.Now.ToLongTimeString()}";
			Debug.WriteLine(s);
#endif
			return null; // method signature is required by ProgressDialog
		}

		private void m_chkExportPictures_CheckedChanged(object sender, EventArgs e)
		{
			PropertyTable.SetProperty(ksLiftExportPicturesPropertyName, m_chkExportPictures.Checked, true, true);
			m_fExportPicturesAndMedia = m_chkExportPictures.Checked;
		}

		private void m_description_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			using (Process.Start(e.LinkText))
			{
			}
		}

		/// <summary>
		/// Perform a Pathway export, bringing up the Pathway configuration dialog, exporting
		/// one or more XHTML files, and then postprocessing as requested.
		/// </summary>
		private void ProcessPathwayExport()
		{
			var app = PropertyTable.GetValue<IApp>(LanguageExplorerConstants.App);
			var cssDialog = Path.Combine(PathwayInstallDirectory, "CssDialog.dll");
			var sf = ReflectionHelper.CreateObject(cssDialog, "SIL.PublishingSolution.Contents", null);
			Debug.Assert(sf != null);
			var cache = PropertyTable.GetValue<LcmCache>(FwUtils.cache);
			ReflectionHelper.SetProperty(sf, "DatabaseName", cache.ProjectId.Name);
			var fContentsExists = SelectOption("ReversalIndexXHTML");
			if (fContentsExists)
			{
				// Inform Pathway if the reversal index is empty (or doesn't exist).  See FWR-3283.
				var riGuid = FwUtils.GetObjectGuidIfValid(PropertyTable, LanguageExplorerConstants.ReversalIndexGuid);
				if (!riGuid.Equals(Guid.Empty))
				{
					fContentsExists = m_cache.ServiceLocator.GetInstance<IReversalIndexRepository>().TryGetObject(riGuid, out var ri) && ri.EntriesOC.Any();
				}
			}
			ReflectionHelper.SetProperty(sf, "ExportReversal", fContentsExists);
			ReflectionHelper.SetProperty(sf, "ReversalExists", fContentsExists);
			ReflectionHelper.SetProperty(sf, "GrammarExists", false);
			var result = (DialogResult)ReflectionHelper.GetResult(sf, "ShowDialog");
			if (result == DialogResult.Cancel)
			{
				return;
			}
			const string MainXhtml = "main.xhtml";
			const string ExpCss = "main.css";
			const string RevXhtml = "FlexRev.xhtml";
			var strOutputPath = (string)ReflectionHelper.GetProperty(sf, "OutputLocationPath");
			var strDictionaryName = (string)ReflectionHelper.GetProperty(sf, "DictionaryName");
			var outPath = Path.Combine(strOutputPath, strDictionaryName);
			var fExistingDirectoryInput = (bool)ReflectionHelper.GetProperty(sf, "ExistingDirectoryInput");
			if (fExistingDirectoryInput)
			{
				var inputPath = (string)ReflectionHelper.GetProperty(sf, "ExistingDirectoryLocationPath");
				if (inputPath != outPath)
				{
					var dirFilter = string.Empty;
					if (strOutputPath == inputPath)
					{
						dirFilter = strDictionaryName;
					}
					try
					{
						if (!Folders.Copy(inputPath, outPath, dirFilter, app.ApplicationName))
						{
							return;
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}
				}
			}
			if (!Folders.CreateDirectory(outPath, app.ApplicationName))
			{
				return;
			}
			var mainFullName = Path.Combine(outPath, MainXhtml);
			var revFullXhtml = Path.Combine(outPath, RevXhtml);
			if (!(bool)ReflectionHelper.GetProperty(sf, "ExportMain"))
			{
				mainFullName = string.Empty;
			}
			if (!(bool)ReflectionHelper.GetProperty(sf, "ExportReversal"))
			{
				revFullXhtml = string.Empty;
			}
			switch (result)
			{
				// No = Skip export of data from Flex but still prepare exported output (ODT, PDF or whatever)
				case DialogResult.No:
					break;
				case DialogResult.Yes:
					if (!DoFlexExports(ExpCss, mainFullName, revFullXhtml))
					{
						Close();
						return;
					}
					break;
			}
			var psExport = Path.Combine(PathwayInstallDirectory, "PsExport.dll");
			var exporter = ReflectionHelper.CreateObject(psExport, "SIL.PublishingSolution.PsExport", null);
			Debug.Assert(exporter != null);
			ReflectionHelper.SetProperty(exporter, "DataType", "Dictionary");
			ReflectionHelper.SetProperty(exporter, "ProgressBar", null);
			ReflectionHelper.CallMethod(exporter, "Export", mainFullName != "" ? mainFullName : revFullXhtml);
			var applicationKey = app.SettingsKey;
			UsageEmailDialog.IncrementLaunchCount(applicationKey);
			var assembly = exporter.GetType().Assembly;
			const string FeedbackEmailAddress = "pathway@sil.org";
			const string utilityLabel = "Pathway";
			UsageEmailDialog.DoTrivialUsageReport(utilityLabel, applicationKey, FeedbackEmailAddress,
				$"1. What do you hope {utilityLabel} will do for you?%0A%0A2. What languages are you working on?", false, 1, assembly);
			UsageEmailDialog.DoTrivialUsageReport(utilityLabel, applicationKey, FeedbackEmailAddress,
				"1. Do you have suggestions to improve the program?%0A%0A2. What are you happy with?", false, 10, assembly);
			UsageEmailDialog.DoTrivialUsageReport(utilityLabel, applicationKey, FeedbackEmailAddress,
				string.Format("1. What would you like to say to others about {0}?%0A%0A2. What languages have you used with {0}", utilityLabel), false, 40, assembly);

			Close();
		}

		/// <summary>
		/// Hand off to Webonary publishing area.
		/// </summary>
		private void ProcessWebonaryExport()
		{
			DictionaryConfigurationServices.ShowUploadToWebonaryDialog(_majorFlexComponentParameters);
		}

		private bool SelectOption(string exportFormat)
		{
			// LT-12279 selected a user disturbing, different menu item
			// return m_exportList.Items.Cast<ListViewItem>().Where(lvi => lvi.Tag.ToString().Contains(exportFormat));
			foreach (var lvi in m_exportList.Items.Cast<ListViewItem>().Where(lvi => lvi.Tag.ToString().Contains(exportFormat)))
			{
				if (ItemDisabled(lvi.Tag.ToString()))
				{
					return false;
				}
				m_exportItems.Insert(0, lvi);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Export process from Fieldworks Language explorer
		/// </summary>
		/// <param name="expCss">Style sheet exported</param>
		/// <param name="mainFullName">Source of main dictionary</param>
		/// <param name="revFullXhtml">source of reversal Index if available in Xhtml format</param>
		/// <returns>True if there is something to do</returns>
		protected bool DoFlexExports(string expCss, string mainFullName, string revFullXhtml)
		{
			if (File.Exists(mainFullName))
			{
				File.Delete(mainFullName);
			}
			if (File.Exists(revFullXhtml))
			{
				File.Delete(revFullXhtml);
			}
			var currInput = string.Empty;
			try
			{
				if (mainFullName != string.Empty)
				{
					ExportFor("ConfiguredXHTML", mainFullName);
				}
				if (revFullXhtml != string.Empty)
				{
					ExportFor("ReversalIndexXHTML", revFullXhtml);
				}
			}
			catch (FileNotFoundException)
			{
				var app = PropertyTable.GetValue<IApp>(LanguageExplorerConstants.App);
				MessageBox.Show($@"The {currInput} Section may be Empty (or) Not exported", app.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;

			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		private void ExportFor(string type, string file)
		{
			if (!SelectOption(type))
			{
				return;
			}
			using (EnsureViewInfo())
			{
				DoExport(file);
				CheckForWellformedXmlFile(file);
			}
		}

		/// <summary>
		/// Checking for well formed xml file with xmldocument to avoid further processing.
		/// </summary>
		/// <param name="xml">Xml file Name to check</param>
		/// <exception cref="FileNotFoundException">if xml file missing</exception>
		/// <exception cref="XmlException">if xml file won't load</exception>
		protected static void CheckForWellformedXmlFile(string xml)
		{
			if (!File.Exists(xml))
			{
				throw new FileNotFoundException();
			}
			var xDoc = new XmlDocument
			{
				XmlResolver = new FileStreamXmlResolver()
			};
			xDoc.Load(xml);
		}

		#region Implementation of IPropertyTableProvider

		/// <summary>
		/// Placement in the IPropertyTableProvider interface lets FwApp call IPropertyTable.DoStuff.
		/// </summary>
		public IPropertyTable PropertyTable { get; private set; }

		#endregion

		#region Implementation of IPublisherProvider

		/// <summary>
		/// Get the IPublisher.
		/// </summary>
		public IPublisher Publisher { get; private set; }

		#endregion

		#region Implementation of ISubscriberProvider

		/// <summary>
		/// Get the ISubscriber.
		/// </summary>
		public ISubscriber Subscriber { get; private set; }

		#endregion

		#region Implementation of IFlexComponent

		/// <summary>
		/// Initialize a FLEx component with the basic interfaces.
		/// </summary>
		/// <param name="flexComponentParameters">Parameter object that contains the required three interfaces.</param>
		public virtual void InitializeFlexComponent(FlexComponentParameters flexComponentParameters)
		{
			FlexComponentParameters.CheckInitializationValues(flexComponentParameters, new FlexComponentParameters(PropertyTable, Publisher, Subscriber));

			PropertyTable = flexComponentParameters.PropertyTable;
			Publisher = flexComponentParameters.Publisher;
			Subscriber = flexComponentParameters.Subscriber;

			m_cache = PropertyTable.GetValue<LcmCache>(FwUtils.cache);
			AccessibleName = GetType().Name;
			// Figure out where to locate the dlg.
			var obj = SettingsKey.GetValue("InsertX");
			if (obj != null)
			{
				var x = (int)obj;
				var y = (int)SettingsKey.GetValue("InsertY");
				var width = (int)SettingsKey.GetValue("InsertWidth", Width);
				var height = (int)SettingsKey.GetValue("InsertHeight", Height);
				var rect = new Rectangle(x, y, width, height);
				ScreenHelper.EnsureVisibleRect(ref rect);
				DesktopBounds = rect;
				StartPosition = FormStartPosition.Manual;
			}
			m_helpTopic = "khtpExportLexicon";
			var helpTopicProvider = PropertyTable.GetValue<IHelpTopicProvider>(LanguageExplorerConstants.HelpTopicProvider);
			helpProvider = new HelpProvider
			{
				HelpNamespace = helpTopicProvider.HelpFile
			};
			helpProvider.SetHelpKeyword(this, helpTopicProvider.GetHelpString(m_helpTopic));
			helpProvider.SetHelpNavigator(this, HelpNavigator.Topic);
			// Determine whether we can support "configured" type export by trying to obtain
			// the XmlVc for an XmlDocView.  Also obtain the database id and class id of the
			// root object.
			InitFromMainControl(PropertyTable.GetValue<object>("currentContentControlObject", null));
			m_recordList = PropertyTable.GetValue<IRecordListRepository>(LanguageExplorerConstants.RecordListRepository).ActiveRecordList;
			m_chkExportPictures.Checked = false;
			m_chkExportPictures.Visible = false;
			m_chkExportPictures.Enabled = false;
			m_fExportPicturesAndMedia = false;
			//Set  m_chkShowInFolder to it's last state.
			var showInFolder = PropertyTable.GetValue("ExportDlgShowInFolder", "true");
			m_chkShowInFolder.Checked = showInFolder.Equals("true");
			m_exportItems = new List<ListViewItem>();
		}

		#endregion

		/// <summary>
		/// Gets the directory for the Pathway application or string.Empty if the directory name
		/// is not in the registry.
		/// </summary>
		private static string PathwayInstallDirectory
		{
			get
			{
				const string installDirValue = "PathwayDir46";
				if (RegistryHelper.RegEntryValueExists(RegistryHelper.CompanyKey, "Pathway", installDirValue, out var regObj))
				{
					return (string)regObj;
				}
				// Some broken Windows machines can have trouble accessing HKLM (LT-15158).
				if (RegistryHelper.CompanyKeyLocalMachine == null)
				{
					var defaultDir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SIL"), "Pathway7");
					return Directory.Exists(defaultDir) ? defaultDir : string.Empty;
				}
				if (RegistryHelper.RegEntryValueExists(RegistryHelper.CompanyKeyLocalMachine, "Pathway", installDirValue, out regObj))
				{
					return (string)regObj;
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets a value indicating whether SIL Pathway is installed.
		/// </summary>
		private static bool IsPathwayInstalled
		{
			get
			{
				var pathwayDirectory = PathwayInstallDirectory;
				return !string.IsNullOrEmpty(pathwayDirectory) && File.Exists(Path.Combine(pathwayDirectory, "PsExport.dll"));
			}
		}
	}
}