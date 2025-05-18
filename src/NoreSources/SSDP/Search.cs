/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace NoreSources.SSDP
{
	/// <summary>
	/// SSDP device or service search request
	/// </summary>
	public class SearchRequest : Message
	{
		/// <summary>
		/// Special search subject to query all available devices and services.
		/// </summary>
		public const string SearchAll = "ssdp:all";

		/// <summary>
		/// Device or service type to search
		/// </summary>
		/// <value>The ST header field value.</value>
		public string Subject
		{
			get
			{
				return TryGetHeaderFieldValue("ST", SearchAll);
			}
			set
			{
				ReplaceHeaderField("ST", value);
			}
		}

		/// <summary>
		/// The IP address and port from which the request was issued
		/// </summary>
		/// <value>Search emitter end point.</value>
		public IPEndPoint EndPoint
		{
			get
			{
				return endPoint;
			}
			set
			{
				endPoint = value;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public SearchRequest() : base()
		{
			endPoint = null;
		}

		/// <summary>
		/// M-SEARCH SSDP request message
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:NoreSources.SSDP.SearchRequest"/>.</returns>
		public override string ToString()
		{
			return "M-SEARCH * HTTP/1.1\r\n" + base.ToString();
		}

		private IPEndPoint endPoint;
	}

	/// <summary>
	/// Response to a search request
	/// </summary>
	public class SearchResponse : Message
	{
		/// <summary>
		/// Search subject (ST header field)
		/// </summary>
		/// <value>The ST header field value.</value>
		public string Subject
		{
			get
			{
				return TryGetHeaderFieldValue("ST", SearchRequest.SearchAll);
			}
			set
			{
				ReplaceHeaderField("ST", value);
			}
		}

		/// <summary>
		/// The unique ID the device or service.
		/// </summary>
		/// <value>The USN header field value.</value>
		public string USN
		{
			get
			{
				return TryGetHeaderFieldValue("USN", "");
			}
			set
			{
				ReplaceHeaderField("USN", value);
			}
		}

		/// <summary>
		/// Expiration delay of the notification
		/// </summary>
		/// <value>The max-age parameter value of the Cache-Control header field.</value>
		public TimeSpan MaxAge
		{
			get
			{
				string text = TryGetHeaderFieldValue("Cache-Control", "");

				if (text.Length > 0)
				{
					var cc = CacheControlHeaderValue.Parse(text);

					if (cc.MaxAge != null)
					{
						return (TimeSpan)cc.MaxAge;
					}
				}

				return new TimeSpan(0, 0, 30);
			}
			set
			{
				CacheControlHeaderValue cc = null;
				string text = TryGetHeaderFieldValue("Cache-Control", "");

				if (text.Length > 0)
				{
					cc = CacheControlHeaderValue.Parse(text);
				}
				else
				{
					cc = new CacheControlHeaderValue();
				}

				cc.MaxAge = value;
				ReplaceHeaderField("Cache-Control", cc.ToString());
			}
		}

		public SearchResponse() : base()
		{ }

		/// <summary>
		/// SSDP search response (i.e. 200 OK HTTP response message)
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:NoreSources.SSDP.SearchResponse"/>.</returns>
		public override string ToString()
		{
			return "HTTP/1.1 200 OK\r\n" + base.ToString();
		}
	}
}