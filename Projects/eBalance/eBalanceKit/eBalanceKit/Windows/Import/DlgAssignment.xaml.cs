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
using System.Windows.Shapes;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für DlgAssignment.xaml
    /// </summary>
    public partial class DlgAssignment : Window {

        private Dictionary<string, bool?> _dict;

        public DlgAssignment(Dictionary<string, bool?> dictionary) {
            InitializeComponent();
            //listColumn.Items. = from col in columnDict select col.Key
            _dict = dictionary;
            listEntries.Items.Clear();
            InitDicts();
        }
        
        private void InitDicts() {
            foreach (KeyValuePair<string, bool?> entry in _dict) {
                //var entry = listColumn.Items.Add(columnEntry.Key);
                //listColumn.Items[entry].
                ListBoxItem item = new ListBoxItem();
                item.Content = entry.Key;
                //ListItem item = new ListItem(new Paragraph(new Run(columnEntry.Key) ));
                switch (entry.Value) {
                    case true:
                        item.Background = Brushes.LightGray;
                        item.IsEnabled = false;
                        break;
                    case null:
                        item.Background = Brushes.Fuchsia;
                        item.IsEnabled = true;
                        break;
                    default:
                        item.IsEnabled = true;
                        break;
                }
                //item.con
                listEntries.Items.Add(item);
            }

        }

        public string AssignedEntry;
        public int AssignedEntryNo;

        private void btOK_Click(object sender, RoutedEventArgs e) {

            if (listEntries.SelectedItems.Count > 0) {

                //_dict[listEntries.SelectedItem.ToString()] = true;
                AssignedEntryNo = listEntries.SelectedIndex;
                AssignedEntry = (listEntries.SelectedItem as ListBoxItem).Content.ToString();
                this.DialogResult = true;
            } else {
                MessageBox.Show("Böse");
            }
        }

    }
}
