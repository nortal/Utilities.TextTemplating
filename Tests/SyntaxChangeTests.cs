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
	}
}
