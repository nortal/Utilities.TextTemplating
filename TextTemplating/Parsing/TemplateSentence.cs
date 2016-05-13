using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a meaningful section of template - either a block of text content or a templating language command.
	/// </summary>
	[Serializable]
	public sealed class TemplateSentence
	{
		internal TemplateSentence(String text, Boolean isCommand)
		{
			this.IsControlCommand = isCommand;
			this.Text = text;
		}
		public Boolean IsControlCommand { get; private set; }
		public String Text { get; private set; }

		// Tracked position of this sentence:
		public int SourceLine { get; internal set; }
		public int SourcePosition { get; internal set; }

		public override string ToString()
		{
			return SourcePositionFormatting.Format(SourceLine, SourcePosition)
				+ (IsControlCommand ? " Command " : " ")
				+ ShortenAndWrap(Text, 50); // a sentence can contain entire books worth of text. First N chars should be enough for overview.
		}

		private static String ShortenAndWrap(String input, int length)
		{
			const string wrapper = @"""";
			if (input == null) { return null; }
			if (input.Length <= length) { return wrapper + input + wrapper; }

			return wrapper + input.Substring(1, length) + "..." + wrapper;
		}
	}
}
