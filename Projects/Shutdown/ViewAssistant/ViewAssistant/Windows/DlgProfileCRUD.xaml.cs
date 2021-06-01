using System.Linq;
using System.Windows;
using System.Windows.Input;
using ViewAssistantBusiness;
using Base.Localisation;
using ViewAssistantBusiness.Models;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for DlgProfileCRUD.xaml
    /// </summary>
    public partial class DlgProfileCRUD
    {
        private ProfileManagementModel Model
        {
            get { return DataContext as ProfileManagementModel; }
        }

        public DlgProfileCRUD()
        {
            InitializeComponent();
        }

        private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteProfile();
        }

        private void DeleteProfile()
        {
            string msg = string.Format(ResourcesCommon.RequestDeleteProfile, Model.SelectedItem.Name ?? "<" + ResourcesCommon.NoName + ">");
            if (MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Model.DeleteSelectedItem();
            }
        }

        private void btnCopyItem_Click(object sender, RoutedEventArgs e)
        {
            Model.CopySelectedProfile();
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            var model = Model.SelectedItem.Clone();
            var dialog = new DlgProfile {DataContext = model};
            dialog.assistantControl.Finish +=
                (x, y) =>
                    {
                        model.Id = Model.SelectedItem.Id;
                        Model.EditItem(model);
                        dialog.Close();
                    };
            dialog.ShowDialog();
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            var model = new ProfileConfigModel();
            var dialog = new DlgProfile {DataContext = model};
            dialog.assistantControl.Finish +=
                (x, y) =>
                    {
                        Model.AddItem(model);
                        dialog.Close();
                    };
            dialog.ShowDialog();
        }

        private void BtnInfoClick(object sender, RoutedEventArgs e)
        {
            var imageButton = sender as AvdWpfControls.ImageButton;
            if (imageButton != null)
            {
                long id = System.Convert.ToInt64(imageButton.Tag.ToString());
                Model.SelectedItem = Model.Items.FirstOrDefault(element => element.Id == id);
                var dialog = new DlgViewProfileSettings();
                dialog.DataContext = Model.SelectedItem;
                dialog.ShowDialog();
            }
        }
    }
}
