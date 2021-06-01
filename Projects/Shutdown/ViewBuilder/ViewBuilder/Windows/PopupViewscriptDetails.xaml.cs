/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-31      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using ProjectDb.Tables;

namespace ViewBuilder.Windows {

    /// <summary>
    /// Interaktionslogik für PopupViewscriptDetails.xaml
    /// </summary>
    public partial class PopupViewscriptDetails : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupViewscriptDetails"/> class.
        /// </summary>
        public PopupViewscriptDetails() {
            InitializeComponent();
            _showDelayedThread = new Thread(ShowDelayed);
            _showDelayedThread.Start();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PopupViewscriptDetails_DataContextChanged);
        }

        void PopupViewscriptDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Viewscript script = (Viewscript)e.NewValue;
            if (script.LastError.Length > 0) {
                this.Height += 300;
            }
        }

        private delegate void voidDelegate();

        private Thread _showDelayedThread;
        private bool _closed;

        /// <summary>
        /// Delayed showup of this window.
        /// </summary>
        private void ShowDelayed() {
            try {
                Thread.Sleep(300);
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                delegate {
                    try {
                        if (!_closed) {
                            this.Show();
                            Owner.Focus();
                        }
                    } catch (Exception) { }
                }));
            } catch (Exception) { }
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            _closed = true;
            try
            {
                _showDelayedThread.Abort();
                while (_showDelayedThread.IsAlive) Thread.Sleep(10);
            } 
            catch
            {
                Window_Closing(sender, e);
            }
        }
    }
}
