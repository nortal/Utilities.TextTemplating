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

using Nortal.Utilities.TextTemplating.Executing;
using Nortal.Utilities.TextTemplating.Parsing;
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
		}

		public TemplateProcessingEngine(SyntaxSettings settings)
			: this()
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
		}

		#region configuration
		// configuration of extension points:
		protected IModelValueExtractor ValueExtractor { get; set; }
		protected ITemplateValueFormatter ValueFormatter { get; set; }

		public CultureInfo FormattingCulture
		{
			get { return this.ValueFormatter.Culture; }
			set { this.ValueFormatter.Culture = value; }
		}
		#endregion

		public ParsedTemplate ParseTemplate(String template)
		{
			if (template == null) { throw new ArgumentNullException("template"); }
			return TemplateParser.Parse(template);
		}

		public String Process(String template, Object model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }

			var parsedTemplate = TemplateParser.Parse(template);
			var document = Process(parsedTemplate, model);
			return document;
		}

		public String Process(ParsedTemplate template, Object model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }
			if (template == null) { throw new ArgumentNullException("template"); }

			String document = TemplateExecutionEngine.CreateDocument(template, model, this.ValueExtractor, this.ValueFormatter);
			return document;
		}

		protected virtual string ResolveSubtemplateByName(string templateName)
		{
			//Todo: 
			throw new NotImplementedException("Override method to add project-specific template resolving logic.");
		}

		public virtual ICollection<String> DiscoverValuePathsFromModel(Object model, int maximumDepth)
		{
			throw new NotImplementedException("TODO");
			//return this.ValueExtractor.DiscoverValidValuePaths(model, maximumDepth).ToList()
			//	.ConvertAll(item => this.Patterns.Settings.BeginTag + item + this.Patterns.Settings.EndTag);
		}
	}
}
