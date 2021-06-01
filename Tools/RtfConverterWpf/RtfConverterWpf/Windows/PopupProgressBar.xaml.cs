/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-28      initial implementation
 *************************************************************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace DatabaseExporter.Windows {
    /// <summary>
    /// Interaktionslogik für Progress.xaml
    /// </summary>
    public partial class PopupProgressBar : Window {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupUpdateScripts"/> class.
        /// </summary>
        public PopupProgressBar() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            (DataContext as BackgroundWorker).WorkerSupportsCancellation = true;
            (DataContext as BackgroundWorker).CancelAsync();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Border control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Thread.Sleep(1000);
        }
    }
}
