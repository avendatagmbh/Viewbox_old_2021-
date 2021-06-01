using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommandLineParser.Arguments;

namespace ViewboxMassTableArchivingTool
{
    public class Options 
    {
        [ValueArgument(typeof(string), 'y', "System", DefaultValue = "", Description = "Set the system to be archived")]
        public string System { get; set; }

        [ValueArgument(typeof(string), 's', "server", DefaultValue = "localhost", Description = "Set target MySQL server. Default: localhost")]
        public string Server { get; set; }

        [ValueArgument(typeof(string), 'u', "user", DefaultValue = "root", Description = "Set username to connection. Default: root")]
        public string User { get; set; }

        [ValueArgument(typeof(string), 'p', "password", DefaultValue = "avendata", Description = "Set password. Default: avendata")]
        public string Password { get; set; }

        [ValueArgument(typeof(string), 'd', "database", DefaultValue = "viewbox", Description = "Set Viewbox database. Default: viewbox")]
        public string Database { get; set; }

        [ValueArgument(typeof(string), 'm', "port", Description = "Set database server port.")]
        public string Port { get; set; }

        [ValueArgument(typeof(string), 'f', "file", Description = "Set import or export CSV file path. Default: tables.csv")]
        public string FilePath { get; set; }

        [SwitchArgument('k', "skip", false, Description = "Skip SAP check")]
        public bool SkipSAPCheck;


        [SwitchArgument('i', "invert", false, Description = "Archive / restore tables that are in CSV")]
        public bool Invert;
        [SwitchArgument('a', "archive", false, Description = "Archive tables that are NOT in CSV")]
        public bool Archive;
        [SwitchArgument('r', "restore", false, Description = "Restore tables that are NOT in CSV")]
        public bool Restore;
        [SwitchArgument('e', "export", false, Description = "Export table names")]
        public bool Export;
        [SwitchArgument('x', "fullexport", false, Description = "Export tables with additional informations")]
        public bool FullExport;
        [SwitchArgument('l', "list", false, Description = "List all tables to console")]
        public bool List;
        [SwitchArgument('v', "viewsenabled", false, Description = "Enable to Archive / restore Views too")]
        public bool ViewsEnabled { get; set; }
    }
}
