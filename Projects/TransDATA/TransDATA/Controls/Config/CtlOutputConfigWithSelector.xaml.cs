using System.Windows;
using System.Windows.Controls;
using TransDATA.Models;
using TransDATA.Windows;

namespace TransDATA.Controls.Config
{
    /// <summary>
    /// Interaktionslogik für CtlOutputConfigWithSelector.xaml
    /// </summary>
    public partial class CtlOutputConfigWithSelector : UserControl
    {
        public CtlOutputConfigWithSelector()
        {
            InitializeComponent();
        }

        private void OpenHistoryButtonClick(object sender, RoutedEventArgs e)
        {
            DlgHistorySelector dlg = new DlgHistorySelector
            {
                DataContext =
                    new HistorySelectorModel(
                        HistorySelectorModel.HistorySelectorModeEnum.OutputMode,
                        ((EditProfileModel)DataContext).Profile)
            };
            dlg.ShowDialog();
            ctlOutputCfg.DatabaseOutput.UpdateUi(((EditProfileModel)DataContext).Profile);
            ctlOutputCfg.GdpduOutput.UpdateUi(((EditProfileModel)DataContext).Profile);
            ctlOutputCfg.CsvOutput.UpdateUi(((EditProfileModel)DataContext).Profile);
        }
    }
}
