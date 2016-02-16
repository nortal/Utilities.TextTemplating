namespace Nortal.Utilities.TextTemplating.Parsing
{
	public enum CommandType : byte
	{
		Unspecified = 0,
		Copy,
		BindFromModel,
		If,
		IfElse,
		IfEnd,
		IfExists,
		IfExistsElse,
		IfExistsEnd,
		Loop,
		LoopEnd,
		Subtemplate,

		//consider universal else/end commands which are not specific to if/ifexists/loop or argument but context-sensitive. explicit mode should be deprecated.
		Else,
		End,
	}
}
