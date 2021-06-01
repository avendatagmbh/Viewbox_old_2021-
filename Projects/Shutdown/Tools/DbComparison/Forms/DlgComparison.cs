using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DbAccess;
using DbComparison.Business;
using Avd.Database.DbSearch.Config;

namespace DbComparison.Forms {
    public partial class DlgComparison : Form {
        private ConfigDatabaseComparer Config { get; set; }

        public DlgComparison(ConfigDatabaseComparer config) {
            InitializeComponent();
            this.Config = config;

            //dbConfigControl1.User = dbConfigControl2.User = "root";
            //dbConfigControl1.Password = dbConfigControl2.Password = "avendata";
            //teDbName1.Text = "comparison1";
            //teDbName2.Text = "comparison2";
            dbConfigControl1.ShowDbName = true;
            dbConfigControl2.ShowDbName = true;

            PopulateControlFromConfig(dbConfigControl1, config.Database1);
            PopulateControlFromConfig(dbConfigControl2, config.Database2);
            //teDbName1.Text = config.Database1.DbName;
            //teDbName2.Text = config.Database2.DbName;
            //dbConfigControl1.Host = "dbdbschenker"; dbConfigControl2.Host = "dbbeiersdorf";
            //dbConfigControl1.User = dbConfigControl2.User = "root";
            //dbConfigControl1.Password = dbConfigControl2.Password = "avendata";
            //teDbName1.Text = "schenker";
            //teDbName2.Text = "beiersdorf";


            //txtCsvOutputDir.Text = "C:\\Users\\beh\\Desktop\\csv-export";
            txtCsvOutputDir.Text = config.OutputDir;
        }

        void PopulateControlFromConfig(AvdControls.DbConfig control, Avd.Database.DbAccess.Config.ConfigDatabase config) {
            control.Host = config.Hostname;
            control.Password = config.Password;
            control.User = config.UserName;
            control.DbName = config.DbName;
            control.DbType = (DatabaseTypes)Enum.Parse(typeof(DatabaseTypes), config.DbType);
        }

        Avd.Database.DbAccess.Config.ConfigDatabase DbAccessConfigToAvdConfig(ConfigDatabase database) {
            Avd.Database.DbAccess.Config.ConfigDatabase result = new Avd.Database.DbAccess.Config.ConfigDatabase() {
                DbName = database.DbName,
                DbType = database.DbType.ToString(),
                UserName = database.UserName,
                Password = database.Password,
                Hostname = database.Hostname,
                Changed = true
            };
            return result;
        }

        private void btnStartComparison_Click(object sender, EventArgs e) {
            ConfigDatabase dbConfig1 = dbConfigControl1.GetConfig();
            ConfigDatabase dbConfig2 = dbConfigControl2.GetConfig();
            //dbConfig1.DbName = teDbName1.Text;
            //dbConfig2.DbName = teDbName2.Text;


            try {
                this.Config.Database1 = DbAccessConfigToAvdConfig(dbConfig1);
                this.Config.Database2 = DbAccessConfigToAvdConfig(dbConfig2);
                this.Config.OutputDir = txtCsvOutputDir.Text;

                DoComparison(dbConfig1, dbConfig2);
                //DoComparison();
                //MessageBox.Show(this, "Pdf erstellt.");
            }
            catch (Exception ex) {
                MessageBox.Show(this, "Fehler beim Herstellen der Datenbankverbindung:" + Environment.NewLine + ex.Message);
            }
        }

        private void DoComparison(ConfigDatabase dbConfig1, ConfigDatabase dbConfig2) {
        
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            this.Enabled = false;
            progressBar.Minimum = 1;
            progressBar.Maximum = 100;
            progressBar.Value = 1;
            progressBar.Visible = true;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync(new KeyValuePair<ConfigDatabase,ConfigDatabase>(dbConfig1, dbConfig2));

        }

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressBar.Value = e.ProgressPercentage;
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.Enabled = true;
            if(!e.Cancelled && e.Error == null)
                MessageBox.Show(this, "Pdf und Csv Dateien erstellt.","", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if(e.Error != null)
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten:" + Environment.NewLine + e.Error.Message, "", MessageBoxButtons.OK,MessageBoxIcon.Error);
            progressBar.Visible = false;
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            KeyValuePair<ConfigDatabase, ConfigDatabase> pair = (KeyValuePair<ConfigDatabase, ConfigDatabase>)e.Argument ;
            ConfigDatabase dbConfig1 = pair.Key;
            ConfigDatabase dbConfig2 = pair.Value;
            
            //dbConfig1.DbName = teDbName1.Text;
            //dbConfig2.DbName = teDbName2.Text;

            if (txtCsvOutputDir.Text.Length == 0)
                throw new Exception("Kein Ausgabe Verzeichnis angegeben");

            using (IDatabase conn1 = DatabaseBuilder.CreateDatabase(dbConfig1)) {
                conn1.Open();
                using (IDatabase conn2 = DatabaseBuilder.CreateDatabase(dbConfig2)) {
                    conn2.Open();
                    DatabaseComparer dbComparer = new DatabaseComparer(conn1, conn2, sender as BackgroundWorker);
                    string dir = txtCsvOutputDir.Text; if (!dir.EndsWith("\\")) dir += "\\";
                    dbComparer.DoWorkAndSave(dir + "comparison_");
                        /*dbComparer.WriteCsv(dir + "comparison_");
                        dbComparer.WritePdf(dir + "comparison_result.pdf");*/
                    
                }
            }
        }

        private void btnSelectCsvDir_Click(object sender, EventArgs e) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (txtCsvOutputDir.Text.Length > 0) {
                dialog.SelectedPath = txtCsvOutputDir.Text;
            }

            dialog.Description = "Csv Ausgabe Verzeichnis auswählen...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK) {
                txtCsvOutputDir.Text = dialog.SelectedPath;
                txtCsvOutputDir.SelectionStart = txtCsvOutputDir.TextLength;
            }
        }

    }
}
