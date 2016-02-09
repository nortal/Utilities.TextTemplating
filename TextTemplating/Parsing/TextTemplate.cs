using System;
using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Represents a parsed and verified template which can be used for generating text documents.
	/// Use static Parse() method for creating instances.
	/// </summary>
	public class TextTemplate
	{
		// Intentionally no public constructors as parsing a template is lot of work which can throw exceptions and object cannot be instantiated without parsed template.

		internal TextTemplate(string originalTemplate, SyntaxTreeNode commandTree)
		{
			this.ParseTree = commandTree;
			this.TemplateText = originalTemplate;
		}

		public static TextTemplate Parse(String documentTemplate, SyntaxSettings settings)
		{
			return TemplateParser.Parse(documentTemplate, settings);
		}

		public String TemplateText { get; private set; }

		public SyntaxTreeNode ParseTree { get; private set; }

		public IDictionary<String, TextTemplate> Subtemplates { get; private set; } = new Dictionary<String, TextTemplate>(2);

		public void AddSubtemplate(String name, TextTemplate subtemplate)
		{
			this.Subtemplates.Add(name, subtemplate);
		}

		public void AddSubtemplate(String name, String template, SyntaxSettings settings = null)
		{
			var parsedTemplate = TextTemplate.Parse(template, settings);
			this.Subtemplates.Add(name, parsedTemplate);
		}
	}
}
