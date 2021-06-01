using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using eBalanceKit.Models.Assistants;
using eBalanceKit.Windows.BalanceList;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.Templates;
using eBalanceKitResources.Localisation;
using Page = eBalanceKitBusiness.Structures.Page;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für BalanceListImportAssistant.xaml
    /// </summary>
    public partial class BalanceListImportAssistant : UserControl, INotifyPropertyChanged {
        private readonly Page _commentPage = new Page("Comment", BalanceListImportConfig.ColumnToNameDict["Comment"]);
        private readonly Page _indexPage = new Page("Index", BalanceListImportConfig.ColumnToNameDict["Index"]);

        private readonly Dictionary<BalanceListImportType, Pages> _pageDictionary =
            new Dictionary<BalanceListImportType, Pages>();

        private readonly List<BalListImpAssistPageBase> _pageList;

        private readonly Page _taxonomyPage = new Page("Taxonomy", BalanceListImportConfig.ColumnToNameDict["Taxonomy"]);

        private int _pageListNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceListImportAssistant" /> class.
        /// </summary>
        public BalanceListImportAssistant() {
            InitializeComponent();

            // Declare the base page list.
            // Page1 -> CSV selection
            // Page2 -> Delimiter selection
            // Columnpage -> Choose columns to analyze + import, this group could contain multiple elements
            // Errorpage -> If there're any errors, this page will be displayed.
            // Summarypage -> Summary of analization.
            _pageList = new List<BalListImpAssistPageBase> {page1, page2, columnPage, errorPage, summaryPage};
            Owner = Utils.UIHelpers.TryFindParent<Window>(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceListImportAssistant" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public BalanceListImportAssistant(Window owner) {
            InitializeComponent();
            Owner = owner ?? Utils.UIHelpers.TryFindParent<Window>(this);
            RefreshInformation(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImportAssistant"/> class.
        /// </summary>
        internal BalanceListImportAssistant(eBalanceKitBusiness.Structures.DbMapping.Document document, Window owner) {
            Owner = owner ?? Utils.UIHelpers.TryFindParent<Window>(this);
            InitializeComponent();

            // Declare the base page list.
            // Page1 -> CSV selection
            // Page2 -> Delimiter selection
            // Columnpage -> Choose columns to analyze + import, this group could contain multiple elements
            // Errorpage -> If there're any errors, this page will be displayed.
            // Summarypage -> Summary of analization.
            _pageList = new List<BalListImpAssistPageBase> {page1, page2, columnPage, errorPage, summaryPage};

            RefreshInformation(true, document);
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        internal Window Owner { get; set; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        internal BalListImportAssistantModel Model { get { return (BalListImportAssistantModel) DataContext; } }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /// <summary>
        /// Refreshes the information.
        /// </summary>
        /// <param name="isRunningSeperate">if set to <c>true</c> [is running seperate].</param>
        /// <param name="document">The document.</param>
        /// <param name="balanceListNumber">The balance list number.</param>
        public void RefreshInformation(bool isRunningSeperate = true,
                                       eBalanceKitBusiness.Structures.DbMapping.Document document = null,
                                       int balanceListNumber = 0) {
            DataContext = new BalListImportAssistantModel(document, Owner, isRunningSeperate, page2.preview.dgCsvData);

            if (balanceListNumber != 0) {
                Model.Importer.Config.BalanceListName = ResourcesBalanceList.DefaultBalanceListName + " " +
                                                        balanceListNumber;
            }

            InitPages();
            assistantControl.SelectedIndex = 0;
            _pageListNumber = 0;
        }

        /// <summary>
        /// Refreshes the information.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="startPage">The start page.</param>
        internal void RefreshInformation(BalListImportAssistantModel model, int startPage) {
            DataContext = model;

            InitPages();

            foreach (Page t in _pageDictionary[model.SelectedImportTypeElement.BalanceListImportType].PageList.Skip(1)) {
                _pageDictionary[model.SelectedImportTypeElement.BalanceListImportType].NextPage();
            }
            assistantControl.SelectedIndex = startPage == -1 ? assistantControl.Items.Count - 1 : startPage;
            _pageListNumber = assistantControl.SelectedIndex;
        }

        /// <summary>
        /// Inits the pages.
        /// </summary>
        private void InitPages() {
            //columnPage = new BalListImpAssistPageColumnSelection();
            columnPage.ColumnSelected -= PageColumnSelected;
            columnPage.ColumnSelected += PageColumnSelected;
            errorPage.NextButtonClicked += PageColumnSelected;
            errorPage.PreviousButtonClicked += PageGoBack;


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

        /// <summary>
        /// Handles the ColumnSelected event of the Page control.
        /// This event occurs when the user clicks on 'Next' on the error page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void PageColumnSelected(object sender, System.EventArgs e) {
            if ((Model.IsLastPage && (!Model.HasErrors || Model.NextIsLastPage)) || !NavigateNext()) {
                return;
            }
            columnPage.preview.SelectedColumn = null;

            if (Model.IsLastPage) {
                assistantControl.NavigateNext();
            }

            Model.OnPropertyChanged("NextIsLastPage");
        }

        /// <summary>
        /// Handles the Click event of the BtnCancel control.
        /// This event occurs when the user clicks on 'Previous' on the error page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            if (Model.RunningSeperate) {
                Owner.DialogResult = false;
                return;
            }

            Model.Status = StatusResult.Canceled;
        }

        /// <summary>
        /// Handles the GoBack event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void PageGoBack(object sender, System.EventArgs e) {
            if (Model.IsFirstPage || !NavigatePrevious()) {
                return;
            }

            columnPage.preview.SelectedColumn = null;

            if (Model.IsLastPage) {
                assistantControl.NavigateNext();
            }

            Model.OnPropertyChanged("NextIsLastPage");
        }

        /// <summary>
        /// Handles the Click event of the BtnFinish control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnFinishClick(object sender, RoutedEventArgs e) {
            if (!Model.ImportData()) {
                return;
            }
            if (Model.RunningSeperate) {
                Owner.DialogResult = true;
            } else {
                Model.Status = StatusResult.Imported;
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnNextClick(object sender, RoutedEventArgs e) {
            if (Model.RunningSeperate || !Model.IsLastPage) {
                if (NavigateNext()) {
                    assistantControl.NavigateNext();
                }
                return;
            }

            BtnFinishClick(sender, e);
        }

        /// <summary>
        /// Handles the Click event of the BtnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnPreviousClick(object sender, RoutedEventArgs e) {
            if (!Model.RunningSeperate && _pageListNumber == 0) {
                Model.Status = StatusResult.GoBack;
                return;
            }

            if (NavigatePrevious()) {
                Model.OnPropertyChanged("NextIsLastPage");
            }
        }

        /// <summary>
        /// Adds the column text.
        /// </summary>
        /// <param name="page">The page.</param>
        private void AddColumnText(Page page) {
            var columnPage = _pageList[2] as BalListImpAssistPageColumnSelection;
            if (columnPage == null) {
                return;
            }

            Model.PageColumnLabel = page.ColumnTitle;
            Model.Importer.SummaryConfig.CurrentPage = page;

            columnPage.SelectedColumnBinding = page.ColumnBinding;
        }

        /// <summary>
        /// Navigates the next.
        /// </summary>
        /// <returns>False if the navigation couldn't be done, else true.</returns>
        private bool NavigateNext() {
            Model.ErrorsSetInThisRound = false;

            if (_pageListNumber < 2) {
                if (_pageList[_pageListNumber].Validate()) {
                    DataGridCreater.CreateColumns(page2.preview.dgCsvData, Model.Importer.PreviewData);
                    page2.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                    page2.preview.dgCsvData.AutoGenerateColumns = false;
                    DataGridCreater.CreateColumns(columnPage.preview.dgCsvData, Model.Importer.PreviewData);
                    columnPage.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                    columnPage.preview.dgCsvData.AutoGenerateColumns = false;
                    Model.Importer.RefreshPreviewData();

                    // why not canceled, if the file contains no rows?
                    if (Model.Importer.PreviewData.Rows.Count == 0) {
                        return false;
                    }
                    if (!page1.checkUseProfile.IsChecked.HasValue || !page1.checkUseProfile.IsChecked.Value ||
                        AccountsProfileManager.SelectedElement == null || _pageListNumber == 2) {
                        _pageListNumber++;
                        if (_pageListNumber == 2) {
                            Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                            pages.SwitchToFirstPage();
                            AddColumnText(pages.CurrentPage());
                            if (Model.Importer.Config.TaxonomyColumnExists) {
                                pages.Add(_taxonomyPage);
                            }
                            if (Model.Importer.Config.Comment) {
                                pages.Add(_commentPage);
                            }
                            if (Model.Importer.Config.Index) {
                                pages.Add(_indexPage);
                            }

                            Model.Importer.SummaryConfig.Pages = pages;
                        }

                        Model.CurrentPage++;
                    }

                    // only continue, if it's a saved profile
                    if (!page1.checkUseProfile.IsChecked.HasValue || !page1.checkUseProfile.IsChecked.Value ||
                        AccountsProfileManager.SelectedElement == null) {
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
                            MessageBox.Show(Owner,
                                            string.Format(
                                                ResourcesBalanceList.
                                                    BalListImportAssistant_NavigateNext_TooFewColumnsToUseImportProfile,
                                                columnInfo.Value, Model.Importer.PreviewData.Columns.Count),
                                            string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                        Model.Importer.Config.ColumnDict[columnInfo.Key] =
                            Model.Importer.PreviewData.Columns[columnInfo.Value].Name;
                        Model.Importer.PreviewData.Columns[columnInfo.Value].Color =
                            Model.Importer.Config.ColorDict[columnInfo.Key];
                    }

                    // same like pressing the next button
                    _pageListNumber++;
                    if (_pageListNumber == 2) {
                        Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                        pages.SwitchToFirstPage();
                        AddColumnText(pages.CurrentPage());
                        if (Model.Importer.Config.TaxonomyColumnExists) {
                            pages.Add(_taxonomyPage);
                        }
                        if (Model.Importer.Config.Comment) {
                            pages.Add(_commentPage);
                        }
                        if (Model.Importer.Config.Index) {
                            pages.Add(_indexPage);
                        }
                        assistantControl.NavigateNext();
                    }

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
                        if (page1.checkUseProfile.IsChecked.HasValue && page1.checkUseProfile.IsChecked.Value &&
                            AccountsProfileManager.SelectedElement != null) {
                            NavigateNext();
                        }
                        return false;
                    }
                } else {
                    if (_pageList[_pageListNumber].Validate()) {
                        DoHandleError();
                        return true;
                    }
                }
            } else if (_pageListNumber == 3) {
                if (_pageList[_pageListNumber].Validate()) {
                    DoHandleError();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles displaying the error page.
        /// </summary>
        private void DoHandleError() {
            _pageListNumber++;
            Model.CurrentPage++;

            // If the model didn't analyze the CSV, then we 
            // force to do it.
            if (!Model.ErrorsSetInThisRound) {
                Model.Importer.LastPageReached(true);
            }
            assistantControl.NavigateNext();

            // Skip the error page if the import will be flawless.
            if (!Model.HasErrors) {
                assistantControl.NavigateNext();
            }
        }

        /// <summary>
        /// Navigates to the previous page in the wizard.
        /// </summary>
        /// <returns>False if the navigation couldn't be done, else true.</returns>
        private bool NavigatePrevious() {
            switch (_pageListNumber) {
                // Cannot navigate before the first page.
                case 0:
                    return false;

                // Navigat to the first page.
                case 1: {
                    _pageListNumber--;
                    Model.CurrentPage--;
                    assistantControl.NavigateBack();
                    NavigatePrevious();
                    return true;
                }

                case 2: {
                    Pages pages = _pageDictionary[Model.Importer.Config.ImportType];
                    bool goBack = pages.HasPreviousPage();
                    if (!goBack) {
                        _pageListNumber--;
                        if (Model.Importer.Config.TaxonomyColumnExists) {
                            pages.Delete(_taxonomyPage);
                        }
                        if (Model.Importer.Config.Comment) {
                            pages.Delete(_commentPage);
                        }
                        if (Model.Importer.Config.Index) {
                            pages.Delete(_indexPage);
                        }
                    } else {
                        AddColumnText(pages.PreviousPage());
                    }

                    RemoveColor();


                    Model.CurrentPage--;
                    // if it's a saved profile press back button, until first page.
                    if (page1.checkUseProfile.IsChecked.HasValue && page1.checkUseProfile.IsChecked.Value &&
                        AccountsProfileManager.SelectedElement != null) {
                        NavigatePrevious();
                    }

                    if (!goBack) {
                        assistantControl.NavigateBack();
                    }

                    return !goBack;
                }

                case 3:
                case 4: {
                    _pageListNumber--;
                    RemoveColor();
                    Model.CurrentPage--;

                    if (!Model.HasErrors) {
                        Model.HasErrors = false;
                        assistantControl.NavigateBack();
                    }

                    if (page1.checkUseProfile.IsChecked.HasValue
                        && page1.checkUseProfile.IsChecked.Value
                        && AccountsProfileManager.SelectedElement != null) {
                        NavigatePrevious();
                    }

                    assistantControl.NavigateBack();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the color.
        /// </summary>
        private void RemoveColor() {
            Model.Importer.Config.ColumnDict.Remove(
                _pageDictionary[Model.Importer.Config.ImportType].CurrentPage().ColumnBinding);

            foreach (IDataColumn column in Model.Importer.PreviewData.Columns) {
                if (Model.Importer.Config.ColumnDict.ContainsValue(column.Name)) {
                    column.Color =
                        Model.Importer.Config.ColorDict[
                            Model.Importer.Config.ColumnDict.First(v => v.Value.Equals(column.Name)).Key];
                    //if (Model.Importer.Config.ColumnDict.ContainsKey(pages.CurrentPage().ColumnBinding)) {
                    //}
                    //break;
                } else {
                    column.Color = Color.White;
                }
            }
            columnPage.preview.SelectedColumn = null;
            Model.Importer.Config.OnPropertyChanged("ColumnDict");
            Model.Importer.RefreshPreviewData();

            DataGridCreater.CreateColumns(columnPage.preview.dgCsvData, Model.Importer.PreviewData);
            columnPage.preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
            columnPage.preview.dgCsvData.AutoGenerateColumns = false;
        }

        private void BtnSaveProfileOnClick(object sender, RoutedEventArgs e) {
            var profileNameModel = new ProfileNameModel();
            var dlgProfileName = new DlgProfileName {Owner = Owner, DataContext = profileNameModel};
            dlgProfileName.ShowDialog();
            if (!profileNameModel.PressedOk || string.IsNullOrEmpty(profileNameModel.ProfileName)) {
                return;
            }

            // this dictionary will be saved as information to the account profile.
            // stores all the clicked columns with the number of ordinal of the column,
            // and it's key string in Config.ColumnDict
            var actualDictionary = new Dictionary<string, int>();
            // BalanceColSigned had value of empty string. This was a problem here.
            IEnumerable<KeyValuePair<string, string>> elements = from keyVal in Model.Importer.Config.ColumnDict
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

            var newProfile = new AccountsInformationProfile {
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
            MessageBox.Show(Owner,
                            ResourcesBalanceList.
                                BalListImportAssistant_BtnSaveProfileOnClick_Successfully_saved_import_profile_, "",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            //
        }

        /// <summary>
        /// Handles the Click event of the btnViewErrors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnViewErrors_Click(object sender, RoutedEventArgs e) {
            var viewErrors = new BalanceListViewErrors {
                Owner = Owner
            };

            viewErrors.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnInvertValues control.
        /// When clicked, all values on the Model will be multiplied by -1.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnInvertValues_Click(object sender, RoutedEventArgs e) { Model.Multipilier *= -1; }
    }
}