using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SubtemplatingTests
	{
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

			String actual = parsed.BuildDocument(model);
			Assert.AreEqual(expected, actual);
		}
	}
}
