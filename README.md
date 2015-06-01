## Realtime Messaging SDK for Xamarin (PCL)

[Realtime Cloud Messaging](http://framework.realtime.co/messaging) is a highly-scalable pub/sub message broker, allowing you to broadcast messages to millions of users, reliably and securely. It's all in the cloud so you don't need to manage servers.

Realtime.Messaging.Portable is a PCL C# implementation of the Realtime Messaging protocol. This plugin uses .Net 4.5 and is compatible with Windows 8, Windows Phone, Windows Phone Silverlight, Xamarin Android, Xamarin iOS, and Xamarin iOS (Classic).

### Dependencies

- [sockets-for-pcl](https://github.com/rdavisau/sockets-for-pcl) (Nuget) 
- Microsoft HTTP Client Libraries (NuGet)
- Microsoft.Bcl (NuGet)
- Microsoft.Bcl.Build (NuGet)
- [Websockets.Portable](https://github.com/NVentimiglia/WebSocket.Portable) 


### Usage



	string applicationKey = "myApplicationKey";
	string authenticationToken = "myAuthenticationToken";
	 
	// Create ORTC client
	OrtcClient ortcClient = new RealtimeFramework.Messaging.OrtcClient();
	ortcClient.ClusterUrl = "http://ortc-developers.realtime.co/server/2.1/";
	 
	// Ortc client handlers
	ortcClient.OnConnected += new OnConnectedDelegate(ortc_OnConnected);
	ortcClient.OnSubscribed += new OnSubscribedDelegate(ortc_OnSubscribed);
	ortcClient.OnException += new OnExceptionDelegate(ortc_OnException);
	 
	ortcClient.connect(applicationKey, authenticationToken);
	 
	private void ortc_OnConnected(object sender)
	{
	    ortcClient.Subscribe("channel1", true, OnMessageCallback);
	}
	 
	private void OnMessageCallback(object sender, string channel, string message)
	{
	    System.Diagnostics.Debug.Writeline("Message received: " + message);
	}
	private void ortc_OnSubscribed(object sender, string channel)
	{
	    ortcClient.Send(channel, "your message");
	}
	 
	private void ortc_OnException(object sender, Exception ex)
	{
	    System.Diagnostics.Debug.Writeline(ex.Message);
	}


### Author
Realtime and [Nicholas Ventimiglia](https://github.com/NVentimiglia)

