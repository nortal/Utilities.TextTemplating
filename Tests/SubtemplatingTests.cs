using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating.Parsing;
using Nortal.Utilities.TextTemplating.Executing;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SubtemplatingTests
	{
		private class TestEngine : TemplateProcessingEngine
		{
			protected override string ResolveSubtemplateByName(string templateName)
			{
				return "Subtemplate " + templateName + ": [[Name]]";
			}
		}

		[TestMethod]
		public void TestSubTemplate()
		{
			var model = new
			{
				Name = "Parent",
				Child = new { Name = "Uudu" },
				AnotherChild = new { Name = "Aadu" }
			};

			const String template = @""
				+ "[[Name]]"
				+ "[[template(MY, Child)]]"
				+ "[[template(ANOTHER, AnotherChild)]]";

			const String expected = @""
				+ "Parent"
				+ "Subtemplate MY: Uudu"
				+ "Subtemplate ANOTHER: Aadu";

			var parsed = TextTemplate.Parse(template);
			parsed.AddSubtemplate("MY", "Subtemplate MY: [[Name]]");
			parsed.AddSubtemplate("ANOTHER", "Subtemplate ANOTHER: [[Name]]");

			String actual = TemplateExecutionEngine.CreateDocument(parsed, model);
			Assert.AreEqual(expected, actual);
		}
	}
}
