﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Contains and orchestrates logic for parsing a template text to its syntax tree form.
	/// </summary>
	internal static class TemplateParser
	{
		internal static TextTemplate Parse(String template, SyntaxSettings syntax)
		{
			var sentences = SentenceScanner.Scan(template, syntax.BeginTag, syntax.EndTag);
			var commands = ParseSentencesToCommands(sentences, syntax);
			var commandTree = BuildSyntaxTree(commands);

			return new TextTemplate(template, commandTree);
		}

		private static SyntaxTreeNode BuildSyntaxTree(IEnumerable<Command> commands)
		{
			var scopeCollector = new ScopedCommandCollector();

			foreach (var command in commands)
			{
				switch (command.Type)
				{
					case CommandType.Copy:
					case CommandType.BindFromModel:
					case CommandType.Subtemplate:
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
						ValidateCommandWithParentScope(scopeCollector.ActiveCommand, command);
						scopeCollector.StartChildScope(command);
						break;
					case CommandType.IfEnd:
					case CommandType.IfExistsEnd:
						ValidateCommandWithParentScope(scopeCollector.ActiveCommand, command);
						var conditionalNode = new SyntaxTreeNode();

						if (scopeCollector.ActiveCommand.Type == CommandType.IfElse || scopeCollector.ActiveCommand.Type == CommandType.IfExistsElse)
						{
							// else block presence indicated that if command used 2 scopes, pick up false-branch:
							conditionalNode.SecondaryScope = scopeCollector.ActiveScope;
							scopeCollector.RestoreParentScope();
						}

						//pick up the true-branch and continue parsing parent scope:
						conditionalNode.Command = scopeCollector.ActiveCommand;
						conditionalNode.PrimaryScope = scopeCollector.ActiveScope;
						scopeCollector.RestoreParentScope();

						scopeCollector.AddToScope(conditionalNode);
						break;
					case CommandType.LoopEnd:
						ValidateCommandWithParentScope(scopeCollector.ActiveCommand, command);
						var loopNode = new SyntaxTreeNode(scopeCollector.ActiveCommand);
						loopNode.PrimaryScope = scopeCollector.ActiveScope;
						scopeCollector.RestoreParentScope();
						scopeCollector.AddToScope(loopNode);
						break;
					case CommandType.End:
						var type = scopeCollector.ActiveCommand.Type;
						if (type == CommandType.If || type == CommandType.IfElse
							|| type == CommandType.IfExists || type == CommandType.IfExistsElse)
						{
							throw new NotImplementedException(); //reuse logic  for ifEnd
						}
						throw new NotImplementedException(); // reuse logic for loopEnd
					default:
						throw new NotImplementedException("Command behavior is not specified for " + command.Type);
				}
			}

			// verify no scope is left unclosed:
			if (scopeCollector.ActiveCommand != null)
			{
				throw new TemplateProcessingException("Started command was not properly closed with and ending tag. Command: " + scopeCollector.ActiveCommand);
			}

			// bring scope items under a single tree root:
			var root = new SyntaxTreeNode();
			root.PrimaryScope = scopeCollector.ActiveScope;
			return root;
		}


		private static void ValidateCommandWithParentScope(Command parentCommand, Command currentCommand)
		{
			switch (currentCommand.Type)
			{
				case CommandType.Copy:
				case CommandType.BindFromModel:
				case CommandType.If:
				case CommandType.IfExists:
				case CommandType.Loop:
				case CommandType.Subtemplate:
					throw new InvalidOperationException("Validation should only be called for scope continuation commands, current command: " + currentCommand.Type);
				case CommandType.IfElse:
					RequireMatchingActiveCommandType(parentCommand, currentCommand, CommandType.If);
					RequireMatchingModelPath(parentCommand, currentCommand);
					break;
				case CommandType.IfExistsElse:
					RequireMatchingActiveCommandType(parentCommand, currentCommand, CommandType.IfExists);
					RequireMatchingModelPath(parentCommand, currentCommand);
					break;
				case CommandType.IfEnd:
					RequireMatchingActiveCommandType(parentCommand, currentCommand, CommandType.If, CommandType.IfElse);
					RequireMatchingModelPath(parentCommand, currentCommand);
					break;
				case CommandType.IfExistsEnd:
					RequireMatchingActiveCommandType(parentCommand, currentCommand, CommandType.IfExists, CommandType.IfExistsElse);
					RequireMatchingModelPath(parentCommand, currentCommand);
					break;
				case CommandType.LoopEnd:
					RequireMatchingActiveCommandType(parentCommand, currentCommand, CommandType.Loop);
					RequireMatchingModelPath(parentCommand, currentCommand);
					break;
				default:
					throw new NotImplementedException("Validation rules not specified for " + currentCommand.Type);
			}
		}

		private static void RequireMatchingModelPath(Command activeCommand, Command currentCommand)
		{
			Debug.Assert(activeCommand != null);
			Debug.Assert(currentCommand != null);
			var activeModelPathCommand = activeCommand as ModelPathCommand;
			if (activeModelPathCommand == null) { throw new TemplateProcessingException($"ModelPathCommand expected but '{activeCommand}' found."); }
			var currentModelPathCommand = currentCommand as ModelPathCommand;
			if (currentModelPathCommand == null) { throw new TemplateProcessingException($"ModelPathCommand expected but '{currentCommand}' found."); }

			if (activeModelPathCommand.ModelPath != currentModelPathCommand.ModelPath)
			{
				throw new TemplateProcessingException($"Scope boundary commands do not match: '{activeCommand}' vs '{currentCommand}'.");
			}
		}

		private static void RequireMatchingActiveCommandType(Command activeCommand, Command currentCommand, CommandType requireType, CommandType? alternativeType = null)
		{
			if (activeCommand == null)
			{
				throw new TemplateProcessingException($"Invalid template. No starting command found for '{currentCommand}'.");
			}

			Boolean isMatch = activeCommand.Type == requireType || activeCommand.Type == alternativeType;
			if (!isMatch)
			{
				throw new TemplateProcessingException($"Invalid template. Unexpected command '{currentCommand}' does not match active command of '{activeCommand}'.");
			}
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
				if (FunctionCallParser.TryParseCommand(sentence.Text, out functionName, out arguments))
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
							NormalizeSelfReferencePath(modelPathCommand, syntax);
							yield return modelPathCommand;
							continue;
						case CommandType.Subtemplate:
							var command = new SubtemplateCommand(type, sentence);
							command.SubtemplateName = RequireArgument(type, arguments, 0);
							command.ModelPath = RequireArgument(type, arguments, 1);
							NormalizeSelfReferencePath(command, syntax);
							yield return command;
							continue;
						case CommandType.Unspecified:
							throw new TemplateProcessingException("Unrecognized control command found in sentence " + sentence.ToString());
						default:
							throw new NotImplementedException("Unhandled command found with type: " + type);
					}
				}
				else // if it is not in function format, assume it is for querying the model:
				{
					var modelBindCommand = new ModelPathCommand(CommandType.BindFromModel, sentence);
					modelBindCommand.ModelPath = sentence.Text.Trim();
					yield return modelBindCommand;
					continue;
				}
			}
		}

		/// <summary>
		/// Replaces localized self reference path marker "this" to it's internal marker.
		/// This ensures the special meaning persistence also during execution where the customized syntax settings are no longer available.
		/// </summary>
		/// <param name="modelPathCommand"></param>
		/// <param name="syntax"></param>
		private static void NormalizeSelfReferencePath(ModelPathCommand modelPathCommand, SyntaxSettings syntax)
		{
			if (modelPathCommand.ModelPath == syntax.SelfReferenceKeyword)
			{
				modelPathCommand.ModelPath = SyntaxSettings.DefaultSelfReferenceKeyword;
			}
		}

		private static String RequireArgument(CommandType type, string[] arguments, int argumentIndex)
		{
			if (arguments == null || arguments.Length < argumentIndex) { throw new Exception("Missing expected argument for command function."); }
			return arguments[argumentIndex];
		}

		/// <summary>
		/// Maps localized command name strings to their internal enum value.
		/// </summary>
		/// <param name="functionName"></param>
		/// <param name="syntax"></param>
		/// <returns></returns>
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

			if (functionName == syntax.SubtemplateCommand) { return CommandType.Subtemplate; }
			return CommandType.Unspecified;
		}
	}
}
