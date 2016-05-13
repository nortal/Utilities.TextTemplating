using System;
using System.Runtime.Serialization;

namespace Nortal.Utilities.TextTemplating.Parsing
{

	/// <summary>
	/// Exception thrown for incorrect template syntax.
	/// </summary>
	[Serializable]
	public sealed class TemplateSyntaxException : Exception
	{
		public TemplateSyntaxException() { }

		public TemplateSyntaxException(string message) : base(message) { }

		public TemplateSyntaxException(TemplateSentence sentence, string message)
			: base(BuildMessage(message, sentence))
		{
			this.Sentence = sentence;
		}

		public TemplateSyntaxException(Command command, string message)
			: this(command?.Source, message)
		{ }

		public TemplateSyntaxException(TemplateSentence sentence, string message, Exception inner)
			: base(BuildMessage(message, sentence), inner)
		{
			this.Sentence = sentence;
		}

		/// <summary>
		/// Context sentence for this exception.
		/// </summary>
		public TemplateSentence Sentence { get; private set; }

		private static String BuildMessage(String message, TemplateSentence sentence = null)
		{
			if (sentence == null) { return "Invalid syntax. " + message; }
			return $"Invalid syntax at {sentence}. {message}";
		}

		#region serialization
		private TemplateSyntaxException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null) { throw new ArgumentNullException(nameof(info)); }
			this.Sentence = (TemplateSentence)info.GetValue(nameof(this.Sentence), typeof(TemplateSentence));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(TemplateSentence), this.Sentence);
		}
		#endregion
	}
}
