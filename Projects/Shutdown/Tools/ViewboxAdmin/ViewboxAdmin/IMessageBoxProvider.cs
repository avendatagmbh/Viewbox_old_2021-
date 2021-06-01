using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ViewboxAdmin
{
    public interface IMessageBoxProvider
    {
            MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);
    }
}
