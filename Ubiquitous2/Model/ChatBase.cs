﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    public class ChatBase : IChat
    {
        private object channelsLock = new object();
        private object toolTipLock = new object();
        private object pollerLock = new object();
        private object joinLock = new object();
        public event EventHandler<ChatServiceEventArgs> MessageReceived;

        public ChatBase(ChatConfig config)
        {
            Config = config;
            IconURL = config.IconURL;
            ChatName = config.ChatName;
            ContentParsers = new List<Action<ChatMessage, IChat>>();     
            ChatChannels = new List<IChatChannel>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();
            IsAnonymous = true;
            ReceiveOwnMessages = false;
            Enabled = Config.Enabled;
            IsChannelCaseSensitive = false;
            JoinByNickName = true;
        }
        public bool IsAnonymous { get; set; }
        public string EmoticonFallbackUrl { get; set; }
        public string EmoticonUrl { get; set; }

        public bool IsWebEmoticons { get; set; }
        public bool IsFallbackEmoticons { get; set; }
        public bool IsChannelCaseSensitive { get; set; }
        public bool JoinByNickName { get; set; }

        public string AnonymousNickName
        { 
            get; 
            set; 
        }

        public string ChatName
        {
            get;
            set;
        }

        public string IconURL
        {
            get;
            set;
        }

        public string NickName
        {
            get;
            set;
        }

        public bool HideViewersCounter
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public bool ReceiveOwnMessages
        {
            get;
            set;
        }

        public virtual bool Start()
        {
            if (Status.IsStarting || Status.IsConnected || Status.IsLoggedIn || Config == null)
            {
                return true;
            }
            Log.WriteInfo("Starting {0} chat", ChatName);

            lock( toolTipLock )
            {
                UI.Dispatch(() => Status.ToolTips.Clear());
            }

            Status.ResetToDefault();
            Status.IsStarting = true;

            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string ?? "";
            var username = Config.GetParameterValue("Username") as string ?? "";
            var password = Config.GetParameterValue("Password") as string ?? "";

            if (tokenCredentials != username + password )
                Config.SetParameterValue("AuthTokenCredentials", null);

            try
            {
                if (!Login())
                    IsAnonymous = true;                    
            }
            catch( Exception e)
            {
                Log.WriteInfo("{0} authorization exception {1}", ChatName, e.Message);
                IsAnonymous = false;
            }

            Status.IsConnecting = true;
            Task.Factory.StartNew(() => JoinChannels());
            InitEmoticons();


            return false;
        }

        public virtual void StopCounterPoller(string channelName)
        {
            UI.Dispatch(() =>
            {
                lock (toolTipLock)
                    Status.ToolTips.RemoveAll(t => t.Header == channelName);
            });

        }

        public void ReadMessage(ChatMessage message)
        {
            if (MessageReceived != null)
            {
                var original = message.Text;
                Log.WriteInfo("Original string:{0}", message.Text);
                if (ContentParsers != null)
                {
                    var number = 1;
                    if( !message.IsParsed )
                    {
                        ContentParsers.ForEach(parser =>
                        {

                            parser(message, this);
                            if (original != message.Text)
                                Log.WriteInfo("After parsing with {0}: {1}", number, message.Text);
                            number++;
                        });
                        message.IsParsed = true;
                    }
                }
                if (message.FromUserName.Equals(NickName, StringComparison.InvariantCultureIgnoreCase))
                    message.IsSentByMe = true;

                MessageReceived(this, new ChatServiceEventArgs() { Message = message });

            }
        }
        public virtual bool Stop()
        {
            if (!Enabled)
                Status.ResetToDefault();

            if (Status.IsStopping)
                return false;

            Log.WriteInfo("Stopping {0} chat", ChatName);

            Status.IsStopping = true;
            Status.IsStarting = false;

            lock( channelsLock )
            {
                ChatChannels.ToList().ForEach(chan =>
                {
                    chan.Leave();
                    if (chan.LeaveCallback != null)
                        chan.LeaveCallback(chan);

                    if (RemoveChannel != null)
                        RemoveChannel(chan.ChannelName, this);
                });
            }

            lock(toolTipLock)
            {
                UI.Dispatch(() => {
                    Status.ViewersCount = 0;
                    Status.MessagesCount = 0;
                    lock( toolTipLock)
                        Status.ToolTips.Clear();
                });
            }


            Thread.Sleep(1000);
            return true;
        }

        public virtual bool Restart()
        {
            if (Status.IsStopping || Status.IsStarting)
                return false;

            Status.ResetToDefault();
            Stop();
            Start();
            return true;
        } 

        public virtual bool SendMessage(ChatMessage message)
        {
            if (IsAnonymous)
                return false;

            lock( channelsLock)
            {
                this.With(x => ChatChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase)))
                    .Do(x =>
                    {
                        if (String.IsNullOrWhiteSpace(message.FromUserName))
                        {
                            message.FromUserName = NickName;
                        }
                        Task.Factory.StartNew(() => x.SendMessage(message));
                        if( ReceiveOwnMessages)
                            ReadMessage(message);
                    });
            }

            return true;
        }

        public Dictionary<string, ChatUser> Users
        {
            get;
            set;
        }

        public List<string> ChatChannelNames
        {
            get;
            set;
        }

        public Action<string, IChat> AddChannel
        {
            get;
            set;
        }

        public Action<string, IChat> RemoveChannel
        {
            get;
            set;
        }

        public Func<string, object> RequestData
        {
            get;
            set;
        }

        public List<Action<ChatMessage, IChat>> ContentParsers
        {
            get;
            set;
        }

        public List<Emoticon> Emoticons
        {
            get;
            set;
        }

        public virtual void DownloadEmoticons(string url)
        {
           
        }

        public ChatConfig Config
        {
            get;
            set;
        }

        public StatusBase Status
        {
            get;
            set;
        }

        public void UpdateStats()
        {
            lock (toolTipLock)
            {
                UI.Dispatch(() =>
                {
                    lock( channelsLock )
                    {
                        var channels = ChatChannels.ToList();

                        Status.ViewersCount = channels.Sum(channel => channel.ChannelStats.ViewersCount);                    
                        Status.MessagesCount = channels.Sum(channel => channel.ChannelStats.MessagesCount);

                        channels.ForEach(channel =>
                        {
                            this.With(x => Status.ToolTips.ToList().FirstOrDefault(tooltip => tooltip.Header.Equals(channel.ChannelName)))
                                .Do(x =>
                                {
                                    x.Number = channel.ChannelStats.ViewersCount;
                                    x.Text = channel.ChannelStats.ViewersCount.ToString();
                                });

                        });
                    }

                });
            }
        }

        public virtual void JoinChannels()
        {
            if (Status.IsStopping || CreateChannel == null)
                return;

            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + 
                (!IsChannelCaseSensitive ? chan.ToLower() : chan).Replace("#", "")                
                ).ToArray();

            if (!String.IsNullOrWhiteSpace(NickName) && 
                !String.IsNullOrWhiteSpace( Config.GetParameterValue("Username") as string ))
            {
                if (!channels.Contains("#" + (!IsChannelCaseSensitive ? NickName.ToLower() : NickName)))
                    channels = channels.Union(new String[] { (!IsChannelCaseSensitive ? NickName.ToLower(): NickName) }).ToArray();
            }

            foreach (var channel in channels)
            {
                var chatChannel = CreateChannel();
                chatChannel.ReadMessage = ReadMessage;
                chatChannel.LeaveCallback = (leaveChannel) =>
                {
                    Log.WriteInfo("{0} leaving channel {1}", Config.ChatName, channel);

                    leaveChannel.ChannelStats.ViewersCount = 0;
                    leaveChannel.Chat.UpdateStats();

                    if (Status.IsLoginFailed)
                        IsAnonymous = true;

                    lock (toolTipLock)
                    {
                        UI.Dispatch(() => Status.ToolTips.RemoveAll(tooltip => tooltip.Header == channel));
                    }

                    lock (channelsLock)
                    {
                        ChatChannels.RemoveAll(chan => chan == null);
                        ChatChannels.RemoveAll(item => item.ChannelName == leaveChannel.ChannelName);
                    }

                    if (RemoveChannel != null)
                        RemoveChannel(leaveChannel.ChannelName, this);

                    if (Status.IsStopping)
                        return;

                    lock( channelsLock  )
                    {
                        if (ChatChannels.Count <= 0)
                        {
                            Status.ResetToDefault();
                            Status.IsConnected = false;
                            Status.IsConnecting = true;
                        }
                    }
                    Thread.Sleep(1000);
                    lock( joinLock )
                        JoinChannel(chatChannel, channel);
                };
                lock (joinLock)
                    Task.Run( () => JoinChannel(chatChannel, channel));
            }

        }

        private void JoinChannel( IChatChannel chatChannel, string channel )
        {
            lock( channelsLock )
            {
                if (!ChatChannels.Any(c => c.ChannelName == channel) && 
                    !channel.Replace("#","").Equals(AnonymousNickName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.WriteInfo("{0} joining {1}", Config.ChatName, channel);

                    if (RemoveChannel != null)
                        RemoveChannel(chatChannel.ChannelName, this);

                    ChatChannels.Add(chatChannel);

                    chatChannel.Join((joinChannel) =>
                    {
                        lock( joinLock )
                        {
                            Log.WriteInfo("{0} joined {1}", Config.ChatName, channel);

                            UpdateStats();
                            if (AddChannel != null)
                                AddChannel(joinChannel.ChannelName, this);

                            if (Status.IsStopping)
                                return;
                        
                            lock (toolTipLock)
                            {
                                if (!Status.ToolTips.ToList().Any(t => t.Header == channel))
                                    UI.Dispatch(() => Status.ToolTips.Add(new ToolTip(channel, joinChannel.ChannelStats.ViewersCount.ToString())));
                            }
                            Status.IsConnecting = false;
                            Status.IsConnected = true;
                        }
                    }, channel);
                }
            }
        }
        public virtual bool Login()
        {
            return true;
        }

        public virtual bool InitEmoticons()
        {
            //Fallback icon list
            if( !String.IsNullOrWhiteSpace( EmoticonFallbackUrl ))
                DownloadEmoticons(EmoticonFallbackUrl.Contains(@":\") ? EmoticonFallbackUrl : AppDomain.CurrentDomain.BaseDirectory + EmoticonFallbackUrl);
            //Web icons
            if( !String.IsNullOrWhiteSpace( EmoticonUrl))
                Task.Factory.StartNew(() => DownloadEmoticons(EmoticonUrl));
            return true;
        }


        public Func<IChatChannel> CreateChannel
        {
            get;
            set;
        }


        public List<IChatChannel> ChatChannels
        {
            get;
            set;
        }
    }
}
