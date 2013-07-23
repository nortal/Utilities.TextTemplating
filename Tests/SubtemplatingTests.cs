using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating;

namespace Nortal.Utilities.TemplatingEngine.Tests
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

			String template = @""
				+ "[[Name]]"
				+ "[[template(MY, Child)]]"
				+ "[[template(ANOTHER, AnotherChild)]]";

			String expected = @""
				+ "Parent"
				+ "Subtemplate MY: Uudu"
				+ "Subtemplate ANOTHER: Aadu";

			String actual = new TestEngine().Process(template, model);
			Assert.AreEqual(expected, actual);
		}
	}
}
