// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class CtlTemplateDetails {

        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point _startPoint;
        
        public CtlTemplateDetails() {
            InitializeComponent();
            txtName.MaxLength =
                (int)
                ClassArgumentHelper.Instance.GetColumnAttribute(
                    new ArgumentSearchQuestion(
                        "eBalanceKitBusiness.Structures.DbMapping.MappingTemplate.MappingTemplateHead", "Name",
                        new[] {"Length"}))[0];
            txtAccountStructure.MaxLength =
                (int)
                ClassArgumentHelper.Instance.GetColumnAttribute(
                    new ArgumentSearchQuestion(
                        "eBalanceKitBusiness.Structures.DbMapping.MappingTemplate.MappingTemplateHead", "AccountStructure",
                        new[] {"Length"}))[0];
            txtComment.MaxLength = 
                (int)
                ClassArgumentHelper.Instance.GetColumnAttribute(
                    new ArgumentSearchQuestion(
                        "eBalanceKitBusiness.Structures.DbMapping.MappingTemplate.MappingTemplateHead", "Comment",
                        new[] {"Length"}))[0];
        }

        private TemplateDetailModel Model { get { return DataContext as TemplateDetailModel; } }
        private Window Owner { get { return UIHelpers.TryFindParent<Window>(this); } }

        private void BtnSaveClick(object sender, RoutedEventArgs e) { Save(); }
        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            if (Model.Template.Id > 0) {
                TemplateManager.ResetTemplate(Model.Template);
            }
            Owner.DialogResult = false;
        }

        private void Save() {
            if (!Validate()) return;
            txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtAccountStructure.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            //TemplateManager.SaveTemplate(Model.Template);
            Owner.DialogResult = true;
        }

        private bool Validate() {
            return true;
        }

        private void dragItem_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            var data = DragDropPopup.DataContext as TemplateGuiDragDropData;
            if (data == null) return;

            if (data.AllowDrop) {
                popupBorder.Opacity = 1.0;
                //this.Cursor = Cursors.No;
            } else {
                popupBorder.Opacity = 0.5;
                //this.Cursor = Cursors.Hand;
            }
        }
        
        private void UserControlDragOver(object sender, DragEventArgs e) {
            if (DragDropPopup.IsOpen) {
                var popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            }
        }

        private void assignmentList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _startPoint = e.GetPosition(assignmentList); }

        private void assignmentList_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && assignmentList.SelectedItem != null && _startPoint != null &&
                assignmentList.SelectedItem is MappingLineGui) {
                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(assignmentList, _startPoint);
                if (tvi == null) return; // no account item under mouse pointer

                Point position = e.GetPosition(assignmentList);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    var assignments = new List<MappingLineGui>();
                    assignments.Add((MappingLineGui)assignmentList.SelectedItem);

                    var data = new TemplateGuiDragDropData(assignments);
                    var dragData = new DataObject("TemplateGUIDragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(assignmentList, dragData, DragDropEffects.Move);

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }

        private void assignmentList_DragOver(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                e.Effects = DragDropEffects.None;
                return;
            }

            var data = e.Data.GetData("TemplateGUIDragDropData") as TemplateGuiDragDropData;

            e.Effects = DragDropEffects.Move;
            data.AllowDrop = true;
        }

        private void assignmentList_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                e.Effects = DragDropEffects.None;
                return;
            }
        }

        private void assignmentList_DragLeave(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                var data = e.Data.GetData("TemplateGUIDragDropData") as TemplateGuiDragDropData;

                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
            }
        }

        private void assignmentList_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                var data = e.Data.GetData("TemplateGUIDragDropData") as TemplateGuiDragDropData;

                List<MappingLineGui> assignments = ((TemplateGuiDragDropData)DragDropPopup.DataContext).Items;
                foreach (MappingLineGui assignment in assignments) {
                    assignment.RemoveFromParents();
                    assignment.ElementId = null;
                }
            }
        }

        private void assignmentList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { assignmentList.Focus(); }

        private void btnDeleteAssignment_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var tvi = UIHelpers.TryFindParent<TreeViewItem>(btn);
            if (tvi != null && tvi.DataContext is MappingLineGui) {
                var ass = tvi.DataContext as MappingLineGui;
                ass.RemoveFromParents();
                ass.ElementId = null;
            }
        }
    }
}