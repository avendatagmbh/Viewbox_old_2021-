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
using Microsoft.Win32;
using System.IO;
using System.Xml;

namespace TransDATA_XMLtoCSV
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            tbXMLFile.Text = @"C:\Users\mag\Desktop\log.xml";
            tbCSVFile.Text = @"N:\mag\log.csv";
        }

        private void btnBrowseXMLFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true) tbXMLFile.Text = dlg.FileName;
        }

        private void btnBrowseCSVFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true) tbCSVFile.Text = dlg.FileName;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbXMLFile.Text) || string.IsNullOrEmpty(tbCSVFile.Text))
            {
                MessageBox.Show("Quell- oder Zieldatei nicht ausgewählt.");
                return;
            }

            try
            {
                var sbRead = new StringBuilder();
                using (var reader = new StreamReader(tbXMLFile.Text, Encoding.Default))
                {
                    while (!reader.EndOfStream)
                    {
                        sbRead.Append(reader.ReadLine());
                    }
                }
                var xml = new XmlDocument();
                xml.LoadXml(sbRead.ToString());
                //xml.Load(tbXMLFile.Text);

                var writer = new StreamWriter(tbCSVFile.Text, false, Encoding.Default);
                writer.WriteLine("Schema;Tabelle;Fehlermeldung");

                foreach (XmlNode node in xml.ChildNodes)
                {
                    if (node.Name == "DataTransferAgent_Export")
                    {
                        foreach(XmlNode childNode in node.ChildNodes){
                            if (childNode.Name == "Table")
                            {
                                var sb = new StringBuilder();
                                foreach (XmlNode innerChildNode in childNode.ChildNodes)
                                {
                                    switch (innerChildNode.Name)
                                    {
                                        case "Schema":
                                            sb.Append(innerChildNode.InnerText);
                                            break;
                                        case "Name":
                                            sb.Append(";");
                                            sb.Append(innerChildNode.InnerText);
                                            break;
                                        case "LogError":
                                            sb.Append(";");
                                            sb.Append(innerChildNode.InnerText);
                                            break;
                                    }
                                }
                                writer.WriteLine(sb.ToString());
                            }
                        }
                    }
                }

                writer.Flush();
                writer.Close();

                MessageBox.Show("Finished");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
