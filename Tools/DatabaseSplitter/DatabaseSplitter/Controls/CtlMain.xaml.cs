using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using DbAccess;
using DatabaseSplitter.Models;

namespace DatabaseSplitter.Controls {
    /// <summary>
    /// Interaktionslogik für CtlMain.xaml
    /// </summary>
    public partial class CtlMain : UserControl {

        public MainModel Model { get { return DataContext as MainModel; } }

        public CtlMain() {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            var dic = new Dictionary<string, List<string>>();

            using (var reader = new StreamReader(Model.SplitFile)) {
                var sb = new StringBuilder();
                while (!reader.EndOfStream) {
                    sb.Append(reader.ReadLine());
                }

                var doc = new XmlDocument();
                doc.LoadXml(sb.ToString());
                var root = doc.DocumentElement;
                foreach (XmlNode child in root.ChildNodes) {
                    switch (child.Name.ToLower()) {
                        case "system":
                            ReadXMLSystem(child, dic);
                            break;
                    }
                }
            }

            using (var db = ConnectionManager.CreateConnection("MySQL", Model.DatabaseModel.Host, Model.DatabaseModel.User,
                                                        Model.DatabaseModel.Password)) {
                if(!db.IsOpen) db.Open();

                var dicSplitTables = new Dictionary<string, string>();

                var sql = "SELECT a.name, b.name FROM " + db.Enquote(Model.DatabaseModel.DatabaseAnalyser) +
                          ".analyse_table_info a " +
                          "join " + db.Enquote(Model.DatabaseModel.DatabaseAnalyser) + ".analyse_column_info b " +
                          "using (tableid) where specialtype = 3";
                using (var reader = db.ExecuteReader(sql)) {
                    while (reader.Read()) {
                        dicSplitTables.Add(reader[0].ToString().ToLower(), reader[1].ToString().ToLower());
                    }
                }

                foreach (var table in db.GetTableList(Model.DatabaseModel.Database)) {
                    if(dicSplitTables.ContainsKey(table.ToLower())) {
                        SplitTable(table.ToLower(), dicSplitTables[table.ToLower()], dic, db);
                    } else {
                        CopyTable(table.ToLower(), dic, db);
                    }
                }
            }
        }

        private void CopyTable(string toLower, Dictionary<string, List<string>> dic, IDatabase db) {
            
        }

        private void SplitTable(string table, string column, Dictionary<string, List<string>> dic, IDatabase db) {
            var cols = new StringBuilder();
            foreach (var dbColumnInfo in db.GetColumnInfos(Model.DatabaseModel.Database, table)) {
                if(dbColumnInfo.Name.ToLower().Equals("_row_no_")) continue;
                if (cols.Length > 0) cols.Append(',');
                cols.Append(db.Enquote(dbColumnInfo.Name.ToLower()));
            }
            
            foreach (var pair in dic) {
                var sb = new StringBuilder();
                foreach (var bukrs in pair.Value) {
                    if (sb.Length > 0) sb.Append(',');
                    sb.Append("'" + bukrs + "'");
                }

                db.DropTableIfExists(pair.Key, table);

                var sql = "create table " + db.Enquote(pair.Key) + "." + db.Enquote(table) + " like " +
                      db.Enquote(Model.DatabaseModel.Database) + "." + db.Enquote(table);
                db.ExecuteScalar(sql);

                sql = "insert into " + db.Enquote(pair.Key) + "." + db.Enquote(table) + "(" + cols.ToString() + ") select " + cols.ToString() + " from " +
                      db.Enquote(Model.DatabaseModel.Database) + "." + db.Enquote(table) + " where " +
                      db.Enquote(column) + " in (" + sb.ToString() + ")";
                db.ExecuteScalar(sql);
            }
        }

        private void ReadXMLSystem(XmlNode node, Dictionary<string, List<string>> dic) {
            var bukrs = new List<string>();
            var db = string.Empty;

            foreach (XmlNode child in node.ChildNodes) {
                switch (child.Name.ToLower()) {
                    case "bukrs":
                        bukrs.Add(child.InnerText);
                        break;

                    case "database":
                        db = child.InnerText;
                        break;
                }
            }

            dic.Add(db, bukrs);
        }
    }
}
