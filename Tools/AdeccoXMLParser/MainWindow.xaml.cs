using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;

namespace AdeccoXMLParser {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() { InitializeComponent(); }

        private void Button_Click(object sender, RoutedEventArgs e) {

            // catching files
            var files = new List<FileInfo>();
            foreach (
                var file in
                    Directory.GetFiles(
                        @"\\lenny\Datenablage\Bereitstellung Adecco Eingangsdaten (17.08.2012)\SimbaExport\Globals")) {
                if (!file.ToLower().EndsWith("xml")) continue;
                files.Add(new FileInfo(file));
            }

            // read / create csv files
            foreach (var fileInfo in files) {
                if (fileInfo.FullName.ToLower().Contains("benutzer.xml") || fileInfo.FullName.ToLower().Contains("checkliste.xml")) continue;
                WorkOnFile(fileInfo);
            }

            MessageBox.Show("Finished...");
        }

        private void WorkOnFile(FileInfo fileInfo) {
            var stream = new StreamReader(fileInfo.FullName);
            var settings = new XmlReaderSettings();

            var subs = new List<string>();
            var dicWriter = new Dictionary<int, StreamWriter>();

            using (var reader = XmlReader.Create(stream, settings)) {
                while (reader.Read()) {
                    switch (reader.NodeType) {
                            case XmlNodeType.Element:
                                if (reader.Name.ToLower() == "sub") {
                                    reader.Read();
                                    if(reader.NodeType != XmlNodeType.Text) throw new NotImplementedException();
                                    subs.Add(reader.Value);
                                } else if (reader.Name.ToLower() == "data") {
                                    reader.Read();
                                    if (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA && reader.NodeType != XmlNodeType.EndElement) {
                                        throw new NotImplementedException();
                                    }
                                    WriteData(reader.Value, dicWriter, subs, fileInfo);
                                }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.ToLower() == "node") {
                                subs.RemoveAt(subs.Count-1);
                            }
                            break;
                    }
                }
            }

            foreach (var pair in dicWriter) {
                pair.Value.Flush();
                pair.Value.Close();
            }
        }

        private void WriteData(string data, Dictionary<int, StreamWriter> dicWriter, List<string> subs, FileInfo fi) {
            if(!dicWriter.ContainsKey(subs.Count - 1)) {
                dicWriter[subs.Count-1] = new StreamWriter(fi.FullName.ToLower().Replace(".xml","_" + (subs.Count - 1) + ".csv"), false);
                var header = new StringBuilder();
                for(int i = 0; i < subs.Count;i++) {
                    if (i > 0) header.Append("#");
                    header.Append("parameter" + i);
                }

                for (int i = 0; i < data.Count(d => d == '#'); i++) {
                    header.Append("#");
                    header.Append("value" + i);
                }
                dicWriter[subs.Count - 1].WriteLine(header + "<<EOL>>");
            }

            var sb = new StringBuilder();
            foreach (var sub in subs) {
                sb.Append(sub + "#");
            }
            dicWriter[subs.Count - 1].WriteLine(sb.ToString() + data + "<<EOL>>");
        }
    }
}
