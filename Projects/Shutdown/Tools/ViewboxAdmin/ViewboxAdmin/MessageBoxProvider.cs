using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ViewboxAdmin
{
    class MessageBoxProvider :IMessageBoxProvider
    {
        public MessageBoxResult Show(string messageBoxText, string caption, System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon, System.Windows.MessageBoxResult defaultResult) {
            return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
        }
    }
}
