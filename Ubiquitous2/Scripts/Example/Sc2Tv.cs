using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                new ConfigField() {  Name = "LastMessageId", Label = "Last message id", DataType = "Text", IsVisible = false, Value = String.Empty }
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
    private WebClientBase loginWebClient = new WebClientBase();
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

            var jsonEmoticons = this.With(x => loginWebClient.Download("http://chat.sc2tv.ru/js/smiles.js"))
                .With(x => Re.GetSubString(x, rePattern));

            if (jsonEmoticons == null)
            {
                Log.WriteError("Unable to get {0} emoticons!", ChatName);
                return;
            }
            else
            {
                var icons = Json.ParseArray(jsonEmoticons);
                if (icons == null)
                    return;

                foreach( var icon in icons )
                {
                    var code = (string)icon["code"];                    
                    var img = (string)icon["img"];

                    if (String.IsNullOrWhiteSpace(code) || String.IsNullOrWhiteSpace(img))
                        continue;

                    var width = (int?)icon["width"];
                    var height = (int?)icon["height"];                    

                    if (width == null)
                        width = 30;
                    if (height == null)
                        height = 30;

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
        IsAnonymous = true;
        return true;
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
    private string lastTime = null;
    private Random random = new Random();

    public Sc2TvChannel(IChat chat)
    {
        Chat = chat;
    }

    public override void Leave()
    {
        if (chatPoller != null)
            chatPoller.Stop();
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
        if (String.IsNullOrWhiteSpace(channel))
            return;
        GetStreamId();
        SetupPollers();
        JoinCallback = callback;
    }
    public void GetStreamId()
    {
        channelId = "226459";
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
                IsAnonymous = false,
                KeepAlive = false,
                IsTimeStamped = true,
            };
            chatPoller.ReadStream = (stream) =>
            {
                if( stream == null )
                    return;

                lock (chatLock)
                {
                    var messagesJson = Json.DeserializeStream<dynamic>(stream);
                    if (messagesJson == null)
                        return;

                    var messages = Json.Sort( Json.ParseArray(messagesJson.messages), "id");
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
            JoinCallback(this);
        }
    }
}


