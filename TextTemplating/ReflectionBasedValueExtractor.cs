/*
	Copyright 2013 Imre Pühvel, AS Nortal
	
	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'ReflectionBasedValueExtractor.cs'.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Nortal.Utilities.TextTemplating
{
	public class ReflectionBasedValueExtractor : IModelValueExtractor
	{
		private const MemberTypes MemberTypesToSearch = MemberTypes.Property | MemberTypes.Field;
		private const BindingFlags BindingFlagsForSearch = BindingFlags.Instance | BindingFlags.Public;

		public IEnumerable<String> DiscoverValidValuePaths(Object exampleModel, int maximumDepth)
		{
			if (exampleModel == null) { throw new ArgumentNullException("exampleModel"); }
			if (maximumDepth < 1) { throw new ArgumentException("Depth must be positive integer.", "depth"); }

			var returnItems = DiscoverValidValuePaths(exampleModel.GetType(), maximumDepth);
			foreach (var item in returnItems) { yield return item; }
		}

		private static IEnumerable<String> DiscoverValidValuePaths(Type type, int depth)
		{
			var validMembers = type.GetMembers(BindingFlagsForSearch)
				.Where(member => (member.MemberType & MemberTypesToSearch) > 0);
			foreach (var member in validMembers)
			{
				yield return member.Name;
				var childType = ExtractMemberType(member);
				Type recurseOnType = childType;

				// For system collections declare only Count 
				if (typeof(System.Collections.ICollection).IsAssignableFrom(childType)
					&& childType.Namespace.StartsWith("System.Collections"))
				{
					yield return member.Name + ".Count";
					recurseOnType = childType.IsGenericType
						? childType.GetGenericArguments().First()
						: typeof(Object);
				}
				else if (childType.IsArray)
				{
					yield return member.Name + ".Length";
					recurseOnType = childType.GetElementType();
				}

				if (recurseOnType.IsClass && depth > 1 && recurseOnType != typeof(String))
				{ //recurse:
					var childItems = DiscoverValidValuePaths(recurseOnType, depth - 1);
					foreach (var item in childItems)
					{
						yield return member.Name + "." + item;
					}
				}
			}
		}

		public object ExtractValue(object model, String valuePath)
		{
			try
			{
				if (model == null) { throw new ArgumentNullException("model"); }
				if (valuePath == null) { throw new ArgumentNullException("valuePath"); }
				//empty valuepath is a reference to itself.
				if (valuePath.Length == 0) { return model; }

				List<String> toParse = valuePath.Split('.').ToList();
				var modelPath = new List<String>(toParse.Count);
				return ExtractValue(model, modelPath, toParse);
			}
			catch (Exception exception)
			{
				String exceptionMessage = String.Format(CultureInfo.InvariantCulture, "Failed to extract value '{0}' from model {1}.", valuePath, model);
				throw new InvalidOperationException(exceptionMessage, exception);
			}
		}

		private static object ExtractValue(object model, List<String> modelPath, List<String> toParse)
		{
			if (toParse.Count == 0) { return model; }
			String innerModelName = toParse.First();

			Object innerModel = null;
			try
			{
				innerModel = ExtractDirectValue(model, innerModelName);
			}
			catch (Exception exception)
			{
				String exceptionMessage = String.Format(CultureInfo.InvariantCulture, "Failed to extract value '{0}' from submodel path '{1}', model: '{2}'.",
					innerModelName,
					String.Join(".", modelPath.ToArray()),
					model
				);
				throw new TemplateProcessingException(exceptionMessage, exception);
			}

			modelPath.Add(innerModelName);
			toParse.RemoveAt(0);
			return ExtractValue(innerModel, modelPath, toParse);
		}


		private static object ExtractDirectValue(object model, String nextObjectName)
		{
			if (model == null) { return null; } //allows writing deep queries without a chain of "if not null" checks (root model is required to be non-null though).

			Type modelType = model.GetType();
			var members = modelType.GetMember(nextObjectName, MemberTypesToSearch, BindingFlagsForSearch);
			if (members.Length == 0) { throw new TemplateProcessingException("No member was found in model with name '" + nextObjectName + "'."); }

			return ExtractDirectValue(model, members.First());
		}

		private static object ExtractDirectValue(object model, MemberInfo member)
		{
			var property = member as PropertyInfo;
			if (property != null)
			{
				if (!property.CanRead) { throw new InvalidOperationException("Property is not readable."); }
				return property.GetValue(model, null);
			}

			var field = member as FieldInfo;
			return field.GetValue(model);
		}

		private static Type ExtractMemberType(MemberInfo member)
		{
			var property = member as PropertyInfo;
			if (property != null)
			{
				return property.PropertyType;
			}

			var field = member as FieldInfo;
			Debug.Assert(field != null);
			return field.FieldType;
		}
	}
}
