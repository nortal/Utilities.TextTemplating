using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	public class SubtemplateCommand : ModelPathCommand
	{
		public SubtemplateCommand() { }
		public SubtemplateCommand(CommandType type, TemplateSentence source) : base(type, source) { }

		internal String SubtemplateName { get; set; }
	}
}
