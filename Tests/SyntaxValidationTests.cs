using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating.Parsing;
using System;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class SyntaxValidationTests
	{
		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		public void TestSyntaxValidation_ElseWithoutIfThrows()
		{
			const String template = @"
[[else(Condition)]]
[[endif(Condition))]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (Exception exception)
			{
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		public void TestSyntaxValidation_IfWithMismatchingEndThrows()
		{
			const String template = @"
[[if(Condition1)]]
[[endif(Condition2))]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (Exception exception)
			{
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		public void TestSyntaxValidation_LoopWithoutEndThrows()
		{
			const String template = @"
[[for(Condition1)]]
some content, no end.
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (Exception exception)
			{
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		public void TestSyntaxValidation_LoopWithMismatchingEndThrows()
		{
			const String template = @"
[[for(Condition1)]]
some content, no end.
[[endfor(Condition2)]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (Exception exception)
			{
				throw;
			}
		}
	}
}
