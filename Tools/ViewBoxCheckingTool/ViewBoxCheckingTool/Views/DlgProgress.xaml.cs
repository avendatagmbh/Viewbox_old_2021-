// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using eBalanceKitBase.Structures;

namespace eBalanceKitBase.Windows
{

    /// <summary>
    /// Interaktionslogik für DlgProgress.xaml
    /// </summary>
    public partial class DlgProgress
    {
        public DlgProgress()
        {
            InitializeComponent();
            ProgressInfo = new ProgressInfo { Parent = this };
            DataContext = ProgressInfo;
        }

        private void SetVisibility()
        {
            try {
                if (String.IsNullOrEmpty(ProgressInfo.WaitMessage)) {
                    txtWait.Visibility = Visibility.Hidden;
                    //pbProgress.Visibility = Visibility.Visible;
                } else {
                    txtWait.Visibility = Visibility.Visible;
                    //pbProgress.Visibility = Visibility.Hidden;                    
                }
            } catch {
            }
        }

        public DlgProgress(Window owner)
            : this()
        {
            Owner = owner;
        }

        #region events
        public event EventHandler Finished;
        public void OnFinished()
        {
            if (Finished != null) Finished(this, new EventArgs());
        }
        #endregion

        private ProgressInfo _progressinfo;
        public ProgressInfo ProgressInfo
        {
            get { return _progressinfo; }
            set
            {
                _progressinfo = value;
                DataContext = ProgressInfo;
                SetVisibility();
            }
        }

        public void Execute(Action action)
        {
            var action1 = new Action(() => {
                action();
                Dispatcher.BeginInvoke(new Action(() => {
                    OnFinished();
                    Close();
                }), DispatcherPriority.Loaded);
            });

            var t = new Thread(new ThreadStart(action1)) {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            t.Start();
            Show();
        }
        public void ExecuteModalWithDispatcher(Action<Dispatcher> action)
        {
            var action1 = new Action(() =>
            {
                action(Dispatcher);

                Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Loaded);
            });

            new Thread(new ThreadStart(action1))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            }.Start();
            ShowDialog();
        }
        public void ExecuteModal(Action action)
        {
            var action1 = new Action(() =>
            {
                action();
                Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Loaded);
            });

            new Thread(new ThreadStart(action1))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            }.Start();
            ShowDialog();
        }

        public void ExecuteModal(Action action, ApartmentState state)
        {
            var action1 = new Action(() => {
                action();
                Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Loaded);
            });

            var thread = new Thread(new ThreadStart(action1)) {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            thread.TrySetApartmentState(state);
            thread.Start();
            ShowDialog();
        }

        public void ExecuteModal(Action<object> action, object param)
        {

            var action1 = new Action<object>(p => {
                action(p);
                Dispatcher.BeginInvoke(new Action(Close));
            });

            new Thread(new ParameterizedThreadStart(action1)) {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            }.Start(param);

            ShowDialog();
        }
    }
}