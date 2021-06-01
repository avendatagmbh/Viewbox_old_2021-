using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.MessageBox
{
    public interface IMessageBoxExt
    {
        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText, string caption,
                                                    System.Windows.MessageBoxButton button,
                                                    System.Windows.MessageBoxImage icon,
                                                    System.Windows.MessageBoxResult defaultResult,
                                                    System.Windows.MessageBoxOptions options);

        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon,
                                             System.Windows.MessageBoxResult defaultResult);

        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon);

        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button);

        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText, string caption);

        System.Windows.MessageBoxResult Show(System.Windows.Window owner, string messageBoxText);

        System.Windows.MessageBoxResult Show(string messageBoxText, string caption,
                                                    System.Windows.MessageBoxButton button,
                                                    System.Windows.MessageBoxImage icon,
                                                    System.Windows.MessageBoxResult defaultResult,
                                                    System.Windows.MessageBoxOptions options);

        System.Windows.MessageBoxResult Show(string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon,
                                             System.Windows.MessageBoxResult defaultResult);

        System.Windows.MessageBoxResult Show(string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon);

        System.Windows.MessageBoxResult Show(string messageBoxText, string caption,
                                             System.Windows.MessageBoxButton button);

        System.Windows.MessageBoxResult Show(string messageBoxText, string caption);

        System.Windows.MessageBoxResult Show(string messageBoxText);
        
    }
}
