using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Utils.MessageBox
{
    public class MessageBoxExt
    {
        public static IMessageBoxExt OwnMessageBox { get; set; }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                                    MessageBoxButton button,
                                                    MessageBoxImage icon,
                                                    MessageBoxResult defaultResult,
                                                    MessageBoxOptions options)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options)
                       : OwnMessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                             MessageBoxButton button, MessageBoxImage icon,
                                             MessageBoxResult defaultResult)

        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult)
                       : OwnMessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                             MessageBoxButton button, MessageBoxImage icon)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText, caption, button, icon)
                       : OwnMessageBox.Show(owner, messageBoxText, caption, button, icon);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                             MessageBoxButton button)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText, caption, button)
                       : OwnMessageBox.Show(owner, messageBoxText, caption, button);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(MessageBoxButton.OK);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText, caption)
                       : OwnMessageBox.Show(owner, messageBoxText, caption);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(MessageBoxButton.OK);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(owner, messageBoxText)
                       : OwnMessageBox.Show(owner, messageBoxText);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption,
                                                    MessageBoxButton button,
                                                    MessageBoxImage icon,
                                                    MessageBoxResult defaultResult,
                                                    MessageBoxOptions options)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options)
                       : OwnMessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption,
                                             MessageBoxButton button, MessageBoxImage icon,
                                             MessageBoxResult defaultResult)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText, caption, button, icon, defaultResult)
                       : OwnMessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption,
                                             MessageBoxButton button, MessageBoxImage icon)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText, caption, button, icon)
                       : OwnMessageBox.Show(messageBoxText, caption, button, icon);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption,
                                             MessageBoxButton button)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(button);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText, caption, button)
                       : OwnMessageBox.Show(messageBoxText, caption, button);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(MessageBoxButton.OK);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText, caption)
                       : OwnMessageBox.Show(messageBoxText, caption);
        }

        public static MessageBoxResult Show(string messageBoxText)
        {
            if (GlobalVariables.UnitTesting)
                return GetDefaultResult(MessageBoxButton.OK);
            return OwnMessageBox == null
                       ? System.Windows.MessageBox.Show(messageBoxText)
                       : OwnMessageBox.Show(messageBoxText);
        }

        private static MessageBoxResult GetDefaultResult(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.OK:
                    return MessageBoxResult.OK;
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    return MessageBoxResult.Yes;
            }
            return MessageBoxResult.OK;
        }
    }
}
