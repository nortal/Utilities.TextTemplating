using System;

namespace Nortal.Utilities.TextTemplating.Parsing
{
	internal static class SourcePositionFormatting
	{
		internal static String Format(int line, int position)
		{
			return $"[Line:{line}, pos: {position}]";
		}
	}
}
