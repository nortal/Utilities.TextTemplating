using System;
using System.Text.RegularExpressions;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Splits a control command sentence that is in the format "name (arg[, arg]*)" to it's components
	/// </summary>
	internal static class FunctionCallParser
	{

		private const String Pattern = @"^\s*
(?<functionName>[^\(]+)
\s*\(\s* 
(?<args>[^,]*)
(\s*,\s* (?<args>[^,]*) )* 
\s*\)\s*$
";
		private const String GroupNameForFunctionName = @"functionName";
		private const String GroupNameForFunctionArguments = @"args";

		private static Regex PatternMatcher = new Regex(Pattern,
			RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline
		);


		internal static Boolean TryParseCommand(String candidate, out String functionName, out String[] arguments)
		{
			functionName = null; arguments = null;
			if (candidate == null || candidate.IndexOf('(') == -1) { return false; } // shortcut. Todo: test if this is beneficial.
			var match = PatternMatcher.Match(candidate);
			if (!match.Success) { return false; }

			functionName = match.Groups[GroupNameForFunctionName].Value;
			var argumentCaptures = match.Groups[GroupNameForFunctionArguments].Captures;
			arguments = new String[argumentCaptures.Count];
			for (int i = 0; i < argumentCaptures.Count; i++)
			{
				arguments[i] = argumentCaptures[i].Value;
			}
			return true;
		}
	}
}
