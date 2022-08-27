/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace NoeeSources.SSDP
{
	/// <summary>
	/// A general reason for issueing a Notification
	/// </summary>
	public enum NotificationEventReason
	{
		/// <summary>
		/// A new device or service has appeared.
		/// </summary>
		Added,
		/// <summary>
		/// Properties of an already known device or service has changed.
		/// </summary>
		Updated,
		/// <summary>
		/// A removal notification was received for a given device or service.
		/// </summary>
		Removed,
		/// <summary>
		/// An existing notification was not renewed before the expiration time.
		/// </summary>
		Expired,
		/// <summary>
		/// Any other reason
		/// </summary>
		Other
	};
	
	/// <summary>
	/// Notification event handler
	/// </summary>
	/// <param name="reason">Notification reason</param>
	/// <param name="notification">Notification message</param>
	public delegate void NotificationEventHandler(
	    Notification notification,
	    NotificationEventReason reason);
	    
	/// <summary>
	/// Protocol option flags
	/// </summary>
	public struct ProtocolOptions
	{
		/// <summary>
		/// Process message immediately instead of processing them in the Update() method
		/// </summary>
		public readonly static int  ImmediateMessageProcessing = (1 << 0);
		
		/// <summary>
		/// Also emit OnNotification events for notification
		/// sent with the Notify() method
		/// </summary>
		public readonly static int  NotifyLoopback = (1 << 1);
		/// <summary>
		/// Emit OnNotification events for all received notification messages
		/// even for already known ones.
		/// </summary>
		public readonly static int  NotifyAll = (1 << 2);
	}
	
	/// <summary>
	/// SSDP protocol instance
	/// </summary>
	public class Protocol
	{
		/// <summary>
		/// Emit notification event for new, updated and removed devices or services.
		/// </summary>
		public event NotificationEventHandler OnNotification;
		
		/// <summary>
		/// Parse SSDP message content
		/// </summary>
		/// <returns>A Message derived class.</returns>
		/// <param name="text">SSDP message text.</param>
		public static Message ParseMessage(string text)
		{
			var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			
			if (lines.Length == 0)
			{
				throw new Exception("Invalid message");
			}
			
			string firstLine = lines[0];
			Message message = null;
			Match match = null;
			Regex requestRegex = new Regex(
			    @"^([a-z_-]+)\s+\*\s+HTTP/[0-9]+\.[0-9]+$",
			    RegexOptions.IgnoreCase);
			    
			Regex responseRegex = new Regex(
			    @"^HTTP/[0-9]+\.[0-9]+\s200\s+",
			    RegexOptions.IgnoreCase);
			    
			if ((match = requestRegex.Match(firstLine)).Success)
			{
				string method = match.Groups[1].ToString();
				
				if (method.ToUpper() == "NOTIFY")
				{
					message = new Notification();
				}
				else if (method.ToUpper() == "M-SEARCH")
				{
					message = new SearchRequest();
				}
			}
			else if ((match = responseRegex.Match(firstLine)).Success)
			{
				message = new SearchResponse();
			}
			
			if (message == null)
			{
				throw new Exception("Unsupported message type " + firstLine);
			}
			
			string name = "";
			string value = "";
			
			for (int i = 1; i < lines.Length; ++i)
			{
				string line = lines[i];
				
				if (line.Length == 0)
				{
					break;
				}
				
				if (line[0] == ' ' || line[0] == '\t')
				{
					if (name.Length == 0)
					{
						throw new Exception("Invalid line " + line);
					}
					
					value += line;
					continue;
				}
				
				if (name.Length > 0)
				{
					try
					{
						if (value.Length > 0)
						{
							message.Headers.Add(name, value);
						}
					}
					catch (Exception) { /* Ignore invalid headers */ }
					
					name = "";
					value = "";
				}
				
				int colon = line.IndexOf(':');
				
				if (colon <= 0)
				{
					throw new Exception("Invalid header field line " + line);
				}
				
				name = line.Substring(0, colon);
				value = line.Substring(colon + 1).TrimStart();
			}
			
			if (name.Length > 0)
			{
				message.Headers.Add(name, value);
			}
			
			return message;
		}
		
		/// <summary>
		/// Host header field value used when creating SSDP messages.
		/// </summary>
		/// <value>The host header value.</value>
		public string HostHeaderValue
		{
			get
			{
				return multicastEndPoint.Address + ":"  + multicastEndPoint.Port;
			}
		}
		
		/// <summary>
		/// User-Agent and Server header field value
		/// used when creating SSDP messages.
		/// </summary>
		/// <value>The signature header value.</value>
		public string SignatureHeaderValue
		{
			get
			{
				return signatureHeaderValue;
			}
			set
			{
				signatureHeaderValue = value;
			}
		}
		
		/// <summary>
		/// Create a notification message filled
		/// with the relevant header fields.
		/// </summary>
		/// <returns>The notification.</returns>
		public Notification CreateNotification()
		{
			var n = new Notification();
			n.Headers.Add("HOST", HostHeaderValue);
			n.Headers.Add("SERVER", SignatureHeaderValue);
			return n;
		}
		
		/// <summary>
		/// Create a SSDP notification corresponding
		/// to the given search response.
		/// </summary>
		/// <returns>The notification.</returns>
		/// <param name="r">The red component.</param>
		public Notification CreateNotification(SearchResponse r)
		{
			var n = new Notification();
			
			foreach (var e in r.Headers)
			{
				string name = e.Key.ToUpper();
				
				if (name == "S")
				{
					continue;
				}
				
				if (name == "ST")
				{
					name = "NT";
				}
				
				n.Headers.Add(name, e.Value);
			}
			
			return n;
		}
		
		/// <summary>
		/// Create a search request filled with the
		/// relevant default header field values.
		/// </summary>
		/// <returns>The search request.</returns>
		public SearchRequest CreateSearchRequest()
		{
			return CreateSearchRequest(SearchRequest.SearchAll);
		}
		
		/// <summary>
		/// Ceate a search request message for the given subject
		/// </summary>
		/// <returns>The search request.</returns>
		/// <param name="subject">Subject.</param>
		public SearchRequest CreateSearchRequest(string subject)
		{
			var sr = new SearchRequest();
			sr.Subject = subject;
			sr.Headers.Add("HOST", HostHeaderValue);
			sr.Headers.Add("USER-AGENT", SignatureHeaderValue);
			sr.Headers.Add("MAN", "\"ssdp:discover\"");
			sr.Headers.Add("MX", "1");
			return sr;
		}
		
		/// <summary>
		/// Create a search response for the given subject
		/// </summary>
		/// <returns>The search response.</returns>
		/// <param name="subject">Subject.</param>
		/// <param name="usn">USN of the device or service matching the subject.</param>
		public SearchResponse CreateSearchResponse(string subject, string usn)
		{
			SearchResponse r = new SearchResponse();
			r.Subject = subject;
			r.Headers.Add("HOST", HostHeaderValue);
			r.Headers.Add("Ext", "");
			CacheControlHeaderValue cc = new CacheControlHeaderValue();
			cc.MaxAge = new TimeSpan(0, 0, 30);
			/// @todo Add no-cache="Ext"
			r.Headers.Add("CACHE-CONTROL", cc.ToString());
			r.USN = usn;
			return r;
		}
		
		/// <summary>
		/// Protocol option and state flags
		/// </summary>
		/// <value>The protocol option and state flags.</value>
		public int Flags
		{
			get
			{
				return flags;
			}
			set
			{
				int publicFlags = (ProtocolOptions.ImmediateMessageProcessing
				                   | ProtocolOptions.NotifyLoopback
				                   | ProtocolOptions.NotifyAll);
				flags &= ~publicFlags;
				flags |= (value & publicFlags);
			}
		}
		
		/// <summary>
		/// Create a SSDP protocol
		/// </summary>
		/// <param name="address">Multicast address.</param>
		/// <param name="port">Multicast port.</param>
		public Protocol(
		    string address = "239.255.255.250",
		    int port = 1900)
		{
			flags = 0;
			expirationLeaway = 5;
			multicastEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
			localEndPoint = new IPEndPoint(IPAddress.Any, multicastEndPoint.Port);
			remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.Diagnostics.FileVersionInfo assemblyVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
			
			signatureHeaderValue =
			    Regex.Replace(Environment.OSVersion.ToString(), @"\s+(?=[0-9.]+$)", "/")
			    + " SSDP/1.0.3"
			    + " NoreSources.SSDP/" + assemblyVersionInfo.FileVersion;
			    
			multicastOption = new MulticastOption(multicastEndPoint.Address, localEndPoint.Address);
			messageBuffer = new byte[2048];
			messageReceptionCallback = new AsyncCallback(
			    MessageReceptionCallback);
			    
			messages = new Queue<Message>();
			applicationNotifications = new Dictionary<string, ProtocolNotification>();
			activeNotifications = new Dictionary<string, ProtocolNotification>();
			
			pendingNotifications = new Queue<PendingNotification>();
			pendingSearches = new Queue<SearchRequest>();
		}
		
		/// <summary>
		/// Search a given device or service type.
		/// </summary>
		/// <param name="subject">Search subject.</param>
		/// <param name="handler">A notification Handler to add to the OnNotification event.</param>
		public void Search(string subject, NotificationEventHandler handler = null)
		{
			Search(CreateSearchRequest(subject), handler);
		}
		
		/// <summary>
		/// Send the given search request
		/// </summary>
		/// <param name="sr">Sr.</param>
		/// <param name="handler">A notification Handler to add to the OnNotification event.</param>
		public void Search(SearchRequest sr, NotificationEventHandler handler = null)
		{
			if (handler != null)
			{
				string subject = sr.Subject;
				
				foreach (var e in activeNotifications)
				{
					Notification n = e.Value.notification;
					
					if (subject == SearchRequest.SearchAll || n.Subject == subject)
					{
						handler(n, NotificationEventReason.Added); /*@todo a special reason */
					}
				}
			}
			
			if ((flags & (int)StateFlags.Started) == 0)
			{
				pendingSearches.Enqueue(sr);
				return;
			}
			
			BeginSendMessage(sr, multicastEndPoint);
		}
		
		/// <summary>
		/// Send a notification on the multicast address
		/// </summary>
		/// <param name="n">Notification to emit.</param>
		/// <param name="persist">If set to <c>true</c>, the notification will be automatically renewed if needed when the Updat() method is invoked.</param>
		public void Notify(Notification n, bool persist = false)
		{
			if ((flags & (int)StateFlags.Started) == 0)
			{
				var p = new PendingNotification();
				p.notification = n;
				p.persist = persist;
				pendingNotifications.Enqueue(p);
				return;
			}
			
			byte[] bytes = null;
			string key = n.Key;
			
			if (applicationNotifications.ContainsKey(key))
			{
				applicationNotifications.Remove(key);
			}
			
			if (persist && n.Type == NotificationType.Alive)
			{
				var pn = new ProtocolNotification(n);
				bytes = pn.messageData;
				applicationNotifications.Add(key, pn);
			}
			else
			{
				bytes = System.Text.Encoding.ASCII.GetBytes(n.ToString());
			}
			
			multicastSocket.BeginSendTo(
			    bytes, 0, bytes.Length, 0,
			    multicastEndPoint,
			    new AsyncCallback(MessageSendingCallback),
			    this);
		}
		
		/// <summary>
		/// Start listening and emitting SSDP messages
		/// </summary>
		public void Start()
		{
			if ((flags & (int)StateFlags.Started) != 0)
			{
				return;
			}
			
			multicastSocket = new Socket(AddressFamily.InterNetwork,
			                             SocketType.Dgram,
			                             ProtocolType.Udp);
			multicastSocket.SetSocketOption(
			    SocketOptionLevel.IP,
			    SocketOptionName.ReuseAddress,
			    true);
			multicastSocket.Bind(localEndPoint);
			multicastSocket.SetSocketOption(SocketOptionLevel.IP,
			                                SocketOptionName.AddMembership,
			                                multicastOption);
			                                
			EndPoint endPoint = (EndPoint)remoteEndPoint;
			multicastSocket.BeginReceiveFrom(
			    messageBuffer, 0, messageBuffer.Length, 0,
			    ref endPoint,
			    messageReceptionCallback, this
			);
			
			flags |= (int)StateFlags.Started;
			
			foreach (var p in pendingNotifications)
			{
				Notify(p.notification, p.persist);
			}
			
			pendingNotifications.Clear();
			
			foreach (var s in pendingSearches)
			{
				Search(s);
			}
			
			pendingSearches.Clear();
		}
		
		/// <summary>
		/// Stop listening and emitting SSDP messages.
		/// </summary>
		public async void Stop()
		{
			if ((flags & (int)StateFlags.Started) == 0)
			{
				return;
			}
			
			foreach (var e in applicationNotifications)
			{
				e.Value.notification.Type = NotificationType.Dead;
				e.Value.Build();
				
				await Task.Factory.FromAsync(
				    multicastSocket.BeginSendTo(
				        e.Value.messageData, 0, e.Value.messageData.Length, 0,
				        multicastEndPoint,
				        new AsyncCallback(MessageSendingCallback),
				        this), null);
				        
			}
			
			multicastSocket.Close();
			multicastSocket = null;
			
			flags &= ~(int)StateFlags.Started;
		}
		
		/// <summary>
		/// Process pending SSDP message and renew persistent notifications.
		/// </summary>
		public void Update()
		{
			Message m;
			
			while (messages.Count > 0)
			{
				m = messages.Dequeue();
				ProcessMessage(m);
			}
			
			DateTime now = DateTime.Now;
			TimeSpan delta;
			
			foreach (var e in applicationNotifications)
			{
				delta = e.Value.expirationDateTime - now;
				
				if (delta.TotalSeconds < expirationLeaway)
				{
					e.Value.Poke();
					multicastSocket.BeginSendTo(
					    e.Value.messageData, 0, e.Value.messageData.Length, 0,
					    multicastEndPoint,
					    new AsyncCallback(MessageSendingCallback),
					    this);
				}
			}
			
			List<string> deads = new List<string>();
			
			foreach (var e in activeNotifications)
			{
				if (applicationNotifications.ContainsKey(e.Key))
				{
					continue;
				}
				
				delta = e.Value.expirationDateTime - now;
				
				if (delta.TotalSeconds < -expirationLeaway)
				{
					deads.Add(e.Key);
					e.Value.notification.Type = NotificationType.Dead;
					
					if (OnNotification != null)
					{
						OnNotification(e.Value.notification, NotificationEventReason.Expired);
					}
				}
			}
			
			foreach (var key in deads)
			{
				/* @todo Event */
				activeNotifications.Remove(key);
			}
		}
		
		private void ProcessMessage(Message message)
		{
			if (message is Notification)
			{
				Notification n = (message as Notification);
				string key = n.Key;
				bool exists = activeNotifications.ContainsKey(key);
				bool isLoopback = applicationNotifications.ContainsKey(key);
				bool notifyLoopback = ((flags & ProtocolOptions.NotifyLoopback) == ProtocolOptions.NotifyLoopback);
				bool emit = ((flags & ProtocolOptions.NotifyAll) == ProtocolOptions.NotifyAll);
				string type = n.Type;
				NotificationEventReason reason = NotificationEventReason.Other;
				
				if (type == NotificationType.Dead)
				{
					reason = NotificationEventReason.Removed;
					
					if (exists)
					{
						emit = emit
						       || !isLoopback
						       || (isLoopback && notifyLoopback);
						ProtocolNotification pn = activeNotifications[key];
						activeNotifications.Remove(key);
					}
					
					if (emit && OnNotification != null)
					{
						OnNotification(n, reason);
					}
					
					return;
				}
				
				if (type != NotificationType.Alive)
				{
					emit = emit
					       || !isLoopback
					       || (isLoopback && notifyLoopback);
					       
					if (emit && OnNotification != null)
					{
						OnNotification(n, reason);
					}
					
					return;
				}
				
				// Alive
				
				if (exists)
				{
					ProtocolNotification pn = activeNotifications[key];
					
					pn.Poke();
					
					if (pn.notification.ToString() != n.ToString())
					{
						reason = NotificationEventReason.Updated;
						emit = emit
						       || !isLoopback
						       || (isLoopback && notifyLoopback);
						       
						// No need to rebuild
						pn.notification = n;
					}
				}
				else
				{
					reason = NotificationEventReason.Added;
					emit = emit
					       || !isLoopback
					       || (isLoopback && notifyLoopback);
					ProtocolNotification pn = new ProtocolNotification(n);
					activeNotifications.Add(key, pn);
				}
				
				if (emit && OnNotification != null)
				{
					OnNotification(n, reason);
				}
				
				return;
			}
			
			if (message is SearchRequest)
			{
				SearchRequest sr = (message as SearchRequest);
				string subject =   sr.Subject;
				
				foreach (var e in applicationNotifications)
				{
					Notification n =  e.Value.notification;
					
					if (subject == SearchRequest.SearchAll || n.Subject == subject)
					{
						SearchResponse r = CreateSearchResponse(n.Subject, n.USN);
						BeginSendMessage(r, sr.EndPoint);
					}
				}
				
				return;
			}
			
			if (message is SearchResponse)
			{
				SearchResponse sr = (message as SearchResponse);
				Notification n = CreateNotification(sr);
				string key = n.Key;
				bool exists = activeNotifications.ContainsKey(key);
				
				if (!exists)
				{
					activeNotifications.Add(key, new ProtocolNotification(n));
				}
				
				if (exists || OnNotification == null)
				{
					return;
				}
				
				OnNotification(n, NotificationEventReason.Added);
				return;
			}
		}
		
		private void HandleMessageSending(IAsyncResult ar)
		{
			try
			{
				multicastSocket.EndSendTo(ar);
			}
			catch (Exception)
			{
				/**
				* @todo What to do with failures
				*/
			}
		}
		
		private void HandleMessageReception(IAsyncResult ar)
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
			
			try
			{
				int bytesRead = multicastSocket.EndReceiveFrom(ar, ref endPoint);
				
				if (bytesRead <= 0)
				{
					return;
				}
				
				string text = Encoding.ASCII.GetString(messageBuffer, 0, bytesRead);
				
				Message message = ParseMessage(text);
				
				if (message is SearchRequest)
				{
					SearchRequest r = (message as SearchRequest);
					
					if (endPoint is IPEndPoint)
					{
						r.EndPoint = (endPoint as IPEndPoint);
					}
				}
				
				if ((flags & (int)ProtocolOptions.ImmediateMessageProcessing) == ProtocolOptions.ImmediateMessageProcessing)
				{
					ProcessMessage(message);
				}
				else
				{
					messages.Enqueue(message);
				}
			}
			catch (Exception)
			{
				/**
				* @todo What to do with invalid headers
				*/
			}
			finally
			{
				endPoint = (EndPoint)remoteEndPoint;
				multicastSocket.BeginReceiveFrom(
				    messageBuffer, 0, messageBuffer.Length, 0,
				    ref endPoint,
				    messageReceptionCallback, this);
			}
		}
		
		private void MessageSendingCallback(IAsyncResult ar)
		{
			(ar.AsyncState as Protocol).HandleMessageSending(ar);
		}
		
		private void MessageReceptionCallback(IAsyncResult ar)
		{
			(ar.AsyncState as Protocol).HandleMessageReception(ar);
		}
		
		private IAsyncResult BeginSendMessage(Message message, EndPoint endPoint = null)
		{
			if (endPoint == null)
			{
				endPoint = multicastEndPoint;
			}
			
			byte [] bytes = System.Text.Encoding.ASCII.GetBytes(message.ToString());
			return multicastSocket.BeginSendTo(
			           bytes, 0, bytes.Length, 0,
			           endPoint,
			           new AsyncCallback(MessageSendingCallback),
			           this);
		}
		
		private struct StateFlags
		{
			public readonly static int  Started = (1 << 32);
		}
		
		private int flags;
		private IPEndPoint localEndPoint;
		private string signatureHeaderValue;
		
		private IPEndPoint multicastEndPoint;
		MulticastOption multicastOption;
		private Socket multicastSocket;
		private byte[] messageBuffer;
		AsyncCallback messageReceptionCallback;
		private IPEndPoint remoteEndPoint;
		
		private Queue<Message> messages;
		private Dictionary<string, ProtocolNotification> applicationNotifications;
		private Dictionary<string, ProtocolNotification> activeNotifications;
		private Queue< PendingNotification > pendingNotifications;
		private Queue<SearchRequest> pendingSearches;
		int expirationLeaway;
	} // Protocol
	
	struct PendingNotification
	{
		public Notification notification;
		public bool persist;
	}
	
	internal class ProtocolNotification
	{
		public ProtocolNotification(Notification n)
		{
			notification = n;
			Poke();
			Build();
		}
		
		public void Poke()
		{
			expirationDateTime = (DateTime.Now + notification.MaxAge);
		}
		
		public void Build()
		{
			messageData = System.Text.Encoding.ASCII.GetBytes(notification.ToString());
		}
		
		public Notification notification;
		public DateTime expirationDateTime;
		public byte[] messageData;
	}
}