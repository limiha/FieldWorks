// Copyright (c) 2017-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Windows.Forms;
using LanguageExplorer.Controls.XMLViews;
using LanguageExplorer.Works;
using SIL.Code;
using SIL.FieldWorks.Filters;
using SIL.LCModel;
using SIL.LCModel.Application;
using SIL.LCModel.Utils;

namespace LanguageExplorer.Areas
{
	internal sealed class SubservientRecordList : RecordList
	{
		/// <summary>
		/// when this is not null, that means there is another clerk managing a list,
		/// and the selected item of that list provides the object that this
		/// RecordClerk gets items out of. For example, the WfiAnalysis clerk
		/// is dependent on the WfiWordform clerk to tell it which wordform it is supposed to
		/// be displaying the analyses of.
		/// </summary>
		private IRecordList _recordListProvidingRootObject;

		internal SubservientRecordList(string id, StatusBar statusBar, RecordFilter defaultFilter, bool allowDeletions, bool shouldHandleDeletion, ISilDataAccessManaged decorator, bool usingAnalysisWs, int flid, ICmObject owner, string propertyName, IRecordList recordListProvidingRootObject)
			: base(id, statusBar, new PropertyRecordSorter("ShortName"), AreaServices.Default, defaultFilter, allowDeletions, shouldHandleDeletion, decorator, usingAnalysisWs, flid, owner, propertyName)
		{
			Guard.AgainstNull(recordListProvidingRootObject, nameof(recordListProvidingRootObject));

			_recordListProvidingRootObject = recordListProvidingRootObject;
		}

		internal SubservientRecordList(string id, StatusBar statusBar, ISilDataAccessManaged decorator, bool usingAnalysisWs, int flid, IRecordList recordListProvidingRootObject)
		{
			Guard.AgainstNull(recordListProvidingRootObject, nameof(recordListProvidingRootObject));
			Guard.AgainstNullOrEmptyString(id, nameof(id));
			Guard.AgainstNull(statusBar, nameof(statusBar));
			Guard.AgainstNull(decorator, nameof(decorator));

			Id = id;
			_statusBar = statusBar;
			m_objectListPublisher = new ObjectListPublisher(decorator, RecordListFlid);
			m_usingAnalysisWs = usingAnalysisWs;
			m_propertyName = string.Empty;
			m_fontName = MiscUtils.StandardSansSerif;
			// Only other current option is to specify an ordinary property (or a virtual one).
			m_flid = flid;
			// Review JohnH(JohnT): This is only useful for dependent clerks, but I don't know how to check this is one.
			m_owningObject = null;
			_recordListProvidingRootObject = recordListProvidingRootObject;
		}

		private string DependentPropertyName => ClerkSelectedObjectPropertyId(_recordListProvidingRootObject.Id);

		#region Overrides of RecordList
		public override bool TryListProvidingRootObject(out IRecordList recordListProvidingRootObject)
		{
			recordListProvidingRootObject = _recordListProvidingRootObject;
			return true;
		}

		protected override void UpdateOwningObject(bool fUpdateOwningObjectOnlyIfChanged = false)
		{
			var old = OwningObject;
			ICmObject newObj = null;
			var rni = PropertyTable.GetValue<RecordNavigationInfo>(DependentPropertyName);
			if (rni != null)
			{
				newObj = rni.MyRecordList.CurrentObject;
			}
			using (var luh = new ListUpdateHelper(this))
			{
				// in general we want to actually reload the list if something as
				// radical as changing the OwningObject occurs, since many subsequent
				// events and messages depend upon this information.
				luh.TriggerPendingReloadOnDispose = true;
				if (rni != null)
				{
					luh.SkipShowRecord = rni.SkipShowRecord;
				}
				if (!fUpdateOwningObjectOnlyIfChanged || !ReferenceEquals(old, newObj))
				{
					OwningObject = newObj;
				}
			}
			if (!ReferenceEquals(old, newObj))
			{
				Publisher.Publish("ClerkOwningObjChanged", this);
			}
		}

		public override void OnPropertyChanged(string name)
		{
			if (name == CurrentFilterPropertyTableId)
			{
				base.OnPropertyChanged(name);
				return;
			}
			if (name == DependentPropertyName)
			{
				UpdateOwningObjectIfNeeded();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			_recordListProvidingRootObject = null;

			base.Dispose(disposing);
		}

		protected override bool IsPrimaryRecordList => false;

		#endregion
	}
}