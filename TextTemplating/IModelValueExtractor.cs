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

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'IModelValueExtractor.cs'.
*/

using System;
using System.Collections.Generic;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Class to control logic how model paths are translated to values.
	/// </summary>
	public interface IModelValueExtractor
	{
		/// <summary>
		/// Determine value which corresponds to given model path on given model.
		/// </summary>
		/// <param name="model">Object to extract values from.</param>
		/// <param name="valuePath">Path to value to pick from model. Can span multiple levels using "." as separator. Ex: "SubModel.Thingy.Name"</param>
		/// <returns>Value from requested path.</returns>
		Object ExtractValue(Object model, String valuePath);

		/// <summary>
		/// Provides a list of valid value paths extractable from given model using this class.
		/// </summary>
		/// <param name="exampleModel">Model to analyze.</param>
		/// <param name="maximumDepth">Depth limit to look into.</param>
		/// <returns>List of valid markers</returns>
		IEnumerable<String> DiscoverValidValuePaths(Object exampleModel, int maximumDepth);
		
	}
}
