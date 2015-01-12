using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model.Chats
{
    public class ConnectcastChat : ChatBase
    {
        public ConnectcastChat(ChatConfig config)
            : base(config)
        {
            EmoticonFallbackUrl = @"Content\dummy.html";
            EmoticonUrl = "http://dummy.com";

            CreateChannel = () => { return new ConnectcastChannel(this); };

            ReceiveOwnMessages = true;

            //ContentParsers.Add(MessageParser.ParseURLs);
            //ContentParsers.Add(MessageParser.ParseEmoticons);

            //IStreamTopic 

            //Info = new StreamInfo()            
            //{
            //    HasDescription = false,
            //    HasGame = true,
            //    HasTopic = true,
            //    ChatName = Config.ChatName,
            //};


            //Games = new ObservableCollection<Game>();

        }

        public override void DownloadEmoticons(string url)
        {

        }

        public override bool Login()
        {
            return true;
        }

    }

    public class ConnectcastChannel : ChatChannelBase
    {
        private const int PING_INTERVAL = 25000;
        private object pollerLock = new object();
        private WebPoller statsPoller;
        private WebSocketBase webSocket;
        private Timer pingTimer;
        private Random random = new Random();
        private Dictionary<string, Action<ConnectcastChannel, dynamic>> packetHandlers = 
            new Dictionary<string, Action<ConnectcastChannel, dynamic>>() {
                {"3probe", ConnectHandler},
                {"42", DataHandler},
                {"3", PongHandler},
            };

        private static void PongHandler(ConnectcastChannel channel, dynamic data)
        {
            Log.WriteInfo("Goodgame pong received");
        }
        private static void DataHandler(ConnectcastChannel channel, dynamic data)
        {
            
        }
        private static void ConnectHandler(ConnectcastChannel channel, dynamic data)
        {
            channel.webSocket.Send("5");
            channel.webSocket.Send(
                String.Format( @"42[""join"",{""room"":""{0}"",""token"":""NOTOKEN""}]", 
                    channel.ChannelName.Replace("#","")
                ));
        }
        public ConnectcastChannel(IChat chat)
        {
            pingTimer = new Timer((obj) =>
            {
                SendPing();
            }, this, Timeout.Infinite, Timeout.Infinite);

            Chat = chat;
        }

        public override void Leave()
        {
            //Disconnect channel

            if( webSocket != null )
                webSocket.Disconnect();

            webSocket = null;

        }

        public override void SendMessage(ChatMessage message)
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            //Send message
        }

        public override void Join(Action<IChatChannel> callback, string channel)
        {
            ChannelName = "#" + channel.Replace("#", "");

            string sid = String.Empty;
            using( WebClientBase webClient = new WebClientBase())
            {
                sid = this.With(x => webClient.Download( 
                    String.Format("https://chat.connectcast.tv:3000/socket.io/?EIO=3&transport=polling&t=",Time.UnixTimestamp() )))
                    .With( x => Re.GetSubString(x, @"""sid"":""(.*?)"""));
            }
            
            if( String.IsNullOrWhiteSpace(sid) && LeaveCallback != null )
            {
                LeaveCallback(this);
                return;
            }

            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            
            JoinCallback = callback;
                       
            webSocket.DisconnectHandler = () =>
            {
                pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };

            webSocket.ReceiveMessageHandler = ReadRawMessage;

            webSocket.Path = String.Format("/socket.io/?EIO=3&transport=websocket&sid={0}", sid);
            webSocket.Port = "3000";
            webSocket.Host = "chat.connectcast.tv";
            webSocket.ConnectHandler = () =>
            {
                pingTimer.Change(PING_INTERVAL, PING_INTERVAL);
                webSocket.Send("2probe");
            };

            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            var command = Re.GetSubString(rawMessage, @"(.*?)\[");
            var data = this.With( x => Re.GetSubString(rawMessage,@".*?(\[.*)"))
                .With( x => JsonUtil.ParseArray(x));
                
            if (String.IsNullOrWhiteSpace(command) || data != null )
                return;

            if( packetHandlers.ContainsKey(command))
                packetHandlers[command](this, data );
        }
        private void SendPing()
        {

        }
    }

}
