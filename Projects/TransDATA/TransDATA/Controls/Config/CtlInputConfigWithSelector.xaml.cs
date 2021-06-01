using System;
using System.Windows;
using System.Windows.Controls;
using TransDATA.Models;
using TransDATA.Windows;

namespace TransDATA.Controls.Config
{
    /// <summary>
    /// Interaktionslogik für CtlInputConfigWithSelector.xaml
    /// </summary>
    public partial class CtlInputConfigWithSelector : UserControl
    {
        public CtlInputConfigWithSelector()
        {
            InitializeComponent();
        }

        private void OpenHistoryButtonClick(object sender, RoutedEventArgs e)
        {
            DlgHistorySelector dlg = new DlgHistorySelector
            {
                DataContext =
                    new HistorySelectorModel(
                        HistorySelectorModel.HistorySelectorModeEnum.InputMode,
                        ((EditProfileModel)DataContext).Profile)
            };
            dlg.ShowDialog();
            try {
                ctlInputCfg.DatabaseInput.UpdateUi(((EditProfileModel)DataContext).Profile);
            } catch (Exception) {
            }

            try {
                ctlInputCfg.CsvInput.UpdateUi(((EditProfileModel)DataContext).Profile);

            } catch (Exception) {
            }
        }
    }
}
