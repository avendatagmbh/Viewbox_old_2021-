using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls
{
    /// <summary>
    ///   Interaktionslogik für CtlNumericUpDown.xaml Usage: <AvdWpfControls:NumericUpDown Minimum="0" Maximum="100"
    ///    MouseWheelStepSize="10" Value="{Binding Threshold,Mode=TwoWay}" AppendString="%" />
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        #region Events

        #region Delegates

        public delegate void ValueChangedEvent(object sender, ValueChangedEventArgs e);

        #endregion

        public event ValueChangedEvent ValueChanged;

        #endregion Events

        public NumericUpDown()
        {
            InitializeComponent();
        }

        #region Minimum

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof (int), typeof (NumericUpDown), new UIPropertyMetadata(0));

        public int Minimum
        {
            get { return (int) GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        #endregion Minimum

        #region Maximum

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof (int), typeof (NumericUpDown), new UIPropertyMetadata(100));

        public int Maximum
        {
            get { return (int) GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        #endregion Maximum

        #region Value

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (int), typeof (NumericUpDown),
                                        new UIPropertyMetadata(100, PropertyChangedCallback));

        public int Value
        {
            get { return (int) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs
                                                        dependencyPropertyChangedEventArgs)
        {
            NumericUpDown numericUpDown = (NumericUpDown) dependencyObject;
            numericUpDown.SetText();
            if (numericUpDown.ValueChanged != null)
                numericUpDown.ValueChanged(numericUpDown,
                                           new ValueChangedEventArgs(
                                               (int) dependencyPropertyChangedEventArgs.OldValue,
                                               (int) dependencyPropertyChangedEventArgs.NewValue));
        }

        #endregion Value

        private void SetText()
        {
            NUDTextBox.Text = Value.ToString() + AppendString;
        }

        private void Increase(int stepSize)
        {
            Value = Math.Min(Maximum, Value + stepSize);
        }

        private void Decrease(int stepSize)
        {
            Value = Math.Max(Minimum, Value - stepSize);
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            Increase(1);
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            Decrease(1);
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Increase(1);
            }

            if (e.Key == Key.Down)
            {
                Decrease(1);
            }
        }

        private void ChangeValueFromText()
        {
            int number = 0;
            if (NUDTextBox.Text != "")
            {
                string content = AppendString.Length == 0
                                     ? NUDTextBox.Text
                                     : NUDTextBox.Text.Replace(AppendString, string.Empty);
                int.TryParse(content, out number);
            }
            if (number > Maximum) number = Maximum;
            if (number < Minimum) number = Minimum;
            Value = number;
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Decrease(MouseWheelStepSize);
            else if (e.Delta > 0)
                Increase(MouseWheelStepSize);
        }

        private void NUDTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeValueFromText();
        }

        private void NUDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChangeValueFromText();
            }
        }

        #region AppendString

        // Using a DependencyProperty as the backing store for AppendString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppendStringProperty =
            DependencyProperty.Register("AppendString", typeof (string), typeof (NumericUpDown),
                                        new UIPropertyMetadata(string.Empty, AppendStringChanged));

        public string AppendString
        {
            get { return (string) GetValue(AppendStringProperty); }
            set { SetValue(AppendStringProperty, value); }
        }

        private static void AppendStringChanged(DependencyObject dependencyObject,
                                                DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            NumericUpDown obj = (NumericUpDown) dependencyObject;
            obj.SetText();
        }

        #endregion AppendString

        #region MouseWheelStepSize

        // Using a DependencyProperty as the backing store for MouseWheelStepSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseWheelStepSizeProperty =
            DependencyProperty.Register("MouseWheelStepSize", typeof (int), typeof (NumericUpDown),
                                        new UIPropertyMetadata(1));

        public int MouseWheelStepSize
        {
            get { return (int) GetValue(MouseWheelStepSizeProperty); }
            set { SetValue(MouseWheelStepSizeProperty, value); }
        }

        #endregion MouseWheelStepSize
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int OldValue { get; private set; }
        public int NewValue { get; private set; }
    }
}