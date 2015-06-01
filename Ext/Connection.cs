using System;
using System.Threading.Tasks;
using RealtimeFramework.Messaging.Exceptions;
using WebSocket.Portable;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;

namespace RealtimeFramework.Messaging.Ext
{
    public class Connection
    {

        #region Events (4)

        public event Action OnOpened;
        public event Action OnClosed;
        public event Action<Exception> OnError;
        public event Action<string> OnMessageReceived;

        #endregion

        #region Attributes

        private WebSocketClient _websocket = null;

        #endregion

        public Task NewConnection()
        {
            return Task.Factory.StartNew(() =>
             {
                 if (_websocket != null)
                 {
                     _websocket.Closed -= client_Closed;
                     _websocket.Error -= client_Error;
                     _websocket.MessageReceived -= client_MessageReceived;
                     _websocket.Opened -= client_Opened;
                     _websocket.Dispose();
                     _websocket = null;
                 }

                 _websocket = new WebSocketClient();
                 _websocket.Closed += client_Closed;
                 _websocket.Error += client_Error;
                 _websocket.MessageReceived += client_MessageReceived;
                 _websocket.Opened += client_Opened;
             });
        }

        void OnLog(LogLevel l, string m)
        {
            System.Diagnostics.Debug.WriteLine(m);
        }

        public async void Connect(string url)
        {
            //System.Diagnostics.Debug.WriteLine("Connection.Connect");
            Uri uri = null;
            var connectionId = Strings.RandomString(8);
            var serverId = Strings.RandomNumber(1, 1000);

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                throw new OrtcEmptyFieldException(String.Format("Invalid URL: {0}", url));
            }

            var prefix = uri != null && "https".Equals(uri.Scheme) ? "wss" : "ws";
            var connectionUrl = String.Format("{0}://{1}:{2}/broadcast/{3}/{4}/websocket", prefix, uri.DnsSafeHost, uri.Port, serverId, connectionId);

            try
            {
                await NewConnection();
                await _websocket.OpenAsync(connectionUrl);
            }
            catch (Exception ex)
            {
                var ev = OnError;
                if (ev != null)
                {
                    ev(new OrtcNotConnectedException("Websocket has encountered an error.", ex));
                }
            }
        }

        public async void Close()
        {
            //System.Diagnostics.Debug.WriteLine("Connection.Close");
            if (_websocket != null)
                await _websocket.CloseAsync();
        }

        public void Send(string message)
        {
            if (_websocket == null)
                return;

            try
            {
                message = "\"" + message + "\"";
                _websocket.SendAsync(message);
            }
            catch
            {

                var ev = OnError;
                if (ev != null)
                {
                    Task.Factory.StartNew(() => ev(new OrtcNotConnectedException("Unable to write to socket.")));
                }
            }
        }

        private void client_Opened()
        {
            Task.Factory.StartNew(() =>
            {
                var ev = OnOpened;
                if (ev != null)
                {
                    ev();
                }
            });
        }

        private void client_MessageReceived(IWebSocketMessage obj)
        {
            //System.Diagnostics.Debug.WriteLine("Connection.client_MessageReceived " + obj);

            if (!obj.IsComplete)
                return;

            Task.Factory.StartNew(() =>
            {
                var ev = OnMessageReceived;
                if (ev != null)
                {
                    ev(obj.ToString());
                }
            });
        }

        private void client_Error(Exception obj)
        {
            //System.Diagnostics.Debug.WriteLine("Connection.client_Error " + obj);

            Task.Factory.StartNew(() =>
            {
                var ev = OnError;
                if (ev != null)
                {
                    ev(obj);
                }
            });
        }

        private void client_Closed()
        {
            //System.Diagnostics.Debug.WriteLine("Connection.client_Closed" );

            if (_websocket != null)
            {
                _websocket.Closed -= client_Closed;
                _websocket.Error -= client_Error;
                _websocket.MessageReceived -= client_MessageReceived;
                _websocket.Opened -= client_Opened;

                _websocket.Dispose();
                _websocket = null;
            }

            Task.Factory.StartNew(() =>
            {
                var ev = OnClosed;
                if (ev != null)
                {
                    ev();
                }
            });
        }

    }
}
