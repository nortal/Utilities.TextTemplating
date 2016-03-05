using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Scans the initial template text and separates commands from non-commands.
	/// </summary>
	internal static class SentenceScanner
	{
		/// <summary>
		/// Turns input into stream of sentences this template is built with.
		/// Each sentence is either a block of content texts or a control commands to templating engine.
		/// </summary>
		/// <param name="template"></param>
		/// <param name="startTag"></param>
		/// <param name="endTag"></param>
		/// <returns></returns>
		internal static IEnumerable<TemplateSentence> Scan(string template, string startTag, string endTag)
		{
			int currentIndex = 0;
			int commandStartIndex = int.MinValue; // not using nullables as indexof operates anyway on special value "-1" for missing index.
			int commandEndIndex = int.MinValue;
			const int NoResultIndex = -1; // as used in IndexOf

			Boolean isInCommandMode = false;

			//tools for line counting:
			int lineCount = 1;
			int lastLineFeedIndex = -Environment.NewLine.Length; // imagined newline before template to set first line first char position correctly.
			int nextLineFeedIndex = template.IndexOf(Environment.NewLine, 0, StringComparison.Ordinal);

			//start sentence scanning:
			while (currentIndex < template.Length)
			{
				// find next tag positions only if currentIndex has passed last known index:
				if (commandStartIndex < currentIndex && commandStartIndex != NoResultIndex)
				{
					commandStartIndex = template.IndexOf(startTag, currentIndex, StringComparison.Ordinal);
				}
				if (commandEndIndex < currentIndex && commandEndIndex != NoResultIndex)
				{
					commandEndIndex = template.IndexOf(endTag, currentIndex, StringComparison.Ordinal);
				}

				int sentenceLength;
				if (commandStartIndex == currentIndex)
				{
					if (isInCommandMode) { throw new Exception("Command mode already started. Previous commands must be closed first."); }
					isInCommandMode = true;
					sentenceLength = startTag.Length;
				}
				else if (commandEndIndex == currentIndex)
				{
					if (!isInCommandMode) { throw new Exception("Unexpected command end before a command start."); }
					isInCommandMode = false;
					sentenceLength = endTag.Length;
				}
				else
				{
					int nextTagIndex = Math.Min(commandStartIndex, commandEndIndex);
					if (nextTagIndex == NoResultIndex)// special case of last few sentences:
					{
						nextTagIndex = isInCommandMode && commandEndIndex != NoResultIndex
							? commandEndIndex // we still have command end marker, consume current command
							: template.Length; // last non-command consumes everything to the end.
					}

					Debug.Assert(nextTagIndex >= currentIndex);
					sentenceLength = nextTagIndex - currentIndex;
					var sentence = new TemplateSentence(template.Substring(currentIndex, sentenceLength), isInCommandMode);
					sentence.SourceLine = lineCount;
					sentence.SourcePosition = currentIndex - lastLineFeedIndex + 1 - Environment.NewLine.Length; // go 1-based + don't count newline chars themselves in position.

					yield return sentence;
				}

				// update linefeed count and index for next round:
				while (nextLineFeedIndex < currentIndex && nextLineFeedIndex != NoResultIndex)
				{
					lineCount++;
					lastLineFeedIndex = nextLineFeedIndex;
					nextLineFeedIndex = template.IndexOf(Environment.NewLine, lastLineFeedIndex + 1);
				}
				currentIndex += sentenceLength; // sentence consumed, moving cursor ahead.
				Debug.Assert(currentIndex <= template.Length);
			}
		}
	}
}
