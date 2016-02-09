using Nortal.Utilities.TextTemplating.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nortal.Utilities.TextTemplating.Executing
{
	/// <summary>
	/// Contains core logic about executing a parsed text template on a given model for generating a text document.
	/// </summary>
	public static class TemplateExecutionEngine
	{

		//TODO: consider how to pass configuration
		public static String CreateDocument(TextTemplate template, Object model, ExecutionConfiguration configuration)
		{
			// set up initial state:
			var state = template.ParseTree;
			List<SyntaxTreeNode> startingNodes = SetStartingNodes(state);

			StringBuilder document = new StringBuilder(); //TODO: test stream later, optimize for <1MB documents.
			ProcessNodes(startingNodes, document, model, String.Empty, null, configuration);
			return document.ToString();
		}

		private static List<SyntaxTreeNode> SetStartingNodes(SyntaxTreeNode state)
		{
			List<SyntaxTreeNode> startingNodes;
			if (state.Command == null)
			{
				// program usually starts with a sequence of commands without any root control command
				startingNodes = state.PrimaryScope;
			}
			else
			{
				startingNodes = new List<SyntaxTreeNode>();
				startingNodes.Add(state);
			}

			return startingNodes;
		}

		private static ModelPathCommand RequireModelPathCommand(SyntaxTreeNode node)
		{
			Debug.Assert(node.Command != null);
			var modelPathCommand = node.Command as ModelPathCommand;
			if (modelPathCommand == null) { throw new TextTemplating.TemplateProcessingException("Invalid node, command requires a model path argument, found: " + node.Command); }
			return modelPathCommand;
		}

		private static Command RequireCommand(SyntaxTreeNode node)
		{
			var command = node.Command;
			if (command == null) { throw new TextTemplating.TemplateProcessingException("Invalid node, missing a command"); }
			return command;
		}

		private static String ProcessNodes(IList<SyntaxTreeNode> statesToExecute, StringBuilder document, Object model, String pathToModel, LoopItemTrackingState currentLoopState, ExecutionConfiguration configuration)
		{
			var stateStack = new Stack<SyntaxTreeNode>(statesToExecute.Count);
			PushAll(stateStack, statesToExecute);

			// tree walker cycle:
			while (stateStack.Count != 0)
			{
				//set up comfort tools:
				SyntaxTreeNode state = stateStack.Pop();
				var command = RequireCommand(state);
				Object modelPathValue;
				ModelPathCommand modelPathCommand;

				//act:
				Debug.Assert(command != null);
				switch (state.Command.Type)
				{
					case CommandType.Copy:
						document.Append(state.Command.Source.OriginalText);
						break;
					case CommandType.BindFromModel:
						String format;
						modelPathCommand = RequireModelPathCommand(state);

						var valuePath = RemoveFormatString(modelPathCommand, out format);
						modelPathValue = ExtractValueUsingLoopStates(model, valuePath, currentLoopState, configuration.ValueExtractor);
						String formattedValue = FormatValue(configuration.ValueFormatter, valuePath, modelPathValue, format);
						document.Append(formattedValue);
						break;
					case CommandType.If:
						modelPathCommand = RequireModelPathCommand(state);
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, configuration.ValueExtractor);
						Boolean? booleanValue = CastToBoolean(modelPathValue, modelPathCommand.ModelPath);

						//note: intentionally avoiding recursion, keep stacktrace cleaner.
						//note: intentionally accepting null as false:
						if (booleanValue == true) { PushAll(stateStack, state.PrimaryScope); }
						else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						break;
					case CommandType.IfExists:
						modelPathCommand = RequireModelPathCommand(state);
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, configuration.ValueExtractor);
						if (modelPathValue != null) { PushAll(stateStack, state.PrimaryScope); }
						else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						break;
					case CommandType.Loop:
						modelPathCommand = RequireModelPathCommand(state);
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, configuration.ValueExtractor);
						IEnumerable loopItems = CastValue<IEnumerable>(modelPathValue, modelPathCommand.ModelPath);
						if (loopItems == null) { break; } // not requiring models to create empty collections.

						var subloopState = new LoopItemTrackingState(modelPathCommand.ModelPath, loopItems, currentLoopState);
						foreach (var item in loopItems)
						{
							subloopState.CurrentItem = item; //TODO: is tracking IEnumerator better approach ?

							// using recursion for simplicity. More readble, avoids stack of stacks and releases objects faster.
							// note that recursion depth is generally really shallow, typically just a few levels, definitely bounded as templates are finite strings.
							ProcessNodes(state.PrimaryScope, document, item, modelPathCommand.ModelPath, subloopState, configuration);
						}
						break;
					case CommandType.SubTemplate:
						modelPathCommand = RequireModelPathCommand(state); // todo: require command with subtemplate structure.
						String subTemplateName = "TODO";

						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, configuration.ValueExtractor);

						TextTemplate foundTemplate;
						if (configuration.Subtemplates == null || !configuration.Subtemplates.TryGetValue(subTemplateName, out foundTemplate))
						{
							throw new TemplateProcessingException("Requested subtemplate was not found:" + subTemplateName);
						}
						// will use recursion, different templates will be processed in total isolation except the submodel passed in from caller:
						String subDocument = TemplateExecutionEngine.CreateDocument(foundTemplate, modelPathValue, configuration);
						document.Append(subDocument);
						break;
					default:
						throw new NotSupportedException("No execution implemented for command of type " + state.Command.Type + ".");
				}
			}
			return document.ToString();
		}

		private static object ExtractValueUsingLoopStates(object model, string valuePath, LoopItemTrackingState loopState, IModelValueExtractor valueExtractor)
		{
			if (valuePath == "this") { return model; } // TODO == this.Patterns.Settings.SelfReferenceKeyword)

			var match = FindMatchingLoopState(loopState, valuePath);
			if (match == null) { return valueExtractor.ExtractValue(model, valuePath); } // extract value from root model.

			Debug.Assert(match != null);
			// extracting value starting from latest matching loop item:
			if (valuePath == match.Path) { return match.CurrentItem; }
			Debug.Assert(valuePath.StartsWith(match.Path + "."));
			string relativePath = valuePath.Substring(match.Path.Length + 1); // +1 skips the dot as well.

			object modelPathValue = valueExtractor.ExtractValue(match.CurrentItem, relativePath);
			return modelPathValue;
		}

		private static T CastValue<T>(Object modelValue, String path)
			where T : class
		{
			T castedValue = modelValue as T;
			if (castedValue == null && modelValue != null)
			{
				throw new Exception(String.Format("Invalid type at path '{0}'. Expected {1}, found {2}",
					path,
					typeof(T).Name,
					modelValue.GetType().Name)
				);
			}
			return castedValue; // intentionally pass null.
		}
		private static Boolean? CastToBoolean(Object modelValue, String path)
		{
			Boolean? castedValue = modelValue as Boolean?;
			if (castedValue == null && modelValue != null)
			{
				throw new Exception(String.Format("Invalid type at path '{0}'. Expected {1}, found {2}",
					path,
					typeof(Boolean).Name,
					modelValue.GetType().Name)
				);
			}
			return castedValue; // intentionally pass null.
		}

		private static LoopItemTrackingState FindMatchingLoopState(LoopItemTrackingState loopState, string valuePath)
		{
			while (loopState != null)
			{
				if (valuePath.StartsWith(loopState.Path))
				{
					if (valuePath.Length == loopState.Path.Length) { return loopState; }
					if (valuePath[loopState.Path.Length] == '.') { return loopState; } // don't be fooled with common prefix (ex: M.Items vs M.ItemsCount)
				}
				// no match, check parent:
				loopState = loopState.ParentLoop;
			}
			return null; // no match
		}

		private static void PushAll(Stack<SyntaxTreeNode> stateStack, IEnumerable<SyntaxTreeNode> scopeToAdd)
		{
			foreach (var item in scopeToAdd.Reverse<SyntaxTreeNode>()) // reversing as stack is FILO
			{
				stateStack.Push(item);
			}
		}

		private static String RemoveFormatString(ModelPathCommand modelPathCommand, out String formatString)
		{
			Debug.Assert(modelPathCommand != null);
			var fullModelPath = modelPathCommand.ModelPath;

			var splitIndex = fullModelPath.IndexOf(':');
			if (splitIndex == -1) { formatString = null; return fullModelPath; }
			String valuePath = fullModelPath.Substring(0, splitIndex);
			formatString = fullModelPath.Substring(splitIndex + 1);
			return valuePath;
		}

		private static String CalculateSubmodelPath(String fullPath, String parentPath)
		{
			if (parentPath.Length == 0) { return fullPath; }
			// if field does not belong to this submodel, keep it as is.
			if (!fullPath.StartsWith(parentPath, StringComparison.Ordinal)) { return null; }
			//otherwise make path model-relative:
			return fullPath.Substring(parentPath.Length).TrimStart('.');
		}

		private static string FormatValue(ITemplateValueFormatter formatter, String valuePath, Object value, String format)
		{
			//extension point to override any value.
			String formattedValue = formatter.FormatValue(value, format);
			return formattedValue;
		}
	}
}
