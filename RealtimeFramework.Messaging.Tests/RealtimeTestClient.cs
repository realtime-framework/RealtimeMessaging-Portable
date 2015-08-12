using System;
using System.Diagnostics;
using RealtimeFramework.Messaging.Exceptions;
using RealtimeFramework.Messaging.Ext;

namespace RealtimeFramework.Messaging.Tests
{
    public class RealtimeTestClient
    {
        public const string AuthToken = "12345";
        public const string AppKey = "";
        public const string PrivateKey = "";
        public const string ClusterUrl = "http://ortc-developers.realtime.co/server/2.1/";
        public const string ClusterUrlSSL = "https://ortc-developers.realtime.co/server/2.1/";

        public OrtcClient client;
        public Stopwatch Watch = new Stopwatch();

        public RealtimeTestClient()
        {
            client = new OrtcClient();
            client.ClusterUrl = ClusterUrlSSL;
            client.ConnectionMetadata = "Xamarin-" + new Random().Next(1000);
            client.HeartbeatTime = 2;

            client.OnConnected += client_OnConnected;
            client.OnDisconnected += client_OnDisconnected;
            client.OnException += client_OnException;
            client.OnReconnected += client_OnReconnected;
            client.OnReconnecting += client_OnReconnecting;
            client.OnSubscribed += client_OnSubscribed;
            client.OnUnsubscribed += client_OnUnsubscribed;
        }

        #region handlers
        void client_OnUnsubscribed(object sender, string channel)
        {
            Success(string.Format("Unsubscribed from {0}", channel));
        }

        void client_OnSubscribed(object sender, string channel)
        {
            Success(string.Format("Subscribed to {0}", channel));
        }

        void client_OnReconnecting(object sender)
        {
            Warning("Reconnecting...");
        }

        void client_OnReconnected(object sender)
        {
            Success("Reconnected");
        }

        void client_OnException(object sender, Exception ex)
        {
            Error(ex.Message);
        }

        void client_OnDisconnected(object sender)
        {
            Warning("Disconnected");
        }

        void client_OnConnected(object sender)
        {

            Watch.Stop();
            Success("Connected");
        }

        void OnPressence(OrtcPresenceException e, Presence arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
                Success("Got Presence ! " + arg.Subscriptions + " Clients");
                foreach (var a in arg.Metadata)
                {
                    Write("Client : " + a.Key);
                }
            }
        }


        void OnEnablePressence(OrtcPresenceException e, string arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
                Success("Presence enabled " + arg);
            }
        }
        void OnDisablePressence(OrtcPresenceException e, string arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
                Success("Presence disabled " + arg);
            }
        }
        void OnMessage(object sender, string channel, string content)
        {
            Write(string.Format("{0} : {1}", channel, content));
        }

        #endregion

        #region write methods

        void Error(string message)
        {
            Write(message);
        }
        void Log(string message)
        {
            Write(message);
        }

        void Warning(string message)
        {
            Write(message);
        }

        void Success(string message)
        {
            Write(message);
        }

        void Write(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        #endregion

        public void Connect()
        {
            Watch.Start();
            Write("Connecting ");
            client.Connect(AppKey, AuthToken);
        }
        public void Disconnect()
        {
            client.Disconnect();
        }
    }
}