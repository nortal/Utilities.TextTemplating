using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class PerformanceTests
	{
		private class AssemblyWalkerExtractor : ReflectionBasedValueExtractor
		{
			public override Object ExtractValue(Object model, String valuePath)
			{
				// it would be better if model were precalculated, but even this gives some baseline test for performance.
				if (valuePath == "Types") { return ((Assembly)model).GetTypes(); }
				if (valuePath == "Methods") { return ((Type)model).GetMethods(); }
				if (valuePath == "Properties") { return ((Type)model).GetProperties(); }
				if (valuePath == "Fields") { return ((Type)model).GetFields(); }
				Object value = base.ExtractValue(model, valuePath);
				return value;
			}
		}

		/// <summary>
		/// Generate a 2.5MB text template using reflection metadata as a model.
		/// </summary>
		[TestMethod]
		public void TestPerformance_ReflectionModel()
		{
			String template = File.ReadAllText(@"PerformanceTestTemplate.html");
			var assembly = Assembly.GetAssembly(typeof(String));

			var stopwatch = Stopwatch.StartNew();

			var configuration = new ExecutionConfiguration() { ValueExtractor = new AssemblyWalkerExtractor() };
			var parsed = TextTemplate.Parse(template);
			var document = parsed.BuildDocument(assembly, configuration);

			stopwatch.Stop();

			// Uncomment for output verifying:
			//File.WriteAllText(@"PerformanceTest_output.html", document, Encoding.UTF8);

			// Takes 1.3 seconds on dev computer. Varies but can still trigger if something goes horribly wrong regarding performance.
			Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
		}
	}
}
