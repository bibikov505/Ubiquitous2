﻿using System;
using UB.Model.IRC;
using UB.Utils;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase
    {
        private object iconLock = new object();
        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo()
        {
            Channels = config.Parameters.StringArrayValue("Channels"),
            HostName = "irc.twitch.tv",
            UserName = config.Parameters.StringValue("Username"),
            Password = config.Parameters.StringValue("Password"),
            Port = 6667,
            RealName = config.Parameters.StringValue("Username"),
            })
        {
            Enabled = config.Enabled;
            ContentParser = contentParser;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            //Fallback icons
            DownloadEmoticons(baseDir + @"Content\twitchemoticons.json");
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons("http://api.twitch.tv/kraken/chat/emoticons") );
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

        }
        public override string IconURL
        {
            get
            {
                return @"/favicon.ico";
            }
        }
        public override string ChatName 
        { 
            get 
            { 
                return "Twitch.tv"; 
            } 
        }

        void contentParser(ChatMessage message)
        {
            //Parse links
            message.Text = Html.ConvertUrlsToLinks(message.Text);

            //Parse emoticons
            lock (iconLock)
            {
                foreach (var emoticon in Emoticons)
                {
                    if (emoticon.Pattern != null)
                        message.Text = Regex.Replace(message.Text, emoticon.Pattern, emoticon.HtmlCode);
                }
            }
        }

        public override void DownloadEmoticons(string url)
        {

            var list = new List<Emoticon>();

            using (var wc = new WebClientBase())
            {
                var jsonEmoticons = this.With(x => wc.Download(url))
                    .With(x => JToken.Parse(x))
                    .With(x => x.SelectToken("emoticons"))
                    .With(x => x.ToObject<JArray>());

                if (jsonEmoticons == null)
                {
                    Debug.Print("Error getting Twitch.tv emoticons!");
                    list = new List<Emoticon>();
                }
                else
                {
                    foreach (dynamic icon in jsonEmoticons.Children())
                    {
                        if (icon != null && icon.images != null && icon.regex != null)
                        {
                            string regex = (string)icon.regex;
                            JArray images = icon.images as JArray;
                            dynamic image = this.With(x => (JArray)icon.images).With(x => (dynamic)x.First);

                            if (image != null && image.width != null && image.height != null && image.url != null)
                            {
                                list.Add(new Emoticon(regex, (string)image.url, (int)image.width, (int)image.height));
                            }

                        }

                    }
                }
            }

            lock (iconLock)
                Emoticons = list;
        }


    }

}
