using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using AvdWpfControls.Handlers;

namespace AvdWpfControls.DeletableTextBox
{
    public enum SearchMode
    {
        Instant,
        Delayed,
    }

    public class DeletableTextBox : TextBox, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register(
                "IsPassword",
                typeof(bool),
                typeof(DeletableTextBox),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                "Password",
                typeof(string),
                typeof(DeletableTextBox),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static void MyPropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Get instance of current control from sender
            // and property value from e.NewValue

            // Set public property on TaregtCatalogControl, e.g.
            ((DeletableTextBox)sender).Password = e.NewValue.ToString();
        }

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register(
                "IsHighlighted",
                typeof(bool),
                typeof(DeletableTextBox),
                new PropertyMetadata(default(bool)));

        public static DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(DeletableTextBox));

        public static DependencyProperty LabelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(DeletableTextBox));

        public static DependencyProperty SearchModeProperty =
            DependencyProperty.Register(
                "SearchMode",
                typeof(SearchMode),
                typeof(DeletableTextBox),
                new PropertyMetadata(SearchMode.Instant));

        private static DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(DeletableTextBox),
                new PropertyMetadata());
        public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        private static DependencyPropertyKey IsMouseLeftButtonDownPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsMouseLeftButtonDown",
                typeof(bool),
                typeof(DeletableTextBox),
                new PropertyMetadata());
        public static DependencyProperty IsMouseLeftButtonDownProperty = IsMouseLeftButtonDownPropertyKey.DependencyProperty;

        public static DependencyProperty SearchEventTimeDelayProperty =
            DependencyProperty.Register(
                "SearchEventTimeDelay",
                typeof(Duration),
                typeof(DeletableTextBox),
                new FrameworkPropertyMetadata(
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    new PropertyChangedCallback(OnSearchEventTimeDelayChanged)));

        public static readonly RoutedEvent SearchEvent =
            EventManager.RegisterRoutedEvent(
                "Search",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(DeletableTextBox));

        public static DependencyProperty InfoTextProperty =
            DependencyProperty.Register(
                "InfoText",
                typeof(string),
                typeof(DeletableTextBox));

        private static DependencyPropertyKey HasInfoTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasInfoText",
                typeof(bool),
                typeof(DeletableTextBox),
                new PropertyMetadata());
        public static DependencyProperty HasInfoTextProperty = HasInfoTextPropertyKey.DependencyProperty;

        static DeletableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DeletableTextBox),
                new FrameworkPropertyMetadata(typeof(DeletableTextBox)));

            Setter mySetter1 = new Setter { Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.Orange) };
            Setter mySetter2 = new Setter { Property = BorderThicknessProperty, Value = new Thickness(1) };

            styleTrigger = new Trigger
            {
                Property = IsHighlightedProperty,
                Value = true,
                Setters = { mySetter1, mySetter2 }
            };

            styleTrigger = new Trigger
            {
                Property = IsMouseOverProperty,
                Value = true,
                Setters = { mySetter1, mySetter2 }
            };

            styleTriggerFocused = new Trigger
            {
                Property = IsFocusedProperty,
                Value = true,
                Setters = { mySetter1, mySetter2 }
            };

            styleTriggerNotFocused = new Trigger
            {
                Property = IsFocusedProperty,
                Value = false,
                Setters = { mySetter2 }
            };
        }

        int selectionStart;
        int selectionLength;
        int oldSelectionStart;
        int oldSelectionLength;
        bool handleLater;
        char? newChar;
        bool selfSet;
        char passwordChar;
        static Trigger styleTrigger;
        static Trigger styleTriggerFocused;
        static Trigger styleTriggerNotFocused;

        private DispatcherTimer searchEventDelayTimer;

        public DeletableTextBox()
            : base()
        {
            searchEventDelayTimer = new DispatcherTimer();
            searchEventDelayTimer.Interval = SearchEventTimeDelay.TimeSpan;
            searchEventDelayTimer.Tick += new EventHandler(OnSeachEventDelayTimerTick);

            Initialized += new EventHandler(myTextBox_Initialized);
            GotKeyboardFocus += TextBoxGotKeyboardFocus;
            LostKeyboardFocus += TextBoxLostKeyboardFocus;
            
            int charCode = 8226;
            passwordChar = (char)charCode;

            Style = TextBoxStyleFocused;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string status)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(status));
            }
        }

        public Style TextBoxStyle
        {
            get
            {
                Style basedOn = (Style)FindResource(typeof(DeletableTextBox));
                Style myStyle = new Style(typeof(DeletableTextBox)) { Triggers = { styleTrigger }, BasedOn = basedOn };
                return myStyle;
            }
        }

        public Style TextBoxStyleFocused
        {
            get
            {
                Style basedOn = (Style)FindResource(typeof(DeletableTextBox));
                Style myStyle = new Style(typeof(DeletableTextBox)) { Triggers = { styleTriggerFocused, styleTriggerNotFocused }, BasedOn = basedOn };
                return myStyle;
            }
        }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HasKeyboardFocus = true;
            TextBox source = e.Source as TextBox;
        }

        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HasKeyboardFocus = false;
            TextBox source = e.Source as TextBox;
        }

        void OnSeachEventDelayTimerTick(object o, System.EventArgs e)
        {
            searchEventDelayTimer.Stop();
            RaiseSearchEvent();
        }

        static void OnSearchEventTimeDelayChanged(
            DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DeletableTextBox stb = o as DeletableTextBox;
            if (stb != null)
            {
                stb.searchEventDelayTimer.Interval = ((Duration)e.NewValue).TimeSpan;
                stb.searchEventDelayTimer.Stop();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            HasText = Text.Length != 0;

            if (SearchMode == SearchMode.Instant)
            {
                searchEventDelayTimer.Stop();
                searchEventDelayTimer.Start();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Border iconBorder = GetTemplateChild("PART_SearchIconBorder") as Border;
            if (iconBorder != null)
            {
                iconBorder.MouseLeftButtonDown += new MouseButtonEventHandler(IconBorder_MouseLeftButtonDown);
                iconBorder.MouseLeftButtonUp += new MouseButtonEventHandler(IconBorder_MouseLeftButtonUp);
                iconBorder.MouseLeave += new MouseEventHandler(IconBorder_MouseLeave);
            }
        }

        private void IconBorder_MouseLeftButtonDown(object obj, MouseButtonEventArgs e)
        {
            IsMouseLeftButtonDown = true;
        }

        private void IconBorder_MouseLeftButtonUp(object obj, MouseButtonEventArgs e)
        {
            if (!IsMouseLeftButtonDown) return;

            if (HasText && SearchMode == SearchMode.Instant)
            {
                this.Text = "";

                //BindingExpression be = GetBindingExpression(TextBox.TextProperty);
                //if (be != null)
                //    be.UpdateSource();
                BindingExpressionWithException.BindingExpressionUpdateWithExceptionHandler(this, TextBox.TextProperty);
            }

            if (HasText && SearchMode == SearchMode.Delayed)
            {
                RaiseSearchEvent();
            }

            IsMouseLeftButtonDown = false;
        }

        private void IconBorder_MouseLeave(object obj, MouseEventArgs e)
        {
            IsMouseLeftButtonDown = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && SearchMode == SearchMode.Instant)
            {
                this.Text = "";
            }
            else if ((e.Key == Key.Return || e.Key == Key.Enter) &&
                SearchMode == SearchMode.Delayed)
            {
                RaiseSearchEvent();
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private void RaiseSearchEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(SearchEvent);
            RaiseEvent(args);
        }

        private void myTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void myTextBox_Initialized(object sender, System.EventArgs e)
        {
            if (IsPassword)
            {
                this.AddHandler(CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler(myTextBox_PreviewExecuted));
                this.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(myTextBox_KeyDown), true);
                this.SelectionChanged += new RoutedEventHandler(myTextBox_SelectionChanged);
            }

            HasInfoText = !string.IsNullOrWhiteSpace(InfoText);
        }

        private void myTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsPassword)
            {
                if (!selfSet)
                {
                    if (Password.Length - 1 == this.Text.Length && this.SelectionLength == 0)
                    {
                        oldSelectionStart = selectionStart;
                        oldSelectionLength = selectionLength;
                        handleLater = true;
                    }
                    else
                    {
                        selfSet = true;

                        if (!Text.Contains(passwordChar.ToString()) || Password.Length != this.Text.Length)
                        {
                            StringBuilder aStringBuilder = new StringBuilder(Password);

                            if (newChar == null)
                                newChar = ' ';

                            try
                            {
                                aStringBuilder.Remove(selectionStart, selectionLength);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                aStringBuilder.Remove(0, selectionLength);
                            }

                            if (newChar != null)
                            {
                                try
                                {
                                    aStringBuilder.Insert(selectionStart, newChar);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    aStringBuilder.Insert(0, newChar);
                                }
                            }

                            Password = aStringBuilder.ToString().TrimStart();

                            selectionStart = this.SelectionStart;
                            selectionLength = this.SelectionLength;
                            //this.Text = new String('•', this.Text.Length);
                            this.Text = new String(passwordChar, Password.Length);

                            this.SelectionStart = selectionStart;
                            this.SelectionLength = selectionLength;
                        }
                        else
                        {
                            selectionStart = this.SelectionStart;
                            selectionLength = this.SelectionLength;
                        }

                        newChar = null;
                        selfSet = false;
                    }
                }
            }
        }

        private void myTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsPassword)
            {
                newChar = GetCharFromKey(e.Key);

                if (handleLater)
                {
                    if (e.Key == Key.Back)
                    {
                        int pos = oldSelectionStart - 1 < 0 ? 0 : oldSelectionStart - 1;
                        try
                        {
                            Password = Password.Remove(pos, 1);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Password = "";
                        }

                        selectionStart = this.SelectionStart;
                    }

                    if (e.Key == Key.Delete)
                    {
                        try
                        {
                            Password = Password.Remove(oldSelectionStart, 1);
                            selectionStart = this.SelectionStart;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Password = "";
                        }
                    }

                    if (Password == "")
                    {
                        selectionStart = 0;
                        oldSelectionStart = 0;
                    }

                    handleLater = false;
                }
            }
        }

        #region Character from Key

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);
        [DllImport("user32.dll")]
        public static extern int ToUnicode(
         uint wVirtKey,
         uint wScanCode,
         byte[] lpKeyState,
         [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] 
        StringBuilder pwszBuff,
         int cchBuff,
         uint wFlags);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        #endregion

        private bool _haskeyboardFocus;
        public bool HasKeyboardFocus
        {
            get
            {
                return _haskeyboardFocus;
            }

            set
            {
                _haskeyboardFocus = value;
                OnPropertyChanged("HasKeyboardFocus");
            }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty) ?? string.Empty; }
            set
            {
                SetValue(PasswordProperty, value);
                //if (!Password.Equals(value)) {
                //    Text = value;
                //}
                //var b = selfSet;
                //selfSet = false;
                if (Password.Length != Text.Length)
                {
                    myTextBox_SelectionChanged(null, null);
                }
                //selfSet = b;
            }
        }

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public Brush LabelTextColor
        {
            get { return (Brush)GetValue(LabelTextColorProperty); }
            set { SetValue(LabelTextColorProperty, value); }
        }

        public SearchMode SearchMode
        {
            get { return (SearchMode)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        public bool HasText
        {
            get { return (bool)GetValue(HasTextProperty); }
            private set { SetValue(HasTextPropertyKey, value); }
        }

        public Duration SearchEventTimeDelay
        {
            get { return (Duration)GetValue(SearchEventTimeDelayProperty); }
            set { SetValue(SearchEventTimeDelayProperty, value); }
        }

        public bool IsMouseLeftButtonDown
        {
            get { return (bool)GetValue(IsMouseLeftButtonDownProperty); }
            private set { SetValue(IsMouseLeftButtonDownPropertyKey, value); }
        }

        public event RoutedEventHandler Search
        {
            add { AddHandler(SearchEvent, value); }
            remove { RemoveHandler(SearchEvent, value); }
        }

        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        public bool HasInfoText
        {
            get { return (bool)GetValue(HasInfoTextProperty); }
            private set { SetValue(HasInfoTextPropertyKey, value); }
        }
    }
}
