// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Taxonomy.Enums;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    /// <summary>
    /// Interaktionslogik für CtlEditListTreeView.xaml
    /// </summary>
    public partial class CtlEditListTreeView : UserControl {
        public CtlEditListTreeView(IPresentationTreeNode root) {
            InitializeComponent();
            tvBalance.ItemsSource = root;
            PresentationTreeRoot = root;
            DataContextChanged += CtlEditListTreeView_DataContextChanged;
        }

        void CtlEditListTreeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null) {
                var vtreeNode = DataContext as ValueTreeNode;
                Action<IPresentationTreeNode> updateValue = null;
                updateValue = (root) => {
                    foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                        IValueTreeEntry value;
                        vtreeNode.Values.TryGetValue(child.Element.Id, out value);
                        child.Value = value;
                        updateValue(child);
                    }
                };
                updateValue(PresentationTreeRoot);
            }
        }

        private IPresentationTreeNode PresentationTreeRoot { get; set; }

        private void tvBalance_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {

                case Key.F2: {
                        var node = tvBalance.SelectedItem as IPresentationTreeNode;
                        var owner = UIHelpers.TryFindParent<Window>(this);

                        if (node != null) {

                            switch (node.Element.ValueType) {
                                case XbrlElementValueTypes.Abstract:
                                case XbrlElementValueTypes.None:
                                case XbrlElementValueTypes.SingleChoice:
                                case XbrlElementValueTypes.MultipleChoice:
                                case XbrlElementValueTypes.Boolean:
                                case XbrlElementValueTypes.Date:
                                case XbrlElementValueTypes.Int:
                                case XbrlElementValueTypes.Numeric:
                                    // no detail editor implemented
                                    break;

                                case XbrlElementValueTypes.Tuple:
                                    if (node.Element.IsList) {
                                        new DlgEditListDetails(node) {
                                            Owner = owner,
                                            DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                                        }.ShowDialog();
                                    }
                                    return;

                                case XbrlElementValueTypes.Monetary:
                                    new DlgEditMonetaryDetails {
                                        Owner = owner,
                                        DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                                    }.ShowDialog();
                                    return;

                                case XbrlElementValueTypes.String:
                                    new DlgEditTextDetails {
                                        Owner = owner,
                                        DataContext =
                                            new PresentationTreeDetailModel(node.Document,
                                                                            node.Value)
                                    }.ShowDialog();
                                    return;
                            }

                            e.Handled = true;
                        }
                        break;
                    }
            }
        }
    }
}