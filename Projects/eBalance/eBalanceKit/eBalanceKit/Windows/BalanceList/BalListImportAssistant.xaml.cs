// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using eBalanceKit.Controls.BalanceList;
using eBalanceKit.Models.Assistants;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Templates;
using eBalanceKitResources.Localisation;
using MessageBox = System.Windows.MessageBox;

namespace eBalanceKit.Windows.BalanceList {
    /// <summary>
    /// Interaktionslogik für BalListImportAssistant.xaml
    /// </summary>
    public partial class BalListImportAssistant : Window {

        private readonly Dictionary<BalanceListImportType, Pages> _pageDictionary = new Dictionary<BalanceListImportType, Pages>();
        private readonly List<BalListImpAssistPageBase> _pageList;
        
        private readonly Page _commentPage = new Page("Comment", BalanceListImportConfig.ColumnToNameDict["Comment"]);
        private readonly Page _taxonomyPage = new Page("Taxonomy", BalanceListImportConfig.ColumnToNameDict["Taxonomy"]);
        private readonly Page _indexPage = new Page("Index", BalanceListImportConfig.ColumnToNameDict["Index"]);

        private int _pageListNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImportAssistant"/> class.
        /// </summary>
        internal BalListImportAssistant(Document document, Window owner) {
            InitializeComponent();
            Model = new BalListImportAssistantModel(document, this);
            DataContext = Model;
            Owner = owner;

            //columnPage = new BalListImpAssistPageColumnSelection();
            columnPage.ColumnSelected += PageColumnSelected;

            _pageList = new List<BalListImpAssistPageBase> {page1, page2, columnPage, summaryPage};


            var accountPage = new Page("AccountNumberCol", BalanceListImportConfig.ColumnToNameDict["AccountNumberCol"]);
            var namePage = new Page("AccountNameCol", BalanceListImportConfig.ColumnToNameDict["AccountNameCol"]);

            var balanceInTwoColumnsPages = new List<Page> {
                accountPage,
                namePage,
                new Page("DebitBalanceCol", BalanceListImportConfig.ColumnToNameDict["DebitBalanceCol"]),
                new Page("CreditBalanceCol", BalanceListImportConfig.ColumnToNameDict["CreditBalanceCol"])
            };

            var signedBalancePages = new List<Page> {
                accountPage,
                namePage,
                new Page("BalanceColSigned", BalanceListImportConfig.ColumnToNameDict["BalanceColSigned"])
            };

            var debitCreditFlagOneColumnPages = new List<Page> {
                accountPage,
                namePage,
                new Page("BalanceCol", BalanceListImportConfig.ColumnToNameDict["BalanceCol"]),
                new Page("DebitCreditFlagCol", BalanceListImportConfig.ColumnToNameDict["DebitCreditFlagCol"])
            };

            var debitCreditFlagTwoColumnsPages = new List<Page> {
                accountPage,
                namePage,
                new Page("BalanceCol", BalanceListImportConfig.ColumnToNameDict["BalanceCol"]),
                new Page("DebitFlagCol", BalanceListImportConfig.ColumnToNameDict["DebitFlagCol"]),
                new Page("CreditFlagCol", BalanceListImportConfig.ColumnToNameDict["CreditFlagCol"])
            };

            _pageDictionary[BalanceListImportType.BalanceInTwoColumns] = new Pages(balanceInTwoColumnsPages);
            _pageDictionary[BalanceListImportType.DebitCreditFlagOneColumn] = new Pages(debitCreditFlagOneColumnPages);
            _pageDictionary[BalanceListImportType.DebitCreditFlagTwoColumns] = new Pages(debitCreditFlagTwoColumnsPages);
            _pageDictionary[BalanceListImportType.SignedBalance] = new Pages(signedBalancePages);
        }

        private BalListImportAssistantModel Model { get; set; }

        private void PageColumnSelected(object sender, System.EventArgs e) {
            if (!Model.IsLastPage) {
                if (NavigateNext()) {
                    columnPage.preview.SelectedColumn = null;

                    if (Model.IsLastPage) {
                        assistantControl.NavigateNext();
                    }
                }
            }
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void BtnFinishClick(object sender, RoutedEventArgs e) {
            //if (!columnPage.Validate()) return;
            if (!Model.ImportData()) return;
            DialogResult = true;
        }

        private void BtnNextClick(object sender, RoutedEventArgs e) {
            if (NavigateNext()) {
                assistantControl.NavigateNext();
            }
        }

        private void BtnPreviousClick(object sender, RoutedEventArgs e) {
            if (NavigatePrevious()) {
                //assistantControl.NavigateBack();
            }
        }

        private void AddColumnText(Page page) {
            var columnPage = _pageList[2] as BalListImpAssistPageColumnSelection;
            if (columnPage == null) return;
            Model.PageColumnLabel = page.ColumnTitle;
            columnPage.SelectedColumnBinding = page.ColumnBinding;
        }

        public bool NavigateNext() {
            if (_pageListNumber < 2) {
                if (_pageList[_pageListNumber].Validate()) {
                    DataGridCreater.CreateColumns(page2.preview.dgCsvData, Model.Importer.PreviewData);
                    page2.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                    page2.preview.dgCsvData.AutoGenerateColumns = false;
                    DataGridCreater.CreateColumns(columnPage.preview.dgCsvData, Model.Importer.PreviewData);
                    columnPage.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                    columnPage.preview.dgCsvData.AutoGenerateColumns = false;
                    Model.Importer.RefreshPreviewData();
                    //Model.Importer.Config.OnPropertyChanged("Encoding");
                    // why not canceled, if the file contains no rows?
                    if (Model.Importer.PreviewData.Rows.Count == 0)
                        //Model.Importer.Config.OnPropertyChanged("CsvFileName");
                        return false;
                    if (AccountsProfileManager.DefaultElement.IsSelected || _pageListNumber == 2) {
                        //_pageList[_pageListNumber++].Visibility = Visibility.Collapsed;
                        _pageListNumber++;
                        if (_pageListNumber == 2) {
                            Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                            pages.SwitchToFirstPage();
                            AddColumnText(pages.CurrentPage());
                            if (Model.Importer.Config.TaxonomyColumnExists) pages.Add(_taxonomyPage);
                            if (Model.Importer.Config.Comment) pages.Add(_commentPage);
                            if (Model.Importer.Config.Index) pages.Add(_indexPage);
                        }
                        //_pageList[_pageListNumber].Visibility = Visibility.Visible;

                        Model.CurrentPage++;
                    }
                    // only continue, if it's a saved profile
                    if (AccountsProfileManager.DefaultElement.IsSelected) {
                        return true;
                    }
                    // if everything is set, just follow with the next page
                    if (_pageListNumber == 2) {
                        NavigateNext();
                        return true;
                    }
                    // set all the relevant data in the model from selected profile.
                    AccountsInformationProfile selectedProfile = AccountsProfileManager.SelectedElement;
                    Model.Importer.CsvReader.Separator = selectedProfile.Separator;
                    Model.Importer.CsvReader.StringsOptionallyEnclosedBy = selectedProfile.Delimiter;
                    Model.Importer.Config.Encoding = selectedProfile.Encoding;
                    Model.Importer.ClearColumns();
                    Model.Importer.Config.ImportType = selectedProfile.BalanceListImportType;
                    Model.Importer.Config.TaxonomyColumnExists = selectedProfile.WithTaxonomyColumn;
                    Model.Importer.Config.Comment = selectedProfile.WithComment;
                    Model.Importer.Config.FirstLineIsHeadline = selectedProfile.WithHeadLine;
                    Model.Importer.Config.Index = selectedProfile.WithIndex;
                    foreach (var columnInfo in selectedProfile.ColumnsDictionary) {
                        if (Model.Importer.PreviewData.Columns.Count <= columnInfo.Value) {
                            MessageBox.Show(this,
                                            string.Format(
                                                ResourcesBalanceList.
                                                    BalListImportAssistant_NavigateNext_TooFewColumnsToUseImportProfile,
                                                columnInfo.Value, Model.Importer.PreviewData.Columns.Count),
                                            string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                        //Model.Importer.Config.ColumnDict[columnInfo.Key] = Model.Importer.PreviewData.Columns[columnInfo.Value].ColumnName;
                        Model.Importer.Config.ColumnDict[columnInfo.Key] =
                            Model.Importer.PreviewData.Columns[columnInfo.Value].Name;
                        Model.Importer.PreviewData.Columns[columnInfo.Value].Color =
                            Model.Importer.Config.ColorDict[columnInfo.Key];
                    }
                    // same like pressing the next button
                    //_pageList[_pageListNumber++].Visibility = Visibility.Collapsed;
                    _pageListNumber++;
                    if (_pageListNumber == 2) {
                        Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                        pages.SwitchToFirstPage();
                        AddColumnText(pages.CurrentPage());
                        if (Model.Importer.Config.TaxonomyColumnExists) pages.Add(_taxonomyPage);
                        if (Model.Importer.Config.Comment) pages.Add(_commentPage);
                        if (Model.Importer.Config.Index) pages.Add(_indexPage);
                        assistantControl.NavigateNext();
                    }
                    //_pageList[_pageListNumber].Visibility = Visibility.Visible;

                    Model.CurrentPage++;
                    NavigateNext();
                    return true;
                }
            } else if (_pageListNumber == 2) {
                Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                DataGridCreater.CreateColumns(columnPage.preview.dgCsvData, Model.Importer.PreviewData);
                columnPage.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                columnPage.preview.dgCsvData.AutoGenerateColumns = false;
                DataGridCreater.CreateColumns(summaryPage.preview.dgCsvData, Model.Importer.PreviewData);
                summaryPage.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                summaryPage.preview.dgCsvData.AutoGenerateColumns = false;
                if (pages.HasNextPage()) {
                    if (_pageList[_pageListNumber].Validate()) {
                        AddColumnText(pages.NextPage());
                        Model.CurrentPage++;
                        // keep pressing the next button if it's a saved profile
                        if (!AccountsProfileManager.DefaultElement.IsSelected) {
                            NavigateNext();
                        }
                        return false;
                    }
                } else {
                    if (_pageList[_pageListNumber].Validate()) {
                        //_pageList[_pageListNumber++].Visibility = Visibility.Collapsed;
                        //_pageList[_pageListNumber].Visibility = Visibility.Visible;
                        _pageListNumber++;
                        Model.CurrentPage++;
                        assistantControl.NavigateNext();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool NavigatePrevious() {
            
            if (_pageListNumber == 0) return false;
            if (_pageListNumber == 1) {
                //_pageList[_pageListNumber--].Visibility = Visibility.Collapsed;
                //_pageList[_pageListNumber].Visibility = Visibility.Visible;
                _pageListNumber--;
                Model.CurrentPage--;
                // if it's a saved profile press back button, until first page.
                //if (!AccountsProfileManager.DefaultElement.IsSelected) {
                    assistantControl.NavigateBack();
                //}
                NavigatePrevious();
                return true;
            } else if (_pageListNumber == 2) {
                Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                var goBack = pages.HasPreviousPage();
                if (!goBack) {
                    _pageListNumber--;
                    //_pageList[_pageListNumber--].Visibility = Visibility.Collapsed;
                    //_pageList[_pageListNumber].Visibility = Visibility.Visible;
                    if (Model.Importer.Config.TaxonomyColumnExists) pages.Delete(_taxonomyPage);
                    if (Model.Importer.Config.Comment) pages.Delete(_commentPage);
                    if (Model.Importer.Config.Index) pages.Delete(_indexPage);
                } else {
                    AddColumnText(pages.PreviousPage());
                }
                DataGridCreater.CreateColumns(page2.preview.dgCsvData, Model.Importer.PreviewData);
                page2.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                page2.preview.dgCsvData.AutoGenerateColumns = false;
                Model.Importer.RefreshPreviewData();
                Model.CurrentPage--;
                // if it's a saved profile press back button, until first page.
                if (!AccountsProfileManager.DefaultElement.IsSelected) {
                    NavigatePrevious();
                }

                if (!goBack) {
                    assistantControl.NavigateBack();
                }

                return !goBack;
            } else if (_pageListNumber == 3) {
                var goBack = Model.IsLastPage;
                _pageListNumber--;
                //_pageList[_pageListNumber--].Visibility = Visibility.Collapsed;
                //_pageList[_pageListNumber].Visibility = Visibility.Visible;
                Model.CurrentPage--;
                // if it's a saved profile press back button, until first page.
                if (!AccountsProfileManager.DefaultElement.IsSelected) {
                    NavigatePrevious();
                }
                //if (goBack) {
                    assistantControl.NavigateBack();
                //}
                
                return true;
            }
            return false;
        }

        #region Nested type: Page
        private class Page {
            public Page(string columnBinding, string columnTitle) {
                ColumnTitle = columnTitle;
                ColumnBinding = columnBinding;
            }

            public string ColumnTitle { get; private set; }
            public string ColumnBinding { get; private set; }
        }
        #endregion

        #region Nested type: Pages
        private class Pages {
            private readonly List<Page> _pages;
            private int _currentPage;

            public Pages(List<Page> pages) { _pages = pages; }
            public void Add(Page page) { _pages.Add(page); }
            public void Delete(Page page) { _pages.Remove(page); }

            public void SwitchToFirstPage() { _currentPage = 0; }
            public bool HasNextPage() { return _currentPage < _pages.Count - 1; }
            public bool HasPreviousPage() { return _currentPage > 0; }

            public Page CurrentPage() { return _pages[_currentPage]; }

            public Page PreviousPage() { return _pages[--_currentPage]; }

            public Page NextPage() { return _pages[++_currentPage]; }
        }
        #endregion

        private void BtnSaveProfileOnClick(object sender, RoutedEventArgs e) {
            ProfileNameModel profileNameModel = new ProfileNameModel();
            DlgProfileName dlgProfileName = new DlgProfileName{Owner=Owner, DataContext = profileNameModel};
            dlgProfileName.ShowDialog();
            if (!profileNameModel.PressedOk || string.IsNullOrEmpty(profileNameModel.ProfileName)) {
                return;
            }

            // this dictionary will be saved as information to the account profile.
            // stores all the clicked columns with the number of ordinal of the column,
            // and it's key string in Config.ColumnDict
            Dictionary<string, int> actualDictionary = new Dictionary<string, int>();
            // BalanceColSigned had value of empty string. This was a problem here.
            var elements = from keyVal in Model.Importer.Config.ColumnDict
                           where !string.IsNullOrEmpty(keyVal.Value)
                           select keyVal;
            foreach (var keyVal in elements) {
                //actualDictionary[keyVal.Key] = Model.Importer.PreviewData.Columns[keyVal.Value].Ordinal;
                for (int i = 0; i < Model.Importer.PreviewData.Columns.Count; i++) {
                    if (Model.Importer.PreviewData.Columns[i].Name == keyVal.Value) {
                        actualDictionary[keyVal.Key] = i;
                        break;
                    }
                }
            }

            AccountsInformationProfile newProfile = new AccountsInformationProfile {
                Comment = string.Empty,
                Delimiter = Model.Importer.CsvReader.StringsOptionallyEnclosedBy,
                Separator = Model.Importer.CsvReader.Separator,
                Name = profileNameModel.ProfileName,
                IsSelected = false,
                Timestamp = DateTime.Now,
                Encoding = Model.Importer.Config.Encoding,
                ColumnsDictionary = actualDictionary,
                BalanceListImportType = Model.Importer.Config.ImportType,
                WithComment = Model.Importer.Config.Comment,
                WithHeadLine = Model.Importer.Config.FirstLineIsHeadline,
                WithTaxonomyColumn = Model.Importer.Config.TaxonomyColumnExists,
                WithIndex = Model.Importer.Config.Index,
                Creator = UserManager.Instance.CurrentUser
            };
            newProfile.Save();
            AccountsProfileManager.Items.Add(newProfile);
            MessageBox.Show(this, ResourcesBalanceList.BalListImportAssistant_BtnSaveProfileOnClick_Successfully_saved_import_profile_, "",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            //
        }

        private void btnShowExampleCSV_Click(object sender, RoutedEventArgs e) {
            new DlgExampleCsv().ShowDialog();
        }
    }
}