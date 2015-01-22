using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UB.Model;
using UB.Utils;

namespace UB.Interactivity
{
    public class ElementPositionTracker :BehaviorBase
    {
        private FrameworkElement element;
        private UIElement container;
        private Point relativeLocation;
        protected override void Attach()
        {
            element = AssociatedObject;
            container = Application.Current.MainWindow;
            element.LayoutUpdated += element_LayoutUpdated;
        }
        void UpdateData()
        {
            if (element == null ||
                Application.Current.MainWindow.WindowState == WindowState.Minimized ||
                !element.IsVisible ||
                !element.IsArrangeValid)
                return;

            relativeLocation = element.TranslatePoint(new Point(0, 0), container);

            X = relativeLocation.X;
            Y = relativeLocation.Y;
        }
        void element_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateData();
        }
        protected override void Cleanup()
        {
            element.LayoutUpdated -= element_LayoutUpdated;
        }

        /// <summary>
        /// The <see cref="Y" /> dependency property's name.
        /// </summary>
        public const string YPropertyName = "Y";

        /// <summary>
        /// Gets or sets the value of the <see cref="Y" />
        /// property. This is a dependency property.
        /// </summary>
        public double Y
        {
            get
            {
                return (double)GetValue(YProperty);
            }
            set
            {
                SetValue(YProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Y" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            YPropertyName,
            typeof(double),
            typeof(ElementPositionTracker),
            new UIPropertyMetadata(double.NaN));

        /// <summary>
        /// The <see cref="X" /> dependency property's name.
        /// </summary>
        public const string XPropertyName = "X";

        /// <summary>
        /// Gets or sets the value of the <see cref="X" />
        /// property. This is a dependency property.
        /// </summary>
        public double X
        {
            get
            {
                return (double)GetValue(XProperty);
            }
            set
            {
                SetValue(XProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="X" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            XPropertyName,
            typeof(double),
            typeof(ElementPositionTracker),
            new UIPropertyMetadata(double.NaN));
    }
}
