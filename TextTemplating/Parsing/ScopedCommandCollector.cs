using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Class to collect commands to a command hierarchy with nested scopes. Used for building the syntax tree.
	/// </summary>
	internal class ScopedCommandCollector
	{
		internal Command ActiveCommand = null;
		private Stack<Command> CommandStack = new Stack<Command>();

		internal List<SyntaxTreeNode> ActiveScope = new List<SyntaxTreeNode>(); // == root node.
		private Stack<List<SyntaxTreeNode>> ScopeStack = new Stack<List<SyntaxTreeNode>>();

		internal void StartChildScope(Command scopeStartingCommand)
		{
			CommandStack.Push(ActiveCommand);
			ActiveCommand = scopeStartingCommand;
			ScopeStack.Push(ActiveScope);
			ActiveScope = new List<SyntaxTreeNode>(); // start new scope.
		}

		internal void AddToScope(Command command)
		{
			AddToScope(new SyntaxTreeNode(command));
		}

		internal void AddToScope(SyntaxTreeNode node)
		{
			this.ActiveScope.Add(node);
		}

		internal void RestoreParentScope()
		{
			ActiveCommand = CommandStack.Pop();
			ActiveScope = ScopeStack.Pop();
		}

		public override string ToString()
		{
			return "Scoped: " + this.ActiveScope.Count + "; Active=" + this.ActiveCommand;
		}
	}
}
