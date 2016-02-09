using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	public static class TemplateParser
	{
		public static TextTemplate Parse(String template, SyntaxSettings syntax)
		{
			var sentences = SentenceScanner.Scan(template, syntax.BeginTag, syntax.EndTag);
			var commands = ParseSentencesToCommands(sentences, syntax);
			var commandTree = BuildSyntaxTree(commands);

			return new TextTemplate(template, commandTree);
		}


		public static SyntaxTreeNode BuildSyntaxTree(IEnumerable<Command> commands)
		{
			var scopeCollector = new ScopedCommandCollector();

			foreach (var command in commands)
			{
				switch (command.Type)
				{
					case CommandType.Copy:
					case CommandType.BindFromModel:
						// simple commans go straight to parent collection, no starting-closing scope.
						scopeCollector.AddToScope(command);
						continue;
					case CommandType.If:
					case CommandType.IfExists:
					case CommandType.Loop:
						scopeCollector.StartChildScope(command);
						break;
					case CommandType.IfElse:
					case CommandType.IfExistsElse:
					case CommandType.Else:
						ValidateElseCommand(scopeCollector.ActiveCommand, command);
						scopeCollector.StartChildScope(command);
						break;
					case CommandType.IfEnd:
					case CommandType.IfExistsEnd:
						ValidateExplicitEndCommand(scopeCollector.ActiveCommand, command);

						var conditionalNode = new SyntaxTreeNode();

						if (scopeCollector.ActiveCommand.Type == CommandType.IfElse || scopeCollector.ActiveCommand.Type == CommandType.IfExistsElse)
						{
							// else block presence indicated that if command used 2 scopes, pick up false-branch:
							conditionalNode.SecondaryScope = scopeCollector.ActiveScope;
							scopeCollector.RestoreParentScope();
						}

						//pick up the true-branch:
						conditionalNode.Command = scopeCollector.ActiveCommand;
						conditionalNode.PrimaryScope = scopeCollector.ActiveScope;
						scopeCollector.RestoreParentScope();

						scopeCollector.AddToScope(conditionalNode);
						break;
					case CommandType.LoopEnd:
						ValidateExplicitLoopEnd(scopeCollector.ActiveCommand, command);
						var loopNode = new SyntaxTreeNode(scopeCollector.ActiveCommand);
						loopNode.PrimaryScope = scopeCollector.ActiveScope;
						scopeCollector.RestoreParentScope();
						scopeCollector.AddToScope(loopNode);
						break;
					case CommandType.SubTemplate:
						throw new NotImplementedException();
					case CommandType.End:
						var type = scopeCollector.ActiveCommand.Type;
						if (type == CommandType.If || type == CommandType.IfElse
							|| type == CommandType.IfExists || type == CommandType.IfExists)
						{
							throw new NotImplementedException(); //reuse logic  for ifEnd
						}

						throw new NotImplementedException(); // reuse logic for loopEnd
					default:
						throw new NotImplementedException("Command behavior is not specified for " + command.Type);
				}
			}

			if (scopeCollector.ActiveCommand != null)
			{
				throw new Exception("Started command was not properly closed with and ending tag. Command: " + scopeCollector.ActiveCommand);
			}
			var root = new SyntaxTreeNode();
			root.PrimaryScope = scopeCollector.ActiveScope;
			return root;
		}


		private static void ValidateExplicitLoopEnd(Command activeCommand, Command command)
		{
			//throw new NotImplementedException();
		}

		private static void ValidateExplicitEndCommand(Command activeCommand, Command command)
		{
			//throw new NotImplementedException();
		}

		private static void ValidateElseCommand(Command activeCommand, Command command)
		{
			//throw new NotImplementedException();
		}

		private static IEnumerable<Command> ParseSentencesToCommands(IEnumerable<TemplateSentence> sentences, SyntaxSettings syntax)
		{
			foreach (var sentence in sentences)
			{
				if (!sentence.IsControlCommand)
				{
					yield return new Command(CommandType.Copy, sentence);
					continue;
				}

				Debug.Assert(sentence.IsControlCommand);
				String functionName;
				String[] arguments;
				if (FunctionCallParser.TryParseCommand(sentence.OriginalText, out functionName, out arguments))
				{
					CommandType type = RecognizeFunctionName(functionName, syntax);
					switch (type)
					{
						case CommandType.If:
						case CommandType.IfElse:
						case CommandType.IfEnd:
						case CommandType.IfExists:
						case CommandType.IfExistsElse:
						case CommandType.IfExistsEnd:
						case CommandType.Loop:
						case CommandType.LoopEnd:
							var modelPathCommand = new ModelPathCommand(type, sentence);
							modelPathCommand.ModelPath = RequireArgument(type, arguments, 0);
							yield return modelPathCommand;
							continue;
						case CommandType.SubTemplate:
							var command = new SubTemplateCommand(type, sentence);
							command.SubTemplateName = RequireArgument(type, arguments, 0);
							command.ModelPath = RequireArgument(type, arguments, 1);
							yield return command;
							continue;
						case CommandType.Unspecified:
							throw new TemplateProcessingException("Unrecognized control command found in sentence " + sentence.ToString());
						default:
							throw new NotImplementedException("Unhandled command found with type: " + type);
					}
				}
				else
				{
					var modelBindCommand = new ModelPathCommand(CommandType.BindFromModel, sentence);
					modelBindCommand.ModelPath = sentence.OriginalText.Trim();
					yield return modelBindCommand;
					continue;
				}
			}
		}

		private static String RequireArgument(CommandType type, string[] arguments, int argumentIndex)
		{
			if (arguments == null || arguments.Length < argumentIndex) { throw new Exception("Missing expected argument for command function."); }
			return arguments[argumentIndex];
		}

		private static CommandType RecognizeFunctionName(string functionName, SyntaxSettings syntax)
		{
			if (functionName == syntax.ConditionalStartCommand) { return CommandType.If; }
			if (functionName == syntax.ConditionalElseCommand) { return CommandType.IfElse; }
			if (functionName == syntax.ConditionalEndCommand) { return CommandType.IfEnd; }
			if (functionName == syntax.ExistsStartCommand) { return CommandType.IfExists; }
			if (functionName == syntax.ExistsElseCommand) { return CommandType.IfExistsElse; }
			if (functionName == syntax.ExistsEndCommand) { return CommandType.IfExistsEnd; }

			if (functionName == syntax.LoopStartCommand) { return CommandType.Loop; }
			if (functionName == syntax.LoopEndCommand) { return CommandType.LoopEnd; }
			return CommandType.Unspecified;
		}
	}
}
