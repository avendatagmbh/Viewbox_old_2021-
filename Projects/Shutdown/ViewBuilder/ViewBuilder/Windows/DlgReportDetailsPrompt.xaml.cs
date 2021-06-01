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

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaction logic for DlgReportDetailsPrompt.xaml
    /// </summary>
    public partial class DlgReportDetailsPrompt : Window
    {
        public event Action CancelClicked;

        public DlgReportDetailsPrompt()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
//            if (string.IsNullOrEmpty(txtProjectManager.Text)) throw new ArgumentNullException("Project Manager field is empty.");
//            if (string.IsNullOrEmpty(txtProjectNumber.Text)) throw new ArgumentNullException("Project Number field is empty.");

            ProjectManager = txtProjectManager.Text;
            ProjectNumber = txtProjectNumber.Text;
            DialogResult = true;
            Close();
        }

        public string ProjectManager { get; private set; }

        public string ProjectNumber { get; private set; }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
            {
                CancelClicked();
            }
            Close();
        }
    }
}
