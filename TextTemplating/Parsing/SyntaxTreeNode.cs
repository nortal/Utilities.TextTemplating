using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a subtree of template syntax tree with a single executable command at its root.
	/// </summary>
	public class SyntaxTreeNode
	{
		public SyntaxTreeNode() { }
		public SyntaxTreeNode(Command command) { this.Command = command; }

		/// <summary>
		/// Declares the acting command owning the nested child scopes.
		/// </summary>
		public Command Command { get; internal set; }

		/// <summary>
		/// Collection of child commands. Null if not applicable to current command.
		/// </summary>
		public List<SyntaxTreeNode> PrimaryScope { get; internal set; }

		/// <summary>
		/// Secondary collection of child commands. For example, to store the else-branch in a if-else-command. Null if not applicable to current command.
		/// </summary>
		public List<SyntaxTreeNode> SecondaryScope { get; internal set; }


		public override string ToString()
		{
			return this.Command?.ToString() ?? "<EmptyCommand>";
		}
	}
}