﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ToolTip :NotifyPropertyChangeBase
    {
        public ToolTip(string header, string text, int number = 0)
        {
            Header = header;
            Text = text;
            Number = number;
        }

        /// <summary>
        /// The <see cref="Number" /> property's name.
        /// </summary>
        public const string NumberPropertyName = "Number";

        private int _number = 0;

        /// <summary>
        /// Sets and gets the Number property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Number
        {
            get
            {
                return _number;
            }

            set
            {
                if (_number == value)
                {
                    return;
                }

                RaisePropertyChanging(NumberPropertyName);
                _number = value;
                RaisePropertyChanged(NumberPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Header" /> property's name.
        /// </summary>
        public const string HeaderPropertyName = "Header";

        private string _header = "";

        /// <summary>
        /// Sets and gets the Header property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Header
        {
            get
            {
                return _header;
            }

            set
            {
                if (_header == value)
                {
                    return;
                }

                RaisePropertyChanging(HeaderPropertyName);
                _header = value;
                RaisePropertyChanged(HeaderPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _text = "";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text == value)
                {
                    return;
                }

                RaisePropertyChanging(TextPropertyName);
                _text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }
    }
}
