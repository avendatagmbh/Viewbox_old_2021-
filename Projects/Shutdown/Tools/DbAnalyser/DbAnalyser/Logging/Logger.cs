using System;
using System.IO;
using System.Threading;

namespace DbAnalyser.Logging
{
    public class Logger
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public static void LogError(String lines)
        {

            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log

            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                string path = "c:\\temp\\LOGS";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filename = String.Format("DbAnalyser.log.txt");
                path = path + "\\" + filename;

                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                lock (path)
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                    
                            sw.WriteLine(lines);
                    
                    }
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}
