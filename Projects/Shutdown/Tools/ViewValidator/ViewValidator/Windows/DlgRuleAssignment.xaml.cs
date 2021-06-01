using System.Windows;
using System.Windows.Input;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.Models;
using ViewValidator.Models.Rules;
using ViewValidatorLogic.Manager;
using System.Windows.Controls;

namespace ViewValidator.Windows {
    public partial class DlgRuleAssignment : Window {

        RuleAssignmentModel Model { get { return DataContext as RuleAssignmentModel; } }

        public DlgRuleAssignment() {
            InitializeComponent();

            
            //foreach (var rule in RuleManager.Instance.ColumnRules) {
            //    StackPanel itemPanel = new StackPanel();
            //    TextBlock ruleText = new TextBlock()
            //                         {Text = rule.Name, Style = (Style) FindResource("NavigationTreeHeaderText")};
                
            //    Border border = new Border() {Style = (Style) FindResource("NavigationTreeItemBg"), Margin = new Thickness(3,3,3,3)};
                
            //    border.Child = ruleText;
            //    columnRulesStackPanel.Children.Add(border);
            //}
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Close();
        }

        #region Drag Drop Handler
        private void HandlePreviewMouseDown(ListBox list, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(list);
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(list, _startPoint);
            if (lbi != null) lbi.IsSelected = true;
        }

        private Point _startPoint;

        private void HandleMouseMove(ListBox listBox, MouseEventArgs e) {
            if (Model.DragDropData.Dragging) return;
            if (e.LeftButton == MouseButtonState.Pressed) {
                ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(listBox, _startPoint);
                if (lbi != null) {
                    lbi.IsSelected = true;
                    var btn = UIHelpers.TryFindFromPoint<Button>(listBox, _startPoint);
                    if (btn == null) {
                        //e.Handled = true;
                        Model.DragDropData.Clear();
                        foreach (object obj in listBox.SelectedItems) {
                            Model.DragDropData.Rules.Add((Rule) obj);
                        }
                        Model.DragDropData.Dragging = true;
                        DragDrop.DoDragDrop(listBox, Model.DragDropData, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void HandleDrop(ListBox listBox, DragEventArgs e) {
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(listBox, e.GetPosition(listBox));
            if (lbi != null) {
                Rule moveToRule = lbi.Content as Rule;
                Model.ProfileRules.MoveRule(Model.DragDropData.Rules[0], moveToRule);
            }
        }
        #endregion Drag Drop Handler

        #region RuleList Events
        private void ruleList_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            HandlePreviewMouseDown(ruleList, e);
        }

        private void ruleList_Drop(object sender, DragEventArgs e) {
            HandleDrop(ruleList, e);
        }

        private void ruleList_MouseMove(object sender, MouseEventArgs e) {
            HandleMouseMove(ruleList, e);
        }
        #endregion RuleList Events

        #region SortRuleList Events
        private void sortRuleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            HandlePreviewMouseDown(sortRuleList, e);
        }

        private void sortRuleList_MouseMove(object sender, MouseEventArgs e) {
            HandleMouseMove(sortRuleList, e);
        }

        private void sortRuleList_Drop(object sender, DragEventArgs e) {
            HandleDrop(sortRuleList, e);
        }
        #endregion SortRuleList Events

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                if (Visibility == Visibility.Visible) {
                    Model.DoUpdate = true;
                    Model.Update();
                } else Model.DoUpdate = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }

        private void Window_DragOver(object sender, DragEventArgs e) {
            if (this.DragDropPopup.IsOpen) {
                Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    e.Effects = DragDropEffects.Copy;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            Model.DragDropData.Dragging = false;
            //If a rule is dropped somewhere in the window where it should not be dropped, then the user most certainly wanted to delete the rule
            if (Model.DragDropData.DragSource != null) {
                Model.DragDropData.DragSource.DeleteRule();
            }
        }


        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            if (ruleList.SelectedIndex != -1) {
                Model.ProfileRules.RemoveRule((Rule)ruleList.SelectedItem);
            }
        }

        private void AddRule() {
            AvdCommon.Rules.Gui.DlgNewRule dlgNewRule = new AvdCommon.Rules.Gui.DlgNewRule() { Owner = this };
            NewRuleModel model = new NewRuleModel();
            dlgNewRule.DataContext = model;

            bool? dialogResult = dlgNewRule.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value && Model.ProfileRules != null) {
                Model.ProfileRules.AddRule(model.EditRuleModel.RuleWithParameters, null, true);
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            AddRule();
        }

        private void NewRule_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void NewRule_Executed(object sender, ExecutedRoutedEventArgs e) {
            e.Handled = true;
            AddRule();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Model.DragDropData.Dragging = false;
        }





        //private void ruleList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        //    ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(ruleList, _startPoint);
        //    if (lbi != null) {
        //        ruleList.UnselectAll();
        //        lbi.IsSelected = true;
        //    }

        //}


        //private void ruleList_DragLeave(object sender, DragEventArgs e) {
        //    if (e.Data.GetDataPresent("RuleListDragDropData")) {
        //        RuleListDragDropData data = e.Data.GetData("RuleListDragDropData") as RuleListDragDropData;

        //        e.Effects = DragDropEffects.None;
        //        data.AllowDrop = false;
        //    }
        //}

        //private void ruleList_DragOver(object sender, DragEventArgs e) {
        //    if (!e.Data.GetDataPresent("RuleListDragDropData")) {
        //        e.Effects = DragDropEffects.None;
        //    } else {
        //        RuleListDragDropData data = e.Data.GetData("RuleListDragDropData") as RuleListDragDropData;
        //        e.Effects = DragDropEffects.Move;
        //        data.AllowDrop = true;
        //    }
        //}


    }
}
