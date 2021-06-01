using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DbAnalyser.MySqlDBCommands;
using DbAnalyser.Profile;

namespace DbAnalyser
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow
    {
        public ConnectionWindow()
        {
            InitializeComponent();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void sourceDatabase_DropDownOpened(object sender, EventArgs e)
        {
            string host = sourceHost.Text;
            string port = sourcePort.Text;
            string uid = sourceUid.Text;
            string pass = sourcePassword.Password;

            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(port))
            {
                string connStr = "SERVER=" + host + ";PORT=" + port + ";UID=" + uid + ";PASSWORD=" + pass + ";";
                if (DbConnection.TestConnection(connStr))
                {
                    DbConnection.SourceConnectionString = connStr;
                    List<string> dbList = DbSelectCommnands.GetDatabaseList(connStr);
                    sourceDatabase.ItemsSource = dbList;
                }
            }
        }

        private void destDatabase_DropDownOpened(object sender, EventArgs e)
        {
            string host = destHost.Text;
            string port = destPort.Text;
            string uid = destUid.Text;
            string pass = destPassword.Password;

            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(port))
            {
                string connStr = "SERVER=" + host + ";PORT=" + port + ";UID=" + uid + ";PASSWORD=" + pass + ";";
                if (DbConnection.TestConnection(connStr))
                {
                    DbConnection.DestinationConnectionString = connStr;
                    List<string> dbList = DbSelectCommnands.GetDatabaseList(connStr);
                    destDatabase.ItemsSource = dbList;
                }
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ApplySettings())
            {
                Close();
            }
        }

        private bool ApplySettings()
        {
            string host = sourceHost.Text;
            string port = sourcePort.Text;
            string uid = sourceUid.Text;
            string pass = sourcePassword.Password;

            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(port))
            {
                string connStr = "SERVER=" + host + ";PORT=" + port + ";UID=" + uid + ";PASSWORD=" + pass + ";";
                if (DbConnection.TestConnection(connStr))
                {
                    DbConnection.SourceConnectionString = connStr;
                }
                else
                {
                    return false;
                }
            }

            host = destHost.Text;
            port = destPort.Text;
            uid = destUid.Text;
            pass = destPassword.Password;

            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(port))
            {
                string connStr = "SERVER=" + host + ";PORT=" + port + ";UID=" + uid + ";PASSWORD=" + pass + ";";
                if (DbConnection.TestConnection(connStr))
                {
                    DbConnection.DestinationConnectionString = connStr;
                }
                else
                {
                    return false;
                }
            }

            DbConnection.Treshold = Int32.Parse(tresholdInput.Text);

            DbConnection.SourceDatabase = sourceDatabase.Text;
            DbConnection.AnalysticDatabase = destDatabase.Text;
            DbConnection.FinalDatabase = FinalDbInput.Text;
            DbConnection.FinalSystemDatabase = DbConnection.FinalDatabase + "_system";
            DbConnection.AllowedThreads = Int32.Parse(allowedThreads.Text);
            DbConnection.InsertStepSize = long.Parse(insertStepSize.Text);
            DbConnection.IsPaused = false;

            DbConnection.FromRowCount = long.Parse(fromRowCount.Text);
            DbConnection.ToRowCount = long.Parse(toRowCount.Text);

            return true;
        }

        private void sourceDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null) FinalDbInput.Text = comboBox.SelectedItem + "_final";
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            var profile = new ProfileInfo
            {
                sourceHost = sourceHost.Text,
                sourceUid = sourceUid.Text,
                sourcePassword = sourcePassword.Password,
                sourceDatabase = sourceDatabase.Text,
                destHost = destHost.Text,
                destUid = destUid.Text,
                destPassword = destPassword.Password,
                destDatabase = destDatabase.Text,
                finalDatabaseName = FinalDbInput.Text
            };

            try
            {
                profile.FromRowCount = long.Parse(fromRowCount.Text);
                profile.ToRowCount = long.Parse(toRowCount.Text);
                profile.sourcePort = Int32.Parse(sourcePort.Text);
                profile.destPort = Int32.Parse(destPort.Text);
                profile.treshold = Int32.Parse(tresholdInput.Text);

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "config",
                    DefaultExt = ".xml",
                    Filter = "Profile configuration (.xml)|*.xml"
                };

                var result = dlg.ShowDialog();
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;
                    var writer = new System.Xml.Serialization.XmlSerializer(typeof(ProfileInfo));
                    var file = new System.IO.StreamWriter(filename);
                    writer.Serialize(file, profile);
                    file.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The inserted value isn't integer!");
            }            
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "Profile configuration (*.xml)|*.xml"
            };

            // Set filter for file extension and default file extension 


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                var reader = new System.Xml.Serialization.XmlSerializer(typeof(ProfileInfo));
                var file = new System.IO.StreamReader(filename);
                var profile = (ProfileInfo)reader.Deserialize(file);

                sourceHost.Text = profile.sourceHost;
                sourcePort.Text = profile.sourcePort.ToString(CultureInfo.InvariantCulture);
                sourceUid.Text = profile.sourceUid;
                sourcePassword.Password = profile.sourcePassword;
                sourceDatabase.Text = profile.sourceDatabase;

                destHost.Text = profile.destHost;
                destPort.Text = profile.destPort.ToString(CultureInfo.InvariantCulture);
                destUid.Text = profile.destUid;
                destPassword.Password = profile.destPassword;
                destDatabase.Text = profile.destDatabase;

                tresholdInput.Text = profile.treshold.ToString(CultureInfo.InvariantCulture);
                FinalDbInput.Text = profile.finalDatabaseName;
            }
        }

    }
}
