using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a meaningful section of template - either a block of text content or a templating language command.
	/// </summary>
	public class TemplateSentence
	{
		public TemplateSentence(String text, Boolean isCommand)
		{
			this.IsControlCommand = isCommand;
			this.Text = text;
		}
		public Boolean IsControlCommand { get; set; }
		public String Text { get; set; }

		// Tracked position of this sentence:
		public int SourceLine { get; set; }
		public int SourcePosition { get; set; }

		public override string ToString()
		{
			return string.Format(@"[Line {0}:{1} {2}] {3}",
				SourceLine, SourcePosition,
				IsControlCommand ? "Command" : "",
				Text);
		}
	}
}
