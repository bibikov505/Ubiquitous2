﻿using System;
using System.Linq;
using UB.Model;

namespace UB.Design
{
    public class DesignDataService : IDataService
    {
        private Random rnd;
        public DesignDataService()
        {
            rnd = new Random();
        }

        public void GetMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;

            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);
            var text = String.Join(" ", Enumerable.Range(0, wordsCount).Select((i, str) => words[i]));

            var message = new ChatMessage(text) {
                FromUserName = "xedoc",
                ImageSource = @"c:\favicon.ico",
                Channel = "#loremipsum"
            };

            callback(message, null);
        }

        public void ReadMessages(Action<ChatMessage[],Exception> callback)
        {

        }
    }
}