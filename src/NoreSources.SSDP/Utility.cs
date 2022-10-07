/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Collections.Generic;

namespace NoreSources.SSDP
{
	internal class Utility
	{
		public static T First<T>(in IEnumerable<T> container, T fallback)
		{
			var e = container.GetEnumerator();
			
			if (!e.MoveNext())
			{
				return fallback;
			}
			
			return e.Current;
		}
	}
}