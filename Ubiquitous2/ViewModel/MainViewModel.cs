﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using UB.Model;
using System.Windows.Threading;
using System.Threading;
using GalaSoft.MvvmLight.Threading;
using UB.Utils;
using UB.View;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IChatDataService _dataService;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        [PreferredConstructor]
        public MainViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            ChannelList = _dataService.ChatChannels;
            SelectedChatChannel = ChannelList[0];

            //var testStatusWindow = new StatusWindow();
            //testStatusWindow.Show();
            

            MessengerInstance.Register<ChatMessage>(this, "SetChannel", (message) => {
                SelectedChatChannel = ChannelList.FirstOrDefault(channel =>
                    channel.ChatName == message.ChatName &&
                    channel.ChannelName == message.Channel) ?? ChannelList[0];

            });
        }

        private RelayCommand _showSettings;

        /// <summary>
        /// Gets the ShowSettings.
        /// </summary>
        public RelayCommand ShowSettings
        {
            get
            {
                return _showSettings
                    ?? (_showSettings = new RelayCommand(
                                          () =>
                                          {
                                              var settings = ServiceLocator.Current.GetInstance<SettingsViewModel>();
                                              settings.Show();
                                          }));
            }
        }

        private RelayCommand _exitApplication;

        /// <summary>
        /// Gets the ExitApplication.
        /// </summary>
        public RelayCommand ExitApplication
        {
            get
            {
                return _exitApplication
                    ?? (_exitApplication = new RelayCommand(
                                          () =>
                                          {
                                              Properties.Ubiqiutous.Default.Save();

                                          }));
            }
        }

        /// <summary>
        /// The <see cref="IsOverlayVisible" /> property's name.
        /// </summary>
        public const string IsOverlayVisiblePropertyName = "IsOverlayVisible";

        private bool _isOverlayVisible = false;

        /// <summary>
        /// Sets and gets the IsOverlayVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOverlayVisible
        {
            get
            {
                return _isOverlayVisible;
            }

            set
            {
                if (_isOverlayVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsOverlayVisiblePropertyName);
                _isOverlayVisible = value;
                RaisePropertyChanged(IsOverlayVisiblePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="IsMouseOver" /> property's name.
        /// </summary>
        public const string IsMouseOverPropertyName = "IsMouseOver";

        private bool _isMouseOver = false;

        /// <summary>
        /// Sets and gets the IsMouseOver property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }

            set
            {
                if (_isMouseOver == value)
                {
                    return;
                }

                RaisePropertyChanging(IsMouseOverPropertyName);
                _isMouseOver = value;
                RaisePropertyChanged(IsMouseOverPropertyName);
            }
        }

        private RelayCommand _hideOverlay;

        /// <summary>
        /// Gets the HideOverlay.
        /// </summary>
        public RelayCommand HideOverlay
        {
            get
            {
                return _hideOverlay
                    ?? (_hideOverlay = new RelayCommand(
                                          () =>
                                          {
                                              IsMouseOver = false;
                                              SwitchOverlay();
                                          }));
            }
        }

        private RelayCommand _showOverlay;

        /// <summary>
        /// Gets the ShowOverlay.
        /// </summary>
        public RelayCommand ShowOverlay
        {
            get
            {
                return _showOverlay
                    ?? (_showOverlay = new RelayCommand(
                                          () =>
                                          {
                                                IsMouseOver = true;
                                                SwitchOverlay();
                                          }));
            }
        }


        /// <summary>
        /// The <see cref="SendText" /> property's name.
        /// </summary>
        public const string SendTextPropertyName = "SendText";

        private string _sendText = "";

        /// <summary>
        /// Sets and gets the SendText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SendText
        {
            get
            {
                return _sendText;
            }

            set
            {
                if (_sendText == value)
                {
                    return;
                }

                RaisePropertyChanging(SendTextPropertyName);
                _sendText = value;
                RaisePropertyChanged(SendTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsFocused" /> property's name.
        /// </summary>
        public const string IsFocusedPropertyName = "IsFocused";

        private bool _isFocused = true;

        /// <summary>
        /// Sets and gets the IsFocused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }

            set
            {
                if (_isFocused == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFocusedPropertyName);
                _isFocused = value;
                RaisePropertyChanged(IsFocusedPropertyName);
            }
        }

        private RelayCommand _setFocused;

        /// <summary>
        /// Gets the SetFocused.
        /// </summary>
        public RelayCommand SetFocused
        {
            get
            {
                return _setFocused
                    ?? (_setFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = true;
                                              SwitchOverlay();
                                          }));
            }
        }

        private RelayCommand _setUnFocused;

        /// <summary>
        /// Gets the SetUnFocused.
        /// </summary>
        public RelayCommand SetUnFocused
        {
            get
            {
                return _setUnFocused
                    ?? (_setUnFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = false;
                                              SwitchOverlay();
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="ChannelList" /> property's name.
        /// </summary>
        public const string ChatListPropertyName = "ChannelList";

        private ObservableCollection<dynamic> _chatList = new ObservableCollection<dynamic>();

        /// <summary>
        /// Sets and gets the ChatList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> ChannelList
        {
            get
            {
                return _chatList;
            }

            set
            {
                if (_chatList == value)
                {
                    return;
                }

                RaisePropertyChanging(ChatListPropertyName);
                _chatList = value;
                RaisePropertyChanged(ChatListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedChatChannel" /> property's name.
        /// </summary>
        public const string SelectedChatChannelPropertyName = "SelectedChatChannel";

        private dynamic _selectedChat;// = new { ChatName = "Ubiquitous 2.0", ChannelName = "#ubiquitous", ChatIconURL = @"/Ubiquitous2;component/Resources/ubiquitous smile.ico" };

        /// <summary>
        /// Sets and gets the SelectedChatChannel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public dynamic SelectedChatChannel
        {
            get
            {
                return _selectedChat;
            }

            set
            {
                if (_selectedChat != null && value != null &&
                    _selectedChat.ChatName == value.ChatName &&
                    _selectedChat.ChannelName == value.ChannelName &&
                    _selectedChat.ChatIconURL == value.ChatIconURL)
                {
                    return;
                }
                SelectedChannelName = value.ChannelName;
                RaisePropertyChanging(SelectedChatChannelPropertyName);
                _selectedChat = value;
                RaisePropertyChanged(SelectedChatChannelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedChannelName" /> property's name.
        /// </summary>
        public const string SelectedChannelNamePropertyName = "SelectedChannelName";

        private string _selectedChannelName = "#xedoc";

        /// <summary>
        /// Sets and gets the SelectedChannelName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedChannelName
        {
            get
            {
                return _selectedChannelName;
            }

            set
            {
                if (_selectedChannelName == value)
                {
                    return;
                }

                RaisePropertyChanging(SelectedChannelNamePropertyName);
                _selectedChannelName = value;
                RaisePropertyChanged(SelectedChannelNamePropertyName);
            }
        }
        private void SwitchOverlay()
        {
            if (IsMouseOver && IsFocused)
            {
                IsOverlayVisible = true;
                ScrollToLastMessage();
                SendTextEditMode = true;

            }
            else
            {
                IsOverlayVisible = false;
                SendTextEditMode = false;
            }

            MessengerInstance.Send<bool>(!IsOverlayVisible, "EnableAutoScroll");

        }

        private RelayCommand _enterCommand;

        /// <summary>
        /// Gets the EnterCommand.
        /// </summary>
        public RelayCommand EnterCommand
        {
            get
            {
                return _enterCommand
                    ?? (_enterCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (SelectedChatChannel == null)
                                                  return;

                                              _dataService.SendMessage(new ChatMessage() { 
                                                Channel = SelectedChatChannel.ChannelName,
                                                ChatName = SelectedChatChannel.ChatName,
                                                IsSentByMe = true,
                                                HighlyImportant = true,
                                                Text = SendText
                                              });
                                              SendText = String.Empty;
                                              ScrollToLastMessage();
                                          }));
            }
        }
        private void ScrollToLastMessage()
        {
            var delaySend = new Timer((obj) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    MessengerInstance.Send<bool>(true, "MessageSent");
                });
            }, this, 100, Timeout.Infinite);

        }

        /// <summary>
        /// The <see cref="SendTextEditMode" /> property's name.
        /// </summary>
        public const string SendTextEditModePropertyName = "SendTextEditMode";

        private bool _sendTextEditMode = false;

        /// <summary>
        /// Sets and gets the SendTextEditMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SendTextEditMode
        {
            get
            {
                return _sendTextEditMode;
            }

            set
            {
                if (_sendTextEditMode == value)
                {
                    return;
                }

                RaisePropertyChanging(SendTextEditModePropertyName);
                _sendTextEditMode = value;
                RaisePropertyChanged(SendTextEditModePropertyName);
            }
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

    }
}