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

	This file is from project https://github.com/NortalLTD/Utilities.TextTemplating, Nortal.Utilities.TextTemplating, file 'DefaultTemplateValueFormatter.cs'.
*/

using System;
using System.Globalization;
using System.Linq;

namespace Nortal.Utilities.TextTemplating
{
	public class DefaultTemplateValueFormatter : ITemplateValueFormatter
	{
		public DefaultTemplateValueFormatter() 
		{
			this.Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
		}

		public CultureInfo Culture { get; set; }

		public String FormatValue(Object value, String format)
		{
			if (value == null) { return null; }
			var formattableValue = value as IFormattable;
			//not formattable:
			if (formattableValue == null)
			{
				if (format != null) { throw new InvalidOperationException("Format cannot be applied to the object of type: " + value.GetType()); }
				return value.ToString();
			}

			//formattable:
			if (format == null) { format = GetDefaultFormatFor(value); }
			return formattableValue.ToString(format, this.Culture);
		}

		private static string GetDefaultFormatFor(Object value)
		{
			Type type = value.GetType();
			if (type.IsGenericType && type == typeof(Nullable<Boolean>)) { type = type.GetGenericArguments().First(); }
			if (type == typeof(DateTime))
			{
				var dateValue = (DateTime)value;
				if (dateValue == dateValue.Date) { return "d"; } //short pattern, 6/15/2009 (en-US)
				return "g"; //short date time pattern: 6/15/2009 1:45PM (en-US)
			}

			return null; //IFormattableDefault.
		}
	}
}
