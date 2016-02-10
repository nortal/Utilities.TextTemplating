namespace Nortal.Utilities.TextTemplating.Executing
{
	/// <summary>
	/// Contains extension points to affect document building process.
	/// </summary>
	public class ExecutionConfiguration
	{
		public IModelValueExtractor ValueExtractor { get; set; } = new ReflectionBasedValueExtractor();
		public ITemplateValueFormatter ValueFormatter { get; set; } = new DefaultTemplateValueFormatter();
	}
}
