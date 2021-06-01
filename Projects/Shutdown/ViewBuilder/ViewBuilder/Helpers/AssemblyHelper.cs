using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace ViewBuilder.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetAssemblyVersion()
        {
            string version = "Version " +
                Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
                Assembly.GetExecutingAssembly().GetName().Version.Build + "." +
                Assembly.GetExecutingAssembly().GetName().Version.Revision;
            return version;
        }

        public static string GetAssemblyDate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            DateTime AssemblyCreationalDate = File.GetLastWriteTime(assembly.Location);

            AssemblyCreationalDate = RetrieveLinkerTimestamp();
            return string.Format("{0}", AssemblyCreationalDate.ToString("dd.MM.yyyy"));
        }

        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}
