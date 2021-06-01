using System.Windows;
using DbAccess.Structures;
using ViewAssistantBusiness.Models;

namespace ViewAssistant.Controls
{
    /// <summary>
    /// Interaction logic for CtlDatabaseSummary.xaml
    /// </summary>
    public partial class CtlDatabaseSummary
    {
        public CtlDatabaseSummary()
        {
            InitializeComponent();
        }

        private void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var db = e.NewValue as DbConfig;
            if (db != null && db.DbType == "Access")
            {
                ServerField.Visibility = Visibility.Visible;
                UsernameField.Visibility = Visibility.Collapsed;
                DatabaseField.Visibility = Visibility.Collapsed;
                PortField.Visibility = Visibility.Collapsed;
                return;
            }
            ServerField.Visibility = Visibility.Visible;
            UsernameField.Visibility = Visibility.Visible;
            DatabaseField.Visibility = Visibility.Visible;
            PortField.Visibility = Visibility.Visible;
        }
    }
}
