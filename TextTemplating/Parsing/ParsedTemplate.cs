using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a parsed and verified template in its syntax tree form.
	/// </summary>
	public class ParsedTemplate
	{
		internal ParsedTemplate(string originalTemplate, SyntaxTreeNode commandTree)
		{
			this.CommandTree = commandTree;
			this.OriginalTemplate = originalTemplate;
		}

		public String OriginalTemplate { get; private set; }

		public SyntaxTreeNode CommandTree { get; private set; }
	}
}
