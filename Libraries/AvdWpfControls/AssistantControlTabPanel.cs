using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls
{
    public class AssistantControlTabPanel : TabControl, INotifyPropertyChanged
    {
        internal static readonly DependencyPropertyKey BackAllowedKey =
            DependencyProperty.RegisterReadOnly(
                "BackAllowed", typeof (bool), typeof (AssistantControlTabPanel), new PropertyMetadata(false));

        public static readonly DependencyProperty BackAllowedProperty = BackAllowedKey.DependencyProperty;

        internal static readonly DependencyPropertyKey NextAllowedKey =
            DependencyProperty.RegisterReadOnly(
                "NextAllowed", typeof (bool), typeof (AssistantControlTabPanel), new PropertyMetadata(false));

        public static readonly DependencyProperty NextAllowedProperty = NextAllowedKey.DependencyProperty;

        internal static readonly DependencyPropertyKey StepCountKey =
            DependencyProperty.RegisterReadOnly(
                "StepCount", typeof (int), typeof (AssistantControlTabPanel), new PropertyMetadata(0));

        public static readonly DependencyProperty StepCountProperty = StepCountKey.DependencyProperty;

        internal static readonly DependencyPropertyKey CurrentStepKey =
            DependencyProperty.RegisterReadOnly(
                "CurrentStep", typeof (int), typeof (AssistantControlTabPanel), new PropertyMetadata(0));

        public static readonly DependencyProperty CurrentStepProperty = CurrentStepKey.DependencyProperty;

        public static readonly RoutedEvent BeforeNextEvent = EventManager.RegisterRoutedEvent(
            "BeforeNext", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControlTabPanel));

        public static readonly RoutedEvent BeforeBackEvent = EventManager.RegisterRoutedEvent(
            "BeforeBack", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControlTabPanel));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                                typeof (ICommand),
                                                                                                typeof (
                                                                                                    AssistantControlTabPanel
                                                                                                    ),
                                                                                                new PropertyMetadata(
                                                                                                    null,
                                                                                                    CommandChanged));

        public EventHandler canExecuteChangedHandler;

        static AssistantControlTabPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AssistantControlTabPanel),
                                                     new FrameworkPropertyMetadata(typeof (AssistantControlTabPanel)));
        }

        public bool NextAllowed
        {
            get
            {
                //if (ValidationValue.HasValue && !ValidationValue.Value) {
                //    return false;
                //}
                for (int i = SelectedIndex + 1; i < Items.Count; i++)
                    if (((TabItem) Items[i]).IsEnabled) return true;
                return false;
            }
        }

        public bool BackAllowed
        {
            get
            {
                //if (ValidationValue.HasValue && !ValidationValue.Value) {
                //    return false;
                //}
                for (int i = SelectedIndex - 1; i >= 0; i--)
                    if (((TabItem) Items[i]).IsEnabled) return true;
                return false;
            }
        }

        public int StepCount
        {
            get
            {
                int steps = 0;
                foreach (object t in Items)
                {
                    if (t is AssistantTabItem)
                    {
                        var item = t as AssistantTabItem;
                        if (!item.IsSummaryPage && item.IsEnabled) steps++;
                    }
                    else
                    {
                        if (((TabItem) t).IsEnabled) steps++;
                    }
                }
                return steps;
            }
        }

        public int CurrentStep
        {
            get
            {
                int step = 1;
                for (int i = 0; i < SelectedIndex; i++)
                {
                    object t = Items[i];
                    if (t is AssistantTabItem)
                    {
                        var item = t as AssistantTabItem;
                        if (!item.IsSummaryPage && item.IsEnabled) step++;
                    }
                    else
                    {
                        if (((TabItem) t).IsEnabled) step++;
                    }
                }
                return step;
            }
        }

        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public IInputElement CommandTarget { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            foreach (var item in Items)
            {
                ((TabItem) item).IsEnabledChanged += (sender, args) => UpdateValues();
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (newValue == null) return;
            foreach (var item in newValue)
            {
                ((TabItem) item).IsEnabledChanged += (sender, args) => UpdateValues();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        //#region ValidationValue
        //private bool? _validationValue;
        //public bool? ValidationValue {
        //    get { return _validationValue; }
        //    set {
        //        _validationValue = value;
        //        SetValue(NextAllowedKey, NextAllowed);
        //    }
        //}
        //#endregion ValidationValue
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateValues();
        }

        private void UpdateValues()
        {
            SetValue(BackAllowedKey, BackAllowed);
            SetValue(NextAllowedKey, NextAllowed);
            SetValue(CurrentStepKey, CurrentStep);
            SetValue(StepCountKey, StepCount);
        }

        public void NavigateNext()
        {
            if (OnBeforeNext()) return;
            if (!NextAllowed) return;
            SelectedIndex++;
            while (!((TabItem) SelectedItem).IsEnabled) SelectedIndex++;
            OnNext();
            //ValidationValue = null;
            //SetValue(NextAllowedKey, NextAllowed);
        }

        public event RoutedEventHandler BeforeNext
        {
            add { AddHandler(BeforeNextEvent, value); }
            remove { RemoveHandler(BeforeNextEvent, value); }
        }

        internal bool OnBeforeNext()
        {
            RoutedEventArgs args = new RoutedEventArgs(BeforeNextEvent);
            RaiseEvent(args);
            //if (args.Handled) {
            //    ValidationValue = false;
            //    SetValue(NextAllowedKey, NextAllowed);
            //}
            return args.Handled;
        }

        public void NavigateBack()
        {
            if (OnBeforeBack()) return;
            if (!BackAllowed) return;
            SelectedIndex--;
            while (SelectedIndex > 0 && !((TabItem) SelectedItem).IsEnabled) SelectedIndex--;
            OnBack();
            //ValidationValue = null;
            //SetValue(BackAllowedKey, BackAllowed);
        }

        internal bool OnBeforeBack()
        {
            RoutedEventArgs args = new RoutedEventArgs(BeforeBackEvent);
            RaiseEvent(args);
            //if (args.Handled) {
            //    ValidationValue = false;
            //    SetValue(BackAllowedKey, BackAllowed);
            //}
            return args.Handled;
        }

        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AssistantControlTabPanel assistantControlTabPanel = (AssistantControlTabPanel) d;
            assistantControlTabPanel.HookUpCommand((ICommand) e.OldValue, (ICommand) e.NewValue);
        }

        // Add a new command to the Command Property.
        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            // If oldCommand is not null, then we need to remove the handlers.
            if (oldCommand != null)
            {
                RemoveCommand(oldCommand, newCommand);
            }
            AddCommand(oldCommand, newCommand);
        }

        // Remove an old command from the Command Property.
        private void RemoveCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }

        // TODO: check on internet in examples what is this.
        // http://stackoverflow.com/questions/10471562/implementing-icommandsource-not-works
        // http://stackoverflow.com/questions/601393/custom-command-wpf
        // http://msdn.microsoft.com/en-us/library/system.windows.input.icommandsource.aspx

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (Command != null)
            {
                RoutedCommand command = Command as RoutedCommand;
                if (command != null)
                {
                    command.Execute(CommandParameter, CommandTarget);
                }
                else
                {
                    (Command).Execute(CommandParameter);
                }
            }
        }

        // Add the command.
        private void AddCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            canExecuteChangedHandler = handler;
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
            }
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command != null)
            {
                RoutedCommand command = Command as RoutedCommand;
                // If a RoutedCommand.
                IsEnabled = command != null
                                ? command.CanExecute(CommandParameter, CommandTarget)
                                : Command.CanExecute(CommandParameter);
            }
        }

        #region Next

        public static readonly RoutedEvent NextEvent = EventManager.RegisterRoutedEvent(
            "Next", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControlTabPanel));

        public event RoutedEventHandler Next
        {
            add { AddHandler(NextEvent, value); }
            remove { RemoveHandler(NextEvent, value); }
        }

        internal void OnNext()
        {
            RaiseEvent(new RoutedEventArgs(NextEvent));
        }

        #endregion Next

        #region Back

        public static readonly RoutedEvent BackEvent = EventManager.RegisterRoutedEvent(
            "Back", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControlTabPanel));

        public event RoutedEventHandler Back
        {
            add { AddHandler(BackEvent, value); }
            remove { RemoveHandler(BackEvent, value); }
        }

        internal void OnBack()
        {
            RaiseEvent(new RoutedEventArgs(BackEvent));
        }

        #endregion Back

        //public static readonly RoutedCommand UpdateValidation = new RoutedCommand();
        //private void ExecuteUpdateValidation(object o, ExecutedRoutedEventArgs e) { ValidationValue = null; }
        //private void CanExecuteUpdateValidation (object o, CanExecuteRoutedEventArgs e) {
        //    Control target = e.Source as Control;
        //    e.CanExecute = target != null;
        //}
    }
}