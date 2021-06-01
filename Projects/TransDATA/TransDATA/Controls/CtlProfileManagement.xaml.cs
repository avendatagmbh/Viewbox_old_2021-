// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-29
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Base.Localisation;
using Config.Interfaces.DbStructure;
using TransDATA.Models;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlProfileManagement.xaml
    /// </summary>
    public partial class CtlProfileManagement : UserControl {
        public CtlProfileManagement() {
            InitializeComponent();
        }

        private ProfileManagementModel Model {
            get { return DataContext as ProfileManagementModel; }
        }

        private void DeleteProfile() {
            string msg = string.Format(ResourcesCommon.RequestDeleteProfile,
                                       Model.SelectedItem.Name ?? "<" + ResourcesCommon.NoName + ">");
            if (MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.Yes) {
                Model.DeleteSelectedItem();
            }
        }

        private void BtnAddItemClick(object sender, RoutedEventArgs e) {
            Model.AddItem();
            lbItems.ScrollIntoView(lbItems.SelectedItem);
        }

        private void BtnDeleteItemClick(object sender, RoutedEventArgs e) {
            DeleteProfile();
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e) {
            //if(Model.SelectedItem != null) {
            //    Model.SelectedItem.Save();
            //    Model.SelectedItem.InputConfig.Save();
            //    Model.SelectedItem.OutputConfig.Save();
            //    Model.SelectedItem.MailConfig.Save();
            //}
            Model.Close();
            UIHelpers.TryFindParent<Window>(this).Close();
        }

        private void lbItems_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                e.Handled = true;
                DeleteProfile();
            }
        }

        private void lbItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //ctlProfile.DataContext = lbItems.SelectedItem == null
            //                             ? null
            //                             : new ProfileDetailModel((IProfile) lbItems.SelectedItem);
        }

        private void BtnCopyItemClick(object sender, RoutedEventArgs e) { Model.CopySelectedProfile(); }
    }
}