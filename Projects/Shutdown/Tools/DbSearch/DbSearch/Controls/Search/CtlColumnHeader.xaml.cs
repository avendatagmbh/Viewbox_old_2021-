using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbSearch.Models.Search;
using Utils;

namespace DbSearch.Controls.Search {
    /// <summary>
    /// Interaktionslogik für CtlColumnHeader.xaml
    /// </summary>
    public partial class CtlColumnHeader : UserControl {
        public CtlColumnHeader() {
            InitializeComponent();
        }
        //~CtlColumnHeader() {
        //    if(Model != null && Model.Column != null && Model.Column.RuleSet != null && Model.Column.RuleSet.AllRules != null)
        //        Model.Column.RuleSet.AllRules.CollectionChanged -= AllRules_CollectionChanged;
        //}

        public ColumnHeaderModel Model { get { return DataContext as ColumnHeaderModel; } }

        private Stopwatch _timer = new Stopwatch();
        private void btnEditComment_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (_timer.ElapsedMilliseconds > 200 || !_timer.IsRunning) {
                commentPopup.IsOpen = true;
                txtComment.Focus();
            }
        }

        private void commentPopup_Closed(object sender, System.EventArgs e) {
            _timer.Reset();
            _timer.Start();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                commentPopup.IsOpen = false;
                rulesPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void btnHideColumn_Click(object sender, RoutedEventArgs e) {
            Model.Column.IsVisible = false;
        }

        private void btnRules_Click(object sender, RoutedEventArgs e) {
            if (_timer.ElapsedMilliseconds > 200 || !_timer.IsRunning) {
                rulesPopup.IsOpen = !rulesPopup.IsOpen;
            }
        }

        #region Hack to close Rule popup
        //This is needed as the binding does not work
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                Model.Column.RuleSet.AllRules.CollectionChanged -= AllRules_CollectionChanged;
                Model.Column.RuleSet.AllRules.CollectionChanged += AllRules_CollectionChanged;
            }
        }

        void AllRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (rulesPopup.IsOpen && Model.Column.RuleSet.AllRules.Count == 0) rulesPopup.IsOpen = false;
        }
        #endregion Hack to close Rule popup


    }
}
