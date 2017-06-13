// Copyright (c) 2003-2017 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)
//
// File: SILExceptions.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
//	The idea is to eventually display a nice dialog box when the configuration author makes a mistake
//	or there is a problem with the installation.
// </remarks>

using System;
using System.Xml;
using System.Xml.Linq;

namespace SIL.FieldWorks.Common.FwUtils
{
	/// <summary>
	/// Use this exception when it looks like the configuration XML itself has a problem.
	/// Thus, end-user should never see these exceptions.
	/// </summary>
	public class FwConfigurationException : ApplicationException
	{
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message, XmlNode node, Exception exInner)
			: base(message + Environment.NewLine + node.OuterXml, exInner)
		{
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message, XElement node, Exception exInner)
			: base(message + Environment.NewLine + node, exInner)
		{
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message, XmlNode node)
			: base(message + Environment.NewLine + node.OuterXml)
		{
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message, XElement node)
			: base(message + Environment.NewLine + node)
		{
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message, Exception exInner)
			: base(message, exInner)
		{
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FwConfigurationException"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public FwConfigurationException(string message)
			: base(message)
		{
		}

		public void ShowDialog()
		{
			System.Windows.Forms.MessageBox.Show(this.Message, FwUtilsStrings.XMLConfigurationError, System.  Windows.Forms.MessageBoxButtons.OK,System.  Windows.  Forms.  MessageBoxIcon.Exclamation);
		}
	}

	/// <summary>
	/// Use this exception when the format of the configuration XML
	/// may be fine, but there is a run-time linking problem with an assembly or class that was specified.
	/// </summary>
	public class RuntimeConfigurationException : ApplicationException
	{
		public RuntimeConfigurationException(string message) :base(message)
		{

		}

		/// <summary>
		/// Use this one if you are inside of a catch block where you have access to the original exception
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public RuntimeConfigurationException(string message, Exception innerException) :base(message, innerException)
		{

		}
	}
}
