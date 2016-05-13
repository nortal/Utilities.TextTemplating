using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating.Parsing;

namespace Nortal.Utilities.TextTemplating.Tests
{
	[TestClass]
	public class LexerTests
	{
		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		[TestCategory(Categories.Parsing)]
		public void Lexer_TestUnclosedCommandThrows()
		{
			const String template = @"
[[for(Condition2)]] 
	>>> [[ CommandNotEnded here  <<< ----------
[[endfor(Condition2)]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (TemplateSyntaxException exception)
			{
				// Unexpected command start tag was found on line 3. Close previous command before starting a new one.
				Assert.IsTrue(exception.Message.Contains("line 3"));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(TemplateSyntaxException))]
		[TestCategory(Categories.Parsing)]
		public void Lexer_TestLoneCommandEndThrows()
		{
			const String template = @"
[[for(Condition2)]] 
	>>> CommandNotStarted ]] <<< ----------
[[endfor(Condition2)]]
";
			try
			{
				var parsed = TextTemplate.Parse(template);
			}
			catch (TemplateSyntaxException exception)
			{
				// 
				Assert.IsTrue(exception.Message.Contains("line 3"));
				throw;
			}
		}
	}
}
