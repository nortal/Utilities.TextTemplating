using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class LoopReplacementTests
	{
		private const String TemplateContentPrefix = @"prefix ";
		private const String TemplateContentSuffix = @"suffix ";

		private const String ExpectedValue = "EXPECTED";

		private class LoopTestModel
		{
			public LoopTestModel(String name) { this.Name = name; }
			// ReSharper disable NotAccessedField.Local
			public String Name;
			public Object Children; // to attach Children of various types if needed.
			// ReSharper restore NotAccessedField.Local

		}

		[TestMethod]
		public void TestLoopSimple()
		{
			var model = new LoopTestModel("root");
			model.Children = "123".ToCharArray();

			const String template = TemplateContentPrefix
				+ "[[ for(Children) ]]"
				+ ExpectedValue
				+ "[[ endfor(Children) ]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedValue
				+ ExpectedValue
				+ ExpectedValue
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestLoopWithindocuments()
		{
			var model = new LoopTestModel("root");
			model.Children = "123".ToCharArray();

			const String template = TemplateContentPrefix
				+ "[[ Name ]] "
				+ "[[ Children.Length]] "
				+ "[[ for(Children) ]]"
					+ "Child[[Children]] "
				+ "[[ endfor(Children) ]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ "root 3 Child1 Child2 Child3 "
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestLoopNestedSimple()
		{
			var model = new LoopTestModel("root");
			model.Children = new LoopTestModel[]
			{
				new LoopTestModel("Child1")
				{
					Children = new LoopTestModel[]
					{
						new LoopTestModel("Child11"),
						new LoopTestModel("Child12"),
						new LoopTestModel("Child13"),
					}
				},
				new LoopTestModel("Child2"),
				new LoopTestModel("Child3")
				{
					Children = new LoopTestModel[]
					{
						new LoopTestModel("Child31"),
						new LoopTestModel("Child32"),
					}
				}
			};
			const String template = TemplateContentPrefix
				+ "[[ for(Children) ]]"
				+ "[[	for(Children.Children) ]]"
				+ "[[		Children.Children.Name ]]"
				+ "[[	endfor(Children.Children) ]]"
				+ "[[ endfor(Children) ]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ "Child11"
				+ "Child12"
				+ "Child13"
				+ "Child31"
				+ "Child32"
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestLoopNestedWithMixedLevelFields()
		{
			var model = new LoopTestModel("root");
			model.Children = new LoopTestModel[]
			{
				new LoopTestModel("Child1")
				{
					Children = new LoopTestModel[]
					{
						new LoopTestModel("Child11"),
						new LoopTestModel("Child12"),
						new LoopTestModel("Child13"),
					}
				},
				new LoopTestModel("Child2"), // no children
				new LoopTestModel("Child3")
				{
					Children = new LoopTestModel[]
					{
						new LoopTestModel("Child31"),
						new LoopTestModel("Child32"),
					}
				}
			};
			const String template = TemplateContentPrefix
				+ "[[for(Children) ]]"
				+ "[[	for(Children.Children) ]]"
				+ " | [[		Name]] -> [[Children.Name]] -> [[Children.Children.Name]]"
				+ "[[	endfor(Children.Children) ]]"
				+ "[[endfor(Children) ]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ " | root -> Child1 -> Child11"
				+ " | root -> Child1 -> Child12"
				+ " | root -> Child1 -> Child13"
				+ " | root -> Child3 -> Child31"
				+ " | root -> Child3 -> Child32"
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

	}
}
