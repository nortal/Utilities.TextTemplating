Nortal.Utilities.TextTemplating
====================================

This assembly provides a text template processing engine capable of merging data from objects (the model) to a text-based template (text, html, xml, ..).

Requirements:
* .Net Standard 2.0 requirements (https://dotnet.microsoft.com/platform/dotnet-standard#versions)

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

Getting started
-----------------
To install download built package from https://www.nuget.org/packages/Nortal.Utilities.TextTemplating or run the following command in the Package Manager Console:

	PM> Install-Package Nortal.Utilities.TextTemplating

All commands are between special tags (by default '[[' and ']]') and will be processed by engine - for example replaced by values from model.

"Hello world" example:

	var engine = new TemplateProcessingEngine();
	var model = new { Name = "World" };
	String result = engine.Process( @"Hello, [[Name]]!", model); 
	//result == "Hello, World!"

Feature syntax and API
-------------------
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


