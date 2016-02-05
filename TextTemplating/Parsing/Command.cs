using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	public class Command
	{
		public Command() { }
		public Command(CommandType type, TemplateSentence source)
		{
			this.Source = source;
			this.Type = type;
		}
		internal CommandType Type;
		internal TemplateSentence Source { get; set; }

		public override string ToString()
		{
			return this.Type + ": " + this.Source.OriginalText;
		}
	}


	

	class BindFromModelCommand : ModelPathCommand
	{
		public BindFromModelCommand() { }
		public BindFromModelCommand(CommandType type, TemplateSentence source) : base(type, source) { }
		internal String FormatString { get; set; }
	}

	class SubTemplateCommand : ModelPathCommand
	{
		public SubTemplateCommand()
		{

		}
		public SubTemplateCommand(CommandType type, TemplateSentence source) : base(type, source) { }
		internal String SubTemplateName;
	}
}
