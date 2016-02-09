using Nortal.Utilities.TextTemplating.Parsing;
using System;
using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating.Executing
{
	public class ExecutionConfiguration
	{
		public IDictionary<String, TextTemplate> Subtemplates { get; set; }
		public IModelValueExtractor ValueExtractor { get; set; }
		public ITemplateValueFormatter ValueFormatter { get; set; }
	}
}
