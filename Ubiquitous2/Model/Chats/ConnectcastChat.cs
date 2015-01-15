using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    public class ConnectcastChat : ChatBase
    {
        public ConnectcastChat(ChatConfig config)
            : base(config)
        {
            ReceiveOwnMessages = false;

            EmoticonFallbackUrl = @"Content\dummy.html";
            EmoticonUrl = "http://dummy.com";

            CreateChannel = () => { return new ConnectcastChannel(this); };
            LoginWebClient = new WebClientBase();

            //ContentParsers.Add(MessageParser.ParseURLs);
            //ContentParsers.Add(MessageParser.ParseEmoticons);
        }

        public override void DownloadEmoticons(string url)
        {

        }

        public override bool Login()
        {
            if (!LoginWithCookies())
            {
                if (!LoginWithUsername())
                {
                    Status.IsLoginFailed = true;
                    return false;
                }
            }
            return true;

        }

        public WebClientBase LoginWebClient
        {
            get;
            set;
        }
        public bool LoginWithCookies()
        {
            IsAnonymous = true;
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            NickName = userName;

            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            var cookies = Config.GetParameterValue("Cookies") as string;

            if (String.IsNullOrWhiteSpace(cookies))
                return false;

            LoginWebClient.CookiesTable = JsonUtil.FromJson<List<Cookie>>(cookies);

            LoginWebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            var messageInfo = this.With(x => LoginWebClient.Upload("http://connectcast.tv/me/messages", ""))
                .With(x => JsonUtil.FromJson<dynamic>(x));

            if (messageInfo == null || messageInfo["unseen"] == null)
                return false;

            Config.SetParameterValue("Cookies", JsonUtil.ToJson(LoginWebClient.CookiesTable));
            Config.SetParameterValue("AuthTokenCredentials", userName + password);

            IsAnonymous = false;

            return true;
        }

        public bool LoginWithUsername()
        {
            var username = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            Config.SetParameterValue("Cookies", null);

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                IsAnonymous = true;
                return true;
            }
            var postData = new MultipartPostData()
            {
                Params = new List<MultipartPostDataParam>()
                {
                    new MultipartPostDataParam("redirect_url", "http://connectcast.tv", MultipartPostDataParamType.Field),
                    new MultipartPostDataParam("user_email", username, MultipartPostDataParamType.Field),
                    new MultipartPostDataParam("user_password", password, MultipartPostDataParamType.Field),
                }
            };
            LoginWebClient.Download("http://connectcast.tv");

            LoginWebClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            var status = this.With(x => LoginWebClient.PostMultipart("http://connectcast.tv/login/process", postData.GetPostData(), postData.Boundary))
                .With(x => JsonUtil.FromJson<dynamic>(x));
            if (status == null || (string)status["status"] != "success")
                return false;

            Config.SetParameterValue("Cookies", JsonUtil.ToJson(LoginWebClient.CookiesTable));
            Config.SetParameterValue("AuthTokenCredentials", username + password);

            return LoginWithCookies();
        }
        


    }

    public class ConnectcastChannel : ChatChannelBase
    {
        private const int PING_INTERVAL = 24000;
        private object pollerLock = new object();
        private string channelToken = "NOTOKEN";
        private WebSocketBase webSocket;
        private WebPoller statsPoller = new WebPoller();
        private Timer disconnectTimer;
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
            if (channel.disconnectTimer != null)
                channel.disconnectTimer.Change(PING_INTERVAL + 1000, Timeout.Infinite);
        }
        private static void DataHandler(ConnectcastChannel channel, dynamic data)
        {
            if (data == null)
                return;

            string type = (string)data[0];
            
            if (String.IsNullOrWhiteSpace(type))
                return;

            if( type.StartsWith("update-chat", StringComparison.InvariantCultureIgnoreCase))
            {
                dynamic senderInfo = (dynamic)data[2];
                string room = (string)senderInfo["room"];
                
                if (String.IsNullOrWhiteSpace(room) ||
                    !room.Equals(channel.ChannelName.Replace("#", ""), StringComparison.InvariantCultureIgnoreCase))
                    return;              

                string name = (string)senderInfo["name"];
                
                string messageHtml = data[1];
                string text = Re.GetSubString(messageHtml, "<p>(.*)</p>");

                if (String.IsNullOrWhiteSpace(name) ||
                    String.IsNullOrWhiteSpace(text))
                    return;

                channel.ChannelStats.MessagesCount++;
                channel.Chat.UpdateStats();

                if (channel.ReadMessage != null)
                    channel.ReadMessage(new ChatMessage()
                    {
                        Channel = channel.ChannelName,
                        ChatIconURL = channel.Chat.IconURL,
                        ChatName = channel.Chat.ChatName,
                        FromUserName = name,
                        HighlyImportant = false,
                        IsSentByMe = false,
                        Text = text,
                    });

            }
        }
        private static void ConnectHandler(ConnectcastChannel channel, dynamic data)
        {
            channel.Chat.Status.IsConnected = true;
            channel.webSocket.Send("5");
            channel.webSocket.Send(
                String.Format( @"42[""join"",{{""room"":""{0}"",""token"":""{1}""}}]", 
                    channel.ChannelName.Replace("#",""),
                    channel.channelToken
                ));

            if (channel.JoinCallback != null)
                channel.JoinCallback(channel);

        }
        public ConnectcastChannel(IChat chat)
        {
            pingTimer = new Timer((obj) =>
            {
                SendPing();
            }, this, Timeout.Infinite, Timeout.Infinite);

            disconnectTimer = new Timer((obj) =>
            {
                Leave();
            },this, Timeout.Infinite, Timeout.Infinite);

            Chat = chat;
        }

        public override void Leave()
        {
            //Disconnect channel
            if (pingTimer != null)
                pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (disconnectTimer != null)
                disconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);

            pingTimer = null;
            disconnectTimer = null;

            if( webSocket != null )
                webSocket.Disconnect();

            if (statsPoller != null)
                statsPoller.Stop();

            webSocket = null;

            

            if (LeaveCallback != null)
                LeaveCallback(this);
        }

        public override void SendMessage(ChatMessage message)
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            webSocket.Send(String.Format(
                @"42[""send"",{{""room"":""{0}"",""msg"":""{1}"",""token"":""{2}""}}]", 
                ChannelName.Replace("#",""),
                message.Text,
                channelToken
                ));

            //Send message
        }

        public override void Join(Action<IChatChannel> callback, string channel)
        {
            ChannelName = "#" + channel.Replace("#", "");

            if (ChannelName.Contains("@"))
                return; 

            string sid = String.Empty;
            using( WebClientBase webClient = new WebClientBase())
            {
                webClient.Cookies = (Chat as ConnectcastChat ).LoginWebClient.Cookies;
                channelToken = this.With(x => webClient.Download(String.Format("http://connectcast.tv/chat/{0}", ChannelName.Replace("#", ""))))
                                .With(x => Re.GetSubString(x, "token[^']*'(.*?)'"));

                sid = this.With(x => webClient.Download( 
                    String.Format("https://chat.connectcast.tv:3000/socket.io/?EIO=3&transport=polling&t=",Time.UnixTimestamp() )))
                    .With( x => Re.GetSubString(x, @"""sid"":""(.*?)"""));

                if (String.IsNullOrWhiteSpace(channelToken) || 
                    channelToken.Equals("NOTOKEN",StringComparison.InvariantCultureIgnoreCase))
                    channelToken = "NOTOKEN";
                else
                    Chat.Status.IsLoggedIn = true;

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
                if( pingTimer != null )
                    pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                Leave();
            };

            webSocket.ReceiveMessageHandler = ReadRawMessage;

            webSocket.Path = String.Format("/socket.io/?EIO=3&transport=websocket&sid={0}", sid);
            webSocket.Port = "3000";
            webSocket.IsSecure = true;
            webSocket.Origin = "http://connectcast.tv";
            webSocket.Cookies = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>("io", sid)
            };
            webSocket.Host = "chat.connectcast.tv";
            webSocket.ConnectHandler = () =>
            {
                if( pingTimer != null )
                    pingTimer.Change(PING_INTERVAL, PING_INTERVAL);

                if (disconnectTimer != null)
                    disconnectTimer.Change(PING_INTERVAL * 2, Timeout.Infinite);

                webSocket.Send("2probe");
            };
            SetupStatsWatcher();
            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            Log.WriteInfo("Connectcast raw message received {0}", rawMessage);

            var command = Re.GetSubString(rawMessage, @"([^\[]*)");
            var data = this.With( x => Re.GetSubString(rawMessage,@".*?(\[.*)$"))
                .With( x => JsonUtil.ParseArray(x));
                
            if (String.IsNullOrWhiteSpace(command) )
                return;

            if( packetHandlers.ContainsKey(command))
                packetHandlers[command](this, data );
        }
        private void SendPing()
        {
            webSocket.Send("2");
        }

        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,     
                Method = "POST",
                Uri = new Uri(String.Format(@"http://connectcast.tv/channel/views?username={0}", ChannelName.Replace("#", ""))),
            };
            statsPoller.Headers["X-Requested-With"] = "XMLHttpRequest";
            statsPoller.ReadString = (stream) =>
            {
                lock (pollerLock)
                {
                    if (stream == null)
                        return;

                    var channelInfo = JsonUtil.FromJson<dynamic>(stream);

                    statsPoller.LastValue = channelInfo;
                    if (channelInfo != null && (int?)channelInfo["count"] != null)
                    {
                        ChannelStats.ViewersCount = (int)channelInfo["count"];
                        Chat.UpdateStats();
                    }
                }
            };
            statsPoller.Start();
        }
    }

}
