using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace DbAnalyser.Helpers
{
    public class CsvListFiltering
    {
        public static List<string> ReadList()
        {
            var list = new List<string>();

            var openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != null)
            {
                try
                {
                    var reader = new StreamReader(File.OpenRead(openFileDialog1.FileName));
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var values = line.Split(';');

                            list.Add(values[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return list;
        }
    }
}
