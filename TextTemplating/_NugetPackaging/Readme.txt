===========================================================
    Nortal.Utilities.TextTemplating
===========================================================

New to syntax? Try this snippet to get yourself started:

	var engine = new TemplateProcessingEngine();
	var model = new { Name = "World" };
	String result = engine.Process( @"Hello, [[Name]]!", model); 
	//result == "Hello, World!"

Check documentation for examples how to use more advanced features like looping, conditional blocks, etc:
	https://github.com/nortal/Utilities.TextTemplating

Have fun!