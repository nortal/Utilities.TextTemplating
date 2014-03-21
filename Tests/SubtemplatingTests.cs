using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

			String actual = new TestEngine().Process(template, model);
			Assert.AreEqual(expected, actual);
		}
	}
}
