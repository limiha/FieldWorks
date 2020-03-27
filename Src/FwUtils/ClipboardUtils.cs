// Copyright (c) 2010-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Windows.Forms;

namespace SIL.FieldWorks.Common.FwUtils
{
	/// <summary>
	/// This basically just wraps the Clipboard class to allow replacing it with a test stub
	/// during unit tests.
	/// </summary>
	public static class ClipboardUtils
	{
		private static IClipboard s_Clipboard = new ClipboardAdapter();

		/// <summary>
		/// Sets the clipboard adapter.
		/// </summary>
		public static void SetClipboardAdapter(IClipboard adapter)
		{
			s_Clipboard = adapter;
		}

		/// <summary>
		/// Indicates whether there is data on the Clipboard in the Text or UnicodeText format,
		/// depending on the operating system.
		/// </summary>
		public static bool ContainsText()
		{
			return s_Clipboard.ContainsText();
		}

		/// <summary>
		/// Retrieves text data from the Clipboard in the Text or UnicodeText format, depending
		/// on the operating system.
		/// </summary>
		public static string GetText()
		{
			return s_Clipboard.GetText();
		}

		/// <summary>
		/// Retrieves the data that is currently on the system Clipboard.
		/// </summary>
		/// <returns>
		/// An IDataObject that represents the data currently on the Clipboard, or
		/// <c>null</c> if there is no data on the Clipboard.
		/// </returns>
		public static IDataObject GetDataObject()
		{
			return s_Clipboard.GetDataObject();
		}

		/// <summary>
		/// Adds text data to the Clipboard in the Text or UnicodeText format, depending on the
		/// operating system.
		/// </summary>
		public static void SetText(string text)
		{
			s_Clipboard.SetText(text);
		}

		/// <summary>
		/// Adds text data to the Clipboard in the format indicated by the specified
		/// TextDataFormat value.
		/// </summary>
		public static void SetText(string text, TextDataFormat format)
		{
			s_Clipboard.SetText(text, format);
		}

		/// <summary>
		/// Places nonpersistent data on the system Clipboard.
		/// </summary>
		public static void SetDataObject(object data)
		{
			s_Clipboard.SetDataObject(data);
		}

		/// <summary>
		/// Places data on the system Clipboard and specifies whether the data should remain on
		/// the Clipboard after the application exits.
		/// </summary>
		/// <param name="data">The data to place on the Clipboard.</param>
		/// <param name="copy"><c>true</c> if you want data to remain on the Clipboard after
		/// this application exits; otherwise, <c>false</c>.</param>
		public static void SetDataObject(object data, bool copy)
		{
			s_Clipboard.SetDataObject(data, copy);
		}

		/// <summary>
		/// Places data on the system Clipboard and specifies whether the data should remain on
		/// the Clipboard after the application exits.
		/// </summary>
		/// <param name="data">The data to place on the Clipboard.</param>
		/// <param name="copy"><c>true</c> if you want data to remain on the Clipboard after
		/// this application exits; otherwise, <c>false</c>.</param>
		/// <param name="retries"># of times to retry</param>
		/// <param name="msDelay"># of milliseconds to delay between retries</param>
		public static void SetDataObject(object data, bool copy, int retries, int msDelay)
		{
			s_Clipboard.SetDataObject(data, copy, retries, msDelay);
		}

		#region ClipboardAdapter class
		private sealed class ClipboardAdapter : IClipboard
		{
			#region IClipboard Members

			/// <summary>
			/// Indicates whether there is data on the Clipboard in the Text or UnicodeText format,
			/// depending on the operating system.
			/// </summary>
			bool IClipboard.ContainsText()
			{
				return Clipboard.ContainsText();
			}

			/// <summary>
			/// Retrieves text data from the Clipboard in the Text or UnicodeText format, depending
			/// on the operating system.
			/// </summary>
			string IClipboard.GetText()
			{
				return Clipboard.GetText();
			}

			/// <summary>
			/// Retrieves the data that is currently on the system Clipboard.
			/// </summary>
			/// <returns>
			/// An IDataObject that represents the data currently on the Clipboard, or
			/// <c>null</c> if there is no data on the Clipboard.
			/// </returns>
			IDataObject IClipboard.GetDataObject()
			{
				return Clipboard.GetDataObject();
			}

			/// <summary>
			/// Adds text data to the Clipboard in the Text or UnicodeText format, depending on the
			/// operating system.
			/// </summary>
			void IClipboard.SetText(string text)
			{
				Clipboard.SetText(text);
			}

			/// <summary>
			/// Adds text data to the Clipboard in the format indicated by the specified
			/// TextDataFormat value.
			/// </summary>
			void IClipboard.SetText(string text, TextDataFormat format)
			{
				Clipboard.SetText(text, format);
			}

			/// <summary>
			/// Places nonpersistent data on the system Clipboard.
			/// </summary>
			void IClipboard.SetDataObject(object data)
			{
				Clipboard.SetDataObject(data);
			}

			/// <summary>
			/// Places data on the system Clipboard and specifies whether the data should remain on
			/// the Clipboard after the application exits.
			/// </summary>
			void IClipboard.SetDataObject(object data, bool copy)
			{
				Clipboard.SetDataObject(data, copy);
			}

			/// <summary>
			/// Places data on the system Clipboard and specifies whether the data should remain on
			/// the Clipboard after the application exits.
			/// </summary>
			void IClipboard.SetDataObject(object data, bool copy, int retries, int msDelay)
			{
				Clipboard.SetDataObject(data, copy, retries, msDelay);
			}

			#endregion
		}
		#endregion
	}
}