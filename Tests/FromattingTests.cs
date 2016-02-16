using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class FromattingTests
	{
		
		private CultureInfo OriginalCulture { get; set; }

		[TestInitialize]
		public void Initialize()
		{
			this.OriginalCulture = CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("et-ee");
		}

		[TestCleanup]
		public void TearDown()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("et-ee");
		}

		[TestMethod]
		public void TestFormattingForDateDefault()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28)
			};
			const String template = "[[ADate]]";
			const String expectedResult = "28.04.2011";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithTimeDefault()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			const String template = "[[ADate]]";
			const String expectedResult = "28.04.2011 17:58";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithStandardFormat()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			const String template = "[[ADate:u]]";
			const String expectedResult = "2011-04-28 17:58:41Z";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithCustomFormat()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			const String template = @"[[ADate:""kala""dd]]";
			const String expectedResult = "kala28";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}
	}
}
