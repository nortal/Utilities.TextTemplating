using System;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Type used in tracking sub model state during template execution phase.
	/// </summary>
	internal sealed class SubmodelChainLink
	{
		public SubmodelChainLink(String path, SubmodelChainLink parentModel = null)
		{
			this.submodelPath = path;
			this.Parent = parentModel;
		}

		/// <summary>
		/// Path to current model from root model.
		/// </summary>
		public String submodelPath { get; private set; }

		/// <summary>
		/// Current model
		/// </summary>
		public Object Submodel { get; set; }

		// navigation property to parent model object if one exists.
		public SubmodelChainLink Parent { get; private set; }
	}
}
