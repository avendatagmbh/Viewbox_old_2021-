using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Utils;
using System.Windows;

namespace eBalanceKitBase.Structures
{
    /// <summary>
    /// usage:
    /// <code>
    /// using (new MainWindowLocker(new LockableSourceParameter(...)))
    ///                                   {
    ///                                       ...Your code...
    ///                                   }
    /// </code>
    /// </summary>
    public class MainWindowLocker : IDisposable
    {
        #region Properties

        private readonly bool _disableNotifications;
        private readonly bool _lockSources;
        private readonly LockableSourceParameter _parameters;
        private Window _parentWindow;
        private Action<LockableSourceParameter> _unLockSourceAction;

        #endregion Properties

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="disableNotifications">Disable the single properties notification events, if the class extends NotifyPropertyChangedBase class</param>
        /// <param name="lockSources">Locks the main forms Treeviews and ListBoxes ItemsSources property</param>
        public MainWindowLocker(
            Window parentWindow, 
            LockableSourceParameter parameters, 
            Action<LockableSourceParameter> lockSourceAction, 
            Action<LockableSourceParameter> unLockSourceAction, 
            bool disableNotifications = true, 
            bool lockSources = true)
        {
            _disableNotifications = disableNotifications;
            _lockSources = lockSources;
            _parameters = parameters;
            _parentWindow = parentWindow;
            _unLockSourceAction = unLockSourceAction;

            if (_disableNotifications)
                NotificationHandling.DisableNotificationEvents();
            if (_lockSources)
                //GlobalResources.MainWindow.Dispatcher.Invoke(new Action(() => ((MainWindowModel)GlobalResources.MainWindow.DataContext).LockSource(_parameters)));
                _parentWindow.Dispatcher.Invoke(new Action(() => lockSourceAction(_parameters)));
        }

        /// <summary>
        /// After using it, it UnLocks the sources, and enables the events
        /// </summary>
        public void Dispose()
        {
            if (_lockSources)
                //GlobalResources.MainWindow.Dispatcher.Invoke(new Action(() => ((MainWindowModel)GlobalResources.MainWindow.DataContext).UnLockSource(_parameters)));
                _parentWindow.Dispatcher.Invoke(new Action(() => _unLockSourceAction(_parameters)));
            if (_disableNotifications)
                NotificationHandling.EnableNotificationEvents();
        }
        #endregion Methods
    }
}
