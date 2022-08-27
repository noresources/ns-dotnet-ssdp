# ns-dotnet-ssdp
SSDP .Net implementation

This project aims to provide a "pure" SSDP implementation.
It can be used as a base for UPnP service discovery protocol
implementation but also for vendor specific protocols.

## Features

* Listen SSDP messages on a multicast group.
* Emit event when a new notificated is received.
* Send search request (`M-SEARCH`)
* Send notification (`NOTIFY`) messages.  
* Automatically re-send registered notifications before expiration timeout.

# Usage

```c#

using System;
using System.Threading;
using NoeeSources.SSDP;

namespace Example {
	
	public class Test {
		
		private static Protocol protocol;
		
		public static void Exit() {
			if (protocol != null) {
				protocol.Stop();
			}
		}
		
		public static void Main(String[] args) {
		
			Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
				Exit();
			};
			
			protocol = new Protocol();
			
			// Invoked for each NEW notification read from
			// the multicast group or from a unicast search response.
			
			protocol.OnNotification += (Notification n, NotificationEventReason r) => {
				Console.WriteLine("Notification "
				                  + n.Type + " "
				                  + r + " "
				                  + n.USN);
			};
			
			// Create a service description notification
			Notification me = protocol.CreateNotification();
			// Shorthand to set the NT header field value
			me.Subject = "vendor-service:foo";
			me.USN = "uuid:600dcafe-34f9-4385-ab50-47d0f5ffb20b::" + me.Subject;
			me.MaxAge = new TimeSpan(0, 0,  60);
			// Manually add HTTP header fields
			me.Headers.Add ("User-defined", "Huh ?i");

		// Submit notification.
		// Notification will be sent when protocol will be started.
		// Then, it will be re-emitted around every me.MaxAge seconds 
			protocol.Notify(me, true);

			// Create a service search request
			// for the same kind of service. 			
			var searchRequest = protocol.CreateSearchRequest(me.Subject);
			protocol.Search(searchRequest);
			
			// Start multicast communication
			// - Send pending notifications
			// - Send pending searches
			// - Listen multicast members messages
			protocol.Start();
			
			Console.WriteLine("CTRL+C to quit");
			
			while (true)
			{
				// Update state
				// - Process received messages
				// - Re-emit registered notification if needed 
				protocol.Update();
				Thread.Sleep(33);
			}
		}
	}
}
```

## References
* [SSDP draft v1.03](https://datatracker.ietf.org/doc/html/draft-cai-ssdp-v1-03)

                                                                                                                 57,8          Bot
