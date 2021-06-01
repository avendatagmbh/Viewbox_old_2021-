using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLineParser.Arguments;

namespace VersionUpdater
{
    internal class Options
    {
        [ValueArgument(typeof(string), 'a', "gitapplicationpath", DefaultValue = "C:\\Program Files (x86)\\Git\\cmd\\", Description = "Set directory of GIT.exe. Default: C:\\Program Files (x86)\\Git\\cmd\\")]
        public string GitApplicationPath { get; set; }

        [ValueArgument(typeof(string), 'r', "gitrepopath", DefaultValue = "C:\\GIT\\", Description = "Set directory of GIT repository. Default: C:\\GIT\\")]
        public string GitRepoPath { get; set; }

        [ValueArgument(typeof(string), 'o', "originalfilepath", DefaultValue = "C:\\GIT\\AssemblyInfo_temp.cs", Description = "Set source file. Default: C:\\GIT\\AssemblyInfo_temp.cs")]
        public string OriginalFilePath { get; set; }

        [ValueArgument(typeof(string), 'd', "destinationfilepath", DefaultValue = "C:\\GIT\\AssemblyInfo.cs", Description = "Set source file. Default: C:\\GIT\\AssemblyInfo.cs")]
        public string DestinationFilePath { get; set; }
        
    }
}
