using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLineParser.Arguments;

namespace ViewRelevantChecker
{
    internal class Options
    {
        [ValueArgument(typeof(string), 'd', "directory", DefaultValue = ".\\", Description = "Set target database. Default: .\\")]
        public string Directory { get; set; }

        [ValueArgument(typeof(string), 'f', "file", DefaultValue = "viewrelevanttables.csv", Description = "Set view relevant tables file path. Default: viewrelevanttables.csv")]
        public string FilePath { get; set; }

        [ValueArgument(typeof(string), 'o', "outputfile", DefaultValue = "foundtables.txt", Description = "Set file path with view relevant tables found. Default: foundtables.txt")]
        public string OutputFilePath { get; set; }

        [ValueArgument(typeof(string), 'l', "log", DefaultValue = "log.txt", Description = "Set log file path. Default: log.txt")]
        public string LogPath { get; set; }

        [ValueArgument(typeof(bool), 'c', "csv", DefaultValue = "False", Description = "Indicates that the imput is multi file csv data. Default: False")]
        public bool CsvInput { get; set; }
    }
}
