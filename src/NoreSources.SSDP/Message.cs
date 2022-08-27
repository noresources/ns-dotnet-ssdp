/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Text;
using System.Net.Http.Headers;

namespace NoreSources.SSDP
{
	/// <summary>
	/// SSDP standard NTS header field values
	/// </summary>
	public class NotificationType
	{
		public static readonly string Alive = "ssdp:alive";
		public static readonly string Dead = "ssdp:byebye";
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
			string text = "";
			text += headers.ToString();
			return text + "\r\n";
		}
		
		protected void ReplaceHeaderField(string name, string value)
		{
			if (Headers.Contains(name))
			{
				headers.Remove(name);
			}
			
			headers.Add(name, value);
		}
		
		protected string TryGetHeaderFieldValue(string name, string fallback)
		{
			if (!Headers.Contains(name))
			{
				return fallback;
			}
			
			return ((string[])headers.GetValues(name))[0];
		}
		
		protected string TryGetHeaderFieldValue(string name, string glue, string fallback)
		{
			if (!Headers.Contains(name))
			{
				return fallback;
			}
			
			return String.Join(glue, ((string[])headers.GetValues(name)));
		}
		
		protected string GetHeaderFieldValue(string name)
		{
			return ((string[])headers.GetValues(name))[0];
		}
		
		protected string GetHeaderFieldValue(string name, string glue)
		{
			return String.Join(glue, ((string[])headers.GetValues(name)));
		}
		
		private HttpHeaders headers;
	}
	
	internal class SSDPHttpHeaders  : HttpHeaders
	{
		public SSDPHttpHeaders() {}
	}
}
