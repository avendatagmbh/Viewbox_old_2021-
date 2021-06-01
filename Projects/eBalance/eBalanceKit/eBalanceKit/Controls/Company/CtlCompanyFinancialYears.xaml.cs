using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Models;
using eBalanceKitResources.Localisation;
using FinancialYear = eBalanceKitBusiness.Structures.DbMapping.FinancialYear;

namespace eBalanceKit.Controls.Company {
    
    /// <summary>
    /// Interaktionslogik für CtlCompanyFinancialYears.xaml
    /// </summary>
    public partial class CtlCompanyFinancialYears : UserControl, INotifyPropertyChanged {
        
        public CtlCompanyFinancialYears() {
            InitializeComponent();
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region eventHandler
        
        private void txtLasstFinancialYear_KeyDown(object sender, KeyEventArgs e) {
            CheckKey(e);
        }

        private void txtFirstFinancialYear_KeyDown(object sender, KeyEventArgs e) {
            CheckKey(e);
        }
        
        #endregion

        internal CtlCompanyFinancialYearsModel Model { get { return this.DataContext as CtlCompanyFinancialYearsModel; } }
        /// <summary>
        /// Checks the key.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private static void CheckKey(KeyEventArgs e) {
            switch (e.Key) {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.Tab:
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Validate() {
            return true;
        }

        private void cboFinancialYearFrom_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // we are still loading the values
            bool loading = e.RemovedItems == null || e.RemovedItems.Count == 0;

            if (this.Model.SelectedFinancialYearFrom > this.Model.SelectedFinancialYearTo) {
                this.Model.SelectedFinancialYearTo = this.Model.SelectedFinancialYearFrom;
            }

            if (loading) {
                return;
            }

            try {
                // throws an Exception if a report for a removed financial year is existing
                this.Model.Company.SetVisibleFinancialYearIntervall(this.Model.SelectedFinancialYearFrom, this.Model.SelectedFinancialYearTo);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                // set back the SelectedFinancialYearFrom to the old value
                this.Model.SelectedFinancialYearFrom = (int) e.RemovedItems[0];
            }
            
        }

        private void cboFinancialYearTo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // we are still loading the values
            bool loading = e.RemovedItems == null || e.RemovedItems.Count == 0;

            if (this.Model.SelectedFinancialYearFrom > this.Model.SelectedFinancialYearTo) {
                this.Model.SelectedFinancialYearFrom = this.Model.SelectedFinancialYearTo;
            }
            if (loading) {
                return;
            }

            try {
                // throws an Exception if a report for a removed financial year is existing
                this.Model.Company.SetVisibleFinancialYearIntervall(this.Model.SelectedFinancialYearFrom, this.Model.SelectedFinancialYearTo);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                // set back the SelectedFinancialYearTo to the old value
                this.Model.SelectedFinancialYearTo = (int)e.RemovedItems[0];
            }
        }
    }
}