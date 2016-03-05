namespace Nortal.Utilities.TextTemplating.Parsing
{
	/// <summary>
	/// Named identifier of type of command
	/// </summary>
	public enum CommandType : byte
	{
		Unspecified = 0,
		// Non-functions:
		Copy,
		BindFromModel,
		
		// Functions:
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
