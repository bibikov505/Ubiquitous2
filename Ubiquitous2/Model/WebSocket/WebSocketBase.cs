﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Utils;
using WebSocket4Net;

namespace UB.Model
{
    public class WebSocketBase
    {
        private WebSocket socket;

        public WebSocketBase()
        {
            Port = "80";
            PingInterval = 25000;
            SubProtocol = String.Empty;
            IsSecure = false;
        }
        public bool IsSecure
        {
            get;
            set;
        }
        public List<KeyValuePair<string, string>> Cookies
        {
            get;
            set;
        }
        public String Host
        {
            get;
            set;
        }
        public String Path
        {
            get;
            set;
        }
        public String Port
        {
            get;
            set;
        }
        public string SubProtocol { get; set; }
        public WebSocketState State { get { return socket.State;  } }
        public void Disconnect()
        {
            try
            {
                if( socket.State == WebSocketState.Open)
                    socket.Close();
            }
            catch{}
        }
        public string Origin { get; set; }
        public void Connect()
        {
            String url;

            if (String.IsNullOrEmpty(Port) ||
                String.IsNullOrEmpty(Host))
                return;

            string protocol = IsSecure ? "wss" : "ws";

            if( Port == "80")
            {
                url = String.Format("{0}://{1}{2}", protocol, Host, Path);
            }
            else
            {
                url = String.Format("{0}://{1}:{2}{3}", protocol, Host, Port, Path);
            }

            try
            {
                socket = new WebSocket(
                    url,
                    SubProtocol,
                    Cookies,
                    null,
                    "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:14.0) Gecko/20100101 Firefox/14.0.1",
                    String.IsNullOrWhiteSpace(Origin) ? "http://" + Host : Origin,
                    WebSocketVersion.DraftHybi10
                    );
                
                if (socket != null)
                {
                    if (PingInterval == 0)
                    {
                        socket.AutoSendPingInterval = 0;
                        socket.EnableAutoSendPing = false;
                    }
                    else
                    {
                        socket.AutoSendPingInterval = PingInterval;
                        socket.EnableAutoSendPing = true;
                    }
                }
                socket.Opened += new EventHandler(socket_Opened);
                socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(socket_MessageReceived);
                socket.Closed += new EventHandler(socket_Closed);
                socket.Error += socket_Error;
                socket.Open();
            }
            catch (Exception e)
            {
                Log.WriteError(String.Format("Websocket connection failed. {0} {1}", url, e.Message));
            }
        }

        void socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Log.WriteError("WebSocket error {0}", e.Exception.Message);

            if (DisconnectHandler != null)
            {
                DisconnectHandler();
                DisconnectHandler = null;
            }
        }

        public bool IsClosed { get {return socket == null || socket.State == WebSocketState.Closed;} }

        void socket_Closed(object sender, EventArgs e)
        {
            if (DisconnectHandler != null)
                DisconnectHandler();
        }
        
        public Action DisconnectHandler { get; set; }
        public Action ConnectHandler { get; set; }
        public Action<string> ReceiveMessageHandler { get; set; }

        public int PingInterval
        {
            get;
            set;
        }
        public void Send(string message)
        {
            try
            {
                socket.Send(message);
            }
            catch
            {
                Log.WriteError("Error sending message to websocket");
            }
        }
        void socket_Opened(object sender, EventArgs e)
        {
            if (ConnectHandler != null)
                ConnectHandler();
        }


        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (ReceiveMessageHandler != null)
                ReceiveMessageHandler(e.Message);
        }
    }
}
