using System;
using System.Collections;

namespace Nortal.Utilities.TextTemplating.Executing
{
	internal class ModelChain
	{
		public ModelChain(String path, ModelChain parentModel = null)
		{
			this.submodelPath = path;
			this.Parent = parentModel;
		}

		public String submodelPath { get; private set; }
		public Object Submodel { get; set; }

		// navigation property to parent loop if exists.
		public ModelChain Parent { get; private set; }
	}
}
