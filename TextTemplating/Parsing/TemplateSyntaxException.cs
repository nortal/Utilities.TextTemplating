using System;
using System.Runtime.Serialization;

namespace Nortal.Utilities.TextTemplating.Parsing
{

	/// <summary>
	/// Exception caused by incorrect template syntax.
	/// </summary>
	[Serializable]
	public sealed class TemplateSyntaxException : Exception
	{
		public TemplateSyntaxException() { }
		public TemplateSyntaxException(string message) : base(message) { }

		public TemplateSyntaxException(TemplateSentence sentence, string message) : base(message)
		{
			this.Sentence = sentence;
		}

		public TemplateSyntaxException(Command command, string message) : this(command?.Source, message) { }

		public TemplateSyntaxException(TemplateSentence sentence, string message, Exception inner) : base(message, inner)
		{
			this.Sentence = sentence;
		}
		private TemplateSyntaxException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null) { throw new ArgumentNullException(nameof(info)); }
			this.Sentence = (TemplateSentence)info.GetValue(nameof(this.Sentence), typeof(TemplateSentence));
		}

		/// <summary>
		/// Context sentence for this exception.
		/// </summary>
		public TemplateSentence Sentence { get; internal set; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(TemplateSentence), this.Sentence);
		}
	}
}
