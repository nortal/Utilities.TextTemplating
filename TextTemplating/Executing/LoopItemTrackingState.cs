using System;
using System.Collections;

namespace Nortal.Utilities.TextTemplating.Executing
{
	internal class LoopItemTrackingState
	{
		public LoopItemTrackingState(String path, IEnumerable allItems, LoopItemTrackingState parent = null)
		{
			this.Path = path;
			this.AllItems = allItems;
			this.ParentLoop = parent;
		}

		public String Path { get; private set; }
		public IEnumerable AllItems { get; private set; }
		public Object CurrentItem { get; set; }

		// navigation property to parent loop if exists.
		public LoopItemTrackingState ParentLoop { get; private set; }
	}
}
