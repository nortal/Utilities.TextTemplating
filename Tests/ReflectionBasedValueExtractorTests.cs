using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nortal.Utilities.TextTemplating;

namespace Nortal.Utilities.TemplatingEngine.Tests
{
	[TestClass]
	public class ReflectionBasedValueExtractorTests
	{
		private class Model
		{
			public class SimpleChildClass
			{
				public String Name = "";
			}

			private Boolean PrivateField = true;
			public String PublicField = "";
			public String PublicProperty { get; set; }

			public List<SimpleChildClass> ChildList = null;
		}

		[TestMethod]
		public void TestListingValidFieldsDepth1()
		{
			var extractor = new ReflectionBasedValueExtractor();
			var model = new Model();

			List<String> actualItems = extractor.DiscoverValidValuePaths(model, 1).ToList();

			var expectedItems = new List<String>()
			{
				"PublicField",
				"PublicProperty",
				"ChildList",
				"ChildList.Count",
			};

			CollectionAssert.AreEquivalent(expectedItems, actualItems);
		}

		[TestMethod]
		public void TestListingValidFieldsDepth2()
		{
			var extractor = new ReflectionBasedValueExtractor();
			var model = new Model();

			List<String> actualItems = extractor.DiscoverValidValuePaths(model, 2).ToList();

			var expectedItems = new List<String>()
			{
				"PublicField",
				"PublicProperty",
				"ChildList",
				"ChildList.Count",
				"ChildList.Name",
			};

			CollectionAssert.AreEquivalent(expectedItems, actualItems);
		}

		[TestMethod]
		public void TestListingValidFieldsWithArray()
		{
			var extractor = new ReflectionBasedValueExtractor();
			var model = new
			{
				Name = "",
				ArrayItems = new Model[] { }
			};

			List<String> actualItems = extractor.DiscoverValidValuePaths(model, 2).ToList();

			var expectedItems = new List<String>()
			{
				"Name",
				"ArrayItems",
				"ArrayItems.Length",
				"ArrayItems.PublicField",
				"ArrayItems.PublicProperty",
				"ArrayItems.ChildList",
				"ArrayItems.ChildList.Count",
			};

			CollectionAssert.AreEquivalent(expectedItems, actualItems);
		}
	}
}
