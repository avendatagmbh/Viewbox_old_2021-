/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-28      initial implementation
 *************************************************************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using Utils;

namespace WpfControlsSample.Windows {
    /// <summary>
    /// Interaktionslogik für Progress.xaml
    /// </summary>
    public partial class DlgProgress : Window {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupUpdateScripts"/> class.
        /// </summary>
        public DlgProgress() {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DlgProgress_DataContextChanged);
        }

        void DlgProgress_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ProgressCalculator progress = DataContext as ProgressCalculator;
            if(progress != null)
            {
                progress.RunWorkerCompleted -= progress_RunWorkerCompleted;
                progress.RunWorkerCompleted += progress_RunWorkerCompleted;
            }
        }

        void progress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ProgressCalculator progress = DataContext as ProgressCalculator;
            if(progress != null)
                progress.CancelAsync();
        }
    }
}
