// Copyright (c) 2015-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Collections.Generic;
using LanguageExplorer;
using LanguageExplorer.Filters;
using NUnit.Framework;
using SIL.LCModel;
using SIL.LCModel.Core.Text;
using SIL.LCModel.Core.WritingSystems;
using SIL.WritingSystems;

namespace LanguageExplorerTests.Filters
{
	[TestFixture]
	public class FindResultsSorterTests : MemoryOnlyBackendProviderRestoredForEachTestTestBase
	{
		private int _citationFlid;
		private int _definitionFlid;

		#region Overrides of LcmTestBase

		public override void TestSetup()
		{
			base.TestSetup();

			_citationFlid = LexEntryTags.kflidCitationForm;
			_definitionFlid = LexSenseTags.kflidDefinition;
		}

		#endregion

		[Test]
		public void SortIsAlphabeticalForNullSearchString()
		{
			var enWs = Cache.DefaultAnalWs;
			var nullString = TsStringUtils.MakeString(null, enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(nullString, sorter);
			var records = CreateRecords(new[] { "c", "b", "a" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "a", "b", "c" }, records);
		}

		[Test]
		public void SortIsAlphabeticalIfNoMatches()
		{

			var enWs = Cache.DefaultAnalWs;
			var noMatchString = TsStringUtils.MakeString("z", enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(noMatchString, sorter);
			var records = CreateRecords(new[] { "c", "b", "a" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "a", "b", "c" }, records);
		}

		[Test]
		public void FullMatchSortsFirstAlphabeticalAfter()
		{

			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("b", enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "c", "b", "a" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "b", "a", "c" }, records);
		}

		[Test]
		public void StartsWithMatchSortsFirstAlphabeticalAfter()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("b", enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "c", "bob", "a" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "bob", "a", "c" }, records);
		}

		[Test]
		public void OtherLanguageSystemCollationWhenCollationInValid()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("buburuq", enWs);

			var mvpWs = (CoreWritingSystemDefinition) Cache.WritingSystemFactory.get_EngineOrNull(enWs);
			mvpWs.DefaultCollation = new SystemCollationDefinition { LanguageTag = "mvp" };

			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, enWs), new WritingSystemComparer(mvpWs)));

			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "Ramban", "buburuq" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "buburuq", "Ramban" }, records);
		}

		[Test]
		public void SortDoesNotThrowWhenIcuRulesAreInvalid()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("buburuq", enWs);

			var mvpWs = (CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(enWs);
			mvpWs.DefaultCollation = new IcuRulesCollationDefinition("standard") { IcuRules = "a < b" };

			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, enWs), new WritingSystemComparer(mvpWs)));

			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "Ramban", "buburuq" });
			Assert.DoesNotThrow(() => resultsSorter.Sort(records));
			VerifySortOrder(new[] { "buburuq", "Ramban" }, records);
		}

		[Test]
		public void FullMatchIsFollowedByStartsWithAlphabeticalAfter()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("bob", enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "c", "bob", "a", "bob and more" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "bob", "bob and more", "a", "c" }, records);
		}

		[Test]
		public void FullMatchIsCaseIgnorant()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("bob", enWs);
			var sorter = new GenRecordSorter(new StringFinderCompare(new OwnMlPropFinder(Cache.DomainDataByFlid, _citationFlid, Cache.DefaultAnalWs),
				new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultAnalWs))));
			var resultsSorter = new FindResultSorter(matchString, sorter);
			var records = CreateRecords(new[] { "c", "Bob", "a", "Bob and more" });
			resultsSorter.Sort(records);
			VerifySortOrder(new[] { "Bob", "Bob and more", "a", "c" }, records);
		}

		[Test]
		public void EmptyDataForIndirectStringPropertyDoesNotCrash()
		{
			var enWs = Cache.DefaultAnalWs;
			var matchString = TsStringUtils.MakeString("irrelevant", enWs);
			// create a sorter that looks at the collection of definitions from the senses
			var sorter = new GenRecordSorter(new StringFinderCompare(new OneIndirectMlPropFinder(Cache.DomainDataByFlid, LexEntryTags.kflidSenses,
				_definitionFlid, Cache.DefaultVernWs), new WritingSystemComparer((CoreWritingSystemDefinition)Cache.WritingSystemFactory.get_EngineOrNull(Cache.DefaultVernWs))));
			var records = CreateRecords("WithDef", "WithoutDef");
			// SUT
			var resultsSorter = new FindResultSorter(matchString, sorter);
			resultsSorter.Sort(records);
			// order here isn't really the SUT. The fact that we got here is the real test.
			VerifySortOrder(new[] { "WithoutDef", "WithDef" }, records);
		}

		private void VerifySortOrder(string[] strings, List<IManyOnePathSortItem> sortedRecords)
		{
			for (var i = 0; i < strings.Length; ++i)
			{
				var record = sortedRecords[i];
				var entry = Cache.ServiceLocator.GetObject(record.KeyObject) as ILexEntry;
				Assert.AreEqual(strings[i], entry.CitationForm.get_String(Cache.DefaultAnalWs).Text);
			}
		}

		/// <summary>
		/// Creates one entry with a sense that has a definition and one entry without and returns the search records for them
		/// </summary>
		private List<IManyOnePathSortItem> CreateRecords(string withDef, string withoutDef)
		{
			var results = new List<IManyOnePathSortItem>();
			var entryfactory = Cache.ServiceLocator.GetInstance<ILexEntryFactory>();
			var headWord = TsStringUtils.MakeString(withDef, Cache.DefaultAnalWs);
			var lexEntry = entryfactory.Create();
			lexEntry.CitationForm.set_String(Cache.DefaultAnalWs, headWord);
			var senseFact = Cache.ServiceLocator.GetInstance<ILexSenseFactory>();
			var mainSense = senseFact.Create();
			lexEntry.SensesOS.Add(mainSense);
			var gloss = TsStringUtils.MakeString("definition", Cache.DefaultAnalWs);
			mainSense.Definition.set_String(Cache.DefaultVernWs, gloss);
			results.Add(new ManyOnePathSortItem(lexEntry));
			var entryWithoutDef = Cache.ServiceLocator.GetInstance<ILexEntryFactory>().Create();
			headWord = TsStringUtils.MakeString(withoutDef, Cache.DefaultAnalWs);
			entryWithoutDef.CitationForm.set_String(Cache.DefaultAnalWs, headWord);
			results.Add(new ManyOnePathSortItem(entryWithoutDef));
			return results;
		}

		private List<IManyOnePathSortItem> CreateRecords(IEnumerable<string> strings)
		{
			var results = new List<IManyOnePathSortItem>();
			var entryfactory = Cache.ServiceLocator.GetInstance<ILexEntryFactory>();
			foreach (var s in strings)
			{
				var headWord = TsStringUtils.MakeString(s, Cache.DefaultAnalWs);
				var lexEntry = entryfactory.Create();
				lexEntry.CitationForm.set_String(Cache.DefaultAnalWs, headWord);
				results.Add(new ManyOnePathSortItem(lexEntry));
			}
			return results;
		}
	}
}