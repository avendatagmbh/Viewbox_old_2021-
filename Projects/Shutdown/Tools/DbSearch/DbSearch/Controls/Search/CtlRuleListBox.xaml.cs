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
using AvdCommon.Rules;
using DbSearch.Models.Rules;
using DbSearch.Structures;
using Utils;

namespace DbSearch.Controls.Search {
    /// <summary>
    /// Interaktionslogik für CtlRuleListBox.xaml
    /// </summary>
    public partial class CtlRuleListBox : UserControl {
        public CtlRuleListBox() {
            InitializeComponent();
        }

        RuleListBoxModel Model { get { return DataContext as RuleListBoxModel; } }

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
                        Model.ClearDragDropData();
                        foreach (object obj in listBox.SelectedItems) {
                            Model.DragDropData.Rules.Add((Rule)obj);
                        }
                        Model.DragDropData.Dragging = true;
                        //DragDrop.DoDragDrop(listBox, Model.DragDropData, DragDropEffects.Copy);
                        StartDragInProcAdorner(e);
                    }
                }
            }
        }

        private void HandleDrop(ListBox listBox, DragEventArgs e) {
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(listBox, e.GetPosition(listBox));
            if (lbi != null) {
                Rule moveToRule = lbi.Content as Rule;
                Model.Rules.MoveRule(Model.DragDropData.Rules[0], moveToRule);
            }
            Model.DragDropData.DeleteRuleOnFinished = false;
        }
        #endregion Drag Drop Handler

        #region RuleList Events
        private void ruleList_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            HandlePreviewMouseDown(ruleList, e);
        }

        private void ruleList_Drop(object sender, DragEventArgs e) {
            HandleDrop(ruleList, e);
            e.Handled = true;
        }

        private void ruleList_MouseMove(object sender, MouseEventArgs e) {
            HandleMouseMove(ruleList, e);
            //if (this.DragDropPopup.IsOpen) {
            //    Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
            //    //DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            //    DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            //    //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            //    //    e.Effects = DragDropEffects.Copy;
            //    DragDrop.DoDragDrop(ruleList, Model.DragDropData, DragDropEffects.Copy);
            //}
            //if (e.LeftButton == MouseButtonState.Pressed && !Model.DragDropData.Dragging) {
            //    Point position = e.GetPosition(null);

            //    if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
            //        Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
            //        StartDragInProcAdorner(e);
            //    }
            //}
        }

        #endregion RuleList Events

        private void StartDragInProcAdorner(MouseEventArgs e) {
            // Let's define our DragScope .. In this case it is every thing inside our main window .. 
            FrameworkElement DragScope = Application.Current.MainWindow.Content as FrameworkElement;
            System.Diagnostics.Debug.Assert(DragScope != null);

            // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
            bool previousDrop = DragScope.AllowDrop;
            DragScope.AllowDrop = true;

            // Let's wire our usual events.. 
            // GiveFeedback just tells it to use no standard cursors..  

            //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
            //this.ruleList.GiveFeedback += feedbackhandler;

            // The DragOver event ... 
            DragEventHandler draghandler = new DragEventHandler(Window1_DragOver);
            DragScope.PreviewDragOver += draghandler;
            // Drag Leave is optional, but write up explains why I like it .. 
            //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
            //DragScope.DragLeave += dragleavehandler;

            // QueryContinue Drag goes with drag leave... 
            QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
            DragScope.QueryContinueDrag += queryhandler;

            //Here we create our adorner.. 
            //_adorner = new DragAdorner(DragScope, (UIElement)this.dragElement, true, 0.5);
            //_layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
            //_layer.Add(_adorner);


            //IsDragging = true;
            //_dragHasLeftScope = false;
            //Finally lets drag drop 
            //DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            DragDropEffects de = DragDrop.DoDragDrop(this.ruleList, Model.DragDropData, DragDropEffects.Move);

            // Clean up our mess :) 
            DragScope.AllowDrop = previousDrop;
            //AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
            //_adorner = null;

            //ruleList.GiveFeedback -= feedbackhandler;
            //DragScope.DragLeave -= dragleavehandler;
            DragScope.QueryContinueDrag -= queryhandler;
            DragScope.PreviewDragOver -= draghandler;

            Model.DragDropData.Dragging = false;
            if (Model.DragDropData.SourceRuleSet != null && Model.DragDropData.DeleteRuleOnFinished) {
                foreach(var rule in Model.DragDropData.Rules)
                    Model.DragDropData.SourceRuleSet.RemoveRule(rule);
            }
        }

        private void UpdateWindowLocation() {
            Win32Helper.POINT p;
            if (!Win32Helper.GetCursorPos(out p)) {
                return;
            }
            Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
            DragDropPopup.PlacementRectangle = new Rect(ruleList.PointFromScreen(new Point(p.X, p.Y)), popupSize);
        }
        private void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            UpdateWindowLocation();
        }

        private void Window1_DragOver(object sender, DragEventArgs e) {
            UpdateWindowLocation();
        }

    }
}
