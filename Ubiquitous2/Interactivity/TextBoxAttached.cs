using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UB.Interactivity
{
    public class TextBoxAttached
    {
        
        /// <summary>
        /// The CaretPos attached property's name.
        /// </summary>
        public const string CaretPosPropertyName = "CaretPos";

        /// <summary>
        /// Gets the value of the CaretPos attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the CaretPos property of the specified object.</returns>
        public static int GetCaretPos(DependencyObject obj)
        {
            return (int)obj.GetValue(CaretPosProperty);
        }

        /// <summary>
        /// Sets the value of the CaretPos attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the CaretPos value of the specified object.</param>
        public static void SetCaretPos(DependencyObject obj, int value)
        {
            obj.SetValue(CaretPosProperty, value);
        }

        /// <summary>
        /// Identifies the CaretPos attached property.
        /// </summary>
        public static readonly DependencyProperty CaretPosProperty = DependencyProperty.RegisterAttached(
            CaretPosPropertyName,
            typeof(int),
            typeof(TextBoxAttached),
            new UIPropertyMetadata(-1, (obj, e) =>
            {
                var box = obj as TextBox;
                if (box == null)
                    return;

                var newPos = (int)e.NewValue;
                if (newPos < 0)
                    newPos = 0;

                box.CaretIndex = newPos;

            }));
    }
}
