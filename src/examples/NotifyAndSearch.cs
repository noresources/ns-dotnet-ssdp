/*
 * Copyright © 2022 by Renaud Guillard (dev@nore.fr)
 * Distributed under the terms of the MIT License, see LICENSE
 */

using System;
using System.Net;
using System.Threading;

using NoreSources.SSDP;

namespace NoreSources.SSDP.Examples
{
	public class SampleTest
	{
		private static Protocol protocol;

		public static void Exit()
		{
			Console.WriteLine("Exit");

			if (protocol != null)
			{
				protocol.Stop();
			}
		}

		public static void Main(String[] args)
		{
			uint flags = 0;

			foreach (var a in args)
			{
				if (a == "--notify-loopback")
				{
					flags = ProtocolOptions.NotifyLoopback;
				}
				else if (a == "--notify-all")
				{
					flags = ProtocolOptions.NotifyAll;
				}
			}

			Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
			{
				Console.WriteLine("Cancelled");
				Exit();
			};

			protocol = new Protocol();
			protocol.Flags = flags;

			Console.WriteLine("Protocol flags: " + protocol.Flags);

			protocol.OnNotification += (Notification n, NotificationEventReason r) =>
			{
				Console.WriteLine("Notification from "
								  + n.Address.ToString() + " "
								  + n.Type + " "
								  + r + " "
								  + n.USN);
			};

			Notification me = protocol.CreateNotification();
			me.Subject = "urn:schemas-nore-fr:service:http:1";
			me.USN = "uuid:" + Guid.NewGuid().ToString() + "::" + me.Subject;
			me.MaxAge = new TimeSpan(0, 0, 60);

			me.Headers.Add("User-defined", "Huh ?i");

			Console.WriteLine("Notify: " + me);
			protocol.Notify(me, true);

			var sr = protocol.CreateSearchRequest(me.Subject);
			Console.WriteLine("Search " + sr);
			protocol.Search(sr);

			// - Starts multicast connection
			// - Send pending notifications
			// - Send pending searches
			protocol.Start();

			Console.WriteLine("CTRL+C to quit");

			while (true)
			{
				protocol.Update();
				Thread.Sleep(33);
			}
		}
	}
}