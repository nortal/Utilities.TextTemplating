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

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'SyntaxSettings.cs'.
*/

using System;
using System.Linq;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Contains configuration describing how functional parts of template look like.
	/// </summary>
	public class SyntaxSettings
	{
		//TODO: consider working on interface to allow taking these from existing Settings file.

		public SyntaxSettings()
		{
			ResetToDefaults();
		}

		public String BeginTag { get; set; }
		public String EndTag { get; set; }

		public String ConditionalStartCommand { get; set; }
		public String ConditionalElseCommand { get; set; }
		public String ConditionalEndCommand { get; set; }

		public String LoopStartCommand { get; set; }
		public String LoopEndCommand { get; set; }

		public String SubtemplateCommand { get; set; }
		public String SelfReferenceKeyword { get; set; }

		public String ExistsStartCommand { get; set; }
		public String ExistsElseCommand { get; set; }
		public String ExistsEndCommand { get; set; }

		/// <summary>
		/// Resets settings to default english values.
		/// </summary>
		public void ResetToDefaults()
		{
			BeginTag = @"[[";
			EndTag = @"]]";

			ConditionalStartCommand = @"if";
			ConditionalElseCommand = @"else";
			ConditionalEndCommand = @"endif";

			LoopStartCommand = @"for";
			LoopEndCommand = @"endfor";

			SubtemplateCommand = @"template";
			SelfReferenceKeyword = @"this";

			ExistsStartCommand = @"ifexists";
			ExistsElseCommand = @"elseexists";
			ExistsEndCommand = @"endifexists";
		}

		/// <summary>
		/// Performs various checks to make sure provided settings are valid for lexer/parser assumptions
		/// </summary>
		private void Validate()
		{
			const string exceptionPrefix = "Invalid syntax configuration: ";
			String[] allCommands = new string[]{
				ConditionalStartCommand,
				ConditionalElseCommand,
				ConditionalEndCommand,
				LoopStartCommand,
				LoopEndCommand,
				ExistsStartCommand,
				ExistsElseCommand,
				ExistsEndCommand,
				SubtemplateCommand,
			};

			// Make sure begin and end tags can be differentiated without looking for pair match.
			if (BeginTag.StartsWith(EndTag) || EndTag.StartsWith(BeginTag))
			{
				throw new TemplateProcessingException(exceptionPrefix + "start and end tag are too similar: '{0}' vs '{1}'.", BeginTag, EndTag);
			}

			// Make sure command names do not contain command tags themselves - avoids longer lookaheads and need to track back.
			var invalidCommandName = allCommands.FirstOrDefault(c => c.Contains(BeginTag) || c.Contains(EndTag));
			if (invalidCommandName != null)
			{
				throw new TemplateProcessingException(exceptionPrefix + "command name '{0}' must not contain start or end tag.", invalidCommandName);
			}

			// Make sure commands are unique - obvious misconfiguration.
			var duplicateCommandName = allCommands.GroupBy(item => item)
				.Where(group => group.Count() >= 2)
				.Select(group => group.Key)
				.FirstOrDefault();
			if (duplicateCommandName != null) { throw new TemplateProcessingException("multiple commands use name '{0}'.", duplicateCommandName); }
		}
	}
}
