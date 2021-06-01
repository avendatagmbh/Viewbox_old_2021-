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
using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;
using DbAccess;


namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgDatabase.xaml
    /// </summary>
    public partial class DlgDatabase : Window {

        public DlgDatabase(DbConfig configDatabase) {
            InitializeComponent();
            ctrlConfigDb.Init(configDatabase);
        }

        /*****************************************************************************************************/
        
        #region properties

        public DbConfig ConfigDatabase {
            get { return (DbConfig)ctrlConfigDb.DataContext; }
            set { ctrlConfigDb.Init(value); }
        }

        #endregion properties

        /****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when the ok-button has been pressed.
        /// </summary>
        public event EventHandler Ok;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when the ok-button has been pressed.
        /// </summary>
        private void OnOk() {
            if (Ok != null) Ok(this, null);
        }

        #endregion eventTrigger

        /*******************************************************************************************************/
        #region eventHandler

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            
            if (ctrlConfigDb.ValidateInput()  && ctrlConfigDb.TestConnection()) {
                OnOk();
                this.Close();
            } else {
                MessageBox.Show(
                    "Nicht alle obligatorischen Datenfelder sind ausgefüllt!",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        #endregion eventHandler

        /*******************************************************************************************************/

        #region methods  

        #endregion methods

        /******************************************************************************************/
    }
}
