using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace eon_viewer {
    class Program {
        static void Main(string[] args) {

            if (args.Count() != 1) return;
            string filename = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\doc\\" + args[0] + ".txt";

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(filename);
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = psi;
            proc.Start();
        }
    }
}
