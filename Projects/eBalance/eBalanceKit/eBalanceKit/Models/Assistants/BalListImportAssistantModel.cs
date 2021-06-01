/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2012-01-18      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AvdCommon.DataGridHelper.Interfaces;
using eBalanceKit.Models.Document;
using System.Data;
using System.Windows;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Globalization;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Import;

namespace eBalanceKit.Models.Assistants {


    internal class BalListImportAssistantModel : INotifyPropertyChanged {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImportAssistantModel"/> class.
        /// </summary>
        public BalListImportAssistantModel(eBalanceKitBusiness.Structures.DbMapping.Document document, Window owner) {
            this.CurrentPage = 0;
            this.Importer = new BalanceListImporter(document, owner);
            this.Importer.Config.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
            this.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
        }

        void Config_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ImportType":
                    OnPropertyChanged("IsLastPage");
                    OnPropertyChanged("PageCount");
                    break;
                case "IsLastPage":
                    if (IsLastPage)
                        this.Importer.LastPageReached();
                    break;
                case "PageCount":
                case "CurrentPage":
                    OnPropertyChanged("StepLabel");
                    break;
            }
        }


        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region properties

        public string StepLabel {
            get { return eBalanceKitResources.Localisation.ResourcesCommon.Page + " " + (_currentPage + 1) + " / " + PageCount; }
        }

        #region PageColumnLabel
        private string _pageColumnLabel;

        public string PageColumnLabel {
            get { return eBalanceKitResources.Localisation.ResourcesBalanceList.ColumnChoice + " " + _pageColumnLabel; }
            set {
                if (_pageColumnLabel != value) {
                    _pageColumnLabel = value;
                    OnPropertyChanged("PageColumnLabel");
                }
            }
        }
        #endregion PageColumnLabel

        public int CurrentPage {
            get { return _currentPage; }
            set {
                if (_currentPage != value) {
                    _currentPage = value;
                    OnPropertyChanged("CurrentPage");
                    OnPropertyChanged("IsFirstPage");
                    OnPropertyChanged("IsLastPage");
                }
            }
        }
        private int _currentPage;

        public bool IsFirstPage { get { return this.CurrentPage == 0; } }
        public bool IsLastPage { get { return this.CurrentPage == this.PageCount - 1; } }                   

        public int PageCount {
            get {
                int addPages = 0;
                if (Importer.Config.TaxonomyColumnExists) addPages++;
                if (Importer.Config.Comment) addPages++;
                if (Importer.Config.Index) addPages++;

                switch (Importer.Config.ImportType) {
                    case  BalanceListImportType.SignedBalance:
                        return 6 + addPages;

                    case BalanceListImportType.DebitCreditFlagOneColumn:
                        return 7 + addPages;

                    case BalanceListImportType.DebitCreditFlagTwoColumns:
                        return 8 + addPages;

                    case BalanceListImportType.BalanceInTwoColumns:
                        return 7 + addPages;

                    default:
                        return 6 + addPages;
                }                
            } 
        }

        public string SampleBalanceListText {
            get {
                return
                    @"Kontonummer;Kontoname;Saldo
2001000;EDV-Software;21232,57
8201000;Betriebs- und Geschäftsausstattung;737,21
8401000;Fuhrpark;19000,21";
            }
        }
        #endregion properties

        #region methods

        public BalanceListImporter Importer { get; private set; }

        #region UseTestConfig

        public bool UseTestConfig {
            get { return Importer.UseTestConfig; }
            set {
                if (Importer.UseTestConfig != value) {
                    Importer.UseTestConfig = value;
                    
                }
            }
        }
        #endregion UseTestConfig

        public bool ImportData() {
            if (!Importer.Import()) {
                MessageBox.Show(Importer.LastError);
                return false;
            } else return true;
        }

        #endregion methods       
    } 
}
