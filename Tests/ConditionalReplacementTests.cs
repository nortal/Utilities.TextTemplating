﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class ConditionalReplacementTests
	{
		private const String TemplateContentPrefix = @"Some document prefix";
		private const String TemplateContentSuffix = @"Some suffix";

		private const String ExpectedToken = @"GOOD";
		private const String WrongToken = @"WRONG";

		[TestInitialize]
		public void Initialize()
		{
			this.Engine = new TemplateProcessingEngine();
		}

		private TemplateProcessingEngine Engine { get; set; }

		[TestMethod]
		public void TestConditionalIfTrue()
		{
			var model = new { ValueTrue = true };
			String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfFalse()
		{
			var model = new { ValueFalse = false };
			String template = TemplateContentPrefix
				+ "[[if(ValueFalse)]]" + WrongToken
				+ "[[endif(ValueFalse)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfTrueNullable()
		{
			var model = new { ValueTrue = (Boolean?)true };
			String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfElseTrue()
		{
			var model = new { ValueTrue = true };
			String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[else(ValueTrue)]]" + WrongToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfElseFalse()
		{
			var model = new { ValueFalse = false };
			String template = TemplateContentPrefix
				+ "[[if(ValueFalse)]]" + WrongToken
				+ "[[else(ValueFalse)]]" + ExpectedToken
				+ "[[endif(ValueFalse)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalNull()
		{
			var model = new { Value = (Boolean?)null };
			String template = TemplateContentPrefix
				+ "[[if(Value)]]" + WrongToken
				+ "[[else(Value)]]" + ExpectedToken
				+ "[[endif(Value)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void TestConditionalInvalidCondition()
		{
			var model = new { NotABoolean = "SomeString" };
			String template = TemplateContentPrefix
				+ "[[if(NotABoolean)]]" + WrongToken
				+ "[[else(NotABoolean)]]" + WrongToken
				+ "[[endif(NotABoolean)]]"
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
		}

		[TestMethod]
		public void TestConditionalSameIfMultipleTimes()
		{
			var model = new { ValueTrue = true };
			String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;
			template += template;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;
			expectedResult += expectedResult;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalNestedTrueThenFalse()
		{
			var model = new
			{
				ValueTrue = true,
				ValueFalse = false
			};
			String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken + "1"
					+ "[[if(ValueFalse)]]" + WrongToken
					+ "[[else(ValueFalse)]]" + ExpectedToken + "2"
					+ "[[endif(ValueFalse)]]"
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedToken + "1"
				+ ExpectedToken + "2"
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestFieldsReplacedInConditional()
		{
			var model = new
			{
				ABoolean = true,
				Value = ExpectedToken
			};
			String template = TemplateContentPrefix
				+ "[[if(ABoolean)]]"
					+ "[[Value]]"
				+ "[[endif(ABoolean)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ model.Value
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}
	}
}