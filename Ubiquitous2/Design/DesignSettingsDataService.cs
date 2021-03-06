﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Design
{
    public class DesignSettingsDataService :ISettingsDataService
    {
        public void GetChatSettings(Action<List<ChatConfig>> callback)
        {
            callback(new List<ChatConfig>() {                 
                       new ChatConfig() { ChatName = "LoremIpsum.tv", IconURL = Icons.DesignMainIcon, Enabled = true, Parameters = new List<ConfigField>() {
                        new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "loremuser" }
                       }},
                    });
        }


        public void GetRandomChatSetting(Action<ChatConfig> callback)
        {
            callback(
                new ChatConfig()
                {
                    ChatName = "LoremIpsum.tv",
                    Enabled = true,
                    Parameters = new List<ConfigField>() {
                        new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "loremuser" }
                       }
                });

        }


        public void GetRandomTextField(Action<ConfigField> callback)
        {
            callback(
                 new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "loremuser" }
            );
        }


        public void GetServiceSettings(Action<List<ServiceConfig>> callback)
        {
            throw new NotImplementedException();
        }


        public void GetAppSettings(Action<AppConfig> callback)
        {
            callback(new AppConfig());
        }
    }
}
