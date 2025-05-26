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
		/// <summary>
		/// Gets the first value of a given header field.
		/// </summary>
		/// <param name="headers">Header map</param>
		/// <param name="name">Header field name</param>
		/// <param name="fallback">Value to return if header map does not contain any occurence of the header field.</param>
		/// <returns>First value of the header field if any. Otherwise, fallback</returns>
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

		public static string NormalizeHeaderLineEndings(string header)
		{
			return header.Replace("\r\n", "\n")
			.Replace("\r", "\n")
			.Replace("\n", "\r\n");
		}
	}
}