/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-28      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SystemDb;
using ProjectDb.Enums;
using ViewBuilder.Models;
using System.Threading;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderBusiness;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.EventArgs;
using ProjectDb.Tables;
using Utils;

namespace ViewBuilder.Windows {
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
