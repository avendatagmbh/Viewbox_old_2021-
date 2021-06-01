using System;
using System.Windows;
using eBalanceKit.Models.Assistants;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für BalListImpAssistPageSummary.xaml
    /// </summary>
    public partial class BalListImpAssistError : BalListImpAssistPageBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImpAssistError" /> class.
        /// </summary>
        public BalListImpAssistError() { InitializeComponent(); }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        private BalListImportAssistantModel Model { get { return DataContext as BalListImportAssistantModel; } }

        /// <summary>
        /// Occurs when [next button is clicked].
        /// </summary>
        public event EventHandler NextButtonClicked;

        /// <summary>
        /// Occurs when [previous button is clicked].
        /// </summary>
        public event EventHandler PreviousButtonClicked;

        /// <summary>
        /// Called when [button is clicked].
        /// </summary>
        /// <param name="ToForward">if set to <c>true</c> [to forward].</param>
        private void OnButtonClicked(bool ToForward) {
            // If we clicked OK, then skip the notification and continue importing.
            if (ToForward && NextButtonClicked != null) {
                NextButtonClicked(this, new System.EventArgs());
            }

                // Else navigate to the previous page of the wizard.
            else if (!ToForward && PreviousButtonClicked != null) {
                PreviousButtonClicked(this, new System.EventArgs());
            }
        }

        /// <summary>
        /// Handles the Click event of the Btn_ImportContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Btn_ImportContinue_Click(object sender, RoutedEventArgs e) { OnButtonClicked(true); }

        /// <summary>
        /// Handles the Click event of the Btn_ImportCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Btn_ImportCancel_Click(object sender, RoutedEventArgs e) { OnButtonClicked(false); }
    }
}