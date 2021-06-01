using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace ViewboxAdmin.ViewModels
{
    /// <summary>
    /// This class is used as a dependency in order to make viewmodel classes testable. 
    /// I used NUnit for testing, and a direct call to Apllication.Current... causes null ref exception from test runner... that is why i abstarct this functionality behind an interface
    /// 
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        public void Invoke(Action action) {
            Application.Current.Dispatcher.Invoke(
                   DispatcherPriority.Background,
                   new Action(action));
        }
    }
}
