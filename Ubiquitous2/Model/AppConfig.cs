﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace UB.Model
{
    public class AppConfig :NotifyPropertyChangeBase
    {
        public AppConfig()
        {
            Parameters = new List<ConfigField>();
        }

        /// <summary>
        /// The <see cref="IsChannelListVisible" /> property's name.
        /// </summary>
        public const string IsChannelListVisiblePropertyName = "IsChannelListVisible";

        private bool _isChannelListVisible = true;

        /// <summary>
        /// Sets and gets the IsChannelListVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsChannelListVisible
        {
            get
            {
                return _isChannelListVisible;
            }

            set
            {
                if (_isChannelListVisible == value)
                {
                    return;
                }

                _isChannelListVisible = value;
                RaisePropertyChanged(IsChannelListVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsChatBoxChannelListEnabled" /> property's name.
        /// </summary>
        public const string IsChatBoxChannelListEnabledPropertyName = "IsChatBoxChannelListEnabled";

        private bool _isChatBoxChannelListEnabled = true;

        /// <summary>
        /// Sets and gets the IsChatBoxChannelListEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsChatBoxChannelListEnabled
        {
            get
            {
                return _isChatBoxChannelListEnabled;
            }

            set
            {
                if (_isChatBoxChannelListEnabled == value)
                {
                    return;
                }

                _isChatBoxChannelListEnabled = value;
                RaisePropertyChanged(IsChatBoxChannelListEnabledPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatBoxChannelListWidth" /> property's name.
        /// </summary>
        public const string ChatBoxChannelListWidthPropertyName = "ChatBoxChannelListWidth";

        private double _chatBoxChannelListWidth = 50;

        /// <summary>
        /// Sets and gets the ChatBoxChannelListWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ChatBoxChannelListWidth
        {
            get
            {
                return _chatBoxChannelListWidth;
            }

            set
            {
                if (_chatBoxChannelListWidth == value)
                {
                    return;
                }

                _chatBoxChannelListWidth = value;
                RaisePropertyChanged(ChatBoxChannelListWidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUserListVisible" /> property's name.
        /// </summary>
        public const string IsUserListVisiblePropertyName = "IsUserListVisible";

        private bool _isUserListVisible = false;

        /// <summary>
        /// Sets and gets the IsUserListVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUserListVisible
        {
            get
            {
                return _isUserListVisible;
            }

            set
            {
                if (_isUserListVisible == value)
                {
                    return;
                }

                _isUserListVisible = value;
                RaisePropertyChanged(IsUserListVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowTimestamp" /> property's name.
        /// </summary>
        public const string ShowTimestampPropertyName = "ShowTimestamp";

        private bool _showTimestamp = true;

        /// <summary>
        /// Sets and gets the ShowTimestamp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowTimestamp
        {
            get
            {
                return _showTimestamp;
            }

            set
            {
                if (_showTimestamp == value)
                {
                    return;
                }

                _showTimestamp = value;
                RaisePropertyChanged(ShowTimestampPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowChannel" /> property's name.
        /// </summary>
        public const string ShowChannelPropertyName = "ShowChannel";

        private bool _showChannel = false;

        /// <summary>
        /// Sets and gets the ShowChannel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowChannel
        {
            get
            {
                return _showChannel;
            }

            set
            {
                if (_showChannel == value)
                {
                    return;
                }

                _showChannel = value;
                RaisePropertyChanged(ShowChannelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ForceAutoScroll" /> property's name.
        /// </summary>
        public const string ForceAutoScrollPropertyName = "ForceAutoScroll";

        private bool _forceAutoScroll = false;

        /// <summary>
        /// Sets and gets the ForceAutoScroll property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ForceAutoScroll
        {
            get
            {
                return _forceAutoScroll;
            }

            set
            {
                if (_forceAutoScroll == value)
                {
                    return;
                }

                _forceAutoScroll = value;
                RaisePropertyChanged(ForceAutoScrollPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeTimestamp" /> property's name.
        /// </summary>
        public const string FontSizeTimestampPropertyName = "FontSizeTimestamp";

        private double _fontSizeTimestamp = 11;

        /// <summary>
        /// Sets and gets the FontSizeTimestamp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeTimestamp
        {
            get
            {
                return _fontSizeTimestamp;
            }

            set
            {
                if (_fontSizeTimestamp == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeTimestampPropertyName);
                _fontSizeTimestamp = value;
                RaisePropertyChanged(FontSizeTimestampPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeNickName" /> property's name.
        /// </summary>
        public const string FontSizeNickNamePropertyName = "FontSizeNickName";

        private double _fontSizeNickName = 11;

        /// <summary>
        /// Sets and gets the FontSizeNickName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeNickName
        {
            get
            {
                return _fontSizeNickName;
            }

            set
            {
                if (_fontSizeNickName == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeNickNamePropertyName);
                _fontSizeNickName = value;
                RaisePropertyChanged(FontSizeNickNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeChannel" /> property's name.
        /// </summary>
        public const string FontSizeChannelPropertyName = "FontSizeChannel";

        private double _fontSizeChannel = 11;

        /// <summary>
        /// Sets and gets the FontSizeChannel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeChannel
        {
            get
            {
                return _fontSizeChannel;
            }

            set
            {
                if (_fontSizeChannel == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeChannelPropertyName);
                _fontSizeChannel = value;
                RaisePropertyChanged(FontSizeChannelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeMessage" /> property's name.
        /// </summary>
        public const string FontSizeMessagePropertyName = "FontSizeMessage";

        private double _fontSizeMessage = 16;

        /// <summary>
        /// Sets and gets the FontSizeMessage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeMessage
        {
            get
            {
                return _fontSizeMessage;
            }

            set
            {
                if (_fontSizeMessage == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeMessagePropertyName);
                _fontSizeMessage = value;
                RaisePropertyChanged(FontSizeMessagePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsReplyBoxPersistent" /> property's name.
        /// </summary>
        public const string IsReplyBoxPersistentPropertyName = "IsReplyBoxPersistent";

        private bool _isReplyBoxPersitent = false;

        /// <summary>
        /// Sets and gets the IsReplyBoxPersistent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsReplyBoxPersistent
        {
            get
            {
                return _isReplyBoxPersitent;
            }

            set
            {
                if (_isReplyBoxPersitent == value)
                {
                    return;
                }

                _isReplyBoxPersitent = value;
                RaisePropertyChanged(IsReplyBoxPersistentPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="IsShortURLEnabled" /> property's name.
        /// </summary>
        public const string IsShortURLEnabledPropertyName = "IsShortURLEnabled";

        private bool _isShortURLEnabled = true;

        /// <summary>
        /// Sets and gets the IsShortURLEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool IsShortURLEnabled
        {
            get
            {
                return _isShortURLEnabled;
            }

            set
            {
                if (_isShortURLEnabled == value)
                {
                    return;
                }

                _isShortURLEnabled = value;
                RaisePropertyChanged(IsShortURLEnabledPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsOnTop" /> property's name.
        /// </summary>
        public const string IsOnTopPropertyName = "IsOnTop";

        private bool _isOnTop = true;

        /// <summary>
        /// Sets and gets the IsOnTop property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool IsOnTop
        {
            get
            {
                return _isOnTop;
            }

            set
            {
                if (_isOnTop == value)
                {
                    return;
                }

                _isOnTop = value;
                RaisePropertyChanged(IsOnTopPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="MouseTransparency" /> property's name.
        /// </summary>
        public const string MouseTransparencyPropertyName = "MouseTransparency";

        private bool _mouseTransparency = false;

        /// <summary>
        /// Sets and gets the MouseTransparency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool MouseTransparency
        {
            get
            {
                return _mouseTransparency;
            }

            set
            {
                if (_mouseTransparency == value)
                {
                    return;
                }

                RaisePropertyChanging(MouseTransparencyPropertyName);
                _mouseTransparency = value;
                RaisePropertyChanged(MouseTransparencyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IndividualMessageBackgroundOpacity" /> property's name.
        /// </summary>
        public const string IndividualMessageBackgroundOpacityPropertyName = "IndividualMessageBackgroundOpacity";

        private double _individualMessageBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the IndividualMessageBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double IndividualMessageBackgroundOpacity
        {
            get
            {
                return _individualMessageBackgroundOpacity;
            }

            set
            {
                if (_individualMessageBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(IndividualMessageBackgroundOpacityPropertyName);
                _individualMessageBackgroundOpacity = value;
                RaisePropertyChanged(IndividualMessageBackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MusicTickerBackgroundOpacity" /> property's name.
        /// </summary>
        public const string MusicTickerBackgroundOpacityPropertyName = "MusicTickerBackgroundOpacity";

        private double _musicTickerBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the MusicTickerBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MusicTickerBackgroundOpacity
        {
            get
            {
                return _musicTickerBackgroundOpacity;
            }

            set
            {
                if (_musicTickerBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(MusicTickerBackgroundOpacityPropertyName);
                _musicTickerBackgroundOpacity = value;
                RaisePropertyChanged(MusicTickerBackgroundOpacityPropertyName);
            }
        }
        
        /// <summary>
        /// The <see cref="MessageBackgroundOpacity" /> property's name.
        /// </summary>
        public const string MessageBackgroundOpacityPropertyName = "MessageBackgroundOpacity";

        private double _messageBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the MessageBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MessageBackgroundOpacity
        {
            get
            {
                return _messageBackgroundOpacity;
            }

            set
            {
                if (_messageBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(MessageBackgroundOpacityPropertyName);
                _messageBackgroundOpacity = value;
                RaisePropertyChanged(MessageBackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BackgroundOpacity" /> property's name.
        /// </summary>
        public const string BackgroundOpacityPropertyName = "BackgroundOpacity";

        private double _backgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the BackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double BackgroundOpacity
        {
            get
            {
                return _backgroundOpacity;
            }

            set
            {
                if (_backgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(BackgroundOpacityPropertyName);
                _backgroundOpacity = value;
                RaisePropertyChanged(BackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EnableTransparency" /> property's name.
        /// </summary>
        public const string EnableTransparencyPropertyName = "EnableTransparency";

        private bool _enableTransparency = false;

        /// <summary>
        /// Sets and gets the EnableTransparency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool EnableTransparency
        {
            get
            {
                return _enableTransparency;
            }

            set
            {
                if (_enableTransparency == value)
                {
                    return;
                }

                RaisePropertyChanging(EnableTransparencyPropertyName);
                _enableTransparency = value;
                RaisePropertyChanged(EnableTransparencyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Parameters" /> property's name.
        /// </summary>
        public const string ParametersPropertyName = "Parameters";

        private List<ConfigField> _parameters = new List<ConfigField>();

        /// <summary>
        /// Sets and gets the Parameters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlArray]
        public List<ConfigField> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                if (_parameters == value)
                {
                    return;
                }

                RaisePropertyChanging(ParametersPropertyName);
                _parameters = value;
                RaisePropertyChanged(ParametersPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ThemeName" /> property's name.
        /// </summary>
        public const string ThemeNamePropertyName = "ThemeName";

        private string _themeName = null;

        /// <summary>
        /// Sets and gets the ThemeName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string ThemeName
        {
            get
            {
                return _themeName;
            }

            set
            {
                if (_themeName == value)
                {
                    return;
                }

                RaisePropertyChanging(ThemeNamePropertyName);
                _themeName = value;
                RaisePropertyChanged(ThemeNamePropertyName);
            }
        }




    }
}
