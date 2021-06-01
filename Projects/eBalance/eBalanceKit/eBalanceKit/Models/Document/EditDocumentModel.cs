// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Taxonomy;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Models.Document {
    /// <summary>
    /// Model class for the CtlEditDocument control.
    /// </summary>
    public class EditDocumentModel : INotifyPropertyChanged {
        public EditDocumentModel(eBalanceKitBusiness.Structures.DbMapping.Document document, Window owner) {
            Owner = owner;
            
            // assign value lists
            Companies = CompanyManager.Instance.AllowedCompanies;
            Systems = SystemManager.Instance.Systems;
            Document = document;
        }

        #region events
        /// <summary>
        /// Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion events

        #region eventTrigger
        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion eventTrigger

        #region fields
        /// <summary>
        /// See property SelectedCompany
        /// </summary>
        private Company _selectedCompany;
        #endregion fields

        #region properties
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>The document.</value>
        private eBalanceKitBusiness.Structures.DbMapping.Document _document;

        /// <summary>
        /// Gets or sets the companies.
        /// </summary>
        /// <value>The companies.</value>
        public ObservableCollection<Company> Companies { get; set; }

        /// <summary>
        /// Gets or sets the selected company.
        /// </summary>
        /// <value>The selected company.</value>
        public Company SelectedCompany {
            get { return _selectedCompany; }
            set {
                if (_selectedCompany != value) {
                    _selectedCompany = value;
                    if (value != null) {
                        //this.SelectedFinancialYear = null;

                        FinancialYears = value.VisibleFinancialYears;
                        if (FinancialYears.Count > 0) SelectedFinancialYear = FinancialYears[0];
                        OnPropertyChanged("FinancialYears");

                        if (GlobalResources.MainWindow.Dispatcher.CheckAccess()) {
                            ((MainWindowModel)GlobalResources.MainWindow.DataContext).RefreshCompanies();
                        } else {
                            GlobalResources.MainWindow.Dispatcher.BeginInvoke(
                                new Action(() => ((MainWindowModel)GlobalResources.MainWindow.DataContext).RefreshCompanies()),
                                DispatcherPriority.Render);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the periods.
        /// </summary>
        /// <value>The periods.</value>
        public ObservableCollection<FinancialYear> FinancialYears { get; set; }

        /// <summary>
        /// Gets or sets the selected period.
        /// </summary>
        /// <value>The selected period.</value>
        private FinancialYear _selectedFinancialYear;
        public FinancialYear SelectedFinancialYear {
            get { return _selectedFinancialYear; }
            set {
                _selectedFinancialYear = value;
                OnPropertyChanged("SelectedFinancialYear");

                if (GlobalResources.MainWindow.Dispatcher.CheckAccess()) {
                    ((MainWindowModel)GlobalResources.MainWindow.DataContext).RefreshCompanies();
                } else {
                    GlobalResources.MainWindow.Dispatcher.BeginInvoke(
                        new Action(() => ((MainWindowModel) GlobalResources.MainWindow.DataContext).RefreshCompanies()),
                        DispatcherPriority.Render);
                }
            }
        }

        /// <summary>
        /// Gets or sets the systems.
        /// </summary>
        /// <value>The systems.</value>
        public ObservableCollection<eBalanceKitBusiness.Structures.DbMapping.System> Systems { get; set; }
        
        #region SelectedSystem
        private eBalanceKitBusiness.Structures.DbMapping.System _selectedSystem;

        /// <summary>
        /// Gets or sets the selected system.
        /// </summary>
        /// <value>The selected system.</value>
        public eBalanceKitBusiness.Structures.DbMapping.System SelectedSystem {
            get { return _selectedSystem; } 
            set {
                _selectedSystem = value;

                if (GlobalResources.MainWindow.Dispatcher.CheckAccess()){
                    if (GlobalResources.MainWindow.DataContext != null) 
                        ((MainWindowModel)GlobalResources.MainWindow.DataContext).RefreshSystems();
                }
                else{
                    GlobalResources.MainWindow.Dispatcher.BeginInvoke(
                        new Action(() => ((MainWindowModel) GlobalResources.MainWindow.DataContext).RefreshSystems()),
                        DispatcherPriority.Render);
                }
            }
        }
        #endregion // SelectedSystem

        public eBalanceKitBusiness.Structures.DbMapping.Document Document {
            get { return _document; }
            set {
                _document = value;
                if (_document != null) {
                    // set selected company
                    if (_document.Company != null) {
                        SelectedCompany = _document.Company;
                        FinancialYears = SelectedCompany.VisibleFinancialYears;
                    } else if (Companies.Count == 1) {
                        SelectedCompany = Companies[0];
                        FinancialYears = SelectedCompany.VisibleFinancialYears;
                    }
                    // set selected system
                    if (_document.System != null) {
                        SelectedSystem = _document.System;
                    } else if (Systems.Count == 1) {
                        SelectedSystem = Systems[0];
                    }

                    // set selected financial year
                    if (_document.FinancialYear != null) {
                        SelectedFinancialYear = _document.FinancialYear;
                    } else if (FinancialYears.Count == 1) {
                        SelectedFinancialYear = FinancialYears[0];
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EditDocumentModel"/> is changed.
        /// </summary>
        /// <value><c>true</c> if changed; otherwise, <c>false</c>.</value>
        public bool Changed { get; set; }

        public Window Owner { get; set; }

        public ReportRights ReportRights { get { return Document != null ? Document.ReportRights : null; } }

        #endregion properties
    }
}