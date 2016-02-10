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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nortal.Utilities.TextTemplating
{

	/// <summary>
	/// Class to provide tools for filling text templates with data from model objects.
	/// </summary>
	//[Obsolete("To be removed in future version. Use TextTemplate class API instead.")] // to uncomment once new API is finished.
	public partial class TemplateProcessingEngine
	{
		public TemplateProcessingEngine()
		{
			//default configuration:
			this.ValueExtractor = new ReflectionBasedValueExtractor();
			this.ValueFormatter = new DefaultTemplateValueFormatter();
			this.Syntax = new SyntaxSettings();
		}

		public TemplateProcessingEngine(SyntaxSettings syntax)
			: this()
		{
			if (syntax == null) { throw new ArgumentNullException(nameof(syntax)); }
			this.Syntax = syntax;
		}

		#region configuration
		// configuration of extension points:
		protected IModelValueExtractor ValueExtractor { get; set; }
		protected ITemplateValueFormatter ValueFormatter { get; set; }
		protected SyntaxSettings Syntax { get; set; }

		public CultureInfo FormattingCulture
		{
			get { return this.ValueFormatter.Culture; }
			set { this.ValueFormatter.Culture = value; }
		}
		#endregion

		public TextTemplate ParseTemplate(String template)
		{
			if (template == null) { throw new ArgumentNullException("template"); }
			return TemplateParser.Parse(template, this.Syntax);
		}

		public String Process(String template, Object model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }

			var parsedTemplate = ParseTemplate(template);
			var document = Process(parsedTemplate, model);
			return document;
		}

		public String Process(TextTemplate template, Object model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }
			if (template == null) { throw new ArgumentNullException("template"); }

			var configuration = new ExecutionConfiguration()
			{
				ValueExtractor = this.ValueExtractor,
				ValueFormatter = this.ValueFormatter,
			};

			String document = TemplateExecutionEngine.CreateDocument(template, model, configuration);
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
