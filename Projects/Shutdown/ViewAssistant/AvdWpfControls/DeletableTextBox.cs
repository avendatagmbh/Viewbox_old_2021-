using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AvdWpfControls.Handlers;

namespace AvdWpfControls {
    public class DeletableTextBox : TextBox, INotifyPropertyChanged {
        private static readonly Trigger StyleTrigger;
        private static readonly Trigger StyleTriggerFocused;
        private static readonly Trigger StyleTriggerNotFocused;

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register(
                "IsPassword",
                typeof (bool),
                typeof (DeletableTextBox),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                "Password",
                typeof (string),
                typeof (DeletableTextBox),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register(
                "IsHighlighted",
                typeof (bool),
                typeof (DeletableTextBox),
                new PropertyMetadata(default(bool)));

        public static DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof (string),
                typeof (DeletableTextBox));

        public static DependencyProperty LabelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof (Brush),
                typeof (DeletableTextBox));

        public static DependencyProperty SearchModeProperty =
            DependencyProperty.Register(
                "SearchMode",
                typeof (SearchMode),
                typeof (DeletableTextBox),
                new PropertyMetadata(SearchMode.Instant));

        private static readonly DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof (bool),
                typeof (DeletableTextBox),
                new PropertyMetadata());

        public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsMouseLeftButtonDownPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsMouseLeftButtonDown",
                typeof (bool),
                typeof (DeletableTextBox),
                new PropertyMetadata());

        public static DependencyProperty IsMouseLeftButtonDownProperty =
            IsMouseLeftButtonDownPropertyKey.DependencyProperty;

        public static DependencyProperty SearchEventTimeDelayProperty =
            DependencyProperty.Register(
                "SearchEventTimeDelay",
                typeof (Duration),
                typeof (DeletableTextBox),
                new FrameworkPropertyMetadata(
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    OnSearchEventTimeDelayChanged));

        public static DependencyProperty InfoTextProperty =
            DependencyProperty.Register(
                "InfoText",
                typeof (string),
                typeof (DeletableTextBox));

        private static readonly DependencyPropertyKey HasInfoTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasInfoText",
                typeof (bool),
                typeof (DeletableTextBox),
                new PropertyMetadata());

        public static DependencyProperty HasInfoTextProperty = HasInfoTextPropertyKey.DependencyProperty;

        public static readonly RoutedEvent SearchEvent =
            EventManager.RegisterRoutedEvent(
                "Search",
                RoutingStrategy.Bubble,
                typeof (RoutedEventHandler),
                typeof (DeletableTextBox));

        private readonly char _passwordChar;
        private readonly DispatcherTimer _searchEventDelayTimer;
        private bool _haskeyboardFocus;
        private bool _handleLater;
        private char? _newChar;
        private int _oldSelectionStart;
        private int _selectionLength;
        private int _selectionStart;
        private bool _selfSet;

        static DeletableTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (DeletableTextBox),
                new FrameworkPropertyMetadata(typeof (DeletableTextBox)));

            var mySetter1 = new Setter {Property = BorderBrushProperty, Value = new SolidColorBrush(Colors.Orange)};
            var mySetter2 = new Setter {Property = BorderThicknessProperty, Value = new Thickness(1)};

            StyleTrigger = new Trigger {
                Property = IsHighlightedProperty,
                Value = true,
                Setters = {mySetter1, mySetter2}
            };

            StyleTrigger = new Trigger {
                Property = IsMouseOverProperty,
                Value = true,
                Setters = {mySetter1, mySetter2}
            };

            StyleTriggerFocused = new Trigger {
                Property = IsFocusedProperty,
                Value = true,
                Setters = {mySetter1, mySetter2}
            };

            StyleTriggerNotFocused = new Trigger {
                Property = IsFocusedProperty,
                Value = false,
                Setters = {mySetter2}
            };
        }

        public DeletableTextBox() {
            _searchEventDelayTimer = new DispatcherTimer();
            _searchEventDelayTimer.Interval = SearchEventTimeDelay.TimeSpan;
            _searchEventDelayTimer.Tick += OnSeachEventDelayTimerTick;

            Initialized += MyTextBoxInitialized;
            GotKeyboardFocus += TextBoxGotKeyboardFocus;
            LostKeyboardFocus += TextBoxLostKeyboardFocus;

            int charCode = 8226;
            _passwordChar = (char) charCode;

            Style = TextBoxStyleFocused;
        }


        public Style TextBoxStyle {
            get {
                var basedOn = (Style) FindResource(typeof (DeletableTextBox));
                var myStyle = new Style(typeof (DeletableTextBox)) {Triggers = {StyleTrigger}, BasedOn = basedOn};
                return myStyle;
            }
        }

        public Style TextBoxStyleFocused {
            get {
                var basedOn = (Style) FindResource(typeof (DeletableTextBox));
                var myStyle = new Style(typeof (DeletableTextBox))
                              {Triggers = {StyleTriggerFocused, StyleTriggerNotFocused}, BasedOn = basedOn};
                return myStyle;
            }
        }

        public bool HasKeyboardFocus {
            get { return _haskeyboardFocus; }

            set {
                _haskeyboardFocus = value;
                OnPropertyChanged("HasKeyboardFocus");
            }
        }

        public string Password {
            get { return (string) GetValue(PasswordProperty) ?? string.Empty; }
            set {
                SetValue(PasswordProperty, value);
                //if (!Password.Equals(value)) {
                //    Text = value;
                //}
                //var b = selfSet;
                //selfSet = false;
                if (Password.Length != Text.Length) {
                    MyTextBoxSelectionChanged(null, null);
                }
                //selfSet = b;
            }
        }

        public bool IsPassword { get { return (bool) GetValue(IsPasswordProperty); } set { SetValue(IsPasswordProperty, value); } }

        public bool IsHighlighted { get { return (bool) GetValue(IsHighlightedProperty); } set { SetValue(IsHighlightedProperty, value); } }

        public string LabelText { get { return (string) GetValue(LabelTextProperty); } set { SetValue(LabelTextProperty, value); } }

        public Brush LabelTextColor { get { return (Brush) GetValue(LabelTextColorProperty); } set { SetValue(LabelTextColorProperty, value); } }

        public SearchMode SearchMode { get { return (SearchMode) GetValue(SearchModeProperty); } set { SetValue(SearchModeProperty, value); } }

        public bool HasText { get { return (bool) GetValue(HasTextProperty); } private set { SetValue(HasTextPropertyKey, value); } }

        public Duration SearchEventTimeDelay { get { return (Duration) GetValue(SearchEventTimeDelayProperty); } set { SetValue(SearchEventTimeDelayProperty, value); } }

        public bool IsMouseLeftButtonDown { get { return (bool) GetValue(IsMouseLeftButtonDownProperty); } private set { SetValue(IsMouseLeftButtonDownPropertyKey, value); } }
        public string InfoText { get { return (string) GetValue(InfoTextProperty); } set { SetValue(InfoTextProperty, value); } }

        public bool HasInfoText { get { return (bool) GetValue(HasInfoTextProperty); } private set { SetValue(HasInfoTextPropertyKey, value); } }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public static void MyPropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            // Get instance of current control from sender
            // and property value from e.NewValue

            // Set public property on TaregtCatalogControl, e.g.
            ((DeletableTextBox) sender).Password = e.NewValue.ToString();
        }

        private void OnPropertyChanged(string status) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(status));
            }
        }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { HasKeyboardFocus = true; }

        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { HasKeyboardFocus = false; }

        private void OnSeachEventDelayTimerTick(object o, EventArgs e) {
            _searchEventDelayTimer.Stop();
            RaiseSearchEvent();
        }

        private static void OnSearchEventTimeDelayChanged(
            DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var stb = o as DeletableTextBox;
            if (stb != null) {
                stb._searchEventDelayTimer.Interval = ((Duration) e.NewValue).TimeSpan;
                stb._searchEventDelayTimer.Stop();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e) {
            base.OnTextChanged(e);

            HasText = Text.Length != 0;

            if (SearchMode == SearchMode.Instant) {
                _searchEventDelayTimer.Stop();
                _searchEventDelayTimer.Start();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var iconBorder = GetTemplateChild("PART_SearchIconBorder") as Border;
            if (iconBorder != null) {
                iconBorder.MouseLeftButtonDown += IconBorderMouseLeftButtonDown;
                iconBorder.MouseLeftButtonUp += IconBorderMouseLeftButtonUp;
                iconBorder.MouseLeave += IconBorderMouseLeave;
            }
        }

        private void IconBorderMouseLeftButtonDown(object obj, MouseButtonEventArgs e) { IsMouseLeftButtonDown = true; }

        private void IconBorderMouseLeftButtonUp(object obj, MouseButtonEventArgs e) {
            if (!IsMouseLeftButtonDown) return;

            if (HasText && SearchMode == SearchMode.Instant) {
                Text = "";

                BindingExpressionWithException.BindingExpressionUpdateWithExceptionHandler(this, TextProperty);
            }

            if (HasText && SearchMode == SearchMode.Delayed) {
                RaiseSearchEvent();
            }

            IsMouseLeftButtonDown = false;
        }

        private void IconBorderMouseLeave(object obj, MouseEventArgs e) { IsMouseLeftButtonDown = false; }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.Escape && SearchMode == SearchMode.Instant) {
                Text = "";
            } else if ((e.Key == Key.Return || e.Key == Key.Enter) &&
                       SearchMode == SearchMode.Delayed) {
                RaiseSearchEvent();
            } else {
                base.OnKeyDown(e);
            }
        }

        private void RaiseSearchEvent() {
            var args = new RoutedEventArgs(SearchEvent);
            RaiseEvent(args);
        }

        private void myTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste) {
                e.Handled = true;
            }
        }

        private void MyTextBoxInitialized(object sender, EventArgs e) {
            if (IsPassword) {
                AddHandler(CommandManager.PreviewExecutedEvent,
                           new ExecutedRoutedEventHandler(myTextBox_PreviewExecuted));
                AddHandler(KeyDownEvent, new KeyEventHandler(MyTextBoxKeyDown), true);
                SelectionChanged += MyTextBoxSelectionChanged;
            }

            HasInfoText = !string.IsNullOrWhiteSpace(InfoText);
        }

        private void MyTextBoxSelectionChanged(object sender, RoutedEventArgs e) {
            if (IsPassword) {
                if (!_selfSet) {
                    if (Password.Length - 1 == Text.Length && SelectionLength == 0) {
                        _oldSelectionStart = _selectionStart;
                        _handleLater = true;
                    } else {
                        _selfSet = true;

                        if (!Text.Contains(_passwordChar.ToString(CultureInfo.InvariantCulture)) || Password.Length != Text.Length) {
                            var aStringBuilder = new StringBuilder(Password);

                            if (_newChar == null)
                                _newChar = ' ';

                            try {
                                aStringBuilder.Remove(_selectionStart, _selectionLength);
                            } catch (ArgumentOutOfRangeException) {
                                aStringBuilder.Remove(0, _selectionLength);
                            }

                            if (_newChar != null) {
                                try {
                                    aStringBuilder.Insert(_selectionStart, _newChar);
                                } catch (ArgumentOutOfRangeException) {
                                    aStringBuilder.Insert(0, _newChar);
                                }
                            }

                            Password = aStringBuilder.ToString().TrimStart();

                            _selectionStart = SelectionStart;
                            _selectionLength = SelectionLength;
                            Text = new String(_passwordChar, Password.Length);

                            SelectionStart = _selectionStart;
                            SelectionLength = _selectionLength;
                        } else {
                            _selectionStart = SelectionStart;
                            _selectionLength = SelectionLength;
                        }

                        _newChar = null;
                        _selfSet = false;
                    }
                }
            }
        }

        private void MyTextBoxKeyDown(object sender, KeyEventArgs e) {
            if (IsPassword) {
                _newChar = GetCharFromKey(e.Key);

                if (_handleLater) {
                    if (e.Key == Key.Back) {
                        int pos = _oldSelectionStart - 1 < 0 ? 0 : _oldSelectionStart - 1;
                        try {
                            Password = Password.Remove(pos, 1);
                        } catch (ArgumentOutOfRangeException) {
                            Password = "";
                        }

                        _selectionStart = SelectionStart;
                    }

                    if (e.Key == Key.Delete) {
                        try {
                            Password = Password.Remove(_oldSelectionStart, 1);
                            _selectionStart = SelectionStart;
                        } catch (ArgumentOutOfRangeException) {
                            Password = "";
                        }
                    }

                    if (Password == "") {
                        _selectionStart = 0;
                        _oldSelectionStart = 0;
                    }

                    _handleLater = false;
                }
            }
        }

        public event RoutedEventHandler Search { add { AddHandler(SearchEvent, value); } remove { RemoveHandler(SearchEvent, value); } }

        #region Character from Key
        public enum MapType : uint {
            MapvkVkToVsc = 0x0,
            MapvkVscToVk = 0x1,
            MapvkVkToChar = 0x2,
            MapvkVscToVkEx = 0x3,
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
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        public static char GetCharFromKey(Key key) {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint) virtualKey, MapType.MapvkVkToVsc);
            var stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result) {
                case -1:
                    break;
                case 0:
                    break;
                case 1: {
                    ch = stringBuilder[0];
                    break;
                }
                default: {
                    ch = stringBuilder[0];
                    break;
                }
            }
            return ch;
        }
        #endregion
    }
}