using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VersionUpdater
{
    class Program
    {
        private static Options _options;

        static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            _options = new Options();
            parser.ExtractArgumentAttributes(_options);
            
            try
            {
                parser.ParseCommandLine(args);

                var pProcess = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = "cmd.exe",
                        Arguments = @"/C git.exe --git-dir=" + _options.GitRepoPath + @".git rev-list HEAD --count",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = _options.GitApplicationPath
                    }
                };
                pProcess.Start();

                var COUNT = int.Parse(pProcess.StandardOutput.ReadToEnd().Trim()).ToString();

                var text = File.ReadAllText(_options.OriginalFilePath);
                text = text.Replace(@"$WCREV$", COUNT.Trim());
                for (var i = 1; i <= COUNT.Length; ++i)
                {
                    text = text.Replace(string.Format(@"$WCREV(U,{0})$", i.ToString()), COUNT.Substring(0, i));
                    text = text.Replace(string.Format(@"$WCREV(L,{0})$", i.ToString()), COUNT.Substring(COUNT.Length - i, i));
                }
                File.WriteAllText(_options.DestinationFilePath, text);
                pProcess.Close();
            } catch (Exception e) {
               throw new Exception(string.Format("Git is not installed or it is not configured in PATH as environment variable : {0}", e.Message));
            }
        }
    }
}
