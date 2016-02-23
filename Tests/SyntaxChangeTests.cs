using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SyntaxChangeTests
	{
		private const String TemplateContentPrefix = @"Some document prefix";
		private const String TemplateContentSuffix = @"Some suffix";

		private const String ExpectedValue = @"EXPECTED";

		private static SyntaxSettings CreateSettings()
		{
			// for testing we'll define a mixture of html-like tags and estonian translations of keywords:
			SyntaxSettings settings = new SyntaxSettings();
			settings.BeginTag = "<b>";
			settings.EndTag = "</b>";
			settings.ConditionalStartCommand = "kui";
			settings.ConditionalElseCommand = "muidu";
			settings.ConditionalEndCommand = "/kui";
			settings.LoopStartCommand = "tsükkel";
			settings.LoopEndCommand = "/tsükkel";
			settings.SelfReferenceKeyword = "mina";
			return settings;
		}


		[TestMethod]
		public void TestCustomSyntax()
		{
			var model = new
			{
				ABoolean = true,
				AField = ExpectedValue,
				Items = "123".ToCharArray()
			};
			const String template = TemplateContentPrefix
				+ "<b>kui(ABoolean)</b>"
					+ ExpectedValue
				+ "<b>/kui(ABoolean)</b>"
				+ "<b>AField</b>"
				+ "<b>tsükkel(Items)</b>"
				+ "X"
				+ "<b>/tsükkel(Items)</b>"
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedValue
				+ model.AField
				+ "XXX"
				+ TemplateContentSuffix;

			var parsed = TextTemplate.Parse(template, CreateSettings());
			String actual = parsed.BuildDocument(model);
			Assert.AreEqual(expectedResult, actual);
		}

		[TestMethod]
		public void TestCustomSyntaxSelfReferenceKeyword()
		{
			Object model = new { Value = "ModelHere" };

			const String template = @""
				+ "<b>Value</b>"
				+ "<b>template(MY, mina)</b>"
				+ "<b>template(ANOTHER, mina)</b>";

			const String expected = @""
				+ "ModelHere"
				+ "Subtemplate MY: ModelHere"
				+ "Subtemplate ANOTHER: ModelHere";

			var syntax = CreateSettings();
			var parsed = TextTemplate.Parse(template, syntax);
			parsed.AddSubtemplate("MY", "Subtemplate MY: <b>Value</b>", syntax);
			parsed.AddSubtemplate("ANOTHER", "Subtemplate ANOTHER: <b>Value</b>", syntax);

			String actual = parsed.BuildDocument(model);
			Assert.AreEqual(expected, actual);
		}
	}
}
