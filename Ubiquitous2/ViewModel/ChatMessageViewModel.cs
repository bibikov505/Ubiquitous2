﻿using Devart.Controls;
using GalaSoft.MvvmLight;
using UB.Utils;
using UB.Model;
using GalaSoft.MvvmLight.Ioc;


namespace UB.ViewModel
{
    public class ChatMessageViewModel : ViewModelBase, IHeightMeasurer
    {
        private readonly IDataService _dataService;

        private double estimatedHeight;
        private double estimatedWidth;
        
        public ChatMessageViewModel()
        {
        }

        [PreferredConstructor]
        public ChatMessageViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetMessage(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            // Report error here
                            return;
                        }

                        Message = item;
                    });
        }

        public ChatMessageViewModel (ChatMessage message)
        {
            Message = message;
        }

        public ChatMessage Message { get; set; }

        public double GetEstimatedHeight(double width)
        {
            // Do not recalc height if text and width are unchanged
            if (estimatedHeight < 0 || estimatedWidth != width)
            {
                estimatedWidth = width;
                estimatedHeight = TextMeasurer.GetEstimatedHeight(Message.Text, width) + 38; // Add margin
            }
            return estimatedHeight;
        }
    }

}

