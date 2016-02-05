using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	class ModelPathCommand : Command
	{
		public ModelPathCommand() { }
		public ModelPathCommand(CommandType type, TemplateSentence source) : base(type, source) { }

		internal String ModelPath { get; set; }
	}
}
