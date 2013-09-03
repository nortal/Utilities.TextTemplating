using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating;

namespace Nortal.Utilities.TemplatingEngine.Tests
{
	[TestClass]
	public class SelfReferenceTests
	{
		private class TestEngine : TemplateProcessingEngine
		{
			protected override string ResolveSubtemplateByName(string templateName)
			{
				return templateName + ": [[Name]]";
			}
		}

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

			String template = "[[this]]";
			String expected = "TestModelSelf";

			String actual = new TestEngine().Process(template, model);
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

			String template = @""
				+ "[[ for(this) ]]"
				+ "[[this.Name]]"
				+ "[[ endfor(this) ]]";

			String expected = "12";

			String actual = new TestEngine().Process(template, model);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TestSelfReferenceInSubTemplate()
		{
			var model = new TestModel{Name = "Test"};

			String template = @""
				+ "Model: [[Name]]"
				+ "Subtemplate [[template(SUB, this)]]";

			String expected = @""
				+ "Model: Test"
				+ "Subtemplate SUB: Test";

			String actual = new TestEngine().Process(template, model);
			Assert.AreEqual(expected, actual);
		}
	}
}
