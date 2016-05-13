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
		[TestCategory(Categories.Parsing)]
		public void TestSyntaxValidation_ElseWithoutIfThrows()
		{
			const String template = @" some content
[[if(A)]] A [[endif(A)]]
The following row is missing a starting command.
[[else(Condition)]]
[[endif(Condition))]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (TemplateSyntaxException exception)
			{
				//Invalid syntax at [Line:4, pos: 3] Command "else(Condition)". No starting command found for 'IfElse: else(Condition)'.
				Assert.IsTrue(exception.Message.Contains("else(Condition)"));
				Assert.AreEqual(4, exception.Sentence.SourceLine);
				Assert.AreEqual(3, exception.Sentence.SourcePosition);
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		[TestCategory(Categories.Parsing)]
		public void TestSyntaxValidation_IfWithMismatchingEndThrows()
		{
			const String template = @"Some content
[[if(Condition1)]]
Endif is present with explicit condition but condition does not match active command.
[[endif(Condition2)]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (TemplateSyntaxException exception)
			{
				// Invalid syntax at [Line:2, pos: 3] Command "endif(Condition2)". Scope boundary commands do not match: 'If: if(Condition1)' vs 'IfEnd: endif(Condition2)'.
				Assert.IsTrue(exception.Message.Contains("endif(Condition2)"));
				Assert.AreEqual(4, exception.Sentence.SourceLine); // context sentence is the starting command.
				Assert.AreEqual(3, exception.Sentence.SourcePosition);
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		[TestCategory(Categories.Parsing)]
		public void TestSyntaxValidation_UnclosedScopeThrows()
		{
			const String template = @"
[[if(Condition1)]]
	condition 1 
[[endif(Condition1)]]
[[for(Condition1)]]
[[endfor(Condition1)]]
1234567890[[for(Condition2)]]
	some content, 
	[[for(Condition3)]]
	[[endfor(Condition3)]]
condition 2 has no end.
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (TemplateSyntaxException exception)
			{
				// expected: Invalid syntax at [Line:7, pos: 13] Command "for(Condition2)". Expected scope ending command was not found.
				Assert.IsTrue(exception.Message.Contains("for(Condition2)"));
				Assert.AreEqual(7, exception.Sentence.SourceLine); // context sentence is the starting command.
				Assert.AreEqual(13, exception.Sentence.SourcePosition);
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		[TestCategory(Categories.Parsing)]
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
			catch (TemplateSyntaxException exception)
			{
				//Invalid syntax at [Line:2, pos: 3] Command "endfor(Condition2)". Scope boundary commands do not match: 'Loop: for(Condition1)' vs 'LoopEnd: endfor(Condition2)'.
				Assert.IsTrue(exception.Message.Contains("endfor(Condition2)"));
				Assert.IsTrue(exception.Message.Contains("for(Condition1)"));
				Assert.AreEqual(4, exception.Sentence.SourceLine); // context sentence is the starting command.
				Assert.AreEqual(3, exception.Sentence.SourcePosition);
				throw;
			}
		}
	}
}
