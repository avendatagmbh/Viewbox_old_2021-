// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;

namespace LineCounter {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Model = new MainWindowModel();
            DataContext = Model;
        }

        private MainWindowModel Model { get; set; }

        private void btnCompute_Click(object sender, RoutedEventArgs e) {
            new DlgProgress(this).ExecuteModal(Compute);
        }

        private void Compute() {

            Dictionary<string, int> counts = new Dictionary<string,int>();
            Dictionary<string, double> sizes = new Dictionary<string, double>();

            Model.Result = string.Empty;
            var folders = Model.Folders.Split(new string[] {Environment.NewLine}, 99, StringSplitOptions.None);
            int totalCount = 0;
            double totalSize = 0;
            foreach (var folder in folders) {
                Model.Log(folder);
                if (!Directory.Exists(folder)) {
                    Model.Log("Folder does not exist!" + Environment.NewLine);
                    continue;
                }

                int folderCount = 0;
                double folderSize = 0;
                foreach (var file in Directory.GetFiles(folder)) {
                    Model.Log("File: " + file);
                    StreamReader r = new StreamReader(file);
                    int count = 0;
                    while (!r.EndOfStream) {
                        totalCount++;
                        folderCount++;
                        count++;
                        r.ReadLine();
                    }

                    var fi = new FileInfo(file);
                    double size = (double)fi.Length / (double)1024 / (double)1024;
                    folderSize += size;
                    totalSize += size;
                    Model.Log("Count: " + count);
                    Model.Log("Size:  " + Math.Round(size,2) + " MB" + Environment.NewLine);

                    if (!counts.ContainsKey(fi.Name)) counts[fi.Name] = 0;
                    if (!sizes.ContainsKey(fi.Name)) sizes[fi.Name] = 0;

                    counts[fi.Name] += count;
                    sizes[fi.Name] += size;
                }

                Model.Log("Folder count: " + folderCount);
                Model.Log("Folder size: " + Math.Round(folderSize, 2) + " MB" + Environment.NewLine);
            }

            Model.Log("Total count: " + totalCount);
            Model.Log("Total size: " + Math.Round(totalSize, 2) + " MB" + Environment.NewLine);

            Model.Log("counts per table:");
            foreach (var count in counts) {
                Model.Log(count.Key + "\t" + count.Value);
            }

            Model.Log(Environment.NewLine + "sizes per table:");
            foreach (var size in sizes ) {
                Model.Log(size.Key + "\t" + Math.Round(size.Value, 2));
            }
        }
    }
}