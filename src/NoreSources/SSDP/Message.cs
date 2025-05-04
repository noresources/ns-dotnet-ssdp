/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using NoreSources;

namespace NoreSources.SSDP
{
	/// <summary>
	/// SSDP standard NTS header field values
	/// </summary>
	public class NotificationType
	{
		public const string Alive = "ssdp:alive";
		public const string Dead = "ssdp:byebye";
	}

	/// <summary>
	/// SSDP message base class
	public class Message : EventArgs
	{
		/// <summary>
		/// Message header fields
		/// </summary>
		/// <value>The SSDP message header fields.</value>
		public HttpHeaders Headers
		{
			get
			{
				return headers;
			}
		}

		public Message() : base()
		{
			headers = new SSDPHttpHeaders();
		}

		/// <summary>
		/// Convert instance to a SSDP message
		/// </summary>
		/// <returns>RFC 2616 representation of HTTP header fields</returns>
		public override string ToString()
		{
			return headers.ToString().ReplaceLineEndings("\r\n");
		}

		protected void ReplaceHeaderField(string name, string value)
		{
			if (Headers.Contains(name))
			{
				headers.Remove(name);
			}

			headers.Add(name, value);
		}

		protected string TryGetHeaderFieldValue(string name, string fallback = null)
		{
			if (!Headers.Contains(name))
			{
				return fallback;
			}

			var values = headers.GetValues(name);

			if (!(values is IEnumerable<string>))
			{
				return fallback;
			}

			return Collections.Utility.First(values, fallback);
		}

		protected string GetHeaderFieldValue(string name)
		{
			return NoreSources.HTTP.Utility.GetFirstHeaderFieldValue(headers, name, null);
		}

		private HttpHeaders headers;
	}

	internal class SSDPHttpHeaders : HttpHeaders
	{
		public SSDPHttpHeaders() { }
	}
}
