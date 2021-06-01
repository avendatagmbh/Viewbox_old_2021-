/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-20      initial implementation
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
using System.Threading;
using ViewBuilderBusiness;
using ViewBuilderBusiness.Structures;
using System.Collections.ObjectModel;
using ViewBuilderBusiness.EventArgs;

namespace ViewBuilder.Windows {
    
    /// <summary>
    /// Interaktionslogik für DlgLoadViewscripts.xaml
    /// </summary>
    public partial class DlgScriptErrors : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgLoadViewscripts"/> class.
        /// </summary>
        public DlgScriptErrors() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        /// <value>The profile.</value>
        private ProfileConfig Profile { get; set; }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Loads the scripts.
        /// </summary>
        public void LoadScripts() {
            new Thread(LoadScriptsThread).Start();   
        }

        /// <summary>
        /// Loads the scripts.
        /// </summary>
        private void LoadScriptsThread() {
        }

        /// <summary>
        /// Handles the KeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                this.Close();
            }
        }
    }
}
