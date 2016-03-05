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

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'ITemplateValueFormatter.cs'.
*/

using System;

namespace Nortal.Utilities.TextTemplating
{
	/// <summary>
	/// Class to control logic how extracted values will be formatted to generated document. For example: handling dates, etc.
	/// </summary>
	public interface ITemplateValueFormatter
	{
		/// <summary>
		/// Turns value extracted from model into its string representation which is to be included in output document.
		/// </summary>
		/// <param name="value">Raw value to format</param>
		/// <param name="format">formatstring as explicitly given by template.</param>
		/// <returns>formatted value</returns>
		String FormatValue(Object value, String format);
	}
}
