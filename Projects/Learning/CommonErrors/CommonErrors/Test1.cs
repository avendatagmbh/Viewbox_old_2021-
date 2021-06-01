using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonErrors {
    static class Test1 {
        enum LogType { Warning, Error, Status };

        struct LogEntry {
            internal LogEntry(DateTime date) : this(){
                Date = date;
                Message = "DemoLog: ";
            }

            internal string Message { get; set; }
            internal LogType LogType { get; set; }
            internal DateTime Date { get; set; }
        }

        public static void Test() {
            LogEntry entry = new LogEntry(DateTime.Now);
            StartProgram(entry);
            entry.Message.Replace("Programm", "App");
            Console.WriteLine(entry.Message);
        }

        private static void StartProgram(LogEntry entry) {
            entry.Message += "Programm gestartet";
            entry.LogType = LogType.Status;
        }

    }
}
