using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class ConditionalReplacementTests
	{
		private const String TemplateContentPrefix = @"Some document prefix";
		private const String TemplateContentSuffix = @"Some suffix";

		private const String ExpectedToken = @"GOOD";
		private const String WrongToken = @"WRONG";

		[TestMethod]
		public void TestConditionalIfTrue()
		{
			var model = new { ValueTrue = true };
			const String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate. Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfFalse()
		{
			var model = new { ValueFalse = false };
			const String template = TemplateContentPrefix
				+ "[[if(ValueFalse)]]" + WrongToken
				+ "[[endif(ValueFalse)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfTrueNullable()
		{
			var model = new { ValueTrue = (Boolean?)true };
			const String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfElseTrue()
		{
			var model = new { ValueTrue = true };
			const String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken
				+ "[[else(ValueTrue)]]" + WrongToken
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalIfElseFalse()
		{
			var model = new { ValueFalse = false };
			const String template = TemplateContentPrefix
				+ "[[if(ValueFalse)]]" + WrongToken
				+ "[[else(ValueFalse)]]" + ExpectedToken
				+ "[[endif(ValueFalse)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestConditionalNull()
		{
			var model = new { Value = (Boolean?)null };
			const String template = TemplateContentPrefix
				+ "[[if(Value)]]" + WrongToken
				+ "[[else(Value)]]" + ExpectedToken
				+ "[[endif(Value)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateProcessingException))]
		public void TestConditionalInvalidCondition()
		{
			var model = new { NotABoolean = "SomeString" };
			const String template = TemplateContentPrefix
				+ "[[if(NotABoolean)]]" + WrongToken
				+ "[[else(NotABoolean)]]" + WrongToken
				+ "[[endif(NotABoolean)]]"
				+ TemplateContentSuffix;

			// ReSharper disable once UnusedVariable
			String actual = TextTemplate.Parse(template).BuildDocument(model);
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

			String actual = TextTemplate.Parse(template).BuildDocument(model);
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
			const String template = TemplateContentPrefix
				+ "[[if(ValueTrue)]]" + ExpectedToken + "1"
					+ "[[if(ValueFalse)]]" + WrongToken
					+ "[[else(ValueFalse)]]" + ExpectedToken + "2"
					+ "[[endif(ValueFalse)]]"
				+ "[[endif(ValueTrue)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken + "1"
				+ ExpectedToken + "2"
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
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
			const String template = TemplateContentPrefix
				+ "[[if(ABoolean)]]"
					+ "[[Value]]"
				+ "[[endif(ABoolean)]]"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ model.Value
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestExistsConditionalIfTrue()
		{
			var model = new { Value = "this value is not null" };
			const String template = TemplateContentPrefix
				+ "[[ifexists(Value)]]" + ExpectedToken
				+ "[[endifexists(Value)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestExistsConditionalIfFalse()
		{
			var model = new { Value = (String)null };
			const String template = TemplateContentPrefix
				+ "[[ifexists(Value)]]" + ExpectedToken
				+ "[[endifexists(Value)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				// nothing is added
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestExistsConditionalIfElseFalse()
		{
			var model = new { Value = (String)null };
			const String template = TemplateContentPrefix
				+ "[[ifexists(Value)]]" + WrongToken
				+ "[[elseexists(Value)]]" + ExpectedToken
				+ "[[endifexists(Value)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ ExpectedToken
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}


		[TestMethod]
		public void TestExistsCollectionConditionalIfTrue()
		{
			var model = new List<DictionaryEntry>()
			{
				new DictionaryEntry() {Key = "Key1", Value = "Value1"},
				new DictionaryEntry() {Key = "Key2", Value = "Value2"}
			};
			
			const String template = TemplateContentPrefix
				+ "[[ifexists(this)]]" 
				+ "[[for(this)]]"
				+ "[[this.Value]]"
				+ "[[endfor(this)]]" 
				+ "[[endifexists(this)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				+ "Value1"
				+ "Value2"
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		/// <summary>
		/// This is a typical scenario where by "ifexists(collection)" we want to test if there are items.
		/// </summary>
		[TestMethod]
		public void TestExistsCollectionConditionalIfEmpty()
		{
			List<DictionaryEntry> model = new List<DictionaryEntry>(0);

			const String template = TemplateContentPrefix
				+ "[[ifexists(this)]]"
				+ "data:"
				+ "[[for(this)]]"
				+ "X"
				+ "[[endfor(this)]]"
				+ "[[endifexists(this)]]"
				+ TemplateContentSuffix;

			const String expectedResult = TemplateContentPrefix
				//no "data:"
				+ TemplateContentSuffix;

			String actual = TextTemplate.Parse(template).BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}
	}
}
