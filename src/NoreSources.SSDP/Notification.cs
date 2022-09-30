/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Net;
using System.Net.Http.Headers;

namespace NoreSources.SSDP
{
	/// <summary>
	/// Notification message
	/// </summary>
	public class Notification : Message, ICloneable
	{
		/// <summary>
		/// Notification type (NTS header field)
		/// </summary>
		/// <value>Value of the HTS header field.</value>
		public string Type
		{
			get
			{
				return TryGetHeaderFieldValue("NTS", NotificationType.Alive);
			}
			set
			{
				ReplaceHeaderField("NTS", value);
			}
		}
		
		/// <summary>
		/// Notification subject (NT heeader field)
		/// </summary>
		/// <value>The NT header field value.</value>
		public string Subject
		{
			get
			{
				return TryGetHeaderFieldValue("NT", "");
			}
			set
			{
				ReplaceHeaderField("NT", value);
			}
		}
		
		/// <summary>
		/// Unique ID of the device or service (USN header field)
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
		/// Notification expiration delay (The max-age parameter value of the Cache-Control header field).
		/// </summary>
		/// Expressed in seconds. Default value is 30.
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
				
				ReplaceHeaderField("Cache-Control", cc.ToString());
			}
		}
		
		public IPAddress Address
		{
			get
			{
				return emitterAddress;
			}
			set
			{
				emitterAddress = value;
			}
		}
		
		/// <summary>
		/// NOTIFY SSDP request message
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:NoreSources.SSDP.Notification"/>.</returns>
		public override string ToString()
		{
			string s = "NOTIFY * HTTP/1.1\r\n";
			
			if (!Headers.Contains("NTS"))
			{
				s += "NTS: " + NotificationType.Alive + "\r\n";
			}
			
			return s + base.ToString();
		}
		
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		
		public Notification() : base()
		{
			emitterAddress = IPAddress.None;
		}
		
		private IPAddress emitterAddress;
	}
}