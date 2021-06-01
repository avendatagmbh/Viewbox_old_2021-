using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utils;

namespace eBalanceKit.Models
{
    /// <summary>
    /// Model for the MessageBoxExt
    /// </summary>
    public class MessageBoxExtModel: NotifyPropertyChangedBase
    {
        #region Properies

        private Window _owner;
        public Window Owner
        {
            get { return _owner; }
            set { _owner = value; OnPropertyChanged("Owner"); }
        }

        private string _messageBoxText;
        public string MessageBoxText
        {
            get { return _messageBoxText; }
            set { _messageBoxText = value; OnPropertyChanged("MessageBoxText"); }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; } 
            set { _caption = value; OnPropertyChanged("Caption"); }
        }

        private MessageBoxButton _button;
        public MessageBoxButton Button
        {
            get { return _button; }
            set { _button = value; OnPropertyChanged("Button"); }
        }

        private MessageBoxImage _icon;
        public MessageBoxImage Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged("Icon"); }
        }

        private MessageBoxResult _defaultResult;
        public MessageBoxResult DefaultResult
        {
            get { return _defaultResult; }
            set { _defaultResult = value; OnPropertyChanged("DefaultResult"); }
        }

        private MessageBoxOptions _options;
        public MessageBoxOptions Options
        {
            get { return _options; }
            set { _options = value; OnPropertyChanged("Options"); }
        }

        private MessageBoxResult _result;
        public MessageBoxResult Result
        {
            get { return _result; }
            set { _result = value; OnPropertyChanged("Result"); }
        }

        private bool _yesNoVisible;
        public bool YesNoVisible
        {
            get { return _yesNoVisible; }
            set { _yesNoVisible = value; OnPropertyChanged("YesNoVisible"); }
        }

        private bool _okVisible;
        public bool OkVisible
        {
            get { return _okVisible; }
            set { _okVisible = value; OnPropertyChanged("OkVisible"); }
        }

        private bool _cancelVisible;
        public bool CancelVisible
        {
            get { return _cancelVisible; }
            set { _cancelVisible = value; OnPropertyChanged("CancelVisible"); }
        }

        private bool _isYesDefault;
        public bool IsYesDefault
        {
            get { return _isYesDefault; }
            set { _isYesDefault = value; OnPropertyChanged("IsYesDefault"); }
        }

        private bool _isNoDefault;
        public bool IsNoDefault
        {
            get { return _isNoDefault; }
            set { _isNoDefault = value; OnPropertyChanged("IsNoDefault"); }
        }

        private bool _isOkDefault;
        public bool IsOkDefault
        {
            get { return _isOkDefault; }
            set { _isOkDefault = value; OnPropertyChanged("IsOkDefault"); }
        }

        private bool _isCancelDefault;
        public bool IsCancelDefault
        {
            get { return _isCancelDefault; }
            set { _isCancelDefault = value; OnPropertyChanged("IsCancelDefault"); }
        }

        private ImageSource _messageImageSource;
        public ImageSource MessageImageSource
        {
            get { return _messageImageSource; }
            set { _messageImageSource = value; OnPropertyChanged("MessageImageSource"); OnPropertyChanged("HasImageSource"); }
        }

        public bool HasImageSource { get; set; }

        #endregion Properies
        
        public void Init(Window owner, string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            // TODO las notimplemented 

            Owner = owner;
            MessageBoxText = messageBoxText;
            Caption = caption;
            Button = button;
            Icon = icon;
            DefaultResult = defaultResult;
            Options = options;
            SetImageSource();
            Caption = caption == "" ? GetCaption() : caption;
            SetButtons();
        }

        /// <summary>
        /// If Caption = "", set a default caption
        /// </summary>
        /// <returns></returns>
        private string GetCaption()
        {
            switch (Icon)
            {
                case MessageBoxImage.Error:
                    return "Error";
                case MessageBoxImage.Warning:
                    return "Warning";
                case MessageBoxImage.Question:
                    return "Question";
                case MessageBoxImage.Information:
                    return "Information";
                default:
                    return "Information";
            }
        }

        /// <summary>
        /// Set the buttons visible, default properties
        /// </summary>
        private void SetButtons()
        {
            OkVisible = false;
            YesNoVisible = false;
            CancelVisible = false;
            switch (_button)
            {
                case MessageBoxButton.OKCancel:
                    OkVisible = CancelVisible = true;
                    break;
                case MessageBoxButton.OK:
                    OkVisible = true;
                    break;
                case MessageBoxButton.YesNo:
                    YesNoVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesNoVisible = CancelVisible = true;
                    break;
            }
            SetButtonDefault();
        }

        /// <summary>
        /// Sets the button default property
        /// </summary>
        private void SetButtonDefault()
        {
            switch (DefaultResult)
            {
                case MessageBoxResult.OK:
                    IsOkDefault = true;
                    break;

                case MessageBoxResult.Yes:
                    IsYesDefault = true;
                    break;

                case MessageBoxResult.No:
                    IsNoDefault = true;
                    break;

                case MessageBoxResult.Cancel:
                    IsCancelDefault = true;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Set the imagesource
        /// </summary>
        private void SetImageSource()
        {
            switch (Icon)
            {
                //case MessageBoxImage.Hand:
                //case MessageBoxImage.Stop:
                case MessageBoxImage.Error:
                    MessageImageSource = ToImageSource(SystemIcons.Error);
                    break;

                //case MessageBoxImage.Exclamation:
                case MessageBoxImage.Warning:
                    MessageImageSource = ToImageSource(SystemIcons.Warning);
                    break;

                case MessageBoxImage.Question:
                    MessageImageSource = ToImageSource(SystemIcons.Question);
                    break;

                //case MessageBoxImage.Asterisk:
                case MessageBoxImage.Information:
                    MessageImageSource = ToImageSource(SystemIcons.Information);
                    break;

                default:
                    MessageImageSource = null;
                    break;
            }
        }

        private static ImageSource ToImageSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return wpfBitmap;
        }
    }
}
