/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System.Text.RegularExpressions;
using System.Text;

using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System;

namespace NoreSources.HTTP.Messages
{
	/// <summary>
	/// HTTP message type
	/// </summary>
	public enum MessageType
	{
		Request,
		Response
	}

	/// <summary>
	/// RFC 2616 Grammar elements
	/// </summary>
	public struct Grammar
	{
		public const string LinearWhiteSpace = "[ \\t]";

		public const string EndOfLine = "\\r\\n";

		public const string Digit = "[0-9]";

		public const string SeparatorCharacterRange = "\\(\\)<>@"
				+ "\\\\;\\\":/\\[\\]\\?="
				+ "\\{\\} \\t";

		public const string Separator = "[" + SeparatorCharacterRange + "]";

		public const string Token = "[^\\p{C}" + SeparatorCharacterRange + "]+";
	}

	/// <summary>
	/// Request line descriptor
	/// </summary>

	public class RequestLine
	{
		public RequestLine() { }

		/// <summary>
		/// Request method
		/// </summary>
		/// <value>The method.</value>
		public HttpMethod Method
		{
			get;
			set;
		}

		/// <summary>
		/// Request URI
		/// </summary>
		/// <value>The request URI.</value>
		public Uri RequestUri
		{
			get;
			set;
		}

		/// <summary>
		/// HTTP protocol version
		/// </summary>
		/// <value>The version.</value>
		public Version Version
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Response status line
	/// </summary>
	public class StatusLine
	{
		public StatusLine() { }

		/// <summary>
		/// HTTP protocol version
		/// </summary>
		/// <value>The version.</value>
		public Version Version
		{
			get;
			set;
		}

		/// <summary>
		/// Response status code
		/// </summary>
		/// <value>The status code.</value>
		public HttpStatusCode StatusCode
		{
			get;
			set;
		}

		/// <summary>
		/// Response reason phrase
		/// </summary>
		/// <value>The reason phrase.</value>
		public string ReasonPhrase
		{
			get;
			set;
		}
	}

	/// <summary>
	/// HTTP message parser
	/// </summary>
	public class Parser
	{

		/// <summary>
		/// URL prefix used to construct request URIs
		/// </summary>
		/// <example>http://example.com</example>
		/// <value>The base URL.</value>
		public string BaseURL
		{
			get;
			set;
		}

		public Parser()
		{
			BaseURL = "http://localhost";
			string pattern = "^(" + Grammar.Token + ")"
							 // TODO real request-uri pattern
							 + " ([^ ]+)"
							 + " HTTP/(" + Grammar.Digit + "+\\." + Grammar.Digit + "+)"
							 + Grammar.EndOfLine;
			requestLineRegex = new Regex(pattern, RegexOptions.IgnoreCase);

			pattern = "HTTP/(" + Grammar.Digit + "+\\." + Grammar.Digit + "+)"
					  + " ([1-9][0-9]*) ([^\\r\\n]+)"
					  + Grammar.EndOfLine;
			statusLineRegex = new Regex(pattern, RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Parse HTTP start line
		/// </summary>
		/// <returns>The message type.</returns>
		/// <param name="requestLine">Request line descriptor. Filled if the start line is a request line.</param>
		/// <param name="statusLine">Status line descriptor. Filled if the start line is a status line.</param>
		/// <param name="startLine">Start line.</param>
		public MessageType ParseStartLine(
			RequestLine requestLine,
			StatusLine statusLine,
			string startLine)
		{
			Match match = null;

			if ((match = requestLineRegex.Match(startLine)).Success)
			{
				if (requestLine != null)
				{
					requestLine.Method = new HttpMethod(match.Groups[1].ToString());
					requestLine.Version = Version.Parse(match.Groups[3].ToString());

					string requestURI = match.Groups[2].ToString();

					if (requestURI != "*")
					{
						try
						{
							requestLine.RequestUri = new Uri(requestURI);
						}
						catch (Exception)
						{
							requestLine.RequestUri = new Uri(BaseURL + requestURI);
						}
					}
				}

				return MessageType.Request;
			}

			if ((match = statusLineRegex.Match(startLine)).Success)
			{
				if (statusLine != null)
				{
					statusLine.Version = Version.Parse(match.Groups[1].ToString());

					try
					{
						statusLine.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), match.Groups[2].ToString());
					}
					catch (Exception) { }

					statusLine.ReasonPhrase = match.Groups[3].ToString();
				}

				return MessageType.Response;
			}

			throw new Exception("Invalid start line");
		}

		/// <summary>
		/// Parse HTTP message header field lines
		/// </summary>
		/// <param name="headers">Header list to populate.</param>
		/// <param name="lines">Header field lines.</param>
		/// <param name="offset">Offset of the first header field line to consider.</param>
		public void ParseHeaders(HttpHeaders headers, string[] lines, int offset = 0)
		{
			if (headers is null)
			{
				throw new ArgumentException("headers argument must be assigned to an object");
			}

			string name = "";
			string value = "";

			for (int i = offset; i < lines.Length; ++i)
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
						throw new Exception("Invalid header field line (empty name)");
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
							headers.Add(name, value);
						}
					}
					catch (Exception) { /* Ignore invalid headers */ }

					name = "";
					value = "";
				}

				int colon = line.IndexOf(':');

				if (colon <= 0)
				{
					throw new Exception("Invalid header field line (no colon)");
				}

				name = line.Substring(0, colon);
				value = line.Substring(colon + 1).TrimStart();
			}

			if (name.Length > 0)
			{
				headers.Add(name, value);
			}
		}

		/// <summary>
		/// Parse a HTTP request message
		/// </summary>
		/// <param name="request">Request to populate.</param>
		/// <param name="text">HTTP message string.</param>
		public void Parse(HttpRequestMessage request, string text)
		{
			if (request is null)
			{
				throw new ArgumentException("request argument must be assigned to an object");
			}

			var p = text.IndexOf("\r\n");

			if (p < 0)
			{
				throw new ArgumentException("Incomplete message");
			}

			string requestLine = text.Substring(0, p + 2);
			Match match = requestLineRegex.Match(requestLine);

			if (!match.Success)
			{
				throw new ArgumentException("Invalid request line");
			}

			StringContent content = null;
			p = text.IndexOf("\r\n\r\n");

			if (p >= 0)
			{
				content = new StringContent(text.Substring(p + 4));
				text = text.Substring(0, p + 4);
			}

			string requestURI = match.Groups[2].ToString();

			try
			{
				if (requestURI != "*")
				{
					request.RequestUri = new Uri(requestURI);
				};
			}
			catch (Exception)
			{
				request.RequestUri = new Uri(BaseURL + requestURI);
			}

			request.Method = new HttpMethod(match.Groups[1].ToString());
			request.Version = Version.Parse(match.Groups[3].ToString());

			var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			ParseHeaders(request.Headers, lines, 1);

			if (content != null)
			{
				request.Content = content;
			}
		}

		/// <summary>
		/// Parse HTTP response message
		/// </summary>
		/// <param name="response">Response to populate.</param>
		/// <param name="text">HTTP response text.</param>
		public void Parse(HttpResponseMessage response, string text)
		{
			if (response is null)
			{
				throw new ArgumentException("response argument must be assigned to an object");
			}

			var p = text.IndexOf("\r\n");

			if (p < 0)
			{
				throw new ArgumentException("Incomplete message");
			}

			string statusLine = text.Substring(0, p + 2);
			Match match = statusLineRegex.Match(statusLine);

			if (!match.Success)
			{
				throw new ArgumentException("Invalid status line");
			}

			StringContent content = null;
			p = text.IndexOf("\r\n\r\n");

			if (p >= 0)
			{
				content = new StringContent(text.Substring(p + 4));
				text = text.Substring(0, p + 4);
			}

			response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), match.Groups[2].ToString());
			response.Version = Version.Parse(match.Groups[1].ToString());

			var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			ParseHeaders(response.Headers, lines, 1);

			if (content != null)
			{
				response.Content = content;
			}
		}

		private Regex requestLineRegex;
		private Regex statusLineRegex;
	}
}