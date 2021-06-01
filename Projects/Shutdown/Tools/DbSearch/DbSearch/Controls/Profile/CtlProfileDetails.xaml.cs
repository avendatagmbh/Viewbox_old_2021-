using System.Windows.Controls;
using DbSearch.Models.Profile;

namespace DbSearch.Controls.Profile {

    /// <summary>
    /// Interaktionslogik für CtlProfileDetails.xaml
    /// </summary>
    public partial class CtlProfileDetails : UserControl {
        ProfileModel Model { get { return DataContext as ProfileModel; } }

        public CtlProfileDetails() {
            InitializeComponent();

            
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                ctlMysqlDatabase.DataContext = new DatabaseModel(Model.Profile.DbConfigView) { IsReadOnly = true };    
            }
        }
    }
}
