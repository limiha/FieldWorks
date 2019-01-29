// Copyright (c) 2018-2019 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;

namespace LanguageExplorer.Areas
{
	/// <summary>
	/// Interface for UI widgets, such as menus or toolbars, that allows for managing what is added to a main component and the related events.
	/// </summary>
	internal interface IToolUiWidgetManager : IDisposable
	{
		/// <summary>
		/// Initialize the implementation.
		/// </summary>
		/// <param name="majorFlexComponentParameters">The main parameter object for a given IFwMainWnd instance.</param>
		/// <param name="area">The area associated with the manager.</param>
		/// <param name="recordList">The record list that provides the current main CmObject being displayed.</param>
		void Initialize(MajorFlexComponentParameters majorFlexComponentParameters, IArea area, IRecordList recordList = null);

		ITool ActiveTool { get; }

		/// <summary>
		/// In preparation for disposal, unwire any shared event handlers.
		/// </summary>
		void UnwireSharedEventHandlers();
	}
}