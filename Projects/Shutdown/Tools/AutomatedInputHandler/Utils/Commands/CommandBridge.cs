using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;

namespace Utils.Commands
{
    /// <summary>
    /// Provides attached properties for the assignment of command behavior on every control.
    /// </summary>
    public class CommandBridge : FrameworkElement
    {

        #region Private Variables

        private static Dictionary<WeakReference, CommandedElementData> _elements;

        #endregion

        #region Constructors
        
        static CommandBridge()
        {
            _elements = new Dictionary<WeakReference, CommandedElementData>();
        }

        #endregion

        #region Dependency Properties

        #region EventName

        /// <summary>
        /// Dependency Property for EventName.
        /// </summary>
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.RegisterAttached("EventName", typeof(string), typeof(CommandBridge),
            new PropertyMetadata("", OnEventNamePropertyChanged));

        private static void OnEventNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.EventName = (string)e.NewValue;
                if (data.Handler == null)
                {
                    CreateHandler(element, data);
                }
            }
        }

        /// <summary>
        /// Sets the EventName attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">The name of the event that should be connected with a command.</param>
        public static void SetEventName(UIElement element, string value)
        {
            element.SetValue(EventNameProperty, value);
        }

        /// <summary>
        /// Sets the EventName attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>The name of the event that should be connected with a command.</returns>
        public static string GetEventName(UIElement element)
        {
            return (string)element.GetValue(EventNameProperty);
        }

        #endregion

        #region Command

        /// <summary>
        /// Dependency Property for Command.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandBridge),
            new PropertyMetadata(null, OnCommandPropertyChanged));

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                ICommand oldValue = e.OldValue as ICommand;
                ICommand newValue = e.NewValue as ICommand;
                if (oldValue != null)
                    oldValue.CanExecuteChanged -= CanExecuteChanged;
                if (newValue != null)
                {
                    if (!CommandHandlerExists(newValue))
                        newValue.CanExecuteChanged += CanExecuteChanged;
                }
                var data = GetElementData(element);
                data.Command = (ICommand)e.NewValue;
                if (data.Handler == null)
                {
                    CreateHandler(element, data);
                }
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the Command attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">An ICommand object.</param>
        public static void SetCommand(UIElement element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Gets the Command attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>An ICommand object.</returns>
        public static ICommand GetCommand(UIElement element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }

        #endregion

        #region CommandParameter

        /// <summary>
        /// Dependency Property for CommandParameter.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandBridge),
            new PropertyMetadata(null, OnCommandParameterPropertyChanged));

        private static void OnCommandParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.CommandParameter = e.NewValue;
                UpdateCanExecute(element);
                CreateHandler(element, data);
            }
        }

        /// <summary>
        /// Sets the CommandParameter attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">An object.</param>
        public static void SetCommandParameter(UIElement element, object value)
        {
            element.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Gets the CommandParameter attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>An object.</returns>
        public static object GetCommandParameter(UIElement element)
        {
            return element.GetValue(CommandParameterProperty);
        }

        #endregion

        #region PropertyName

        /// <summary>
        /// Dependency Property for PropertyName.
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(CommandBridge),
            new PropertyMetadata("", OnPropertyNamePropertyChanged));

        private static void OnPropertyNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null && e.NewValue != null)
            {
                var data = GetElementData(element);
                data.PropertyName = (string)e.NewValue;
                if (data.Handler == null)
                {
                    CreateHandler(element, data);
                }
            }
        }

        /// <summary>
        /// Sets the PropertyName attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">Name of a property.</param>
        public static void SetPropertyName(UIElement element, object value)
        {
            element.SetValue(PropertyNameProperty, value);
        }

        /// <summary>
        /// Gets the PropertyName attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>Name of a property.</returns>
        public static object GetPropertyName(UIElement element)
        {
            return element.GetValue(PropertyNameProperty);
        }

        #endregion

        #region CanExecutePropertyValue

        /// <summary>
        /// Dependency Property for CanExecutePropertyValue.
        /// </summary>
        public static readonly DependencyProperty CanExecutePropertyValueProperty =
            DependencyProperty.RegisterAttached("CanExecutePropertyValue", typeof(object), typeof(CommandBridge),
            new PropertyMetadata(null, OnCanExecutePropertyValuePropertyChanged));

        private static void OnCanExecutePropertyValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.CanExecutePropertyValue = e.NewValue;
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the CanExecutePropertyValue attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">The value that should be applied, if the assined command can be executed.</param>
        public static void SetCanExecutePropertyValue(UIElement element, object value)
        {
            element.SetValue(CanExecutePropertyValueProperty, value);
        }

        /// <summary>
        /// Gets the CanExecutePropertyValue attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>The value that should be applied, if the assined command can be executed.</returns>
        public static object GetCanExecutePropertyValue(UIElement element)
        {
            return element.GetValue(CanExecutePropertyValueProperty);
        }

        #endregion

        #region CanNotExecutePropertyValue

        /// <summary>
        /// Dependency Property for CanNotExecutePropertyValue.
        /// </summary>
        public static readonly DependencyProperty CanNotExecutePropertyValueProperty =
            DependencyProperty.RegisterAttached("CanNotExecutePropertyValue", typeof(object), typeof(CommandBridge),
            new PropertyMetadata(null, OnCanNotExecutePropertyValuePropertyChanged));

        private static void OnCanNotExecutePropertyValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.CanNotExecutePropertyValue = e.NewValue;
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the CanNotExecutePropertyValue attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">The value that should be applied, if the assined command can't be executed.</param>
        public static void SetCanNotExecutePropertyValue(UIElement element, object value)
        {
            element.SetValue(CanNotExecutePropertyValueProperty, value);
        }

        /// <summary>
        /// Gets the CanNotExecutePropertyValue attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>The value that should be applied, if the assined command can't be executed.</returns>
        public static object GetCanNotExecutePropertyValue(UIElement element)
        {
            return element.GetValue(CanNotExecutePropertyValueProperty);
        }

        #endregion

        #region Converter

        /// <summary>
        /// Dependency Property for Converter.
        /// </summary>
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.RegisterAttached("Converter", typeof(IValueConverter), typeof(CommandBridge),
            new PropertyMetadata(null, OnConverterPropertyChanged));

        private static void OnConverterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.Converter = (IValueConverter)e.NewValue;
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the Converter attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">An IValueConverter instance for converting the property values.</param>
        public static void SetConverter(UIElement element, IValueConverter value)
        {
            element.SetValue(ConverterProperty, value);
        }

        /// <summary>
        /// Gets the Converter attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>An IValueConverter instance for converting the property values.</returns>
        public static IValueConverter GetConverter(UIElement element)
        {
            return (IValueConverter)element.GetValue(ConverterProperty);
        }

        #endregion

        #region ConverterParameter

        /// <summary>
        /// Dependency Property for ConverterParameter.
        /// </summary>
        public static readonly DependencyProperty ConverterParameterProperty =
            DependencyProperty.RegisterAttached("ConverterParameter", typeof(object), typeof(CommandBridge),
            new PropertyMetadata(null, OnConverterParameterPropertyChanged));

        private static void OnConverterParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.ConverterParameter = e.NewValue;
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the ConverterParameter attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">An object that can passed to the converter.</param>
        public static void SetConverterParameter(UIElement element, object value)
        {
            element.SetValue(ConverterParameterProperty, value);
        }

        /// <summary>
        /// Gets the ConverterParameter attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>An object that can passed to the converter.</returns>
        public static object GetConverterParameter(UIElement element)
        {
            return element.GetValue(ConverterParameterProperty);
        }

        #endregion

        #region CommandAction

        /// <summary>
        /// Dependency Property for CommandAction.
        /// </summary>
        public static readonly DependencyProperty CommandActionProperty =
            DependencyProperty.RegisterAttached("CommandAction", typeof(CommandAction), typeof(CommandBridge),
            new PropertyMetadata(CommandAction.Disable, OnCommandActionPropertyChanged));

        private static void OnCommandActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                var data = GetElementData(element);
                data.CommandAction = (CommandAction)e.NewValue;
                UpdateCanExecute(element);
            }
        }

        /// <summary>
        /// Sets the CommandAction attached property.
        /// </summary>
        /// <param name="element">Element to attach.</param>
        /// <param name="value">A value of the CommandAction enumeration, which controls the command behavior on the element.</param>
        public static void SetCommandAction(UIElement element, CommandAction value)
        {
            element.SetValue(CommandActionProperty, value);
        }

        /// <summary>
        /// Gets the CommandAction attached property.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>A value of the CommandAction enumeration, which controls the command behavior on the element.</returns>
        public static CommandAction GetCommandAction(UIElement element)
        {
            return (CommandAction)element.GetValue(CommandActionProperty);
        }

        #endregion

        #endregion

        #region Private Functions

        #region GetElementData

        private static CommandedElementData GetElementData(DependencyObject obj)
        {
            if (obj == null) return null;
            CommandedElementData value = null;
            List<WeakReference> removeList = new List<WeakReference>();
            foreach (WeakReference wr in _elements.Keys)
            {
                if (wr.IsAlive)
                {
                    if (wr.Target == obj)
                    {
                        value = _elements[wr];
                    }
                }
                else
                {
                    removeList.Add(wr);
                }
            }
            foreach (WeakReference wr in removeList)
            {
                _elements.Remove(wr);
            }

            if (value != null)
                return value;

            var data = new CommandedElementData() { CommandAction = CommandAction.Disable };
            _elements.Add(new WeakReference(obj), data);
            return data;
        }
        
        #endregion

        #region CanExecuteChanged

        private static bool CommandHandlerExists(ICommand command)
        {
            foreach (var item in _elements)
            {
                if (item.Value.Command != null && item.Value.Command.Equals(command))
                {
                    return true;
                }
            }
            return false;
        }

        private static void CanExecuteChanged(object sender, EventArgs e)
        {
            var command = sender as ICommand;
            foreach (var item in _elements)
            {
                if (item.Key.IsAlive &&
                    item.Value.Command != null &&
                    item.Value.Command.Equals(command))
                {
                    UpdateCanExecute((UIElement)item.Key.Target);
                }
            }
        }

        #endregion

        #region UpdateCanExecute

        private static void UpdateCanExecute(UIElement element)
        {
            if (element == null) return;
            var data = GetElementData(element);
            if (data.Command == null) return;
            
            // check execution state
            bool canExecute = (data.Command != null) && data.Command.CanExecute(data.CommandParameter);

            // apply command action
            if (data.CommandAction == CommandAction.Disable && element is Control)
            {
                ((Control)element).IsEnabled = canExecute;
            }
            else if (data.CommandAction == CommandAction.Hide && element is FrameworkElement)
            {
                ((FrameworkElement)element).Visibility = (canExecute) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(data.PropertyName))
                return;

            // assign property values
            PropertyInfo info = element.GetType().GetProperty(data.PropertyName);
            if (info != null)
            {
                try
                {
                    // if no property values present and property is bool, assign it directly
                    if (info.PropertyType.Equals(typeof(bool)) && data.CanExecutePropertyValue == null && data.CanNotExecutePropertyValue == null)
                    {
                        info.SetValue(element, canExecute, new object[] { });
                    }
                    else
                    {
                        // assign property values
                        object value;
                        if (canExecute)
                            value = ConvertValue(data.CanExecutePropertyValue, info.PropertyType, data.Converter, data.ConverterParameter);
                        else
                            value = ConvertValue(data.CanNotExecutePropertyValue, info.PropertyType, data.Converter, data.ConverterParameter);
                        info.SetValue(element, value, new object[] { });
                    }
                }
                catch { }
            }
        }

        private static object ConvertValue(object propertyValue, Type targetType, IValueConverter converter, object converterParameter)
        {
            try
            {
                if (propertyValue.GetType().Equals(targetType))
                    return propertyValue;

                // use converter
                if (converter != null)
                {
                    return converter.Convert(propertyValue, targetType, converterParameter, System.Threading.Thread.CurrentThread.CurrentCulture);
                }
                else
                {
                    // try to use type converter via TypeConverterAttribute
                    var attribs = targetType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                    if (attribs != null && attribs.Length > 0)
                    {
                        var attrib = attribs[0] as TypeConverterAttribute;
                        Type converterType = Type.GetType(attrib.ConverterTypeName);
                        if (converterType != null)
                        {
                            TypeConverter typeConverter = Activator.CreateInstance<TypeConverter>();
                            if (typeConverter != null)
                                return typeConverter.ConvertTo(propertyValue, targetType);
                        }
                    }

                    // try to use BrushConverter
                    //if (typeof(System.Windows.Media.Brush).IsAssignableFrom(targetType))
                    //{
                    //    return new BrushConverter().Convert(propertyValue, targetType, null, System.Threading.Thread.CurrentThread.CurrentCulture);
                    //}

                    // handle nullable types
                    if (targetType.IsGenericType &&
                        targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        Type originalType = targetType.GetGenericTypeDefinition();
                        Type t1 = targetType.GetGenericArguments()[0];
                        Type constructed = originalType.MakeGenericType(t1);
                        object value = null;
                        if (propertyValue != null)
                        {
                            value = Convert.ChangeType(propertyValue, t1, System.Threading.Thread.CurrentThread.CurrentCulture);
                            return Activator.CreateInstance(constructed, value);
                        }
                        return value;
                    }
                    else
                    {
                        // try standard convertation
                        return Convert.ChangeType(propertyValue, targetType, System.Threading.Thread.CurrentThread.CurrentCulture);
                    }
                }
            }
            catch{}
            return null;
        }

        #endregion

        #region CreateHandler

        private static void CreateHandler(UIElement element, CommandedElementData elementData)
        {
            if (element == null || string.IsNullOrEmpty(elementData.EventName) || elementData.Command == null)
                return;
            if (elementData.Handler != null)
                RemoveHandler(element, elementData);

            // reflect the event
            EventInfo eventInfo = element.GetType().GetEvent(elementData.EventName);
            if (eventInfo == null)
                throw new InvalidOperationException(string.Format("The element \"{0}\" dosn't contain an event with the name \"{1}\"!", element.GetType().Name, elementData.EventName));
            Type handlerType = eventInfo.EventHandlerType;
            MethodInfo invokeMethod = handlerType.GetMethod("Invoke");
            ParameterInfo[] parms = invokeMethod.GetParameters();
            Type eventArgsType = parms[1].ParameterType;

            // create generic handler
            var genericHandlerType = typeof(GenericHandler<>).MakeGenericType(new Type[] { eventArgsType });
            var handler = Activator.CreateInstance(genericHandlerType, new object[] { elementData.Command, elementData.CommandParameter });// eventArgsType)
            var handlerMethod = handler.GetType().GetMethod("OnCompleted");
            var handlerDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, handler, handlerMethod);

            // add handler
            eventInfo.AddEventHandler(element, handlerDelegate);
            elementData.Handler = new WeakReference(handlerDelegate);
        }

        #endregion

        #region RemoveHandler

        private static void RemoveHandler(UIElement element, CommandedElementData elementData)
        {
            if (element != null &&
                !string.IsNullOrEmpty(elementData.EventName) &&
                elementData.Handler != null &&
                elementData.Handler.IsAlive)
            {
                EventInfo eventInfo = element.GetType().GetEvent(elementData.EventName);
                eventInfo.RemoveEventHandler(element, (Delegate)elementData.Handler.Target);
            }
        }

        #endregion

        #endregion

        #region Public Functions

        /// <summary>
        /// Removes all event handlers of all assigned controls.
        /// </summary>
        public void RemoveHandlers()
        {
            foreach (var item in _elements)
            {
                if (item.Value.Handler != null && item.Key.IsAlive)
                {
                    RemoveHandler((UIElement)item.Key.Target, item.Value);
                }
            }
            _elements.Clear();
        }

        #endregion

        #region CommandedElementData
        
        class CommandedElementData : IDisposable
        {
            public string EventName { get; set; }
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }
            public CommandAction CommandAction { get; set; }
            public string PropertyName { get; set; }
            public object CanExecutePropertyValue { get; set; }
            public object CanNotExecutePropertyValue { get; set; }
            public IValueConverter Converter { get; set; }
            public object ConverterParameter { get; set; }
            public WeakReference Handler { get; set; }

            #region IDisposable Members

            public void Dispose()
            {
                this.Command = null;
                this.CommandParameter = null;
                this.CanExecutePropertyValue = null;
                this.CanNotExecutePropertyValue = null;
                this.Converter = null;
                this.ConverterParameter = null;
                this.Handler = null;
            }

            #endregion
        }
        
        #endregion

        #region GenericHandler
        
        class GenericHandler<TEventArgs>
        {
            private ICommand _command;
            private object _commandParams;

            public GenericHandler(
                ICommand command,
                object commandParams)
            {
                _command = command;
                _commandParams = commandParams;
            }

            public void OnCompleted(object sender, TEventArgs e)
            {
                var parms = (_commandParams != null) ? _commandParams : sender;
                if (_command.CanExecute(parms))
                    _command.Execute(parms);
            }
        }

        #endregion

    }
}
