using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ViewValidator.Models.ListView {

    /// <summary>
    /// Base class for list view models.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-17</since>
    public class ListViewModelBase {
        
        public Panel DataPanel { get; set; }
        public Panel ListPanel { get; set; }
        public Window Owner { get; set; }
        public ListBox CreateListBox() {
            ListBox listBox = new ListBox();
            //listBox.SetBinding(ListBox.NameProperty, "lstItems");
            listBox.SetBinding(ListBox.ItemsSourceProperty, new Binding("Items"));
            listBox.SetBinding(ListBox.SelectedItemProperty, new Binding("SelectedItem"));
            listBox.SetBinding(ListBox.IsEnabledProperty, new Binding("IsAccessible"));
            //listBox.SetBinding(ListBox.BackgroundProperty, new Binding("{StaticResource ListBgBrush}"));
            listBox.Background = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
            listBox.DisplayMemberPath = "DisplayString";
            return listBox;
        }
        virtual public bool NewItemAllowed { get { return true; } }
        virtual public bool DeleteItemAllowed { get { return true; } }
        virtual public bool IsAccessible { get { return true; } }
        //virtual public string HeaderString {get { return Selected}}
    }
}
