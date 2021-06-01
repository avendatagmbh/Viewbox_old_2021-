using DbAccess.Structures;
using System.Collections.ObjectModel;

namespace ViewValidator.Structures {
    public class DbConfigModel {

        public DbConfigModel() {
            MySql = new DbConfig("MySQL") { Password = "avendata", Username = "root", Port = 3306 , Hostname= "dbworker-s2"};
            Access = new DbConfig("Access") { Hostname = "Q:\\Großprojekte\\Josef Möbius Bau-AG\\05 Verprobungsdaten\\FiBu\\Verprobung.mdb" };
            
            MySqlCols = new ObservableCollection<string>();
            AccessCols = new ObservableCollection<string>();

            ChosenDBMySQL = "";
            ChosenDBAccess = "";

            ChosenTableMySQL = "";
            ChosenTableAccess = "";

            ChosenAccessCols = new ObservableCollection<string>();
            ChosenMySqlCols = new ObservableCollection<string>();

            tempItems = new ObservableCollection<string>();
        }

        public DbConfig MySql { get; set; }
        public DbConfig Access { get; set; }

        public ObservableCollection<string> MySqlCols { get; set; }
        public ObservableCollection<string> AccessCols { get; set; }
        public ObservableCollection<string> ChosenMySqlCols { get; set; }
        public ObservableCollection<string> ChosenAccessCols { get; set; }

        public string ChosenDBMySQL;
        public string ChosenDBAccess;

        public string ChosenTableMySQL;
        public string ChosenTableAccess;


        public ObservableCollection<string> tempItems { get; set; }

    }
}
