using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class FromattingTests
	{
		[TestInitialize]
		public void Initialize()
		{
			this.Engine = new TemplateProcessingEngine();
			this.Engine.FormattingCulture = CultureInfo.GetCultureInfo("et-ee");
		}

		private TemplateProcessingEngine Engine { get; set; }

		[TestMethod]
		public void TestFormattingForDateDefault()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28)
			};
			String template = "[[ADate]]";
			String expectedResult = "28.04.2011";

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithTimeDefault()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			String template = "[[ADate]]";
			String expectedResult = "28.04.2011 17:58";

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithStandardFormat()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			String template = "[[ADate:u]]";
			String expectedResult = "2011-04-28 17:58:41Z";

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFormattingForDateWithCustomFormat()
		{
			var model = new
			{
				ADate = new DateTime(2011, 04, 28, 17, 58, 41)
			};
			String template = @"[[ADate:""kala""dd]]";
			String expectedResult = "kala28";

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}
	}
}
