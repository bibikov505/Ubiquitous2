using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ChannelsSideTabsViewModel : ViewModelBase
    {
        private IChatDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the ChannelsSideTabsViewModel class.
        /// </summary>
        [PreferredConstructor]
        public ChannelsSideTabsViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            if( IsInDesignMode )
            {
                Channels = new ObservableCollection<dynamic>()
                {
                   new { ChatName = "AllChats", ChannelName = "#allchats", ChatIconURL = Icons.MainIcon },
                };
            }
            else
            {
                Channels = dataService.ChatChannels;
            }
        }

        /// <summary>
        /// The <see cref="Channels" /> property's name.
        /// </summary>
        public const string ChannelsPropertyName = "Channels";

        private ObservableCollection<dynamic> _channels;

        /// <summary>
        /// Sets and gets the Channels property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> Channels
        {
            get
            {
                return _channels;
            }

            set
            {
                if (_channels == value)
                {
                    return;
                }

                _channels = value;
                RaisePropertyChanged(ChannelsPropertyName);
            }
        }




    }
}