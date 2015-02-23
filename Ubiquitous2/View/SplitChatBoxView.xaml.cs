using System.Windows;
using System.Windows.Controls;
using UB.Model;

namespace UB.View
{
    /// <summary>
    /// Description for SplitChatBoxView.
    /// </summary>
    public partial class SplitChatBoxView : UserControl
    {
        private static GridLengthConverter gridLengthConverter = new GridLengthConverter();
        /// <summary>
        /// Initializes a new instance of the SplitChatBoxView class.
        /// </summary>
        public SplitChatBoxView()
        {
            InitializeComponent();
        }

        public object LeftContent
        {
            get { return leftContent.Content; }
            set { leftContent.Content = value; }
        }

        public object RightContent
        {
            get { return rightContent.Content; }
            set { rightContent.Content = value; }
        }
        
        /// <summary>
        /// The <see cref="LeftColumnWidthTemp" /> dependency property's name.
        /// </summary>
        public const string LeftColumnWidthTempPropertyName = "LeftColumnWidthTemp";

        /// <summary>
        /// Gets or sets the value of the <see cref="LeftColumnWidthTemp" />
        /// property. This is a dependency property.
        /// </summary>
        public double LeftColumnWidthTemp
        {
            get
            {
                return (double)GetValue(LeftColumnWidthTempProperty);
            }
            set
            {
                SetValue(LeftColumnWidthTempProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="LeftColumnWidthTemp" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftColumnWidthTempProperty = DependencyProperty.Register(
            LeftColumnWidthTempPropertyName,
            typeof(double),
            typeof(SplitChatBoxView),
            new UIPropertyMetadata(50.0, (o, e) => {
                var splitBox = o as SplitChatBoxView;
                if (splitBox == null)
                    return;

                if( (bool)splitBox.GetValue(IsLeftColumnVisibleProperty))
                    splitBox.SetValue(LeftColumnWidthProperty, (double)e.NewValue);
            }));
        /// <summary>
        /// The <see cref="LeftColumnWidth" /> dependency property's name.
        /// </summary>
        public const string LeftColumnWidthPropertyName = "LeftColumnWidth";

        /// <summary>
        /// Gets or sets the value of the <see cref="LeftColumnWidth" />
        /// property. This is a dependency property.
        /// </summary>
        public double LeftColumnWidth
        {
            get
            {
                return (double)GetValue(LeftColumnWidthProperty);
            }
            set
            {
                SetValue(LeftColumnWidthProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="LeftColumnWidth" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftColumnWidthProperty = DependencyProperty.Register(
            LeftColumnWidthPropertyName,
            typeof(double),
            typeof(SplitChatBoxView),
            new UIPropertyMetadata(50.0, (o, e) => {
                var splitBox = o as SplitChatBoxView;

                if (splitBox == null)
                    return;

                if ((bool)splitBox.GetValue(IsLeftColumnVisibleProperty))
                {
                    splitBox.SetValue(LeftColumnWidthTempProperty, (double)e.NewValue);
                }

            }));    

        /// <summary>
        /// The <see cref="IsLeftColumnVisible" /> dependency property's name.
        /// </summary>
        public const string IsLeftColumnVisiblePropertyName = "IsLeftColumnVisible";

        /// <summary>
        /// Gets or sets the value of the <see cref="IsLeftColumnVisible" />
        /// property. This is a dependency property.
        /// </summary>
        public bool IsLeftColumnVisible
        {
            get
            {
                return (bool)GetValue(IsLeftColumnVisibleProperty);
            }
            set
            {
                SetValue(IsLeftColumnVisibleProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="IsLeftColumnVisible" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLeftColumnVisibleProperty = DependencyProperty.Register(
            IsLeftColumnVisiblePropertyName,
            typeof(bool),
            typeof(SplitChatBoxView),
            new UIPropertyMetadata(true, (o, e) => {
                var isVisible = (bool)e.NewValue;
                var splitBox = o as SplitChatBoxView;

                if (splitBox == null)
                    return;

                if( !isVisible )
                {
                    splitBox.leftColumnDefinition.Width = new GridLength(0);
                    splitBox.leftColumnDefinition.MinWidth = 0;
                    splitBox.middleColumnDefinition.Width = new GridLength(0);
                    splitBox.middleColumnDefinition.MinWidth = 0;
                }
                else
                {
                    splitBox.leftColumnDefinition.MinWidth = 16.0;
                    splitBox.leftColumnDefinition.Width = (GridLength)gridLengthConverter.ConvertFrom((double)splitBox.LeftColumnWidth);
                    splitBox.middleColumnDefinition.Width = new GridLength(3);
                }
            }));
        
    }
}