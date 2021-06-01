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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutomationWPF.Windows;
using AutomationWPF.ViewModels;

namespace AutomationWPF.Controls
{
    /// <summary>
    /// Interaction logic for CtlMain.xaml
    /// </summary>
    public partial class CtlMain : UserControl
    {
        public CtlMain()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();

            ctlEventConfigTreeView.DataContext = new EventConfigViewModel();
        }

        private void btnMenuNewConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSummary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoViewModel m_IVM = new InfoViewModel();

            DlgInfo m_Info = new DlgInfo();
            m_Info.DataContext = m_IVM;
            m_Info.ShowDialog();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunSelecteds_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            DlgSettings m_Settings = new DlgSettings();
            m_Settings.ShowDialog();
        }
    }
}
