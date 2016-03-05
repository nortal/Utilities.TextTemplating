using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a special command with 2 arguments: referenced template name and a path within current model.
	/// </summary>
	public class SubtemplateCommand : ModelPathCommand
	{
		public SubtemplateCommand(CommandType type, TemplateSentence source) : base(type, source) { }

		public String SubtemplateName { get; set; }
	}
}
