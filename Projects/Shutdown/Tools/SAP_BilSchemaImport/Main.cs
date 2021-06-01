using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DbAccess;
using System.IO;
using System.Text.RegularExpressions;
using DbAccess.Structures;

namespace SAP_BilSchemaImport {
    public partial class Main : Form {
        public Main() {
            InitializeComponent();

            dbConfig1 = new DbConfig("MySQL");

            this.txtAccountsStructure.Text = "SCDE";
            //this.txtSourceFile.Text = @"Q:\Großprojekte\Wavin GmbH\Wavin\NL\Bilant";
            this.txtSourceFile.Text = @"C:\Users\beh\Documents\SAP\Bilanz";
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnStart_Click(object sender, EventArgs e) {

            if (txtAccountsStructure.Text.Length == 0) {
                MessageBox.Show("Es wurde noch kein Kontenplan angegeben.");
                return;
            }

            if (txtSourceFile.Text.Length == 0) {
                MessageBox.Show("Es wurde noch keine Quelldatei ausgewählt.");
                return;
            }

            if (!File.Exists(txtSourceFile.Text) && !Directory.Exists(txtSourceFile.Text)) {
                MessageBox.Show("Die angegebene Quelldatei ist nicht vorhanden.");
                return;
            }

            dbConfig1.Hostname = textBox1.Text;
            dbConfig1.Username = textBox2.Text;
            dbConfig1.Password = textBox3.Text;
            dbConfig1.DbName = comboBox1.SelectedItem.ToString();

            if (File.Exists(txtSourceFile.Text))
                ImportFile(txtSourceFile.Text, txtAccountsStructure.Text);
            else {
                List<FileResult> results = ImportDirectory(txtSourceFile.Text);
                ResultsForm form = new ResultsForm(results);
                form.ShowDialog();

            }

        }

        private List<FileResult> ImportDirectory(string path) {
            List<FileResult> results = new List<FileResult>();
            //foreach (var filename in Directory.GetFiles(path, "*.txt")) {
            foreach (var filename in Directory.GetFiles(path)) {
                StreamReader reader = null;
                try {
                    reader = new StreamReader(filename, Encoding.GetEncoding(1252));
                    string sPattern = @".*\d+ - \d+ [X_]\|[X_]";
                    HashSet<string> accountStructures = new HashSet<string>();

                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern)) {
                            if (!line.Contains("---")) line = line.Replace("|--", "---");
                            if (!line.Contains("---"))
                                throw new Exception("Line \"" + line + "\" should contain --- or |-");
                            //line = NormalizeRow(line);
                            Match match = Regex.Match(line, @"---(.*?)\d+ - \d+ [X_]|[X_]");
                            if (match.Groups.Count != 2) {
                                throw new Exception("Could not extract base name");
                            }
                            string account = match.Groups[1].ToString().Replace("-", "").Replace("|", "").Trim();
                            if (!accountStructures.Contains(account) && !string.IsNullOrEmpty(account))
                                accountStructures.Add(account);
                        }
                    }
                    if (accountStructures.Count == 0)
                        throw new Exception("Could not find account structure for filename " + filename);
                    reader.Close();
                    using (IDatabase db = ConnectionManager.CreateConnection(dbConfig1)) {
                        db.Open();
                        using (reader = new StreamReader(filename, Encoding.GetEncoding(1252))) {
                            ImportStructure(db, reader, string.Join(";", accountStructures.ToArray()));
                        }
                    }
                    results.Add(new FileResult(filename) {AccountStructures = accountStructures});
                } catch (Exception ex) {
                    results.Add(new FileResult(filename) {Error = ex.Message});
                    //MessageBox.Show("Fehler beim Öffnen der Quelldatei: " + Environment.NewLine + ex.Message);
                } finally {
                    if (reader != null) reader.Close();
                }
            }
            return results;
        }

        private void ImportFile(string filename, string accountsStructure) {
            using (IDatabase db = ConnectionManager.CreateConnection(dbConfig1)) {
                try {
                    db.Open();

                    StreamReader oReader = null;
                    try {
                        oReader = new StreamReader(filename, Encoding.GetEncoding(1252));
                        ImportStructure(db, oReader, accountsStructure);
                        MessageBox.Show("Import abgeschlossen");

                    } catch (Exception ex) {
                        MessageBox.Show("Fehler beim Öffnen der Quelldatei: " + Environment.NewLine + ex.Message);
                    } finally {
                        if (oReader != null) oReader.Close();
                    }

                } catch (Exception ex) {
                    MessageBox.Show("Fehler beim Öffnen der Datenbankverbindung: " + Environment.NewLine + ex.Message);

                } finally {
                    if (db.IsOpen) db.Close();
                }
            }
        }

        private void ImportStructure(IDatabase db, StreamReader oReader, string accountsStructure) {
            int nLine = 0;
            string structureName = string.Empty;
            var accStructureNames = accountsStructure.Trim().Split(';').ToList();

            List<BilRow> bilRows = new List<BilRow>();
            int[] Header = new int[20];

            int[] GroupParent = new int[20];

            bool[] IsAccountRange = new bool[20];

            while (!oReader.EndOfStream) {
                string sLine = oReader.ReadLine();

                System.Diagnostics.Debug.WriteLine(sLine);

                if (sLine.Trim().Length == 0) continue;

                if (sLine.Trim().StartsWith("|") || sLine.Trim().StartsWith("---")) {
                    sLine = NormalizeRow(sLine);
                } else {
                    // assign structure name
                    int spaceIndex = sLine.Trim().IndexOf(" ");
                    if (spaceIndex == -1) {
                        structureName = sLine.Trim();
                    } else {
                        structureName = sLine.Trim().Substring(0, sLine.Trim().IndexOf(" "));
                    }
                    continue;
                }

                if (!sLine.StartsWith("  |") || sLine.Replace("|", "").Trim().Length == 0) {
                    // ignore empty lines
                    continue;
                }

                int level = sLine
                                .Replace("_|X", "")
                                .Replace("X|_", "")
                                .Replace("X|X", "")
                                .Split('|').Count() - 1;

                for (int i = level; i < Header.Length; i++) {
                    Header[i] = 0;
                }

                for (int i = level; i < GroupParent.Length; i++) {
                    if(level == 0) {
                        GroupParent[i] = 0;
                    }else {
                        GroupParent[i] = GroupParent[level - 1];
                    }
                }

                for (int i = level; i < IsAccountRange.Length; i++) {
                    IsAccountRange[i] = false;
                }


                nLine++;

                BilRow oBilRow = new BilRow();

                var guvLine = sLine.Trim().ToLower();
                if (guvLine.EndsWith("bilanzergebnis gewinn")) oBilRow.AdditionalInformation = "H";
                if (guvLine.EndsWith("bilanzergebnis verlust")) oBilRow.AdditionalInformation = "S";

                string title = sLine.Replace("  |", "").Trim();
                while (title.StartsWith(" ") || title.StartsWith("-")) title = title.Substring(1);

                // analyse debit (soll) and credit (haben) signs
                if (sLine.Contains("X|X")) {
                    oBilRow.Debit = true;
                    oBilRow.Credit = true;
                } else if (sLine.Contains("_|X")) {
                    oBilRow.Debit = false;
                    oBilRow.Credit = true;
                    if (level != 0 && GroupParent[level - 1] != 0) GroupParent[level] = GroupParent[level - 1];
                    else GroupParent[level] = nLine;
                } else if (sLine.Contains("X|_")) {
                    oBilRow.Debit = true;
                    oBilRow.Credit = false;
                    if (level != 0 && GroupParent[level - 1] != 0) GroupParent[level] = GroupParent[level - 1];
                    else GroupParent[level] = nLine;
                } else {
                    oBilRow.Debit = false;
                    oBilRow.Credit = false;
                }

                if (title.Contains("X|_  -->")) title = title.Substring(0, title.IndexOf("X|_  -->"));
                if (title.Contains("_|X  -->")) title = title.Substring(0, title.IndexOf("_|X  -->"));
                title = title.Replace(" X|_", "").Replace(" _|X", "").Replace(" X|X", "");
                
                //split title into account-number and text            
                var accNumberStr = string.Empty;

                // Kontenbereich lesen
                var accStructureName = String.Empty;
                foreach (var a in accStructureNames) {
                    if (title.StartsWith(a) && title.Substring(accStructureName.Length, 1) != "'") {//e.g. DFDS' share of net result in Q:\Großprojekte\DSV\DSV_SAP\Bilanzschemata\bilanz dsv 030\DSV.txt
                        accStructureName = a;
                        IsAccountRange[level] = true;
                        string[] parts = title.Substring(accStructureName.Length).Trim().Split(' ');
                        oBilRow.AccountFrom = parts[0];
                        oBilRow.AccountTo = parts[2];
                        oBilRow.AccountStructure = accStructureName;
                        break;
                    }
                }

                // Werte in Array speichern
                oBilRow.Id = nLine;
                oBilRow.Level = level;
                oBilRow.Parent = Header[level - 1];
                if ((oBilRow.Parent == 0) && level > 1) {
                    oBilRow.Parent = Header[level - 2];
                }
                oBilRow.HighestGroupId = GroupParent[level];
                
                Regex regexAccNumber = new Regex("^[0-9A-Za-z/]{7,10} .*");

                // Kontonummer lesen
                if (regexAccNumber.IsMatch(title)) {
                    accNumberStr = title.Substring(0, title.IndexOf(' '));
                    title = title.Substring(title.IndexOf(' '), title.Length - accNumberStr.Length);
                    while (title.StartsWith(" ") || title.StartsWith("-")) title = title.Substring(1);
                }

                oBilRow.ShortName = accNumberStr.Replace("'", "");
                oBilRow.Name = title.Replace("'", "");

                if (IsAccountRange[level - 1]) {
                    oBilRow.Account = accNumberStr;
                    oBilRow.Type = "K";
                    break;
                } else {
                    var line = sLine;
                    while(line.StartsWith("|") || line.StartsWith("-") || line.StartsWith(" ")) {
                        line = line.Substring(1);
                    }
                    if (line.StartsWith(accStructureName)) {
                        oBilRow.Type = "B";

                        Regex bereich = new Regex("^[0-9A-Za-z/]{7,10} - [0-9A-Za-z/]{7,10} .*");
                        var newLine = line.Substring(accStructureName.Length);
                        if (bereich.IsMatch(newLine.Trim())) {
                            if (newLine.Contains("X|_  -->")) newLine = newLine.Substring(0, newLine.IndexOf("X|_  -->"));
                            if (newLine.Contains("_|X  -->")) newLine = newLine.Substring(0, newLine.IndexOf("_|X  -->"));
                            newLine = newLine.Replace("X|_", "").Replace("_|X", "").Replace("X|X", "");

                            var newName = newLine.Substring(newLine.IndexOf(" - ") + 3);
                            newName = newName.Substring(newName.IndexOf(' ')).Trim();
                            oBilRow.Name = newName.Replace("'", "");
                        }

                        if (line.Contains("X|X") || line.Contains("_|X") || line.Contains("X|_")) {
                            // Kontenbereich ist nur Überschrift
                            Header[level] = oBilRow.Id;
                        }
                        break;
                    } else {
                        Header[level] = oBilRow.Id;
                        oBilRow.Type = "H";
                        if (!oBilRow.Debit && !oBilRow.Credit) {
                            oBilRow.Debit = true;
                            oBilRow.Credit = true;
                        }   
                    }
                }

                bilRows.Add(oBilRow);
            }

            const string tabName = "_bgv_Bilanzstruktur";

            string sqlCreateTable = "CREATE TABLE IF NOT EXISTS " + tabName + " (" +
                                    "`ID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT," +
                                    "`BilStr` VARCHAR(10) NOT NULL," +
                                    "`Parent` INTEGER UNSIGNED NOT NULL," +
                                    "`Ebene` INTEGER UNSIGNED NOT NULL," +
                                    "`Nummer` VARCHAR(10) NOT NULL," +
                                    "`Titel` VARCHAR(255) NOT NULL," +
                                    "`Konto` VARCHAR(20) NOT NULL," +
                                    "`KontoVon` VARCHAR(20) NOT NULL," +
                                    "`KontoBis` VARCHAR(20) NOT NULL," +
                                    "`Typ` VARCHAR(1) NOT NULL," +
                                    "`Soll` BOOLEAN NOT NULL," +
                                    "`Haben` BOOLEAN NOT NULL," +
                                    "`HighestGroupId` INTEGER UNSIGNED NOT NULL," +
                                    "`AdditionalInformation` VARCHAR(3)," +
                                    "`AccountStructure` VARCHAR(4)," +
                                    "PRIMARY KEY (`ID`, `BilStr`))" +
                                    "ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci " +
                                    "COMMENT = 'Bilanzstruktur für View-Erstellung';";

            db.ExecuteNonQuery(sqlCreateTable);

            db.ExecuteNonQuery("DELETE FROM " + tabName + " WHERE BilStr = '" + structureName + "'");

            string sqlInsertPrefix =
                "INSERT INTO " + tabName + " " +
                "(" +
                "bilStr," +
                "ID," +
                "Parent," +
                "Ebene," +
                "Nummer," +
                "Titel," +
                "Konto," +
                "KontoVon," +
                "KontoBis," +
                "Typ," +
                "Soll," +
                "Haben," +
                "HighestGroupId," +
                "AdditionalInformation," +
                "AccountStructure" +
                ") VALUES ";

            string sqlInsert = string.Empty;
            int nRow = 0;

            foreach (BilRow oBilRow in bilRows) {
                if (oBilRow.Id > 0) {
                    if (sqlInsert.Length > 0) sqlInsert += ",";

                    sqlInsert +=
                        "(" +
                        "'" + structureName + "'," +
                        oBilRow.Id + "," +
                        oBilRow.Parent + "," +
                        oBilRow.Level + "," +
                        "'" + oBilRow.ShortName + "'," +
                        "'" + oBilRow.Name + "'," +
                        "'" + oBilRow.Account + "'," +
                        "'" + oBilRow.AccountFrom + "'," +
                        "'" + oBilRow.AccountTo + "'," +
                        "'" + oBilRow.Type + "'," +
                        (oBilRow.Debit ? "True" : "False") + "," +
                        (oBilRow.Credit ? "True" : "False") + "," +
                        oBilRow.HighestGroupId + "," +
                        (oBilRow.AdditionalInformation != null ? "'" + oBilRow.AdditionalInformation + "'" : "NULL") + "," +
                        (oBilRow.AccountStructure != null ? "'" + oBilRow.AccountStructure + "'" : "NULL") +
                        ")";
                } else {
                    System.Diagnostics.Debug.WriteLine("Eintrag mit ID 0 gefunden!");
                }

                if (++nRow%100 == 0) {
                    db.ExecuteNonQuery(sqlInsertPrefix + sqlInsert);
                    sqlInsert = string.Empty;
                }
            }

            if (sqlInsert.Length > 0) {
                db.ExecuteNonQuery(sqlInsertPrefix + sqlInsert);
            }
        }

        private static string NormalizeRow(string sLine) {
            // normalize row
            if (sLine.StartsWith("   ")) sLine = "  |" + sLine.Substring(3, sLine.Length - 3);
            sLine = sLine.Replace(" ---", " |--");
            while (sLine.Contains("|    ")) {
                sLine = sLine.Replace("|    ", "|   |");
            }
            return sLine;
        }

        private void btnSelectSourcefile_Click(object sender, EventArgs e) {
            using (OpenFileDialog oFileDialog = new OpenFileDialog()) {
                if (oFileDialog.ShowDialog() == DialogResult.OK) {
                    txtSourceFile.Text = oFileDialog.FileName;
                }
            }
        }

        private void comboBox1_Click(object sender, EventArgs e) {
            dbConfig1.Hostname = textBox1.Text;
            dbConfig1.Username = textBox2.Text;
            dbConfig1.Password = textBox3.Text;
            dbConfig1.DbName = "";
            var list = new List<string>();
            using (var db = ConnectionManager.CreateConnection(dbConfig1)) {
                db.Open();
                list = db.GetSchemaList();
            }
            comboBox1.DataSource = list;
        }

        private void button1_Click(object sender, EventArgs e) {
            try {
                dbConfig1.Hostname = textBox1.Text;
                dbConfig1.Username = textBox2.Text;
                dbConfig1.Password = textBox3.Text;
                dbConfig1.DbName = comboBox1.SelectedItem.ToString();
                using(var db = ConnectionManager.CreateConnection(dbConfig1)) {
                    db.Open();
                }
                MessageBox.Show("Die Verbindung wurde erfolgreich hergestellt.", "Verbindungstest erfolgreich");
            }catch(Exception ex) {
                MessageBox.Show(ex.Message, "Fehler beim Verbindungsaufbau");
            }
        }
    }
}
