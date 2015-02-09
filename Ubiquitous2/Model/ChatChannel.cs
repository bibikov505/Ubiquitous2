using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ChatChannel : NotifyPropertyChangeBase
    {
        /// <summary>
        /// The <see cref="ChatName" /> property's name.
        /// </summary>
        public const string ChatNamePropertyName = "ChatName";

        private string _chatName = String.Empty;

        /// <summary>
        /// Sets and gets the ChatName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChatName
        {
            get
            {
                return _chatName;
            }

            set
            {
                if (_chatName == value)
                {
                    return;
                }

                _chatName = value;
                RaisePropertyChanged(ChatNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChannelName" /> property's name.
        /// </summary>
        public const string ChannelNamePropertyName = "ChannelName";

        private string _channelName = String.Empty;

        /// <summary>
        /// Sets and gets the ChannelName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChannelName
        {
            get
            {
                return _channelName;
            }

            set
            {
                if (_channelName == value)
                {
                    return;
                }

                _channelName = value;
                RaisePropertyChanged(ChannelNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatIconURL" /> property's name.
        /// </summary>
        public const string ChatIconURLPropertyName = "ChatIconURL";

        private string _chatIconURL = String.Empty;

        /// <summary>
        /// Sets and gets the ChatIconURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChatIconURL
        {
            get
            {
                return _chatIconURL;
            }

            set
            {
                if (_chatIconURL == value)
                {
                    return;
                }

                _chatIconURL = value;
                RaisePropertyChanged(ChatIconURLPropertyName);
            }
        }
    }
}
