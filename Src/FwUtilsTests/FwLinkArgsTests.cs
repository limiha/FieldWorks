// Copyright (c) 2010-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using NUnit.Framework;

namespace SIL.FieldWorks.Common.FwUtils
{
	/// <summary />
	[TestFixture]
	public class FwLinkArgsTests
	{
		#region Equals tests
		/// <summary>
		/// Tests the Equals method when the parameter is another FwLinkArgs with
		/// the exact same information.
		/// </summary>
		[Test]
		public void Equals_ExactlyTheSame()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsTrue(args1.Equals(new FwLinkArgs("myTool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the Equals method when the parameter is the same FwLinkArgs
		/// </summary>
		[Test]
		public void Equals_SameObject()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsTrue(args1.Equals(args1));
		}

		/// <summary>
		/// Tests the Equals method with a null parameter
		/// </summary>
		[Test]
		public void Equals_NullParameter()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsFalse(args1.Equals(null));
		}

		/// <summary>
		/// Tests the Equals method when the parameter is another FwLinkArgs with a
		/// different tool name
		/// </summary>
		[Test]
		public void Equals_DifferByToolName()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsFalse(args1.Equals(new FwLinkArgs("myOtherTool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the Equals method when the parameter is another FwLinkArgs with a
		/// tool name that differs only in case
		/// </summary>
		[Test]
		public void Equals_ToolNameDiffersByCase()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("MyTool", newGuid, "myTag");
			Assert.IsFalse(args1.Equals(new FwLinkArgs("mytool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the Equals method when the parameter is another FwLinkArgs with a
		/// different target guid
		/// </summary>
		[Test]
		public void Equals_DiffereByGuid()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsFalse(args1.Equals(new FwLinkArgs("myTool", Guid.NewGuid(), "myTag")));
		}

		/// <summary>
		/// Tests the Equals method when the parameter is another FwLinkArgs with a
		/// tag that is an empty string
		/// </summary>
		[Test]
		public void Equals_TagOfArgumentZeroLength()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsFalse(args1.Equals(new FwLinkArgs("myTool", newGuid, string.Empty)));
		}

		/// <summary>
		/// Tests the Equals method when the object has a tag that is an empty string
		/// and the parameter is another FwLinkArgs with a non-empty tag
		/// </summary>
		[Test]
		public void Equals_ThisTagZeroLength()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, string.Empty);
			Assert.IsFalse(args1.Equals(new FwLinkArgs("myTool", newGuid, "myTag")));
		}
		#endregion

		#region EssentiallyEquals tests

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is another FwLinkArgs with
		/// the exact same information.
		/// </summary>
		[Test]
		public void EssentiallyEquals_ExactlyTheSame()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsTrue(args1.EssentiallyEquals(new FwLinkArgs("myTool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is the same FwLinkArgs
		/// </summary>
		[Test]
		public void EssentiallyEquals_SameObject()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsTrue(args1.EssentiallyEquals(args1));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method with a null parameter
		/// </summary>
		[Test]
		public void EssentiallyEquals_NullParameter()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsFalse(args1.EssentiallyEquals(null));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is another FwLinkArgs with a
		/// different tool name
		/// </summary>
		[Test]
		public void EssentiallyEquals_DifferByToolName()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsFalse(args1.EssentiallyEquals(new FwLinkArgs("myOtherTool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is another FwLinkArgs with a
		/// tool name that differs only in case
		/// </summary>
		[Test]
		public void EssentiallyEquals_ToolNameDiffersByCase()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("MyTool", newGuid, "myTag");
			Assert.IsFalse(args1.EssentiallyEquals(new FwLinkArgs("mytool", newGuid, "myTag")));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is another FwLinkArgs with a
		/// different target guid
		/// </summary>
		[Test]
		public void EssentiallyEquals_DiffereByGuid()
		{
			var args1 = new FwLinkArgs("myTool", Guid.NewGuid(), "myTag");
			Assert.IsFalse(args1.EssentiallyEquals(new FwLinkArgs("myTool", Guid.NewGuid(), "myTag")));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the parameter is another FwLinkArgs with a
		/// tag that is an empty string
		/// </summary>
		[Test]
		public void EssentiallyEquals_TagOfArgumentZeroLength()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, "myTag");
			Assert.IsTrue(args1.EssentiallyEquals(new FwLinkArgs("myTool", newGuid, string.Empty)));
		}

		/// <summary>
		/// Tests the EssentiallyEquals method when the object has a tag that is an empty string
		/// and the parameter is another FwLinkArgs with a non-empty tag
		/// </summary>
		[Test]
		public void EssentiallyEquals_ThisTagZeroLength()
		{
			var newGuid = Guid.NewGuid();
			var args1 = new FwLinkArgs("myTool", newGuid, string.Empty);
			Assert.IsTrue(args1.EssentiallyEquals(new FwLinkArgs("myTool", newGuid, "myTag")));
		}
		#endregion

		#region FwAppArgs tests

		/// <summary>
		/// Tests creating FwAppArgs with a link parameter without the '-link' specified
		/// </summary>
		[Test]
		public void CreateFwAppArgs_Link_NoKeySpecified()
		{
			var args = new FwAppArgs("silfw://localhost/link?&database=primate&tool=default&guid=F48AC2E4-27E3-404e-965D-9672337E0AAF&tag=");
			Assert.AreEqual("primate", args.Database);
			Assert.AreEqual(String.Empty, args.Tag);
			Assert.AreEqual(new Guid("F48AC2E4-27E3-404e-965D-9672337E0AAF"), args.TargetGuid);
			Assert.AreEqual("default", args.ToolName);
			Assert.IsTrue(args.HasLinkInformation);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(0, args.LinkProperties.Count);
		}

		/// <summary>
		/// Tests creating FwAppArgs with a -link parameter specified
		/// </summary>
		[Test]
		public void CreateFwAppArgs_Link_OverridesOtherSettings()
		{
			var args = new FwAppArgs("-db", "monkey", "-link", "silfw://localhost/link?&database=primate&tool=default&guid=F48AC2E4-27E3-404e-965D-9672337E0AAF&tag=front");
			Assert.AreEqual("primate", args.Database);
			Assert.AreEqual("front", args.Tag);
			Assert.AreEqual(new Guid("F48AC2E4-27E3-404e-965D-9672337E0AAF"), args.TargetGuid);
			Assert.AreEqual("default", args.ToolName);
			Assert.IsTrue(args.HasLinkInformation);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(0, args.LinkProperties.Count);
		}

		/// <summary>
		/// Tests creating FwAppArgs with a link parameter without a database specified
		/// </summary>
		[Test]
		public void CreateFwAppArgs_Link_NoDatabaseSpecified()
		{
			var args = new FwAppArgs("silfw://localhost/link?&tool=default&guid=F48AC2E4-27E3-404e-965D-9672337E0AAF&tag=");
			Assert.IsTrue(args.ShowHelp, "Bad arguments should set ShowHelp to true");
		}

		/// <summary>
		/// Tests creating FwAppArgs
		/// </summary>
		[Test]
		public void CreateFwAppArgs_Normal()
		{
			var args = new FwAppArgs("-db", "monkey");
			Assert.AreEqual("monkey", args.Database);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}

		/// <summary>
		/// Tests creating FwAppArgs when an unknown switch is passed in (this is okay
		/// because maybe the specific app will know what to do with it).
		/// </summary>
		[Test]
		public void CreateFwAppArgs_UnknownSwitch()
		{
			var args = new FwAppArgs("-init", "DN");
			Assert.AreEqual(1, args.LinkProperties.Count);
			Assert.AreEqual("init", args.LinkProperties[0].Name);
			Assert.AreEqual("DN", args.LinkProperties[0].Value);
			Assert.AreEqual(string.Empty, args.Database);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}

		/// <summary>
		/// Tests creating FwAppArgs when -db and -proj are both specified
		/// </summary>
		[Test]
		public void CreateFwAppArgs_DbAndProjSame()
		{
			var args = new FwAppArgs("-db", "tim", "-proj", "monkey");
			Assert.IsTrue(args.ShowHelp, "Bad arguments should set ShowHelp to true");
		}

		/// <summary>
		/// Tests creating FwAppArgs when no space separates the switches and values.
		/// </summary>
		[Test]
		public void CreateFwAppArgs_RunTogether()
		{
			var args = new FwAppArgs("-projmonkey", "-typexml");
			Assert.AreEqual("monkey", args.Database);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(1, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}

		/// <summary>
		/// Tests creating FwAppArgs when user is requesting help.
		/// </summary>
		[Test]
		public void CreateFwAppArgs_Help()
		{
			var args = new FwAppArgs("-?", "-db", "monkey");
			Assert.IsTrue(args.ShowHelp);
			Assert.AreEqual(string.Empty, args.Database, "Showing help should ignore all other parameters");
			Assert.AreEqual(string.Empty, args.DatabaseType, "Showing help should ignore all other parameters");
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);

			args = new FwAppArgs("-h");
			Assert.IsTrue(args.ShowHelp);
			Assert.AreEqual(string.Empty, args.Database);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);

			args = new FwAppArgs("-help");
			Assert.IsTrue(args.ShowHelp);
			Assert.AreEqual(string.Empty, args.Database);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}

		/// <summary>
		/// Tests creating FwAppArgs with a command-line parameter whose value is a
		/// quoted string consisting of multiple words.
		/// </summary>
		[Test]
		public void CreateFwAppArgs_MultiWordQuotedValue()
		{
			var args = new FwAppArgs("-db", "monkey on a string.fwdata");
			Assert.AreEqual("monkey on a string.fwdata", args.Database);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}

		/// <summary>
		/// Can open database by absolute path
		/// </summary>
		[Test]
		[Platform(Include = "Linux")]
		public void CreateFwAppArgs_DbAbsolutePath_Linux()
		{
			var args = new FwAppArgs("-db", "/database.fwdata");
			Assert.AreEqual("/database.fwdata", args.Database, "Should be able to open up database by absolute path");
			Assert.AreEqual(string.Empty, args.ConfigFile);
			Assert.AreEqual(string.Empty, args.DatabaseType);
			Assert.AreEqual(string.Empty, args.Locale);
			Assert.IsFalse(args.ShowHelp);
			Assert.AreEqual(0, args.LinkProperties.Count);
			Assert.AreEqual(string.Empty, args.Tag);
			Assert.AreEqual(Guid.Empty, args.TargetGuid);
			Assert.AreEqual(string.Empty, args.ToolName);
			Assert.IsFalse(args.HasLinkInformation);
		}
		#endregion
	}
}