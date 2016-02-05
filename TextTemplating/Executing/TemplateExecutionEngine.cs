using Nortal.Utilities.TextTemplating.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nortal.Utilities.TextTemplating.Executing
{

	public static class TemplateExecutionEngine
	{

		//TODO: consider how to pass configuration
		public static String CreateDocument(ParsedTemplate template, Object model, IModelValueExtractor valueExtractor, ITemplateValueFormatter valueFormatter)
		{
			// set up initial state:
			var state = template.CommandTree;
			List<SyntaxTreeNode> startingNodes = SetStartingNodes(state);

			StringBuilder document = new StringBuilder(); //TODO: test stream later.
			ProcessNodes(startingNodes, document, model, String.Empty, null, valueExtractor, valueFormatter);
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

		private static String ProcessNodes(IList<SyntaxTreeNode> statesToExecute, StringBuilder document, Object model, String pathToModel, LoopItemTrackingState currentLoopState, IModelValueExtractor valueExtractor, ITemplateValueFormatter valueFormatter)
		{
			var stateStack = new Stack<SyntaxTreeNode>(statesToExecute.Count);
			PushAll(stateStack, statesToExecute);

			// tree walker cycle:
			while (stateStack.Count != 0)
			{
				//set up comfort tools:
				SyntaxTreeNode state = stateStack.Pop();
				var command = state.Command;
				var modelPathCommand = command as ModelPathCommand;
				Object modelPathValue;

				//act:
				Debug.Assert(command != null);
				switch (state.Command.Type)
				{
					case CommandType.Copy:
						document.Append(state.Command.Source.OriginalText);
						break;
					case CommandType.BindFromModel:
						String format;
						var valuePath = RemoveFormatString(modelPathCommand, out format);
						modelPathValue = ExtractValueUsingLoopStates(model, valuePath, currentLoopState, valueExtractor);
						String formattedValue = FormatValue(valueFormatter, valuePath, modelPathValue, format);
						document.Append(formattedValue);
						break;
					case CommandType.If:
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, valueExtractor);
						Boolean? booleanValue = CastToBoolean(modelPathValue, modelPathCommand.ModelPath);

						//note: intentionally avoiding recursion, keep stacktrace cleaner.
						//note: intentionally accepting null as false:
						if (booleanValue == true) { PushAll(stateStack, state.PrimaryScope); }
						else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						break;
					case CommandType.IfExists:
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, valueExtractor);
						if (modelPathValue != null) { PushAll(stateStack, state.PrimaryScope); }
						else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						break;
					case CommandType.Loop:
						modelPathValue = ExtractValueUsingLoopStates(model, modelPathCommand.ModelPath, currentLoopState, valueExtractor);
						IEnumerable loopItems = CastValue<IEnumerable>(modelPathValue, modelPathCommand.ModelPath);
						if (loopItems == null) { break; } // not requiring models to create empty collections.

						var subloopState = new LoopItemTrackingState(modelPathCommand.ModelPath, loopItems, currentLoopState);
						foreach (var item in loopItems)
						{
							subloopState.CurrentItem = item; //TODO: is tracking IEnumerator better approach ?

							// using recursion for simplicity. More readble, and avoids stack of stacks and releases objects faster.
							// note that recursion depth is generally really shallow, definitely bounded.
							ProcessNodes(state.PrimaryScope, document, item, modelPathCommand.ModelPath, subloopState, valueExtractor, valueFormatter);
						}
						break;
					case CommandType.SubTemplate:
						// will use recursion.
						throw new NotImplementedException();
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
