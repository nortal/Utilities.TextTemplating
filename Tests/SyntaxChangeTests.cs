using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SyntaxChangeTests
	{
		private const String TemplateContentPrefix = @"Some document prefix";
		private const String TemplateContentSuffix = @"Some suffix";

		private static readonly String TemplateCommandSeparator = Environment.NewLine;
		private const String ExpectedValue = @"EXPECTED";

		[TestInitialize]
		public void Initialize()
		{
			SyntaxSettings settings = new SyntaxSettings();
			settings.BeginTag = "=|";
			settings.EndTag = "|=";
			settings.ConditionalStartCommand = "kui";
			settings.ConditionalElseCommand = "muidu";
			settings.ConditionalEndCommand = "/kui";
			settings.LoopStartCommand = "tsükkel";
			settings.LoopEndCommand = "/tsükkel";

			this.Engine = new TemplateProcessingEngine(settings);
		}

		private TemplateProcessingEngine Engine { get; set; }

		[TestMethod]
		public void TestCustomSyntax()
		{
			var model = new
			{
				ABoolean = true,
				AField = ExpectedValue,
				Items = "123".ToCharArray()
			};
			String template = TemplateContentPrefix
				+ "=|kui(ABoolean)|="
					+ ExpectedValue
				+ "=|/kui(ABoolean)|="
				+ "=|AField|="
				+ "=|tsükkel(Items)|="
				+ "X"
				+ "=|/tsükkel(Items)|="
				+ TemplateContentSuffix;

			String expectedResult = TemplateContentPrefix
				+ ExpectedValue
				+ model.AField
				+ "XXX"
				+ TemplateContentSuffix;

			String actual = this.Engine.Process(template, model);
			Assert.AreEqual(expectedResult, actual);
		}

	}
}
