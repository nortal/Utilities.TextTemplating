using Nortal.Utilities.TextTemplating.Executing;
using Nortal.Utilities.TextTemplating.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Represents a parsed and verified template which can be used for generating text documents.
	/// Use static Parse() method for creating instances.
	/// </summary>
	[DebuggerDisplay(@" TextTemplate: {TemplateText}")]
	public class TextTemplate
	{
		// Intentionally no public constructors as parsing a template is lot of work which can throw exceptions and object cannot be instantiated without parsed template.

		internal TextTemplate(string templateAsText, SyntaxTreeNode commandTree)
		{
			this.ParseTree = commandTree;
			this.TemplateText = templateAsText;
		}

		/// <summary>
		/// Prepare template for document generation.
		/// </summary>
		/// <param name="documentTemplate"></param>
		/// <param name="settings"></param>
		/// <returns>Template with it's parsed form.</returns>
		public static TextTemplate Parse(String documentTemplate, SyntaxSettings settings = null)
		{
			if (settings == null) { settings = new SyntaxSettings(); }
			return TemplateParser.Parse(documentTemplate, settings);
		}

		/// <summary>
		/// Original template text used for document generation.
		/// </summary>
		public String TemplateText { get; private set; }

		/// <summary>
		/// Parsed template command tree.
		/// </summary>
		public SyntaxTreeNode ParseTree { get; private set; }

		#region Subtemplate management
		/// <summary>
		/// Named subtemplates available to this template.
		/// </summary>
		public IDictionary<String, TextTemplate> Subtemplates { get; private set; } = new Dictionary<String, TextTemplate>(2);

		/// <summary>
		/// Adds given template to the list of available subtemplates.
		/// </summary>
		/// <param name="name">Name with which this template can be referenced from its parent template.</param>
		/// <param name="subtemplate"></param>
		public void AddSubtemplate(String name, TextTemplate subtemplate)
		{
			this.Subtemplates.Add(name, subtemplate);
		}

		/// <summary>
		/// Adds given template to the list of available subtemplates.
		/// </summary>
		/// <param name="name">Name with which this template can be referenced from its parent template.</param>
		/// <param name="template">Template to register as subtemplate.</param>
		/// <param name="settings"></param>
		public void AddSubtemplate(String name, String template, SyntaxSettings settings = null)
		{
			var parsedTemplate = TextTemplate.Parse(template, settings);
			this.Subtemplates.Add(name, parsedTemplate);
		}
		#endregion

		public String BuildDocument(Object model, ExecutionConfiguration configuration = null)
		{
			return TemplateExecutionEngine.CreateDocument(this, model, configuration);
		}
	}
}
