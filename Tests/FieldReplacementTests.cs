using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class FieldReplacementTests
	{
		private const String TemplateContentPrefix = @"Some document prefix";
		private const String TemplateContentSuffix = @"Some suffix";
		private const String ExpectedToken = @"GOOD";

		private static readonly String TemplateCommandSeparator = Environment.NewLine;

		[TestInitialize]
		public void Initialize()
		{
			this.Engine = new TemplateProcessingEngine();
		}

		private TemplateProcessingEngine Engine { get; set; }

		private class MemberAccessTestModel
		{
			public String Field = "Field";
			public String Property { get { return "Property"; } }
			internal String NotPublic = "NotPublic";
		}

		[TestMethod]
		public void TestFieldsCanAccessFieldsAndProperties()
		{
			var model = new MemberAccessTestModel();
			String template = TemplateContentPrefix
				+ "[[Field]]"
				+ "[[Property]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ model.Field
				+ model.Property
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestFieldsCannotAccessNonPublicMembers()
		{
			var model = new MemberAccessTestModel();
			String template = TemplateContentPrefix
				+ "[[NotPublic]]"
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
		}

		[TestMethod]
		public void TestFieldsSimple()
		{
			var model = new
			{
				ABoolean = true,
				AInteger = 1,
				ANullableNotSet = (int?)null,
				ANullableSet = (int?)2,
				Child = new Object()
			};
			String template = TemplateContentPrefix
				+ "[[ABoolean]]" + TemplateCommandSeparator
				+ "[[AInteger]]" + TemplateCommandSeparator
				+ "[[ANullableNotSet]]" + TemplateCommandSeparator
				+ "[[ANullableSet]]" + TemplateCommandSeparator
				+ "[[Child]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix

				+ "True" + TemplateCommandSeparator
				+ "1" + TemplateCommandSeparator
				+ "" + TemplateCommandSeparator
				+ "2" + TemplateCommandSeparator
				+ "System.Object"
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFieldsChildObjectField()
		{
			var model = new
			{
				Child = new
				{
					SecondChild = new
					{
						Value = ExpectedToken
					}
				}
			};

			String template = "[[Child.SecondChild.Value]]";

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(ExpectedToken, actual);
		}


		[TestMethod]
		public void TestFieldsChildPropertyWithParentNull()
		{
			var model = new
			{
				Child = (Object)null
			};
			String template = TemplateContentPrefix
				+ "[[Child.NotParsed.NotparsedAsWell.Value]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFieldReplacementIgnoresCommands()
		{
			var model = new { Value = "WRONG." };
			String template = TemplateContentPrefix
				+ "[[somecommand(Value)]]"
				+ TemplateContentSuffix;

			String expectedResult = template;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFieldsIgnoreWhitespace()
		{
			var model = new { Value = ExpectedToken };
			String template = TemplateContentPrefix
				+ "[[ \t \n\r    Value  \t \n\r    ]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}
	}
}
