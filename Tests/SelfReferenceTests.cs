using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SelfReferenceTests
	{

		private class TestModel
		{
			public String Name { get; set; }

			public override string ToString()
			{
				return "TestModelSelf";
			}
		}

		[TestMethod]
		public void TestSelfReferenceAsField()
		{
			var model = new TestModel
			{
				Name = "Parent",
			};

			const String template = "[[this]]";
			const String expected = "TestModelSelf";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expected, actual);
		}


		[TestMethod]
		public void TestSelfReferenceInLoop()
		{
			var model = new TestModel[]
			{
				new TestModel { Name = "1" },
				new TestModel { Name = "2" },
			};

			const String template = @""
				+ "[[ for(this) ]]"
				+ "[[this.Name]]"
				+ "[[ endfor(this) ]]";

			const String expected = "12";

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TestSelfReferenceInSubTemplate()
		{
			var model = new TestModel{Name = "Test"};

			const String template = @""
				+ "Model: [[Name]] "
				+ "Subtemplate [[template(SUB, this)]]";

			const String expected = @""
				+ "Model: Test "
				+ "Subtemplate SUB: Test";

			var parsed = TextTemplate.Parse(template);
			parsed.AddSubtemplate("SUB", "SUB: [[Name]]");

			String actual = parsed.BuildDocument(model);
			Assert.AreEqual(expected, actual);
		}
	}
}
