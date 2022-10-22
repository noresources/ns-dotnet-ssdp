/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;

namespace NoreSources.HTTP
{
	public class Utility
	{
		public static string GetFirstHeaderFieldValue(
				HttpHeaders headers,
				string name,
				string fallback = null)
		{
			var e = headers.GetValues(name).GetEnumerator();

			if (!e.MoveNext())
			{
				return fallback;
			}

			return e.Current;
		}
	}
}
