/*
	Copyright 2013 Imre Pühvel, AS Nortal
	
	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'TemplateProcessingEngine.cs'.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nortal.Utilities.TextTemplating
{

	/// <summary>
	/// Class to provide tools for filling text templates with data from model objects.
	/// </summary>
	public partial class TemplateProcessingEngine
	{
		public TemplateProcessingEngine()
		{
			//default implementations.
			this.ValueExtractor = new ReflectionBasedValueExtractor();
			this.ValueFormatter = new DefaultTemplateValueFormatter();
			this.Patterns = new RegexProvider(new SyntaxSettings());
		}

		public TemplateProcessingEngine(SyntaxSettings settings)
			: this()
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			this.Patterns = new RegexProvider(settings);
		}

		/// <summary>
		/// 
		/// </summary>
		protected IModelValueExtractor ValueExtractor { get; set; }
		protected ITemplateValueFormatter ValueFormatter { get; set; }
		private RegexProvider Patterns { get; set; }

		public CultureInfo FormattingCulture
		{
			get { return this.ValueFormatter.Culture; }
			set { this.ValueFormatter.Culture = value; }
		}

		public String Process(String template, Object model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }
			if (template == null) { throw new ArgumentNullException("template"); }

			return Process(template, model, pathToModel: "");
		}

		private String Process(String template, Object model, String pathToModel)
		{
			//loops must trigger first to apply collection-based rules with proper context before parent level can replace its tokens
			template = ProcessLoopCommands(template, model, pathToModel); //items in loop content cannot be replaced at any model level, so they have to be expanded and processed first. Consider alternatives as early trimming would increase performance.
			template = ProcessConditonCommands(template, model, pathToModel);
			template = ProcessFieldReplacements(template, model, pathToModel);
			template = ProcessSubtemplateCommands(template, model, pathToModel);
			return template;
		}

		#region FieldReplacement logic
		private string ProcessFieldReplacements(String template, Object model, String pathToModel)
		{
			//No fields overlapping is not allowed - single run is enough.
			return this.Patterns.RegexForFields.Replace(template, match => ReplaceField(match, model, pathToModel));
		}

		private String ReplaceField(Match match, Object model, String pathToModel)
		{
			if (!match.Success) { throw new InvalidOperationException("Unexpected match state."); }

			// find condition from match
			var fieldNameBlock = match.Groups[RegexProvider.GroupNames.FieldName];
			var formatBlock = match.Groups[RegexProvider.GroupNames.FieldFormat];

			if (!fieldNameBlock.Success) { throw new InvalidOperationException("FieldName not found"); }

			String valuePath = CalculateSubmodelPath(fieldNameBlock.Value, pathToModel);
			if (valuePath == null) { return match.Value; }

			Object value = this.ValueExtractor.ExtractValue(model, valuePath);
			String format = formatBlock.Success ? formatBlock.Value : null;
			String formattedValue = FormatValue(valuePath, value, format);
			return formattedValue;
		}

		private String CalculateSubmodelPath(String fullPath, String parentPath)
		{
			if (parentPath.Length == 0) { return fullPath; }
			// if field does not belong to this submodel, keep it as is.
			if (!fullPath.StartsWith(parentPath, StringComparison.Ordinal)) { return null; }
			//otherwise make path model-relative:
			return fullPath.Substring(parentPath.Length).TrimStart('.');
		}

		protected virtual string FormatValue(String valuePath, Object value, String format)
		{
			//extension point to override any value.
			String formattedValue = this.ValueFormatter.FormatValue(value, format);
			return formattedValue;
		}

		#endregion

		#region Loop replacement logic
		private string ProcessLoopCommands(String template, Object model, String pathToModel)
		{
			Boolean hasChanges = false;
			do
			{
				hasChanges = false;
				template = this.Patterns.RegexForLoop.Replace(template, match =>
				{
					hasChanges = true;
					return ReplaceLoop(match, model, pathToModel);
				});
			} while (hasChanges);
			return template;
		}

		private string ReplaceLoop(Match match, object model, String pathToModel)
		{
			if (!match.Success) { throw new InvalidOperationException("Unexpected match state."); }

			// find condition from match
			var fieldNameBlock = match.Groups[RegexProvider.GroupNames.LoopItems];
			var contentBlock = match.Groups[RegexProvider.GroupNames.LoopContent];

			if (!fieldNameBlock.Success) { throw new InvalidOperationException("FieldName not found"); }
			if (!contentBlock.Success) { throw new InvalidOperationException("ContentBlock not found for loop " + fieldNameBlock.Value); }

			String valuePath = CalculateSubmodelPath(fieldNameBlock.Value, pathToModel);
			if (valuePath == null) { return match.Value; }

			// find values from model
			Object itemsValue = this.ValueExtractor.ExtractValue(model, valuePath);
			if (itemsValue == null) { return null; } //missing collection is equivalent to empty collection to avoid the need for "if collection exists" wrappers
			var collection = itemsValue as IEnumerable;

			int count = 0;
			var collectedSubTemplates = new StringBuilder(100);
			foreach (var item in collection)
			{
				count++;
				String itemPath = String.IsNullOrEmpty(pathToModel)
					? valuePath
					: pathToModel + "." + valuePath;
				String processedSubTemplate = Process(contentBlock.Value, item, itemPath);
				collectedSubTemplates.Append(processedSubTemplate);
			}
			return collectedSubTemplates.ToString();
		}

		#endregion

		#region conditional replacement logic

		private string ProcessConditonCommands(String template, Object model, String pathToModel)
		{
			//to avoid tracking overlapping replacements, trigger replacements until no matches are found.
			Boolean hasChanges = false;
			do
			{
				hasChanges = false;
				template = this.Patterns.RegexForConditional.Replace(template, match =>
				{
					hasChanges = true;
					return ReplaceConditional(match, model, pathToModel);
				});
			} while (hasChanges);
			return template;
		}

		private String ReplaceConditional(Match match, Object model, String pathToModel)
		{
			if (!match.Success) { throw new InvalidOperationException("Unexpected match state."); }

			// find condition from match
			var condition = match.Groups[RegexProvider.GroupNames.Condition];
			var yesBlock = match.Groups[RegexProvider.GroupNames.ConditionYesBlock];
			var noblock = match.Groups[RegexProvider.GroupNames.ConditionNoBlock];

			if (!condition.Success) { throw new InvalidOperationException("Invalid conditional value: " + condition.Value); }

			String valuePath = CalculateSubmodelPath(condition.Value, pathToModel);
			if (valuePath == null) { return match.Value; }

			Object value = this.ValueExtractor.ExtractValue(model, valuePath);
			var booleanValue = value as Nullable<Boolean>;
			if (booleanValue == null && value != null)
			{
				throw new Exception(String.Format(@"Template conditional '{0}' is not a valid boolean value. Invalid value is '{1}' of type {2}.", condition.Value, value, value.GetType()));
			}

			var returnBlock = booleanValue == true //ReflectionValueExtractor.IsValueTrue(value, condition.Value)
				? yesBlock
				: noblock;

			if (returnBlock.Success) { return returnBlock.Value; }
			return null;
		}
		#endregion

		private string ProcessSubtemplateCommands(String template, Object model, String pathToModel)
		{
			return this.Patterns.RegexForSubtemplate.Replace(template, match => ReplaceSubtemplates(match, model, pathToModel));
		}

		private string ReplaceSubtemplates(Match match, object model, string pathToModel)
		{
			if (!match.Success) { throw new InvalidOperationException("Unexpected match state."); }

			// find condition from match
			var templateName = match.Groups[RegexProvider.GroupNames.SubtemplateName];
			var modelBlock = match.Groups[RegexProvider.GroupNames.SubtemplateModel];

			if (!templateName.Success) { throw new InvalidOperationException("Template command must be given subtemplate name as first argument: " + match.Value); }
			if (!modelBlock.Success) { throw new InvalidOperationException("Template command must be given a model as second argument. " + match.Value); }

			String valuePath = CalculateSubmodelPath(modelBlock.Value, pathToModel);
			if (valuePath == null) { return match.Value; }

			var subtemplateModel = this.ValueExtractor.ExtractValue(model, valuePath);

			String subtemplate = ResolveSubtemplateByName(templateName.Value);
			try
			{
				var processedSubTemplate = this.Process(subtemplate, subtemplateModel, ""); //"" = reset pathToModel
				return processedSubTemplate;
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException("Failed processing subtemplate " + subtemplate + ", for model " + subtemplateModel, exception);
			}
		}

		protected virtual string ResolveSubtemplateByName(string templateName)
		{
			throw new NotImplementedException("Override method to add project-specific template resolving logic.");
		}

		public virtual ICollection<String> DiscoverValuePathsFromModel(Object model, int maximumDepth)
		{
			return this.ValueExtractor.DiscoverValidValuePaths(model, maximumDepth).ToList()
				.ConvertAll(item => this.Patterns.Settings.BeginTag + item + this.Patterns.Settings.EndTag);
		}
	}
}
