Nortal.Utilities.TextTemplating
====================================

This assembly provides a text template processing engine capable of merging data from objects (the model) to a text-based template (text, html, xml, ..).

Requirements:
 * Microsoft .Net Framework 3.5 Client Profile.

Main features
-------------
* access public fields and properties from model
* no special setup required for models - any object can be used
* model fields are accessed only when used by template (values are "pulled" from model).
* can use child objects members in template at any depth
* can conditionally show/hide a template block
* can iterate a template block for every collection member
* can use subtemplates
* templating syntax is customizable and localizable.

Syntax and API
-------------------
All commands are between special tags (by default '[[' and ']]') and will be processed by engine - for example replaced by values from model.

API usage & Hello world:

	var engine = new TemplateProcessingEngine();
	var model = new { Name = "World" };
	String result = engine.Process( @"Hello, [[Name]]!", model); 
	//result == "Hello, World!"

If you want to help users to discover valid values that can be used on a template:

	ICollection<String> items = engine.DiscoverValuePathsFromModel(model, maximumDepth:2);

To apply formatting to model values:

	[[MyDateTime:u]] applies a standard .net format string "u".
	[[MyDateTime:dd.MM.yyyy]] applies a custom .net format string

To access child objects or their members in a template:

	[[ChildObject.GrandChildObject.FieldName]]

To conditionally include or exclude a section in template based on a Boolean field in model:

	[[ if (IsPanicRequired) ]]
		The end is nigh! Thou shall burn!
	[[ else (IsPanicRequired) ]]
		Oh, relax and enjoy the ride..
	[[ endif (IsPanicRequired) ]]

To conditionally include or exclude a section based on existence of a value:

	[[ ifexists (Name) ]]
		Your name is [[ Name ]].
	[[ elseifexists (Name) ]]
		I don't know who you are.
	[[ endifexists (Name) ]]

To process a template for each item in collection member model.Children:

	[[ for (Children) ]]
		Shown for every child. this one is [[Children.Name]].
	[[ endfor (Children) ]]

Extension points
------------------
it is possible to
* change templating syntax, including
	- use different command markers [[ and ]]
	- use different keywords  instead of if, for, ..
* implement a different engine to find values from model
* control how values are formatted in output by default

Tips and Tricks
----------------
* Use &lt;b&gt; and &lt;/b&gt; in command markers to emphasize command statements in HTML template
* You can find working examples of most features in source code, check the unit-testing project.


