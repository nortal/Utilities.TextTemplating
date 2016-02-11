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

		/// <summary>
		/// Combines a template with model and generates an output text document.
		/// </summary>
		/// <param name="template"></param>
		/// <param name="model"></param>
		/// <param name="configuration"></param>
		/// <returns>Text document based on given template</returns>
		public static String CreateDocument(TextTemplate template, Object model, ExecutionConfiguration configuration = null)
		{
			if (template == null) { throw new ArgumentNullException(nameof(template)); }
			// model null CAN be acceptable in some scenarios.
			if (configuration == null) { configuration = new ExecutionConfiguration(); }

			// set up initial state:
			List<SyntaxTreeNode> startingNodes = SetStartingNodes(template.ParseTree);

			StringBuilder document = new StringBuilder(); //TODO: test stream later, optimize for <1MB documents.
			ModelChain modelChain = new ModelChain(path: String.Empty) { Submodel = model }; // root object is the head of tracked model chain items.

			ProcessNodes(startingNodes, document, template.Subtemplates, modelChain, configuration);
			return document.ToString();
		}

		/// <summary>
		/// The root node may but can exceptionally NOT contain a command. For recursion it is preferable to work with scopes instead of single nodes.
		/// This method prepares the starting scope based on given root node.
		/// </summary>
		/// <param name="node">root state</param>
		/// <returns>List of commands to execute</returns>
		private static List<SyntaxTreeNode> SetStartingNodes(SyntaxTreeNode node)
		{
			List<SyntaxTreeNode> startingNodes;
			if (node.Command == null)
			{
				startingNodes = node.PrimaryScope;
				Debug.Assert(node.SecondaryScope == null);
			}
			else
			{
				startingNodes = new List<SyntaxTreeNode>();
				startingNodes.Add(node);
			}

			return startingNodes;
		}

		private static T RequireCommandOfType<T>(SyntaxTreeNode node)
			where T : Command
		{
			Debug.Assert(node != null);
			var properlyCastedCommand = node.Command as T;
			if (properlyCastedCommand == null) { throw new TemplateProcessingException($"Invalid node, command required a Command of type {typeof(T).Name}, found: {node.Command}"); }
			return properlyCastedCommand;
		}

		/// <summary>
		/// This method describes the actual translation of template commands into document
		/// </summary>
		/// <param name="statesToExecute">queue of commands to execute, FIFO.</param>
		/// <param name="document">Collects the document generated so far</param>
		/// <param name="subtemplates">List of prepared and usable subtemplates.</param>
		/// <param name="modelChain">Models on which the current commands are executing. Contains a chosen model for each loop choice + the root model.</param>
		/// <param name="configuration"></param>
		private static void ProcessNodes(IList<SyntaxTreeNode> statesToExecute, StringBuilder document, IDictionary<String, TextTemplate> subtemplates, ModelChain modelChain, ExecutionConfiguration configuration)
		{
			var stateStack = new Stack<SyntaxTreeNode>(statesToExecute.Count);
			PushAll(stateStack, statesToExecute);

			// tree walker cycle:
			while (stateStack.Count != 0)
			{
				//set up comfort tools:
				SyntaxTreeNode state = stateStack.Pop();
				Object modelPathValue;

				//act:
				switch (state.Command.Type)
				{
					case CommandType.Copy:
						{
							var copyCommand = RequireCommandOfType<Command>(state);
							document.Append(copyCommand.Source.OriginalText);
						}
						break;
					case CommandType.BindFromModel:
						{
							var bindCommand = RequireCommandOfType<ModelPathCommand>(state);

							String format;
							var valuePath = RemoveFormatString(bindCommand, out format); //TODO: consider moving parsing formatString to Command object.
							modelPathValue = ExtractValueUsingModelChain(valuePath, modelChain, configuration.ValueExtractor);
							String formattedValue = FormatValue(configuration.ValueFormatter, valuePath, modelPathValue, format);
							document.Append(formattedValue);
						}
						break;
					case CommandType.If:
						{
							var ifCommand = RequireCommandOfType<ModelPathCommand>(state);
							modelPathValue = ExtractValueUsingModelChain(ifCommand, modelChain, configuration.ValueExtractor);
							Boolean? condition = CastToBoolean(modelPathValue, ifCommand.ModelPath);

							//note: intentionally avoiding recursion, keep stacktrace cleaner.
							//note: intentionally accepting null as false:
							if (condition == true) { PushAll(stateStack, state.PrimaryScope); }
							else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						}
						break;
					case CommandType.IfExists:
						{
							var ifExistsCommand = RequireCommandOfType<ModelPathCommand>(state);
							modelPathValue = ExtractValueUsingModelChain(ifExistsCommand, modelChain, configuration.ValueExtractor);
							if (modelPathValue != null) { PushAll(stateStack, state.PrimaryScope); }
							else if (state.SecondaryScope != null) { PushAll(stateStack, state.SecondaryScope); }
						}
						break;
					case CommandType.Loop:
						{
							var loopCommand = RequireCommandOfType<ModelPathCommand>(state);
							modelPathValue = ExtractValueUsingModelChain(loopCommand, modelChain, configuration.ValueExtractor);
							IEnumerable loopItems = CastValue<IEnumerable>(modelPathValue, loopCommand.ModelPath);
							if (loopItems == null) { break; } // we do not require models to create empty collections.

							var modelChainNextLink = new ModelChain(loopCommand.ModelPath, modelChain);
							foreach (var item in loopItems)
							{
								//updating loop modelChain link to next item.
								modelChainNextLink.Submodel = item;

								// using recursion for simplicity. More readble, avoids stack of stacks and releases objects faster.
								// note that recursion depth is generally really shallow, typically just a few levels, definitely bounded as templates are finite strings.
								ProcessNodes(state.PrimaryScope, document, subtemplates, modelChainNextLink, configuration);
							}
						}
						break;
					case CommandType.SubTemplate:
						{
							var subtemplateCommand = RequireCommandOfType<SubtemplateCommand>(state);
							String subTemplateName = subtemplateCommand.SubtemplateName;

							modelPathValue = ExtractValueUsingModelChain(subtemplateCommand, modelChain, configuration.ValueExtractor);

							TextTemplate foundTemplate;
							if (subtemplates == null || !subtemplates.TryGetValue(subTemplateName, out foundTemplate))
							{
								throw new TemplateProcessingException("Requested subtemplate was not found:" + subTemplateName);
							}
							// will use recursion, different templates will be processed in total isolation except the submodel passed in from caller:
							String subDocument = TemplateExecutionEngine.CreateDocument(foundTemplate, modelPathValue, configuration);
							document.Append(subDocument);
						}
						break;
					default:
						throw new NotSupportedException("No execution implemented for command of type " + state.Command.Type + ".");
				}
			}
		}


		private static object ExtractValueUsingModelChain(ModelPathCommand command, ModelChain modelChain, IModelValueExtractor valueExtractor)
		{
			Debug.Assert(command != null);
			return ExtractValueUsingModelChain(command.ModelPath, modelChain, valueExtractor);
		}

		private static object ExtractValueUsingModelChain(string requestedPath, ModelChain modelChain, IModelValueExtractor valueExtractor)
		{
			// find out which model to use for parent (determines which loop items are used in this iteration)
			var match = FindMatchingParentModel(modelChain, requestedPath);
			Debug.Assert(match != null); // if nothing else then at least the root must match.

			// shortcut on root model:
			if (match.Parent == null) {
				//special keyword "this" requires no walking at all from root
				// TODO == this.Patterns.Settings.SelfReferenceKeyword)
				if (requestedPath == "this") { return match.Submodel; }
				
				// no relative path calculation needed:
				return valueExtractor.ExtractValue(match.Submodel, requestedPath);
			}

			//shortcut if requested value matches exactly a model in chain - no walking required:
			if (requestedPath == match.submodelPath) { return match.Submodel; }

			//we have a submodel but need to walk to sub-elements using a relative path:
			Debug.Assert(requestedPath.StartsWith(match.submodelPath + "."));
			string relativePath = requestedPath.Substring(match.submodelPath.Length + 1); // +1 skips the dot as well.
			object requestedValue = valueExtractor.ExtractValue(match.Submodel, relativePath);
			return requestedValue;
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
				throw new TemplateProcessingException(String.Format("Invalid type at path '{0}'. Expected {1}, found {2}",
					path,
					typeof(Boolean).Name,
					modelValue.GetType().Name)
				);
			}
			return castedValue; // intentionally pass null.
		}

		private static ModelChain FindMatchingParentModel(ModelChain modelChain, string valuePath)
		{
			Debug.Assert(modelChain != null); // should have at least root state.
			while (modelChain != null)
			{
				if (modelChain.Parent == null) { return modelChain; }
				if (valuePath.StartsWith(modelChain.submodelPath))
				{
					if (valuePath.Length == modelChain.submodelPath.Length) { return modelChain; }
					if (valuePath[modelChain.submodelPath.Length] == '.') { return modelChain; } // don't be fooled with common prefix (ex: M.Items vs M.ItemsCount)
				}
				// no match, go check parent:
				modelChain = modelChain.Parent;
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
