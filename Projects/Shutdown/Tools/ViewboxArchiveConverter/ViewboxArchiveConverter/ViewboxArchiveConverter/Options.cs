using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLineParser.Arguments;

namespace ViewboxArchiveConverter
{
    internal class Options
    {
        [ValueArgument(typeof(string), 'a', "archivedirectory", DefaultValue = ".\\", Description = "Set archive directory. Default: .\\")]
        public string ArchiveDirectory { get; set; }

        [ValueArgument(typeof(string), 't', "thumbnaildirectory", DefaultValue = ".\\", Description = "Set thumbnail directory. Default: .\\Thumbnail")]
        public string ThumbnailDirectory { get; set; }

        [ValueArgument(typeof(string), 'e', "pdfdirectory", DefaultValue = ".\\", Description = "Set pdf directory. Default: .\\Pdf")]
        public string PdfDirectory { get; set; }

        [ValueArgument(typeof(string), 'c', "continue", DefaultValue = "true", Description = "Set whether the existing thumbnails files will be skipped (true) or recreated (false). Default: true")]
        public string Continue { get; set; }

        [ValueArgument(typeof(string), 'l', "logfile", DefaultValue = "", Description = "Set the default log file. Default: NULL")]
        public string LogFile { get; set; }

        [ValueArgument(typeof(string), 'i', "input", DefaultValue = "*.*", Description = "Set the default pattern. Default: *.*")]
        public string Input { get; set; }

        [ValueArgument(typeof(string), 'f', "format", DefaultValue = ".jpg", Description = "Set the default format to convert to. Default: .jpg")]
        public string Format { get; set; }

        [ValueArgument(typeof(string), 'r', "reorderfolders", DefaultValue = "false", Description = "Reorder folder structure based on database. Default: false")]
        public string ReorderFolders { get; set; }

        [ValueArgument(typeof(string), 'x', "recursive", DefaultValue = "viewbox", Description = "Search files in subfolders. Default: false")]
        public string Recursive { get; set; }

        [ValueArgument(typeof(string), 'n', "defaultformat", DefaultValue = "false", Description = "Ignore default format. Default: false")]
        public string IgnoreFormats { get; set; }

        // database

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
        
    }
}
