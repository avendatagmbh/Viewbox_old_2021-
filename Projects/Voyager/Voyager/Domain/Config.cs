using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using DbAccess;

namespace Domain {
    public static class Config {

        private const string dbType = "MySQL";
        private const string Hostname = "chip";
        private const string dbName = "voyager";
        private const string Username = "root";
        private const string Password = "avendata";

       
        static Config(){
            Conf = new DbConfig(dbType);
            Conf.Hostname = Hostname;
            Conf.DbName = dbName;
            Conf.Username = Username;
            Conf.Password = Password;
            Conf.Port = 3306;
            DbName = dbName;
         }

        public static DbConfig Conf { get; private set; }

        public static string DbName { get; private set; }
    }
}
