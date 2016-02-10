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
}
