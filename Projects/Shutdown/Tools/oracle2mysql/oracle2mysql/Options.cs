using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommandLineParser.Arguments;

namespace oracle2mysql
{
    internal class Options
    {

        [ValueArgument(typeof (string), 'o', "oracle", DefaultValue = "tuidb", Description = "Set source oracle service name. Default: tuidb")]
        public string OracleServiceName { get; set; }

        [ValueArgument(typeof (string), 's', "server", DefaultValue = "localhost", Description = "Set target MySQL server. Default: localhost")]
        public string Server { get; set; }

        [ValueArgument(typeof (string), 'u', "user", DefaultValue = "root", Description = "Set username to connection. Default: root")]
        public string User { get; set; }

        [ValueArgument(typeof (string), 'p', "password", DefaultValue = "avendata", Description = "Set password. Default: avendata")]
        public string Password { get; set; }

        [ValueArgument(typeof (string), 'd', "database", DefaultValue = "tuidb", Description = "Set target database. Default: tuidb")]
        public string Database { get; set; }

        [ValueArgument(typeof (string), 'f', "file", DefaultValue = "tables.txt", Description = "Set import TXT file path. Default: tables.txt")]
        public string FilePath { get; set; }

        [ValueArgument(typeof (string), 'l', "log", DefaultValue = "log.txt", Description = "Set log file path. Default: log.txt")]
        public string LogPath { get; set; }

        [ValueArgument(typeof (string), 't', "tables", DefaultValue = "tables-with-error.txt", Description = "Set tables with error file path. Default: tables-with-error.txt")]
        public string TablesWithErrorPath { get; set; }

        [SwitchArgument('c', "continue", false, AllowMultiple = false, Description = "If table exists, continue transfer instead of drop table")]
        public bool Continue { get; set; }


        [ValueArgument(typeof(string), 'q', "oracleuser", DefaultValue = "system", Description = "Set username to connection. Default: system")]
        public string Oracleuser { get; set; }

        [ValueArgument(typeof(string), 'w', "oraclepassword", DefaultValue = "system", Description = "Set password. Default: system")]
        public string Oraclepassword { get; set; }

        //[ValueArgument(typeof(int), 'b', "batchsize", DefaultValue = "100", Description = "Insert batchsize. Default: 100")]
        //public int BatchSize { get; set; }

    }
}
