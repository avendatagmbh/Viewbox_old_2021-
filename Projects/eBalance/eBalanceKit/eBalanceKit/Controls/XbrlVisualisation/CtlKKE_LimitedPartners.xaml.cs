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
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKit.Windows;
using eBalanceKit.Structures;
using eBalanceKit.Models;
using System.ComponentModel;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlKKE_LimitedPartners.xaml
    /// </summary>
    public partial class CtlKKE_LimitedPartners : UserControl {
        public CtlKKE_LimitedPartners() {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this_DataContextChanged);
        }

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the DataContextChanged event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (this.Model != null) {
                if (this.Model.ValueTreeRoot != null) {
                    Init();
                } else {
                    this.Model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                }
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ValueTreeRoot") {
                if (this.Model.ValueTreeRoot != null) {
                    Init();
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            XbrlElementValue_List value = (XbrlElementValue_List)((Button)sender).DataContext;

            if (value != null) {
                if (!value.AddItemAllowed) {
                    // item not added (unsufficient rights)
                    var owner = UIHelpers.TryFindParent<Window>(this);
                    MessageBox.Show(owner, "Sie haben keine Berechtigung zum Hinzufügen neuer Listeneinträge.");
                } else {
                    bool requestInit = (value.Items.Count == 0);
                    ValueTreeNode node = value.AddValue(); ;
                    value.SelectedItem = node;
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            XbrlElementValue_List value = (XbrlElementValue_List)((Button)sender).DataContext;
            if (!value.DeleteItemAllowed) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                MessageBox.Show(owner, "Sie haben keine Berechtigung zum Löschen von Listeneinträgen.");
            } else if (value != null) {
                value.DeleteSelectedValue();
            }
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        private DisplayValueTreeModel Model {
            get {
                return this.DataContext as DisplayValueTreeModel;
            }
        }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Inits this instance.
        /// </summary>
        private void Init() {

            if (this.Model.IsInitalized) return;

            if (this.Model.ValueTreeRoot != null) {
                TaxonomyUIElements uiElements = this.Model.UIElements;
                ValueTreeNode root = this.Model.ValueTreeRoot;

                uiElements.AddInfo(stackPanel1, this.Model.Elements["de-gaap-ci_kke.limitedPartners.name"], bindingPath: "Values[de-gaap-ci_kke.limitedPartners.name]");
                uiElements.AddInfo(stackPanel1, this.Model.Elements["de-gaap-ci_kke.limitedPartners.name.group"], bindingPath: "Values[de-gaap-ci_kke.limitedPartners.name.group]");
                uiElements.AddInfo(stackPanel1, this.Model.Elements["de-gaap-ci_kke.limitedPartners.name.number"], bindingPath: "Values[de-gaap-ci_kke.limitedPartners.name.number]");
                uiElements.AddMonetaryComputed(stackPanel1, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts"], "", bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts]");

                this.Model.UIElements.AddInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.name"], bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.name]", maxWidth: 461, forceVerticalOrientation: true, width: -1);

                this.Model.UIElements.AddSeparator(stackPanel2);

                this.Model.UIElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.begin"], "+", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.begin]");

                /////////////////////////////////////////////
                // Felder auskommentiert da diese laut Taxonomie nicht zu den jeweiligen Tuples gehören, womit diese auch nicht innerhalb der Liste dargestellt werden können
                /////////////////////////////////////////////

                uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits"], "+", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits]");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits.incRealEst"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits.privateTax"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits.tanBookvalue"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.deposits.tanOther"], "");

                uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals"], "-", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals]");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.privateTax"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.specialExtordExpenses"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.costRealEst"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.nonCash"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.tanBookvalue"], "");
                //uiElements.AddMonetaryInfo(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.withdrawals.tanOther"], "");

                //uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.capAdjust6bRes"], "+");
                uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.icomeShare"], "+", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.icomeShare]");
                uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.Capitalmovements"], "+", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.Capitalmovements]");
                uiElements.AddMonetaryInput(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.CapitaladjustmentOther"], "+", leftMargin: 10, bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd.CapitaladjustmentOther]");

                uiElements.AddDoubleSeparator(stackPanel2);
                uiElements.AddMonetaryComputed(stackPanel2, this.Model.Elements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd"], "=", bindingPath: "Values[de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.sumYearEnd]", leftMargin: 10);

                this.Model.RegisterGotFocusEventHandler();
                this.Model.IsInitalized = true;
            }
        }

        #endregion methods
    }
}
