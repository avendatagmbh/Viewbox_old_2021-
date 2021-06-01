using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKit.Controls.XbrlVisualisation {

    /// <summary>
    /// Interaktionslogik für XbrlListView.xaml
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-30</since>
    public partial class XbrlListView : UserControl {

        #region constructor
        public XbrlListView() {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(CtlListView_DataContextChanged);
        }
        #endregion

        #region events
        public event EventHandler GUIInitRequested;
        private void OnInitRequested() { if (GUIInitRequested != null) GUIInitRequested(this, new System.EventArgs()); }
        #endregion events

        #region eventHandler
        void CtlListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Data != null) {
                if (Data.Items.Count > 0) OnInitRequested();
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            if (Data != null) {
                if (!Data.AddItemAllowed) {
                    // item not added (unsufficient rights)
                    var owner = UIHelpers.TryFindParent<Window>(this);
                    MessageBox.Show(owner, "Sie haben keine Berechtigung zum Hinzufügen neuer Listeneinträge.");
                } else {
                    bool requestInit = (Data.Items.Count == 0);
                    Data.AddValue();
                    if (requestInit) {
                        OnInitRequested();
                    }
                }
            }
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            if (!Data.DeleteItemAllowed) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                MessageBox.Show(owner, "Sie haben keine Berechtigung zum Löschen von Listeneinträgen.");
            } else if (Data != null) {
                Data.DeleteSelectedValue();
            }
        }

        #endregion eventHandler

        #region properties

        #region Data
        XbrlElementValue_List Data {
            get { return this.DataContext as XbrlElementValue_List; }
        }
        #endregion

        #region DisplayMemberPath
        public string DisplayMemberPath {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set {
                SetValue(DisplayMemberPathProperty, value);

                var trigger = new DataTrigger();
                trigger.Binding = new Binding(value);
                trigger.Value = null;
                var setter = new Setter(TextBlock.TextProperty, "<keine Bezeichnung>");
                trigger.Setters.Add(setter);

                var dt = new DataTemplate();
                var factory = new FrameworkElementFactory(typeof(TextBlock));
                var style = new Style();
                style.Setters.Add(new Setter(TextBlock.TextProperty, new Binding(value)));
                style.Triggers.Add(trigger);
                style.TargetType = typeof (TextBlock);
                factory.SetValue(TextBlock.StyleProperty, style);
               
                dt.Triggers.Add(trigger);

                dt.VisualTree = factory;
                lstItems.ItemTemplate = dt;
            }
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(XbrlListView), new UIPropertyMetadata(""));

        #endregion

        #region DataPanel
        public Panel DataPanel {
            get { return dataPanel; }
        }
        #endregion

        #endregion properties

    }
}
