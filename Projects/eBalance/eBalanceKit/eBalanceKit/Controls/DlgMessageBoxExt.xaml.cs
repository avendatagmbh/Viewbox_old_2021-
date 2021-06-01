using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utils.MessageBox;
using eBalanceKit.Models;

namespace eBalanceKit.Controls
{
    /// <summary>
    /// Interaction logic for MessageBoxExt.xaml - Singleton
    /// 
    /// MessageBoxOptions handling not implemented
    /// </summary>
    public partial class DlgMessageBoxExt : Window, IMessageBoxExt
    {
        #region Properties

        private static DlgMessageBoxExt DlgMessageBoxExtInstance { get; set; }

        #endregion

        #region Methods

        public static DlgMessageBoxExt GetInstance()
        {
            return DlgMessageBoxExtInstance ?? (DlgMessageBoxExtInstance = new DlgMessageBoxExt(new MessageBoxExtModel()));
        }
        
        private DlgMessageBoxExt(MessageBoxExtModel model)
        {
            DataContext = model;
            InitializeComponent();
        }

        private MessageBoxResult ShowExt()
        {
            var model = (MessageBoxExtModel) DataContext;
            PlayMessageBeep(model.Icon);
            ShowDialog();
            GetInstance();
            return model.Result;
        }

        private static void PlayMessageBeep(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Error:
                    SystemSounds.Hand.Play();
                    break;
                case MessageBoxImage.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case MessageBoxImage.Question:
                    SystemSounds.Question.Play();
                    break;
                case MessageBoxImage.Information:
                    SystemSounds.Asterisk.Play();
                    break;
                default:
                    SystemSounds.Beep.Play();
                    break;
            }
        }
        #endregion Methods


        #region EventHandlers

        private void btnButtonYes_Click(object sender, RoutedEventArgs e)
        {
            ((MessageBoxExtModel)DataContext).Result = MessageBoxResult.Yes;
            DialogResult = true;
        }

        private void btnButtonNo_Click(object sender, RoutedEventArgs e)
        {
            ((MessageBoxExtModel)DataContext).Result = MessageBoxResult.No;
            DialogResult = true;
        }

        private void btnButtonOk_Click(object sender, RoutedEventArgs e)
        {
            ((MessageBoxExtModel)DataContext).Result = MessageBoxResult.OK;
            DialogResult = true;
        }

        private void btnButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ((MessageBoxExtModel)DataContext).Result = MessageBoxResult.Cancel;
            DialogResult = true;
        }

        #endregion EventHandlers

        #region IMessageBoxExt
        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText, caption, button, icon, defaultResult, options);
            return ShowExt();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText, caption, button, icon, defaultResult);
            return ShowExt();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText, caption, button, icon);
            return ShowExt();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText, caption, button);
            return ShowExt();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText, caption);
            return ShowExt();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText)
        {
            ((MessageBoxExtModel)DataContext).Init(owner, messageBoxText);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText, caption, button, icon, defaultResult, options);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText, caption, button, icon, defaultResult);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText, caption, button, icon);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText, caption, button);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText, string caption)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText, caption);
            return ShowExt();
        }

        public MessageBoxResult Show(string messageBoxText)
        {
            ((MessageBoxExtModel)DataContext).Init(null, messageBoxText);
            return ShowExt();
        }

        #endregion IMessageBoxExt

    }
}
