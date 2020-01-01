using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace wKpr.Utils
{
    public class TextBoxHelperAP
    {
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.RegisterAttached("SelectionMode",
                                                                                                             typeof(string),
                                                                                                             typeof(TextBoxHelperAP),
                                                                                                             new UIPropertyMetadata(null,OnPropertyChangedCallback));


        private static void OnPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = dependencyObject as TextBox;
            if (uiElement == null) 
                return;

            //if ((e.NewValue is string ))
            //{
            //    _mode = e.NewValue as string;
            //}
            uiElement.MouseDoubleClick += UiElement_MouseDoubleClick; 
            
        }

        private static void UiElement_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox ui = sender as TextBox;
            if (ui!= null)
            {
                ui.SelectAll();
            }
        }

        public static void SetSelectionMode(UIElement element, string value)
        {
            element.SetValue(SelectionModeProperty, value);
        }

        public static string GetSelectionMode(UIElement element)
        {
            return (string)element.GetValue(SelectionModeProperty);
        }

     }
}

