using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using eBalanceKit.Controls.XbrlVisualisation;
using eBalanceKit.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKit.Models;
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitControls;
using AvdWpfControls;

namespace eBalanceKit.Controls.Company {
    
    /// <summary>
    /// Interaktionslogik für CtlCompanyInfo.xaml
    /// </summary>
    public partial class CtlCompanyInfo : UserControl {

        #region constructor
        public CtlCompanyInfo(ValueTreeWrapper valueTreeWrapper, bool hideFinancialYears = false) {
            InitializeComponent();
            this.ValueTreeWrapper = valueTreeWrapper;
            this.DataContextChanged += CtlCompanyInfo_DataContextChanged;

            if (hideFinancialYears) tabItemFinancialYears.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        #region event handler
        void CtlCompanyInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                Init();
            }
        }

        #endregion event handler

        #region properties

        #region Model
        private CompanyDisplayValueTreeModel Model { get { return this.DataContext as CompanyDisplayValueTreeModel; } }
        #endregion

        #region CtlCompanyFinancialYearsModel
        internal CtlCompanyFinancialYearsModel CtlCompanyFinancialYearsModel {
            get { return _ctlCompanyFinancialYearsModel; }
            set { _ctlCompanyFinancialYearsModel = value; }
        }
        private CtlCompanyFinancialYearsModel _ctlCompanyFinancialYearsModel = new CtlCompanyFinancialYearsModel();
        #endregion

        private ValueTreeWrapper ValueTreeWrapper{get;set;}
        #endregion properties

        #region methods

        #region Init
        private void Init() {
            
            TaxonomyUIElements ui = Model.UIElements;
            try {
                FillPanelFunction[] panelFillFunc = new FillPanelFunction[] { FillPanelCommon, FillPanelIdNumbers, FillPanelShareholder, FillPanelIncorporation, FillPanelStockExch, FillPanelContact, FillPanelOther, FillPanelParentCompany, FillPanelFinancialYears };
                XbrlBasePanel[] panels = new XbrlBasePanel[] { panelCommon, panelIdNumbers, panelShareholder, panelIncorporation, panelStockExch, panelContact, panelOther, panelParentCompany, panelFinancialYears};
                List<CompanyDisplayValueTreeModel> models = new List<CompanyDisplayValueTreeModel>();
                for (int i = 0; i < panelFillFunc.Length; ++i) {
                    CompanyDisplayValueTreeModel newModel = new CompanyDisplayValueTreeModel(TaxonomyManager.GCD_Taxonomy, ValueTreeWrapper, null){Company = Model.Company};
                    panels[i].DataContext = newModel;
                    panelFillFunc[i](panels[i].Model.UIElements, newModel, panels[i]);
                    Model.AddChild(newModel);
                    newModel.RegisterGotFocusEventHandler();
                    models.Add(newModel);
                }
                Model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
                    if (args.PropertyName == "Company")
                        foreach (var model in models) model.Company = Model.Company;
                    CtlCompanyFinancialYearsModel.Company = Model.Company;
                };

                if (Model.Company != null) {
                    foreach (var model in models) model.Company = Model.Company;
                    CtlCompanyFinancialYearsModel.Company = Model.Company;
                }

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            Model.RegisterGotFocusEventHandler();
        }
        #endregion Init

        public delegate void FillPanelFunction(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, XbrlBasePanel panel);

        #region FillPanelCommon
        public static void FillPanelCommon(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, XbrlBasePanel panelToFill) {
            try {
                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);

                panel = AddGroupBorder(spLeft);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.name"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.name.formerName"], width: 0, height: 50, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.name.dateOfLastChange"], width: 0, forceVerticalOrientation: true);

                TextBox tb = model.UIElements.UIElements["de-gcd_genInfo.company.id.name"].UIElement as TextBox;
                //The XbrlElementValueBase.DoDbUpdate is needed to avoid a bug which changes the name of a company
                //once a different report is selected.
                tb.TextChanged += delegate(object sender, TextChangedEventArgs args) { if (model.Company != null && XbrlElementValueBase.DoDbUpdate) model.Company.Name = (sender as TextBox).Text; };
                tb.LostFocus += (sender, args) => CompanyManager.Instance.SaveCompany(model.Company);
                
                // For init
                if (model.Company != null) model.ValueTreeRoot.Values["de-gcd_genInfo.company.id.name"].Value = model.Company.Name;

                panel = AddGroupBorder(spLeft);
                
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.legalStatus"], width: 0, forceVerticalOrientation: true);
                
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.legalStatus.formerStatus"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.legalStatus.dateOfLastChange"], width: 0, forceVerticalOrientation: true);

                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.location"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.location.street"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.location.houseNo"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.location.zipCode"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.location.city"], width: 0, forceVerticalOrientation: true);
                //ui.AddCompanyInfo(panel, Model.Elements["de-gcd_genInfo.company.id.location.country"], width: 0, forceVerticalOrientation: true);
                //ui.AddCompanyInfo(panel, Model.Elements["de-gcd_genInfo.company.id.location.country.isoCode"], width: 0, forceVerticalOrientation: true);

                TextBlock tbLocationCounry = new TextBlock { Margin = new Thickness(0,5,0,0)};
                tbLocationCounry.SetBinding(TextBlock.ToolTipProperty, new Binding("Elements[de-gcd_genInfo.company.id.location.country].Documentation"));
                tbLocationCounry.SetBinding(TextBlock.TextProperty, new Binding("Elements[de-gcd_genInfo.company.id.location.country].MandatoryLabel"));
                panel.Children.Add(tbLocationCounry);

                //ComboBox cbLocationCountry = new ComboBox { ItemsSource = Country.Countries, DataContext = this };
                ComboBox cbLocationCountry = new ComboBox { ItemsSource = Country.Countries, DataContext = model };
                cbLocationCountry.SetBinding(IsEnabledProperty, new Binding("IsAllowedImport"));
                cbLocationCountry.SetBinding(Selector.SelectedItemProperty, new Binding("SelectedLocationCountry") { Mode = BindingMode.TwoWay });
                panel.Children.Add(cbLocationCountry);

                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.street"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.houseNo"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.zipCode"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.city"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.country"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements["de-gcd_genInfo.company.id.adress.country.isoCode"], width: 0, forceVerticalOrientation: true);

            
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelCommon

        #region FillPanelIdNumbers
        public static void FillPanelIdNumbers(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, XbrlBasePanel panelToFill) {
            try {
                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);

                string root = "de-gcd_genInfo.company.id.idNo.type.companyId";
                string bExpr = "ValueTreeRoot.Values[de-gcd_genInfo.company.id.idNo].Items[0].Values[de-gcd_genInfo.company.id.idNo.type.companyId.{0}]";
               
                panel = AddGroupBorder(spLeft);
                AddIdNumbersLeftPanel(ui, model, bExpr, root, panel);


                panel = AddGroupBorder(spRight);
                AddIdNumbersRightPanel(ui, model, bExpr, root, panel);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private static void AddIdNumbersRightPanel(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, string bExpr,
                                                   string root, StackPanel panel) {
            ui.AddCompanyInfo(panel, model.Elements[root + ".BKN"], bindingPath: string.Format(bExpr, "BKN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".BUN"], bindingPath: string.Format(bExpr, "BUN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".IN"], bindingPath: string.Format(bExpr, "IN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".EN"], bindingPath: string.Format(bExpr, "EN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".SN"], bindingPath: string.Format(bExpr, "SN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".S"], bindingPath: string.Format(bExpr, "S"), width: 0,
                              forceVerticalOrientation: true);

            // ToDo implement if taxonomy bugfix is available
            //ui.AddCompanyInfo(panel, model.Elements[root + ".SERL"], bindingPath: string.Format(bExpr, "SERL"), width: 0,
            //                  forceVerticalOrientation: true);
        }

        private static void AddIdNumbersLeftPanel(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, string bExpr,
                                                  string root, StackPanel panel) {
            ui.AddCompanyInfo(panel, model.Elements[root + ".BF4"], bindingPath: string.Format(bExpr, "BF4"), width: 0,
                              forceVerticalOrientation: true);

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)});

            panel.Children.Add(grid);
            ui.AddCompanyInfo(grid, model.Elements[root + ".ST13"], bindingPath: string.Format(bExpr, "ST13"), width: 0,
                              forceVerticalOrientation: true);

            ImageButton btnTaxIDInfo = new ImageButton();

            btnTaxIDInfo.ImageSource =
                new System.Windows.Media.Imaging.BitmapImage(new Uri("/eBalanceKitResources;component/Resources/Info.png",
                                                                     UriKind.Relative));

            btnTaxIDInfo.Margin = new Thickness(2, 32, 2, 3);
            btnTaxIDInfo.Click += btnTaxIDInfo_Click;
            btnTaxIDInfo.VerticalAlignment = VerticalAlignment.Top;


            grid.Children.Add(btnTaxIDInfo);
            Grid.SetColumn(btnTaxIDInfo, 2);


            ui.AddCompanyInfo(panel, model.Elements[root + ".STID"], bindingPath: string.Format(bExpr, "STID"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".HRN"], bindingPath: string.Format(bExpr, "HRN"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".UID"], bindingPath: string.Format(bExpr, "UID"), width: 0,
                              forceVerticalOrientation: true);
            ui.AddCompanyInfo(panel, model.Elements[root + ".STWID"], bindingPath: string.Format(bExpr, "STWID"), width: 0,
                              forceVerticalOrientation: true);
        }

        static void btnTaxIDInfo_Click(object sender, RoutedEventArgs e) {
            TaxIdInfo taxIdInfo = new TaxIdInfo();
            System.Windows.Controls.Primitives.Popup popupTaxInfo = new System.Windows.Controls.Primitives.Popup();
            popupTaxInfo.StaysOpen = false;
            popupTaxInfo.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            popupTaxInfo.PlacementTarget = (Button)sender;
            popupTaxInfo.Child = taxIdInfo;
            popupTaxInfo.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Slide;
            popupTaxInfo.AllowsTransparency = true;
            popupTaxInfo.IsOpen = true;



        }
        #endregion FillPanelIdNumbers

        #region FillPanelIncorporation
        public static void FillPanelIncorporation(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);

                panel = AddGroupBorder(spLeft);
                string root = "de-gcd_genInfo.company.id.Incorporation";

                ui.AddCompanyInfo(panel, Model.Elements[root + ".Type"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".prefix"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".section"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".number"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".suffix"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".court"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".dateOfFirstRegistration"], width: 0, forceVerticalOrientation: true);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelIncorporation

        #region FillPanelStockExch
        public static void FillPanelStockExch(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);

                panel = AddGroupBorder(spLeft);
                string root = "de-gcd_genInfo.company.id.stockExch";
                string bExpr = "ValueTreeRoot.Values[de-gcd_genInfo.company.id.stockExch].Items[0].Values[de-gcd_genInfo.company.id.stockExch.{0}]";

                ui.AddCompanyInfo(panel, Model.Elements[root + ".city"], bindingPath: string.Format(bExpr, "city"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".ticker"], bindingPath: string.Format(bExpr, "ticker"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".market"], bindingPath: string.Format(bExpr, "market"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".typeOfSecurity"], bindingPath: string.Format(bExpr, "typeOfSecurity"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".securityCode"], bindingPath: string.Format(bExpr, "securityCode"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".securityCode.entry"], bindingPath: string.Format(bExpr, "securityCode.entry"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".securityCode.type"], bindingPath: string.Format(bExpr, "securityCode.type"), width: 0, forceVerticalOrientation: true);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelStockExch

        #region FillPanelShareholder
        public static void FillPanelShareholder(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                string root = "de-gcd_genInfo.company.id.shareholder";

                XbrlListView XbrlListView = new XbrlListView { Name = "shareholderList" };
                XbrlListView.DisplayMemberPath = "DisplayString";
                XbrlListView.SetBinding(XbrlListView.DataContextProperty, "ValueTreeRoot.Values[" + root + "]");
                //XbrlListView.DataContextChanged += new DependencyPropertyChangedEventHandler(XbrlListView_DataContextChanged);
                panelToFill.ContentPanel.Children.Add(XbrlListView);
                panelToFill.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                panelToFill.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                //shareholderTabItem.DataContext = XbrlListView;
                //shareholderTabItem.SetBinding(
                //    EbkWarningTabItem.WarningMessageVisibilityProperty,
                //    new Binding("DataContext.ChildHasValidationWarning") {
                //        Converter = new Converters.BoolToVisibilityConverter { VisibleValue = true, HiddenState = System.Windows.Visibility.Collapsed }
                //    });

                //shareholderTabItem.SetBinding(
                //    EbkWarningTabItem.ErrorMessageVisibilityProperty,
                //    new Binding("DataContext.ChildHasValidationError") {
                //        Converter = new Converters.BoolToVisibilityConverter { VisibleValue = true, HiddenState = System.Windows.Visibility.Collapsed }
                //    });

                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, XbrlListView.dataPanel);

                string bExpr = "Values[de-gcd_genInfo.company.id.shareholder.ShareDivideKey].Items[0].Values[de-gcd_genInfo.company.id.shareholder.ShareDivideKey.{0}]";

                panel = AddGroupBorder(spLeft);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".name"], bindingPath: "Values[" + root + ".name]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".currentnumber"], bindingPath: "Values[" + root + ".currentnumber]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".signerId"], bindingPath: "Values[" + root + ".signerId]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".taxnumber"], bindingPath: "Values[" + root + ".taxnumber]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".taxid"], bindingPath: "Values[" + root + ".taxid]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".WID"], bindingPath: "Values[" + root + ".WID]", width: 0, forceVerticalOrientation: true);

                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".ProfitDivideKey"], bindingPath: "Values[" + root + ".ProfitDivideKey]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".ProfitDivideKey.dateOfunderyearChange"], bindingPath: "Values[" + root + ".ProfitDivideKey.dateOfunderyearChange]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".ProfitDivideKey.formerkey"], bindingPath: "Values[" + root + ".ProfitDivideKey.formerkey]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".legalStatus"], bindingPath: "Values[" + root + ".legalStatus]", width: 0, forceVerticalOrientation: true);

                TextBlock tbShareDivideKey = new TextBlock { Margin = new Thickness(0, 5, 0, 0) };
                tbShareDivideKey.SetBinding(TextBlock.ToolTipProperty, new Binding("Elements[de-gcd_genInfo.company.id.shareholder.ShareDivideKey].Documentation"));
                tbShareDivideKey.SetBinding(TextBlock.TextProperty, new Binding("Elements[de-gcd_genInfo.company.id.shareholder.ShareDivideKey].MandatoryLabel"));
                panel.Children.Add(tbShareDivideKey);

                StackPanel subPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
                panel.Children.Add(subPanel);
                ui.AddCompanyInfo(subPanel, Model.Elements[root + ".ShareDivideKey.numerator"], bindingPath: string.Format(bExpr, "numerator"), width: 30, hideLabel: true, setTopMargin: false);
                subPanel.Children.Add(new TextBlock { Text = " / ", VerticalAlignment = System.Windows.VerticalAlignment.Center });
                ui.AddCompanyInfo(subPanel, Model.Elements[root + ".ShareDivideKey.denominator"], bindingPath: string.Format(bExpr, "denominator"), width: 30, hideLabel: true, setTopMargin: false);

                ui.AddCompanyInfo(panel, Model.Elements[root + ".SpecialBalanceRequired"], bindingPath: "Values[" + root + ".SpecialBalanceRequired]", width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".extensionRequired"], bindingPath: "Values[" + root + ".extensionRequired]", width: 0, forceVerticalOrientation: true);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        void XbrlListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ((CompanyDisplayValueTreeModel)panelShareholder.DataContext).UIElements.ContainsValidationElementChanged();
            foreach (var temp in ((CompanyDisplayValueTreeModel)panelShareholder.DataContext).UIElements.UIElements.Values) {
                temp.Warning.InvalidateVisual();
                temp.Error.InvalidateVisual();
                temp.UIElement.InvalidateVisual();
            }
            //((XbrlListView) sender).Parent.
        }
        #endregion

        #region FillPanelContact
        public static void FillPanelContact(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                string root = "de-gcd_genInfo.company.id.contactAddress";

                XbrlListView XbrlListView = new XbrlListView();
                XbrlListView.DisplayMemberPath = "DisplayString";
                XbrlListView.SetBinding(DataContextProperty, "ValueTreeRoot.Values[" + root + "]");
                panelToFill.ContentPanel.Children.Add(XbrlListView);
                panelToFill.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                panelToFill.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, XbrlListView.dataPanel);
                
                panel = AddGroupBorder(spLeft);                
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person]");                
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.name"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.name]");
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.dept"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.dept]");
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.function"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.function]");

                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.phone"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.phone]");
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.fax"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.fax]");
                ui.AddCompanyInfo(panel, Model.Elements[root + ".person.eMail"], width: 0, forceVerticalOrientation: true, bindingPath: "Values[" + root + ".person.eMail]");

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelContact

        #region FillPanelOther
        public static void FillPanelOther(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                
                string root = "de-gcd_genInfo.company.id";

                StackPanel spLeft, spRight, panel;
                AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);

                panel = AddGroupBorder(spLeft);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".lastTaxAudit"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".sizeClass"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".business"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".CompanyStatus"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".FoundationDate"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".taxGroupKstEst"], width: 0, forceVerticalOrientation: true);

                string bExpr1 = "ValueTreeRoot.Values[de-gcd_genInfo.company.id.bankAcct].Items[0].Values[de-gcd_genInfo.company.id.bankAcct.{0}]";

                panel = AddGroupBorder(spLeft);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.acctHolderName"], bindingPath: string.Format(bExpr1, "acctHolderName"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.bankName"], bindingPath: string.Format(bExpr1, "bankName"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.bankCode"], bindingPath: string.Format(bExpr1, "bankCode"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.acctNo"], bindingPath: string.Format(bExpr1, "acctNo"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.IBAN"], bindingPath: string.Format(bExpr1, "IBAN"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".bankAcct.BIC"], bindingPath: string.Format(bExpr1, "BIC"), width: 0, forceVerticalOrientation: true);
           


                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".internet"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".internet.description"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".internet.url"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".comingfrom"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".companyLogo"], width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements["de-gcd_genInfo.company.userSpecific"], width: 0, forceVerticalOrientation: true);
                

                panel = AddGroupBorder(spRight);
                string bExpr = "ValueTreeRoot.Values[" + root + ".industry].Items[0].Values[" + root + ".industry.{0}]";
                ui.AddCompanyInfo(panel, Model.Elements[root + ".industry.keyType"], bindingPath: string.Format(bExpr, "keyType"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".industry.keyEntry"], bindingPath: string.Format(bExpr, "keyEntry"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, Model.Elements[root + ".industry.name"], bindingPath: string.Format(bExpr, "name"), width: 0, forceVerticalOrientation: true);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelOther

        #region FillPanelFinancialYears
        private void FillPanelFinancialYears(TaxonomyUIElements ui, CompanyDisplayValueTreeModel Model, XbrlBasePanel panelToFill) {
            try {
                panelToFill.ContentPanel.Children.Add(new CtlCompanyFinancialYears { DataContext = this.CtlCompanyFinancialYearsModel });
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion FillPanelFinancialYears

        #region FillPanellParentCompany
        public static void FillPanelParentCompany(TaxonomyUIElements ui, CompanyDisplayValueTreeModel model, XbrlBasePanel panelToFill) {
            try {
                string root = "de-gcd_genInfo.company.id.parent";
                StackPanel spLeft, spRight, panel;
                Grid baseGrid = AddDoubleStackPanel(out spLeft, out spRight, panelToFill.ContentPanel);
                baseGrid.RowDefinitions.Add(new RowDefinition() { Height=GridLength.Auto });
                baseGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                Grid.SetRow(spLeft, 1);
                Grid.SetRow(spRight, 1);

                var parentCompanyGrid = CreateParentCompanyGrid(model);
                baseGrid.Children.Add(parentCompanyGrid);

                panel = AddGroupBorder(spLeft);
                string basePath = "ValueTreeRoot.Values[" + root + "].Items[0].Values[" + root + ".{0}]";
                ui.AddCompanyInfo(panel, model.Elements[root + ".name"], bindingPath: string.Format(basePath, "name"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".name.formerName"], bindingPath: string.Format(basePath, "name.formerName"), width: 0, height: 50, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".name.dateOfLastChange"], bindingPath: string.Format(basePath, "name.dateOfLastChange"), width: 0, height: 50, forceVerticalOrientation: true);
                
                panel = AddGroupBorder(spLeft);

                ui.AddCompanyInfo(panel, model.Elements[root + ".legalStatus"], bindingPath: string.Format(basePath, "legalStatus"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".legalStatus.formerStatus"], bindingPath: string.Format(basePath, "legalStatus.formerStatus"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".legalStatus.dateOfLastChange"], bindingPath: string.Format(basePath, "legalStatus.dateOfLastChange"), width: 0, forceVerticalOrientation: true);

                panel = AddGroupBorder(spRight);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location"], bindingPath: string.Format(basePath, "location"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.street"], bindingPath: string.Format(basePath, "location.street"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.houseNo"], bindingPath: string.Format(basePath, "location.houseNo"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.zipCode"], bindingPath: string.Format(basePath, "location.zipCode"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.city"], bindingPath: string.Format(basePath, "location.city"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.country"], bindingPath: string.Format(basePath, "location.country"), width: 0, forceVerticalOrientation: true);
                ui.AddCompanyInfo(panel, model.Elements[root + ".location.country.isoCode"], bindingPath: string.Format(basePath, "location.country.isoCode"), width: 0, forceVerticalOrientation: true);


                root = "de-gcd_genInfo.company.id.idNo.type.companyId";
                //string bExpr = "ValueTreeRoot.Values[de-gcd_genInfo.company.id.parent.idNo].Items[0].Values[de-gcd_genInfo.company.id.idNo.type.companyId.{0}]";
                string bExpr = "ValueTreeRoot.Values[de-gcd_genInfo.company.id.parent].Items[0].Values[de-gcd_genInfo.company.id.parent.idNo].Items[0].Values[de-gcd_genInfo.company.id.idNo.type.companyId.{0}]";

                panel = AddGroupBorder(spLeft);
                AddIdNumbersLeftPanel(ui, model, bExpr, root, panel);


                panel = AddGroupBorder(spRight);
                AddIdNumbersRightPanel(ui, model, bExpr, root, panel);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private static Grid CreateParentCompanyGrid(CompanyDisplayValueTreeModel model) {
            Grid parentCompanyGrid = new Grid() { Margin = new Thickness(5, 5, 0, 0) };
            parentCompanyGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            parentCompanyGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(0, GridUnitType.Star)});
            parentCompanyGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});

            parentCompanyGrid.Children.Add(new TextBlock() {
                                                               Text = "Daten aus folgendem Unternehmen übernehmen",
                                                           });
            ComboBox parentCompanyCombobox = new ComboBox() { ItemsSource = CompanyManager.Instance.AllowedCompanies, Margin = new Thickness(5, 0, 0, 0), MinWidth = 200 };
            parentCompanyCombobox.SetBinding(Selector.SelectedItemProperty, "SelectedParentCompany");
            parentCompanyCombobox.SetBinding(UIElement.IsEnabledProperty, new Binding("IsAllowedImport") {UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});
            Grid.SetColumn(parentCompanyCombobox, 1);
            parentCompanyGrid.Children.Add(parentCompanyCombobox);

            Button applyValuesButton = new Button() { Content = "Daten übernehmen", Margin = new Thickness(5, 0, 0, 0) };
            applyValuesButton.SetBinding(UIElement.IsEnabledProperty, new Binding("IsAllowedImport") {UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});
            applyValuesButton.Click += delegate { model.ApplyParentCompanyValues(); };
            Grid.SetColumn(applyValuesButton, 2);
            parentCompanyGrid.Children.Add(applyValuesButton);
            return parentCompanyGrid;
        }


        #endregion FillPanellParentCompany


        #region helper methods

        private static Grid AddDoubleStackPanel(out StackPanel spLeft, out StackPanel spRight, Panel basePanel) {
            spLeft = new StackPanel();
            spRight = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star), MaxWidth = 600 });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star), MaxWidth = 600 });
            grid.Children.Add(spLeft);
            grid.Children.Add(spRight);
            Grid.SetColumn(spRight, 1);
            basePanel.Children.Add(grid);
            return grid;
        }

        private static StackPanel AddGroupBorder(Panel panel) {
            //Border b = new Border { Style = this.FindResource("GroupBoxBorderDetailViews") as Style, Margin = new Thickness(0, 5, 0, 0) };
            Border b = new Border { Style = panel.FindResource("GroupBoxBorderDetailViews") as Style, Margin = new Thickness(0, 5, 0, 0) };
            StackPanel sp = new StackPanel();
            b.Child = sp;
            panel.Children.Add(b);
            return sp;
        }

        #endregion helper methods

        #endregion methods

    }
}
