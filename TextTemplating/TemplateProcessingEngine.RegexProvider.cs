/*
	Copyright 2013 Imre Pühvel, AS Nortal
	
	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'TemplateProcessingEngine.RegexProvider.cs'.
*/

using System;
using System.Text.RegularExpressions;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Provides regular expressions to implement configuration described by SyntaxSettings.
	/// </summary>
	partial class TemplateProcessingEngine
	{
		private sealed class RegexProvider
		{
			internal RegexProvider(SyntaxSettings settings)
			{
				this.Settings = settings ?? new SyntaxSettings();
				if (this.Settings.BeginTag == this.Settings.EndTag) { throw new ArgumentException("BeginTag and EndTag must be different.", "settings"); }
				InitializePatterns();
			}

			internal SyntaxSettings Settings { get; private set; }

			private void InitializePatterns()
			{
				const RegexOptions options = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline;

				this.RegexForConditional = new Regex(this.BuildRegexPatternForIf(), options);
				this.RegexForLoop = new Regex(this.BuildRegexPatternForLoop(), options);
				this.RegexForFields = new Regex(this.BuildRegexPatternForFields(), options);
				this.RegexForSubtemplate = new Regex(this.BuildRegexPatternForApplyTemplate(), options);
			}

			internal static class GroupNames
			{
				// constants not directly used within patterns to keep builds simpler to read. Should be sufficiently handled with unit-tests.
				internal const String LoopItems = @"items";
				internal const String LoopContent = @"content";
				internal const String Condition = @"condition";
				internal const String ConditionYesBlock = @"YesBlock";
				internal const String ConditionNoBlock = @"NoBlock";
				internal const String FieldName = @"name";
				internal const String FieldFormat = @"format";
				internal const String SubtemplateName = @"template";
				internal const String SubtemplateModel = @"model";
			}

			internal Regex RegexForLoop { get; private set; }
			internal Regex RegexForFields { get; private set; }
			internal Regex RegexForConditional { get; private set; }
			internal Regex RegexForSubtemplate { get; private set; }
			
			#region Build regex patterns
			private const String OptionalWhitespace = @"\s*";

			private String SyntaxStart { get { return Environment.NewLine + Regex.Escape(this.Settings.BeginTag) + OptionalWhitespace; } }
			private String SyntaxEnd { get { return OptionalWhitespace + Regex.Escape(this.Settings.EndTag) + Environment.NewLine; } }

			private String CommandStart(String command)
			{
				return SyntaxStart
					+ Regex.Escape(command)
					+ OptionalWhitespace + @"\("
					+ OptionalWhitespace;
			}

			private String CommandEnd
			{
				get { return OptionalWhitespace + @"\)" + SyntaxEnd; }
			}

			private String BuildRegexPatternForIf()
			{
				var settings = this.Settings;
				return CommandStart(settings.ConditionalStartCommand) + @" (?<condition>.+?) " + CommandEnd
					+ Environment.NewLine + @"(?<YesBlock>.*?)"
					+ Environment.NewLine + @"("
					+ Environment.NewLine + "\t" + CommandStart(settings.ConditionalElseCommand) + @" \k<condition> " + CommandEnd
					+ Environment.NewLine + "\t" + @"(?<NoBlock>.*?)"
					+ Environment.NewLine + @")?"
					+ Environment.NewLine + CommandStart(settings.ConditionalEndCommand) + @" \k<condition> " + CommandEnd;
			}

			private String BuildRegexPatternForLoop()
			{
				var settings = this.Settings;
				return CommandStart(settings.LoopStartCommand) + @" (?<items>.+?) " + CommandEnd
					+ Environment.NewLine + @"(?<content>.*?)"
					+ Environment.NewLine + CommandStart(settings.LoopEndCommand) + @" \k<items> " + CommandEnd;
			}

			private String BuildRegexPatternForFields()
			{
				return SyntaxStart + @" (?<name>[^\(\)]+?)" + OptionalWhitespace + @"(:(?<format> [^\(\)]+? ))? " + SyntaxEnd;
			}

			private String BuildRegexPatternForApplyTemplate() 
			{
				return CommandStart(this.Settings.SubtemplateCommand) + @" (?<template>[^,]+?)" + OptionalWhitespace
					+"," +OptionalWhitespace + "(?<model>.+?)" + CommandEnd;
			}
			#endregion
		}
	}
}
