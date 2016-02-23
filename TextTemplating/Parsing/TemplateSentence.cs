using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a meaningful section of template - either a block of text content or a templating language command.
	/// </summary>
	public class TemplateSentence
	{
		public TemplateSentence(String originalText, Boolean isCommand)
		{
			this.IsControlCommand = isCommand;
			this.OriginalText = originalText;
		}
		public Boolean IsControlCommand { get; set; }
		public String OriginalText { get; set; }

		public int SourceLine { get; set; }
		public int SourcePosition { get; set; }

		public override string ToString()
		{
			return string.Format(@"[Line {0}:{1} {2}] {3}",
				SourceLine, SourcePosition,
				IsControlCommand ? "Command" : "",
				OriginalText);
		}
	}
}
