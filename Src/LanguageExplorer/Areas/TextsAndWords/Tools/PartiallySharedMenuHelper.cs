// Copyright (c) 2018-2019 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Diagnostics;
using System.Windows.Forms;
using LanguageExplorer.Areas.TextsAndWords.Interlinear;
using SIL.Code;

namespace LanguageExplorer.Areas.TextsAndWords.Tools
{
	/// <summary>
	/// This menu helper is shared between these tools: ConcordanceTool, ComplexConcordanceTool, and WordListConcordanceTool.
	/// </summary>
	internal sealed class PartiallySharedMenuHelper : IToolUiWidgetManager
	{
		private MajorFlexComponentParameters _majorFlexComponentParameters;
		private IArea _area;
		private IAreaUiWidgetManager _textAndWordsAreaMenuHelper;
		private ToolStripMenuItem _editFindMenu;
		private ToolStripMenuItem _editFindAndReplaceMenu;
		private ToolStripMenuItem _replaceToolStripMenuItem;
		private ToolStripItem _insertFindAndReplaceButton;
		private InterlinMasterNoTitleBar _interlinMasterNoTitleBar;

		internal PartiallySharedMenuHelper(InterlinMasterNoTitleBar interlinMasterNoTitleBar)
		{
			Guard.AgainstNull(interlinMasterNoTitleBar, nameof(interlinMasterNoTitleBar));

			_interlinMasterNoTitleBar = interlinMasterNoTitleBar;
		}

		#region Implementation of IToolUiWidgetManager
		/// <inheritdoc />
		void IToolUiWidgetManager.Initialize(MajorFlexComponentParameters majorFlexComponentParameters, IArea area, IRecordList recordList)
		{
			Guard.AgainstNull(majorFlexComponentParameters, nameof(majorFlexComponentParameters));
			Guard.AgainstNull(area, nameof(area));

			_majorFlexComponentParameters = majorFlexComponentParameters;
			_area = area;
			var textAndWordsAreaMenuHelper = new TextAndWordsAreaMenuHelper();
			_textAndWordsAreaMenuHelper = textAndWordsAreaMenuHelper;
			_textAndWordsAreaMenuHelper.Initialize(majorFlexComponentParameters, area, this, recordList);
			_insertFindAndReplaceButton = ToolbarServices.GetInsertFindAndReplaceToolStripItem(_majorFlexComponentParameters.ToolStripContainer);
			_insertFindAndReplaceButton.Click += EditFindMenu_Click;
			_editFindMenu = MenuServices.GetEditFindMenu(_majorFlexComponentParameters.MenuStrip);
			_editFindMenu.Click += EditFindMenu_Click;
			_replaceToolStripMenuItem = MenuServices.GetEditFindAndReplaceMenu(_majorFlexComponentParameters.MenuStrip);
			_replaceToolStripMenuItem.Click += EditFindMenu_Click;
			Application.Idle += Application_Idle;
			textAndWordsAreaMenuHelper.AddMenusForAllButConcordanceTool();
		}

		/// <inheritdoc />
		ITool IToolUiWidgetManager.ActiveTool => _area.ActiveTool;

		/// <inheritdoc />
		void IToolUiWidgetManager.UnwireSharedEventHandlers()
		{
			_textAndWordsAreaMenuHelper.UnwireSharedEventHandlers();
		}
		#endregion

		private void Application_Idle(object sender, EventArgs e)
		{
#if RANDYTODO_TEST_Application_Idle
// TODO: Remove when finished sorting out idle issues.
Debug.WriteLine($"Start: Application.Idle run at: '{DateTime.Now:HH:mm:ss.ffff}': on '{GetType().Name}'.");
#endif
			// Sort out whether to display/enable the _editFindMenu.
			// NB: This will work the same for the Edit-Replace menu.
			var oldVisible = _editFindMenu.Visible;
			var oldEnabled = _editFindMenu.Enabled;
			bool newEnabled;
			var newVisible = _interlinMasterNoTitleBar.CanDisplayFindTexMenutOrFindAndReplaceTextMenu(out newEnabled);
			if (oldVisible != newVisible)
			{
				_editFindMenu.Visible = newVisible;
				_replaceToolStripMenuItem.Visible = newVisible;
				_insertFindAndReplaceButton.Visible = newVisible;
			}
			if (oldEnabled != newEnabled)
			{
				_editFindMenu.Enabled = newEnabled;
				_replaceToolStripMenuItem.Enabled = newEnabled;
				_insertFindAndReplaceButton.Enabled = newEnabled;
			}
#if RANDYTODO_TEST_Application_Idle
// TODO: Remove when finished sorting out idle issues.
Debug.WriteLine($"End: Application.Idle run at: '{DateTime.Now:HH:mm:ss.ffff}': on '{GetType().Name}'.");
#endif
		}

		private void EditFindMenu_Click(object sender, EventArgs e)
		{
			_interlinMasterNoTitleBar.HandleFindAndReplace(sender == _insertFindAndReplaceButton || sender == _replaceToolStripMenuItem);
		}

		#region Implementation of IDisposable
		private bool _isDisposed;

		~PartiallySharedMenuHelper()
		{
			// The base class finalizer is called automatically.
			Dispose(false);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SuppressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			if (_isDisposed)
			{
				// No need to run it more than once.
				return;
			}

			if (disposing)
			{
				Application.Idle -= Application_Idle;
				_editFindMenu.Click -= EditFindMenu_Click;
				_insertFindAndReplaceButton.Click -= EditFindMenu_Click;
				_insertFindAndReplaceButton.Visible = _insertFindAndReplaceButton.Enabled = false;
				_replaceToolStripMenuItem.Click -= EditFindMenu_Click;
				_replaceToolStripMenuItem.Visible = _replaceToolStripMenuItem.Enabled = false;
			}
			_majorFlexComponentParameters = null;
			_editFindMenu = null;
			_insertFindAndReplaceButton = null;
			_interlinMasterNoTitleBar = null;
			_replaceToolStripMenuItem = null;

			_isDisposed = true;
		}
		#endregion
	}
}