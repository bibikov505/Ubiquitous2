using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UB.Model;
using UB.Utils;


public class Sc2TvScript : IScript
{
    //Chat config
    public object OnConfigRequest()
    {
        return new ChatConfig()
        {
            ChatName = "Sc2tv.ru", // Unique chat name           
            IconURL = AppDomain.CurrentDomain.GetData("DataDirectory") + @"\Scripts\Example\Sc2Tv.png", // Icon path
            Parameters = new List<ConfigField>()
            {
                new ConfigField() {  Name = "LastMessageId", Label = "Last message id", DataType = "Text", IsVisible = false, Value = String.Empty },
                new ConfigField() {  Name = "Cookies", Label = "Login cookies", DataType = "Text", IsVisible = false, Value = String.Empty }
            }
        };
    }
    //Chat object creation
    public object OnObjectRequest(object config)
    {
        //Return instance
        return new Sc2TvChat(config as ChatConfig);
    }
}

//Chat implementation
public class Sc2TvChat : ChatBase
{
    private object iconParseLock = new object();
    public Sc2TvChat(ChatConfig config)
        : base(config)
    {
        EmoticonUrl = "http://chat.sc2tv.ru/js/smiles.js";
        EmoticonFallbackUrl = AppDomain.CurrentDomain.GetData("DataDirectory") + @"\Scripts\Example\Sc2TvSmilesFallback.js";

        CreateChannel = () => { return new Sc2TvChannel(this); };

        ReceiveOwnMessages = false;
        
        ContentParsers.Add(MessageParser.RemoveBBCode);
        ContentParsers.Add(MessageParser.UnescapeHtml);
        ContentParsers.Add(MessageParser.ParseURLs);
        ContentParsers.Add(MessageParser.ParseEmoticons);

        LoginWebClient = new WebClientBase();
    }
    public WebClientBase LoginWebClient
    {
        get;
        set;
    }
    public override void DownloadEmoticons(string url)
    {
        var rePattern = @"smiles[\s|=]*(\[.*?\]);";

        if (IsFallbackEmoticons && IsWebEmoticons)
            return;

        lock (iconParseLock)
        {
            var list = new List<Emoticon>();
            if (Emoticons == null)
                Emoticons = new List<Emoticon>();

            var jsonEmoticons = this.With(x => LoginWebClient.Download(url))
                .With(x => Re.GetSubString(x, rePattern));

            if (jsonEmoticons == null)
            {
                Log.WriteError("Unable to get {0} emoticons!", ChatName);
                return;
            }
            else
            {
                var icons = JsonUtil.ParseArray(jsonEmoticons);
                if (icons == null)
                    return;

                foreach( var icon in icons )
                {
                    var code = (string)icon["code"];                    
                    var img = (string)icon["img"];

                    if (String.IsNullOrWhiteSpace(code) || String.IsNullOrWhiteSpace(img))
                        continue;

                    var width = (int?)icon["width"] ?? 30;
                    var height = (int?)icon["height"] ?? 30;                    

                    var smileUrl = "http://chat.sc2tv.ru/img/" + img;
                    list.Add(new Emoticon(":s" + code,smileUrl,(int)width,(int)height));

                }
                if (list.Count > 0)
                {
                    Emoticons = list.ToList();
                    if (IsFallbackEmoticons)
                        IsWebEmoticons = true;

                    IsFallbackEmoticons = true;
                }
            }
        }

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

        var userInfo = this.With(x => LoginWebClient.Download("http://chat.sc2tv.ru/gate.php?task=GetUserInfo"))
            .With(x => JsonUtil.FromJson<dynamic>(x)) ?? 0;

        if (userInfo["uid"] == 0)
            return false;

        LoginWebClient.SetCookie("chat_token", (string)userInfo["token"], "chat.sc2tv.ru");
        IsAnonymous = false;

        return true;
    }

    public bool LoginWithUsername()
    {
        var indexPage = LoginWebClient.Download("http://sc2tv.ru");
        if (String.IsNullOrWhiteSpace(indexPage))
            return false;

        var username = Config.GetParameterValue("Username") as string;
        var password = Config.GetParameterValue("Password") as string;

        if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) )
        {
            IsAnonymous = true;
            return true;
        }

        var formBuildId = Re.GetSubString(indexPage, @"form_build_id.*?value=""(.*?)""");
        var formId = Re.GetSubString(indexPage, @"form_id.*?value=""(.*?)""");
        if (String.IsNullOrWhiteSpace(formBuildId) || String.IsNullOrWhiteSpace(formId) ||
            String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            return false;

        LoginWebClient.ContentType = ContentType.UrlEncodedUTF8;
        LoginWebClient.Upload("http://sc2tv.ru/node", 
            String.Format("name={0}&pass={1}&form_build_id={2}&form_id={3}",
            Url.Encode(username), Url.Encode(password) ,formBuildId, formId));

        if (String.IsNullOrEmpty(LoginWebClient.CookieValue("drupal_uid", "http://sc2tv.ru")))
            return false;

        Config.SetParameterValue("Cookies", JsonUtil.ToJson(LoginWebClient.CookiesTable));
        Config.SetParameterValue("AuthTokenCredentials", username + password);

        return LoginWithCookies();
    }

}

//Chat channel implementation
public class Sc2TvChannel : ChatChannelBase
{
    private object pollerLock = new object();
    private object chatLock = new object();
    private WebPoller chatPoller;
    private long lastMessageId = 0;
    private string channelId = null;
    private Random random = new Random();

    public Sc2TvChannel(IChat chat)
    {
        Chat = chat;
    }

    public override void Leave()
    {
        if (chatPoller != null)
            chatPoller.Stop();

        if (LeaveCallback != null)
            LeaveCallback(this);
    }

    public override void SendMessage(ChatMessage message)
    {
        if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
            String.IsNullOrWhiteSpace(message.FromUserName) ||
            String.IsNullOrWhiteSpace(message.Text))
            return;

        var webClient = (Chat as Sc2TvChat).LoginWebClient;
        if (webClient == null || String.IsNullOrWhiteSpace(channelId) )
            return;

        var chatToken = webClient.CookieValue("chat_token", "http://chat.sc2tv.ru");
        if (String.IsNullOrWhiteSpace(chatToken))
            return;

        var postData = String.Format("task=WriteMessage&message={0}&channel_id={1}&token={2}", 
            Url.Encode(message.Text), 
            channelId,
            chatToken);

        webClient.ContentType = ContentType.UrlEncodedUTF8;
        webClient.Upload("http://chat.sc2tv.ru/gate.php", postData);

        //Send message
    }

    public override void Join(Action<IChatChannel> callback, string channel)
    {
        ChannelName = "#" + channel.Replace("#", "");
        if (String.IsNullOrWhiteSpace(channel))
            return;
        GetStreamId();
        SetupPollers();
        JoinCallback = callback;
    }
    public void GetStreamId()
    {
       using( WebClientBase webClient = new WebClientBase())
       {
           var channelPage = webClient.Download( String.Format( "http://sc2tv.ru/channel/{0}", ChannelName.Replace("#","") ));
           channelId = Re.GetSubString(channelPage, @"channelId=(\d+)");
       }
    }
    public void SetupPollers()
    {
        var oldMessageId = Chat.Config.GetParameterValue("LastMessageId") as long?;
        if (oldMessageId != null)
            lastMessageId = (long)oldMessageId;

        if (!String.IsNullOrWhiteSpace(channelId))
        {
            #region Chatpoller
            chatPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://chat.sc2tv.ru/memfs/channel-{0}.json", channelId)),
                IsLongPoll = false,
                Interval = 5000,
                TimeoutMs = 10000,
                TimeoutOnError = false,
                IsAnonymous = false,
                KeepAlive = false,
                IsTimeStamped = true,      
            };
            chatPoller.ReadStream = (stream) =>
            {

                if( !Chat.Status.IsConnected )
                {
                    Chat.Status.IsConnected = true;
                    Chat.Status.IsLoggedIn = !Chat.IsAnonymous;
                    JoinCallback(this);
                }

                if( stream == null )
                    return;

                lock (chatLock)
                {
                    var messagesJson = JsonUtil.DeserializeStream<dynamic>(stream);
                    if (messagesJson == null)
                        return;

                    var messages = JsonUtil.Sort( JsonUtil.ParseArray(messagesJson.messages), "id");
                    if (messages == null)
                        return; 

                    foreach( var message in messages )
                    {
                        var messageId = (long?)message["id"];
                        var userName = (string)message["name"];
                        var text = (string)message["message"];
                        var date = (string)message["date"];
                        var chanId = (long?)message["channelId"];

                        if (messageId == null ||
                            String.IsNullOrEmpty(userName) ||
                            String.IsNullOrEmpty(text))
                            return;

                        if (lastMessageId >= messageId)
                            continue;

                        lastMessageId = (long)messageId;

                        Chat.Config.SetParameterValue("LastMessageId", lastMessageId);

                        if (ReadMessage != null)
                            ReadMessage(new ChatMessage()
                            {
                                Channel = ChannelName,
                                ChatIconURL = Chat.IconURL,
                                ChatName = Chat.ChatName,
                                FromUserName = userName,
                                HighlyImportant = false,
                                IsSentByMe = false,
                                Text = text,
                            });
                        Chat.UpdateStats();
                        ChannelStats.MessagesCount++;
                    }

                }
            };
            chatPoller.Start();
            #endregion

            ChannelStats.ViewersCount = 0;
            Chat.UpdateStats();
        }
    }
}


