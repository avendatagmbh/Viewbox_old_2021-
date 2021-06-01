using System.Windows;

namespace DbSearch.Windows {
    /// <summary>
    /// Select a saved key for the foreign key searching
    /// </summary>
    public partial class DlgSelectTable : Window {
        #region Constructor
            /// <summary>
            /// Constructor
            /// </summary>
            public DlgSelectTable() {
                InitializeComponent();
            }
        #endregion Constructor

        #region Events
            /// <summary>
            /// The cancel event
            /// </summary>
            /// <param name="sender">Sender object</param>
            /// <param name="e">Event</param>
            private void btnCancel_Click(object sender, RoutedEventArgs e) {
                DialogResult = false; 
                Close();
            }

            /// <summary>
            /// The submit event
            /// </summary>
            /// <param name="sender">Sender object</param>
            /// <param name="e">Event</param>
            private void btnOk_Click(object sender, RoutedEventArgs e) {
                DialogResult = true;
                Close();
            }
        #endregion Events
    }
}
