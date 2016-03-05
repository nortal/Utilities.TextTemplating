namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Contains extension points to affect document building process.
	/// </summary>
	public sealed class ExecutionConfiguration
	{
		/// <summary>
		/// Class to control logic how model paths are translated to values. Default implementation uses reflection.
		/// </summary>
		public IModelValueExtractor ValueExtractor { get; set; } = new ReflectionBasedValueExtractor();

		/// <summary>
		/// Class to control logic how extracted values will be formatted to generated document. For example: handling dates, etc.
		/// </summary>
		public ITemplateValueFormatter ValueFormatter { get; set; } = new DefaultTemplateValueFormatter();
	}
}
