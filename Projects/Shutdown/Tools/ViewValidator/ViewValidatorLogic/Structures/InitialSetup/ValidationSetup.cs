// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:50
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.ObjectModel;
using DbAccess.Structures;

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class ValidationSetup {
        public DbConfig DbConfigValidation {get;set;}
        public DbConfig DbConfigView { get; set; }
        public ObservableCollection<TableMapping> TableMappings { get; set; }
        public int ErrorLimit { get; set; }

        public ValidationSetup() {
            this.TableMappings = new ObservableCollection<TableMapping>();
            this.DbConfigValidation = new DbConfig("Access");
            this.DbConfigView = new DbConfig("MySQL") { Password = "avendata", Username = "root", Port = 3306, Hostname = "localhost" };
            this.ErrorLimit = 100;
        }
    }
}
