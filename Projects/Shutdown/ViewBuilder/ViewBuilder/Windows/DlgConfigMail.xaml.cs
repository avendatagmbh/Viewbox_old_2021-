/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-15      initial implementation
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
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Windows {

    /// <summary>
    /// Interaktionslogik für DlgConfigMail.xaml
    /// </summary>
    public partial class DlgConfigMail : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgConfigMail"/> class.
        /// </summary>
        /// <param name="configMail">The config mail.</param>
        public DlgConfigMail(MailConfig configMail) {
            InitializeComponent();
            this.DataContext = configMail;
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when the ok-button has been pressed.
        /// </summary>
        public event EventHandler Ok;

        #endregion events

        /*****************************************************************************************************/

        #region eventsTrigger

        /// <summary>
        /// Called when the ok-button has been pressed.
        /// </summary>
        private void OnOk() {
            UpdateBindingSources();
            if (Ok != null) Ok(this, null);
        }

        #endregion eventsTrigger

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the KeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {            
            OnOk();
            this.Close();
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            this.DataContext = null;
        }
        
        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the binding sources.
        /// </summary>
        private void UpdateBindingSources() {
            chkSendMailOnError.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            chkSendMailOnViewFinished.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            chkSendFinalReport.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            chkSendDailyReport.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();

            if (optOncePerDay.IsChecked == true) optOncePerDay.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
            else if (optTwicePerDay.IsChecked == true) optTwicePerDay.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
        }

        #endregion methods

    }
}
