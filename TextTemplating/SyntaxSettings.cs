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

namespace Nortal.Utilities.TextTemplating
{
	public class SyntaxSettings
	{
		public SyntaxSettings()
		{
			Reset();
		}

		public String BeginTag { get; set; }
		public String EndTag { get; set; }

		public String ConditionalStartCommand { get; set; }
		public String ConditionalElseCommand { get; set; }
		public String ConditionalEndCommand { get; set; }

		public String LoopStartCommand { get; set; }
		public String LoopEndCommand { get; set; }

		public String SubtemplateCommand { get; set; }

		private void Reset()
		{
			BeginTag = @"[[";
			EndTag = @"]]";

			ConditionalStartCommand = @"if";
			ConditionalElseCommand = @"else";
			ConditionalEndCommand = @"endif";

			LoopStartCommand = @"for";
			LoopEndCommand = @"endfor";

			SubtemplateCommand = @"template";
		}
	}
}
