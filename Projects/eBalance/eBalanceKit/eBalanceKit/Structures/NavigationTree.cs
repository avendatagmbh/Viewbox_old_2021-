// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AvdWpfControls;
using Taxonomy;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKit.Controls;
using eBalanceKit.Controls.Company;
using eBalanceKit.Controls.Document;
using eBalanceKit.Controls.XbrlVisualisation;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;
using eBalanceKit.Models;
using eBalanceKit.Windows.Reconciliation;
using eBalanceKitBusiness.EventArgs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using IPresentationTree = Taxonomy.Interfaces.PresentationTree.IPresentationTree;
using PresentationTreeNode = eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode;

namespace eBalanceKit.Structures {
    /// <summary>
    /// This class represents the navigation tree of the main window.
    /// </summary>
    internal class NavigationTree : NotifyPropertyChangedBase, INavigationTree, IEnumerable<INavigationTreeEntryBase> {

        #region [ Members ]

        private const string nodeIdPrefix = "nodeId_";

        #endregion [ Members ]

        #region constructor
        public NavigationTree(Window owner, ObjectWrapper<Document> documentWrapper) {
            Owner = owner;
            DocumentWrapper = documentWrapper;
            DocumentWrapper.PropertyChanged += DocumentWrapperPropertyChanged;
            DocumentWrapper.PropertyChanging += DocumentWrapperPropertyChanging;
            if (DocumentWrapper.Value != null) DocumentWrapper.Value.AssignedTaxonomyInfoChanged += DocumentAssignedTaxonomyInfoChanged;

            ValueTreeWrapperGAAP = new ValueTreeWrapper();
            ValueTreeWrapperGCD = new ValueTreeWrapper();
        }
        #endregion constructor

        #region event handler
        private void DocumentAssignedTaxonomyInfoChanged(object sender, AssignedTaxonomyInfoChangedEventArgs e) {
            if (!Owner.Dispatcher.CheckAccess()) {
                Owner.Dispatcher.BeginInvoke(
                    new Action(() => DocumentAssignedTaxonomyInfoChanged(sender, e)), DispatcherPriority.Render);
                return;
            }
            // remove current presentation trees (except of the gcd tree)
            while (NavigationTreeReport.Children.Count > 1) NavigationTreeReport.Children.RemoveAt(1);

            // add presentation trees for new taxonomy
            InitNavigation_Report();
        }

        private void DocumentWrapperPropertyChanging(object sender, PropertyChangingEventArgs e) {
            if (DocumentWrapper.Value != null)
                DocumentWrapper.Value.AssignedTaxonomyInfoChanged -= DocumentAssignedTaxonomyInfoChanged;
        }

        private void DocumentWrapperPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Value":

                    NavigationTreeReport.IsVisible = (DocumentWrapper.Value != null);

                    // remove current presentation trees (except of the gcd tree)
                    while (NavigationTreeReport.Children.Count > 1) NavigationTreeReport.Children.RemoveAt(1);

                    if (DocumentWrapper.Value != null) {
                        ((FrameworkElement) (NavigationTreeReport as NavigationTreeEntry).Content).DataContext = DocumentWrapper.Value;

                        DocumentWrapper.Value.AssignedTaxonomyInfoChanged += DocumentAssignedTaxonomyInfoChanged;
                        ValueTreeWrapperGAAP.ValueTreeRoot = DocumentWrapper.Value.ValueTreeMain.Root;
                        ValueTreeWrapperGCD.ValueTreeRoot = DocumentWrapper.Value.ValueTreeGcd.Root;
                        InitNavigation_Report();

                        //DocumentWrapper.Value.PropertyChanged -= CheckReconciliationVisibility;
                        //DocumentWrapper.Value.PropertyChanged += CheckReconciliationVisibility;
                    }
                    break;
            }
        }

        //private void CheckReconciliationVisibility(object sender, PropertyChangedEventArgs args) {
        //    if (args.PropertyName != "IsCommercialBalanceSheet") return;
        //    var node = FindEntry("de-gaap-ci_hbst.transfer");
        //    if (node != null) {
        //        node.IsVisible = (DocumentWrapper.Value.IsCommercialBalanceSheet &&
        //                          DocumentWrapper.Value.ReportRights.ReadTransferValuesAllowed);
        //    }
        //}

        #endregion event handler

        private NavigationTreeEntryBase FindEntry(string elementId, NavigationTreeEntryBase root = null) {
            if (root == null)
                return _children.Select(child => FindEntry(elementId, child as NavigationTreeEntryBase)).FirstOrDefault(result => result != null);

            if (root.XbrlElem != null && root.XbrlElem.Id == elementId)
                return root;

            return root.Children.Select(child => FindEntry(elementId, child as NavigationTreeEntryBase)).FirstOrDefault(result => result != null);
        }

        #region properties
        //--------------------------------------------------------------------------------
        // changed from NavigationTreeEntry to INavigationTreeEntryBase
        public INavigationTreeEntryBase NavigationTreeReport { get; set; }
        internal NavigationTreeEntry ReportCompanyEntry { get; private set; }
        private Window Owner { get; set; }
        private ValueTreeWrapper ValueTreeWrapperGCD { get; set; }
        private ValueTreeWrapper ValueTreeWrapperGAAP { get; set; }

        private ObjectWrapper<Document> DocumentWrapper { get; set; }

        #region Children
        private readonly ObservableCollection<INavigationTreeEntryBase> _children =
            new ObservableCollection<INavigationTreeEntryBase>();

        public IEnumerable<INavigationTreeEntryBase> Children { get { return _children; } }
        #endregion Children;

        //--------------------------------------------------------------------------------
        #endregion properties

        #region InitNavigation
        internal void InitNavigation() {

            // create report root node
            var reportProperties = new CtlDocumentProperties(){DataContext = null};

            NavigationTreeReport = new NavigationTreeEntry {
                Header = ResourcesCommon.Report,
                Content = reportProperties,
                IsVisible = false,
                NodeId = nodeIdPrefix + "report"
            };

            _children.Add(NavigationTreeReport);

            // init GCD part
            InitNavigation_CommonData(NavigationTreeReport as NavigationTreeEntry);
            _children[0].IsSelected = true;
        }
        #endregion

        #region InitNavigation_Report
        internal void InitNavigation_Report() {
            NavigationTreeEntry root = NavigationTreeReport as NavigationTreeEntry;

            ITaxonomy taxonomy = DocumentWrapper.Value.MainTaxonomy;
            foreach (IPresentationTree ptree in taxonomy.PresentationTrees.Where(ptree => ptree.Role.Style.IsVisible)) {
                switch (ptree.Role.Id) {
                    case "role_changesEquityAccounts":
                        InitNavigationKKE(ptree.Role, root);
                        break;

                    case "role_detailedInformation":
                        // no direct display due to special handling
                        continue;

                    case "role_transfersCommercialCodeToTax":
                        InitNavigation_TransferCommercialCodeToTax(ptree.Role, root);
                        break;

                    default:
                        
                        AddNavigation_TreeStyle(root, ptree);
                        break;
                }
            }
            CheckVisibility();
        }
        #endregion

        private void AddNavigation_TreeStyle(NavigationTreeEntry root, IPresentationTree ptree) {
            var model = new TaxonomyViewModel(Owner, DocumentWrapper) {
                RoleURI = ptree.Role.RoleUri,
                Elements = DocumentWrapper.Value.MainTaxonomy.Elements
            };

            AddNavigationTreeEntry(
                ptree.Role.Name, 
                new CtlTaxonomyTreeView(),
                presentationTreeRoots: ptree.RootEntries,
                dataContext: model,
                parent: root,
                showBalanceList: ptree.Role.Style.ShowBalanceList,
                roleId:ptree.Role.Id);            
        }

        private void InitNavigation_CommonData(NavigationTreeEntry parent) {
            IPresentationTree ptree =
            TaxonomyManager.GCD_Taxonomy.GetPresentationTree("http://www.xbrl.de/taxonomies/de-gcd/role/gcd");
            XbrlBasePanel basePanel;
            XbrlTextPanel textPanel;
            XbrlStackPanel stackPanel;
            XbrlTwoColStackPanel twoColStackPanel;
            XbrlListView XbrlListView;
            NavigationTreeEntry entry;
            NavigationTreeEntry subEntry;
            NavigationTreeEntry subSubEntry;
            StackPanel sp;

            entry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo"),
                                              new XbrlBasePanel { Header = GetLabel("de-gcd_genInfo") }, parent, xbrlElem: GetElement("de-gcd_genInfo"));

            #region document information
            textPanel = new XbrlTextPanel {Header = GetLabel("de-gcd_genInfo.doc")};
            subEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.doc"), textPanel, parent: entry, xbrlElem:GetElement("de-gcd_genInfo.doc"));
            textPanel.Init("de-gcd_genInfo.doc.userSpecific", showTextHeader: true);

            #region genInfo.doc.id
            stackPanel = new XbrlStackPanel {Header = GetLabel("de-gcd_genInfo.doc.id")};
            subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.doc.id"), stackPanel, parent: subEntry, xbrlElem:GetElement("de-gcd_genInfo.doc.id"));
            foreach (IPresentationTreeEntry node in ptree.GetNode("de-gcd_genInfo.doc.id").Children) {
                stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, (node).Element, width: 0);
            }
            stackPanel.Model.RegisterGotFocusEventHandler();
            #endregion genInfo.doc.id

            #region genInfo.doc.author
            basePanel = new XbrlBasePanel {Header = GetLabel("de-gcd_genInfo.doc.author")};
            subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.doc.author"), basePanel, parent: subEntry, xbrlElem:GetElement("de-gcd_genInfo.doc.author"));

            XbrlListView = new XbrlListView();
            XbrlListView.DisplayMemberPath = "DisplayString";
            XbrlListView.SetBinding(FrameworkElement.DataContextProperty, "ValueTreeRoot.Values[de-gcd_genInfo.doc.author]");
            basePanel.ContentPanel.Children.Add(XbrlListView);
            basePanel.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            basePanel.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            sp = new StackPanel();
            XbrlListView.dataPanel.Children.Add(sp);
            foreach (IPresentationTreeEntry node in ptree.GetNode("de-gcd_genInfo.doc.author").Children) {
                var pnode = (IPresentationTreeNode) node;
                basePanel.Model.UIElements.AddInfo(sp, basePanel.Model.Elements[pnode.Element.Id],
                                                   bindingPath: "Values[" + pnode.Element.Id + "]", width: 0);
            }
            basePanel.Model.RegisterGotFocusEventHandler();
            #endregion genInfo.doc.author

            #region #endregion genInfo.doc.rev
            stackPanel = new XbrlStackPanel {Header = GetLabel("de-gcd_genInfo.doc.rev")};
            subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.doc.rev"), stackPanel, parent: subEntry, xbrlElem:GetElement("de-gcd_genInfo.doc.rev"));
            foreach (IPresentationTreeEntry node in ptree.GetNode("de-gcd_genInfo.doc.rev").Children) {
                var pnode = (IPresentationTreeNode) node;
                if (pnode.Element.Id == "de-gcd_genInfo.doc.rev.date") {
                    stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, pnode.Element, width: 0, height: 100);
                } else {
                    stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, pnode.Element, width: 0);
                }
            }
            stackPanel.Model.RegisterGotFocusEventHandler();
            #endregion genInfo.doc.rev

            #endregion document information

            #region report information
            textPanel = new XbrlTextPanel {Header = GetLabel("de-gcd_genInfo.report")};
            subEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.report"), textPanel, parent: entry, xbrlElem:GetElement("de-gcd_genInfo.report"));
            textPanel.Model.UIElements.AddInfo(textPanel.ContentPanel,
                                               textPanel.Model.Elements["de-gcd_genInfo.report.autoNumbering"], width: 0);
            textPanel.Init("de-gcd_genInfo.report.userSpecific", showTextHeader: true);

            #region genInfo.report.id
            twoColStackPanel = new XbrlTwoColStackPanel {Header = GetLabel("de-gcd_genInfo.report.id")};
            twoColStackPanel.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            NavigationTreeEntry midEntry = AddNavigationTreeEntryGCD(
                GetLabel("de-gcd_genInfo.report.id"),
                twoColStackPanel,
                parent: subEntry,
                xbrlElem: TaxonomyManager.GCD_Taxonomy.Elements["de-gcd_genInfo.report.id"]);

            foreach (IPresentationTreeEntry node in ptree.GetNode("de-gcd_genInfo.report.id").Children) {
                if (node.Element.Id == "de-gcd_genInfo.report.id.specialAccountingStandard") continue; // special element threatment in document settings

                var pnode = (IPresentationTreeNode) node;
                if (pnode.Element.Id == "de-gcd_genInfo.report.id.reportElement") {
                    // create multiple choice value list
                    var values = new List<IElement>();
                    if (pnode.Element.Children.Count > 0) {
                        string substitutionGroup = pnode.Element.Children[0].Name;
                        foreach (IElement elem in TaxonomyManager.GCD_Taxonomy.Elements.Values) {
                            if (elem.SubstitutionGroup.EndsWith(substitutionGroup)) {
                                values.Add(elem);
                            }
                        }
                    }

                    twoColStackPanel.Model.UIElements.AddInfo(twoColStackPanel.ContentPanelRight, pnode.Element,
                                                              width: 0, multipleChoiceValues: values);
                } else if (pnode.Element.Id == "de-gcd_genInfo.report.id.accordingTo") {
                    stackPanel = new XbrlStackPanel {Header = GetLabel("de-gcd_genInfo.report.id.accordingTo")};
                    string bindingRoot = "ValueTreeRoot.Values[de-gcd_genInfo.report.id.accordingTo].Items[0]";
                    subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.report.id.accordingTo"), stackPanel,
                                                            parent: subEntry, xbrlElem:GetElement("de-gcd_genInfo.report.id.accordingTo"));
                    stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel,
                                                        stackPanel.Model.Elements["de-gcd_genInfo.report.id.accordingTo.name"],
                                                        width: 0,
                                                        bindingPath:
                                                            bindingRoot + ".Values[de-gcd_genInfo.report.id.accordingTo.name]");
                    stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel,
                                                        stackPanel.Model.Elements[
                                                            "de-gcd_genInfo.report.id.accordingTo.yearEnd"], width: 0,
                                                        bindingPath:
                                                            bindingRoot +
                                                            ".Values[de-gcd_genInfo.report.id.accordingTo.yearEnd]");
                    foreach (
                        IPresentationTreeEntry node1 in ptree.GetNode("de-gcd_genInfo.report.id.accordingTo.idNo").Children) {
                        var pnode1 = (IPresentationTreeNode) node1;

                        if (pnode1.Element.Id.EndsWith(".SERL")) {
                            continue;
                        }

                        stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, pnode1.Element, width: 0,
                                                            bindingPath:
                                                                bindingRoot +
                                                                ".Values[de-gcd_genInfo.report.id.accordingTo.idNo].Items[0].Values[" +
                                                                pnode1.Element.Id + "]");
                    }

                    stackPanel.Model.RegisterGotFocusEventHandler();
                } else if (pnode.Element.Id == "de-gcd_genInfo.report.id.reportStatus.approvalAndDisclosureDate") {
                    foreach (IPresentationTreeEntry node1 in ptree.GetNode("de-gcd_genInfo.report.id.reportStatus.approvalAndDisclosureDate").Children) {
                        twoColStackPanel.Model.UIElements.AddInfo(twoColStackPanel.ContentPanelLeft, node1.Element, width: 0,
                                                                  forceVerticalOrientation: true);
                    }
                } else {
                    twoColStackPanel.Model.UIElements.AddInfo(twoColStackPanel.ContentPanelLeft, pnode.Element, width: 0,
                                                              forceVerticalOrientation: true);
                }
            }
            twoColStackPanel.Model.RegisterGotFocusEventHandler();
            #endregion genInfo.report.id

            #region genInfo.report.period
            stackPanel = new XbrlStackPanel {Header = GetLabel("de-gcd_genInfo.report.period")};
            subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.report.period"), stackPanel, parent: subEntry, xbrlElem:GetElement("de-gcd_genInfo.report.period"));
            IPresentationTreeNode presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.reportPeriodBegin");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.reportPeriodEnd");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.fiscalYearBegin");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpFiscalYearBegin = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[2]).Children[2]);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.fiscalYearEnd");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpFiscalYearEnd = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[3]).Children[2]);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.balSheetClosingDate");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpBalSheetClosingDate = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[4]).Children[2]);            
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.fiscalPreciousYearBegin") ?? ptree.GetNode("de-gcd_genInfo.report.period.fiscalPreviousYearBegin");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpFiscalYearBeginPrevious = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[5]).Children[2]);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.fiscalPreciousYearEnd") ?? ptree.GetNode("de-gcd_genInfo.report.period.fiscalPreviousYearEnd");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpFiscalYearEndPrevious = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[6]).Children[2]);
            presentationNode = ptree.GetNode("de-gcd_genInfo.report.period.balSheetClosingDatePreviousYear") ?? ptree.GetNode("de-gcd_genInfo.report.period.balSheetClosingDatePreciousYear");
            stackPanel.Model.UIElements.AddInfo(stackPanel.ContentPanel, presentationNode.Element, width: 0);
            //_dpBalSheetClosingDatePrevious = ((SearchableDatePicker) ((Panel) stackPanel.ContentPanel.Children[7]).Children[2]);
            stackPanel.Model.RegisterGotFocusEventHandler();
            #endregion genInfo.report.period

            #region genInfo.report.audit
            //var ptrees = PresentationTree.CreatePresentationTrees(TaxonomyManager.GCD_Taxonomy, null, null);
            //var model = new TaxonomyViewModel(Owner, DocumentWrapper, ptrees.Values.First()) {
            //    RoleURI = ptree.Role.RoleUri,
            //    Elements = TaxonomyManager.GCD_Taxonomy.Elements,                
            //};

            //AddNavigationTreeEntry(
            //    GetLabel("de-gcd_genInfo.report.audit"),
            //    new CtlTaxonomyTreeView { PresentationRootName = "de-gcd_genInfo.report.audit" },
            //    presentationTreeRoots: ptree.RootEntries,
            //    dataContext: model,
            //    parent: subEntry,
            //    showBalanceList: ptree.Role.Style.ShowBalanceList,
            //    roleId: ptree.Role.Id);
            
            //basePanel = new XbrlBasePanel { Header = GetLabel("de-gcd_genInfo.report.audit") };
            //subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.report.audit"), basePanel, parent: subEntry);
            // TODO add genInfo.report.audit
            #endregion genInfo.report.audit

            #region genInfo.report.preparationAttestation
            //textPanel = new XbrlTextPanel();
            //subSubEntry = AddNavigationTreeEntryGCD(GetLabel("de-gcd_genInfo.report.preparationAttestation"), textPanel, parent: subEntry);
            //textPanel.Init("de-gcd_genInfo.report.preparationAttestation");
            // TODO add genInfo.report.preparationAttestation
            #endregion genInfo.report.preparationAttestation

            #endregion report information

            #region company information
            //var companyInfo = new CtlCompanyInfo(ValueTreeWrapperGCD, true);
            ReportCompanyEntry = AddNavigationTreeEntryGCD("Unternehmen", new XbrlTextPanel() {Header = "Unternehmen"},
                                                           parent: entry,
                                                           xbrlElem: ptree.GetNode("de-gcd_genInfo.company").Element,
                                                           roleId: ptree.Role.Id);

            CompanyNavigationEntry[] companyNavEntries = new[] {
                                                                   new CompanyNavigationEntry("Allgemein", CtlCompanyInfo.FillPanelCommon, "de-gcd_genInfo.company.id"),
                                                                   new CompanyNavigationEntry("Kennnummern", CtlCompanyInfo.FillPanelIdNumbers, "de-gcd_genInfo.company.id.idNo"),
                                                                   new CompanyNavigationEntry("Gesellschafter", CtlCompanyInfo.FillPanelShareholder, "de-gcd_genInfo.company.id"),
                                                                   new CompanyNavigationEntry("Registereintrag", CtlCompanyInfo.FillPanelIncorporation, "de-gcd_genInfo.company.id.Incorporation"),
                                                                   new CompanyNavigationEntry("Börseneintrag", CtlCompanyInfo.FillPanelStockExch, "de-gcd_genInfo.company.id.stockExch"),
                                                                   new CompanyNavigationEntry("Kontaktperson", CtlCompanyInfo.FillPanelContact, "de-gcd_genInfo.company.id"),
                                                                   new CompanyNavigationEntry("Sonstige Informationen", CtlCompanyInfo.FillPanelOther, "de-gcd_genInfo.company.id"),
                                                                   new CompanyNavigationEntry("Mutterunternehmen", CtlCompanyInfo.FillPanelParentCompany, "de-gcd_genInfo.company.id.parent"),
                                                                   
                                                               };

            foreach(var companyNavEntry in companyNavEntries) {
                CompanyDisplayValueTreeModel newModel = new CompanyDisplayValueTreeModel(TaxonomyManager.GCD_Taxonomy,
                                                                                         ValueTreeWrapperGCD,
                                                                                         DocumentWrapper);
                var xbrlPanel = new XbrlBasePanel() { Header = companyNavEntry.Header };
                FocusManager.SetIsFocusScope(xbrlPanel, true);
                companyNavEntry.FillPanelFunction(newModel.UIElements, newModel, xbrlPanel);
                newModel.RegisterGotFocusEventHandler();
                AddNavigationTreeEntry(companyNavEntry.Header, xbrlPanel, newModel, parent: ReportCompanyEntry,
                                       xbrlElem: ptree.GetNode(companyNavEntry.ElementId).Element,
                                       showBalanceList: false, presentationTreeRoots: ptree.GetNode(companyNavEntry.ElementId).OfType<PresentationTreeNode>(), roleId: ptree.Role.Id);
            }
            #endregion company information
        }

        class CompanyNavigationEntry {
            internal CompanyNavigationEntry(string header, CtlCompanyInfo.FillPanelFunction fillFunction, string elementId) {
                Header = header;
                FillPanelFunction = fillFunction;
                ElementId = elementId;
            }
            public string Header { get; set; }
            public CtlCompanyInfo.FillPanelFunction FillPanelFunction { get; set; }
            public string ElementId { get; set; }
        }

        private void InitNavigationKKE(IRoleType role, NavigationTreeEntry parent) {
            var textPanel = new XbrlBasePanel {Header = GetLabel("de-gaap-ci_kke")};
            NavigationTreeEntry entry = AddNavigationTreeEntryGAAP(GetLabel("de-gaap-ci_kke"), textPanel, parent, showBalanceList: role.Style.ShowBalanceList, xbrlElem:GetElement("de-gaap-ci_kke"));
            AddNavigationTreeEntryGAAP(GetLabel("de-gaap-ci_kke.unlimitedPartners"), new CtlKKE_UnlimitedPartners(), entry,xbrlElem:GetElement("de-gaap-ci_kke.unlimitedPartners"));
            AddNavigationTreeEntryGAAP(GetLabel("de-gaap-ci_kke.limitedPartners"), new CtlKKE_LimitedPartners(), entry, xbrlElem:GetElement("de-gaap-ci_kke.unlimitedPartners"));
        }

        private void InitNavigation_TransferCommercialCodeToTax(IRoleType role, NavigationTreeEntry parent = null) {
            NavigationTreeEntry entry = AddNavigationTreeEntryGAAP(role.Name, new XbrlBasePanel {Header = role.Name},
                                                                   parent, showBalanceList: role.Style.ShowBalanceList);
            entry.RoleId = role.Id;
            // Überleitungsrechnung
            {
                //const string id = "de-gaap-ci_hbst.transfer";
                //var header = GetLabel(id);
                //var panel = new CtlReconciliations(header);
                //AddNavigationTreeEntry(header, panel, new ReconciliationsModel(DocumentWrapper.Value), entry, GetElement(id),
                //                       false)
                //    .IsVisible = (DocumentWrapper.Value.IsCommercialBalanceSheet &&
                //                  DocumentWrapper.Value.ReportRights.ReadTransferValuesAllowed);
            }

            {
                // specialPartner_SupplementBalanceSheet (Sonderbilanzen als Freitext)
                const string id = "de-gaap-ci_hbst.specialPartner_SupplementBalanceSheet.content";
                var textPanel = new XbrlTextPanel();
                AddNavigationTreeEntryGAAP(GetLabel(id), textPanel, entry, GetElement(id), false);
                textPanel.Init(id);
                textPanel.Model.RegisterGotFocusEventHandler();
            }
        }

        private IElement GetElement(string xbrlElemId) {
            return xbrlElemId.StartsWith("de-gcd")
                       ? (TaxonomyManager.GCD_Taxonomy.Elements.ContainsKey(xbrlElemId)
                              ? TaxonomyManager.GCD_Taxonomy.Elements[xbrlElemId]
                              : null)
                       : (DocumentWrapper.Value.MainTaxonomy.Elements.ContainsKey(xbrlElemId)
                              ? DocumentWrapper.Value.MainTaxonomy.Elements[xbrlElemId]
                              : null);
        }

        private string GetLabel(string xbrlElemId) {
            return xbrlElemId.StartsWith("de-gcd")
                       ? (TaxonomyManager.GCD_Taxonomy.Elements.ContainsKey(xbrlElemId)
                              ? TaxonomyManager.GCD_Taxonomy.Elements[xbrlElemId].Label
                              : xbrlElemId)
                       : (DocumentWrapper.Value.MainTaxonomy.Elements.ContainsKey(xbrlElemId)
                              ? DocumentWrapper.Value.MainTaxonomy.Elements[xbrlElemId].Label
                              : xbrlElemId);
        }

        private NavigationTreeEntry AddNavigationTreeEntryGAAP(
            string header,
            UIElement elem,
            NavigationTreeEntry parent = null,
            IElement xbrlElem = null,
            bool showBalanceList = true) {
            return AddNavigationTreeEntry(
                header,
                elem,
                new DisplayValueTreeModel(DocumentWrapper.Value.MainTaxonomy, ValueTreeWrapperGAAP),
                parent: parent,
                xbrlElem: xbrlElem,
                showBalanceList: showBalanceList);
        }

        private NavigationTreeEntry AddNavigationTreeEntryGCD(
            string header,
            UIElement elem,
            NavigationTreeEntry parent = null,
            IElement xbrlElem = null,
            string roleId = null) {
            return AddNavigationTreeEntry(header, elem,
                                          new DisplayValueTreeModel(TaxonomyManager.GCD_Taxonomy, ValueTreeWrapperGCD),
                                          parent: parent, xbrlElem: xbrlElem, showBalanceList: false, roleId: roleId);
        }

        private NavigationTreeEntry AddNavigationTreeEntry(
            string header,
            UIElement elem,
            object dataContext,
            NavigationTreeEntry parent = null,
            IElement xbrlElem = null,
            bool showBalanceList = true,
            IEnumerable<IPresentationTreeNode> presentationTreeRoots = null,
            string roleId = null) {
            ((FrameworkElement) elem).DataContext = dataContext;

            var entry = new NavigationTreeEntry {
                PresentationTreeRoots = presentationTreeRoots,
                Header = header,
                Content = elem,
                XbrlElem = xbrlElem,
                Model = dataContext,
                ShowBalanceList = showBalanceList,
                RoleId = roleId
            };

            if (parent == null) _children.Add(entry);
            else {
                parent.Children.Add(entry);
                entry.Parent = parent;
            }

            return entry;
        }

        #region Validate
        /// <summary>
        /// Validates all items in this instance.
        /// </summary>
        public void Validate() {
            foreach (NavigationTreeEntry entry in Children) {
                entry.Validate();
            }
        }
        #endregion Validate

        /// <summary>
        /// Check the visibility for all NavigationTreeEntries
        /// </summary>
        /// <param name="showAllEntries">Defines if all entries should be shown or only the entries for the selected legal form (default)</param>
        public void CheckVisibility(bool showAllEntries = false) {
            
            if (DocumentManager.Instance.CurrentDocument != null) {

            //foreach (var child in Children) {
                foreach (NavigationTreeEntry navigationTreeEntry in
                    Children.Last().Children.OfType<NavigationTreeEntry>()) {
                    navigationTreeEntry.CheckVisibility(showAllEntries);
                }
            //}
                foreach (var reportPart in DocumentManager.Instance.CurrentDocument.TaxonomyPart.ReportParts.Values) {
                    reportPart.IsVisible =
                        new eBalanceKitBusiness.Structures.PresentationTreeFilter(null).GetVisibilityForGlobalOptions(
                            DocumentManager.Instance.CurrentDocument,
                            reportPart.Element);
                }
            }
            // ToDo: If Visibility of GCD infos should be updated regarding the global options
            // this must be uncommented + the visibility binding stuff in eBalanceKit\Structures\TaxonomyUIElements.cs
            // foreach (PresentationTreeNode node in DocumentManager.Instance.CurrentDocument.GcdPresentationTreeNodes.Values) {
            //    node.IsVisible =
            //        new eBalanceKitBusiness.Structures.PresentationTreeFilter(null).GetVisibilityForGlobalOptions(
            //            DocumentManager.Instance.CurrentDocument, node.Element);
            //}

            // A child in the report section that is not visible anymore is selected
            if (Children.Last().Children.Any(c => c.IsSelected && !c.IsVisible)) {
                // select the "Report" node
                Children.Last().IsSelected = true;
            }
        }

        #region HideValidationWarnings

        public void UpdateVisibilityValidationWarning(Document doc) {


            foreach (var value in doc.ValueTreeGcd.Root.Values.Values) {
                value.UserOptionChanged();
            }
            foreach (var value in doc.ValueTreeMain.Root.Values.Values) {
                value.UserOptionChanged();
            }

            List<INavigationTreeEntryBase> childList = new List<INavigationTreeEntryBase>();

            foreach (var child in Children) {
                childList.AddRange(GetChildList(child));
            }

            foreach (var navigationTreeEntry in childList) {
                // dummy set of property to fire the OnPropertyChanged (Additional Eventhandler for eBalanceKitBusiness.Options.GlobalUserOptions.UserOptions.HideAllWarnings not possible because of static context)
                var treeEntry = navigationTreeEntry as NavigationTreeEntry;
                if (treeEntry != null)
                    treeEntry.UserOptionChanged();

                if (navigationTreeEntry.Model != null && navigationTreeEntry.Model is DisplayValueTreeModel) {
                    (navigationTreeEntry.Model as DisplayValueTreeModel).UpdateChildrenContainsValidationWarningElement();
                }

                //// no presentation tree in this navigationTreeEntry 
                //if (navigationTreeEntry.PresentationTreeRoots == null) continue;
                
                //foreach (var presentationTreeRoot in navigationTreeEntry.PresentationTreeRoots) {
                //    var rootNode = (Taxonomy.PresentationTree.PresentationTreeNode)presentationTreeRoot;


                //    // Get the root entries according to the PresentationTreeRoots --> done this way because rootNode can not be parsed to eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode
                //    var rootEntries = from tree in doc.GaapPresentationTrees.Values
                //                      where
                //                          tree is PresentationTree && tree.RootEntries.First() is PresentationTreeNode &&
                //                          tree.RootEntries.Any(x => x.Element.Id == rootNode.Element.Id)
                //                      select ((PresentationTree)tree).RootEntries;

                //    // if there are rootentries
                //    if (rootEntries.Any()) {
                //        foreach (PresentationTreeNode node in rootEntries.First()) {

                //            foreach (var child in GetChildList(node)) {
                //                if (child.Value != null) {
                //                    child.Value.UserOptionChanged();
                //                }
                //            }
                //        }
                //    }
                //    else {
                //        // Look the root up in the ValueTreeGcd
                //        Debug.WriteLine("Seems to be in the GCD");

                //        var crootEntries = from value in doc.ValueTreeGcd.Root.Values
                //                           where
                //                               value.Value.Element.Id == rootNode.Element.Id
                //                           select (value.Value);

                //        foreach (var valueTreeEntry in crootEntries) {
                //            // Should be only one
                //            valueTreeEntry.UserOptionChanged();
                //        }
                //    }
                //}
            }
        }


        /// <summary>
        /// Get a list of all NavigationTreeEntries.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <returns></returns>
        private List<INavigationTreeEntryBase> GetChildList(INavigationTreeEntryBase start) {
            List<INavigationTreeEntryBase> result = new List<INavigationTreeEntryBase>();
            result.Add(start);
            foreach (var child in start.Children) {
                result.Add(child);
                result.AddRange(GetChildList(child));
            }
            return result;
        }

        /// <summary>
        /// Get a list of all PresentationTreeNodes.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <returns></returns>
        private List<PresentationTreeNode> GetChildList(PresentationTreeNode start) {
            List<PresentationTreeNode> result = new List<PresentationTreeNode>();
            result.Add(start);
            foreach (var child in start.Children) {
                if (child is PresentationTreeNode) {
                    result.Add(child as PresentationTreeNode);
                    result.AddRange(GetChildList(child as PresentationTreeNode));
                }
            }
            return result;
        }

        // Methods to hide validation 
        ///// <summary>
        ///// Show only validation errors and no warnings.
        ///// </summary>
        ///// <param name="doc">The current document.</param>
        //public void HideAllValidationWarnings(Document doc) {
        //    HideValidationWarnings(doc, true);
        //}

        ///// <summary>
        ///// Show only validation errors and warnings that were not manualy disabled.
        ///// </summary>
        ///// <param name="doc">The current document.</param>
        //public void HideChoosenValidationWarnings(Document doc) {
        //    HideValidationWarnings(doc, false);
        //}


        ///// <summary>
        ///// Hide warnings but show errors in each presentation and navigation tree.
        ///// </summary>
        ///// <param name="doc">The current loaded document.</param>
        ///// <param name="hideAllWarnings">Hide all warnings? false = hide only choosen warnings (default)</param>
        //private void HideValidationWarnings(Document doc, bool hideAllWarnings = false) {
        //    List<INavigationTreeEntryBase> childList = new List<INavigationTreeEntryBase>();

        //    foreach (var child in Children) {
        //        childList.AddRange(GetChildList(child));
        //    }

        //    foreach (var navigationTreeEntry in childList) {
        //        // dummy set of property to fire the OnPropertyChanged (Additional Eventhandler for eBalanceKitBusiness.Options.GlobalUserOptions.UserOptions.HideAllWarnings not possible because of static context)
        //        var treeEntry = navigationTreeEntry as NavigationTreeEntry;
        //        if (treeEntry != null)
        //            treeEntry.UserOptionChanged();
                
        //        // no presentation tree in this navigationTreeEntry 
        //        if (navigationTreeEntry.PresentationTreeRoots == null) continue;
        //        foreach (var presentationTreeRoot in navigationTreeEntry.PresentationTreeRoots) {
        //            HideValidationWarnings((Taxonomy.PresentationTree.PresentationTreeNode)presentationTreeRoot, doc, hideAllWarnings);
        //        }

        //        if (navigationTreeEntry.Model != null && navigationTreeEntry.Model is DisplayValueTreeModel) {
        //            HideValidationWarnings((navigationTreeEntry.Model as DisplayValueTreeModel), hideAllWarnings);
        //        }

        //    }
        //}

        ///// <summary>
        ///// Hide warnings but show errors in the presentation tree starting with the specified node.
        ///// </summary>
        ///// <param name="rootNode">The node where the hide warning process starts.</param>
        ///// <param name="doc">The current loaded document.</param>
        ///// <param name="hideAllWarnings">Hide all warnings? false = hide only choosen warnings (default)</param>
        //private void HideValidationWarnings(Taxonomy.PresentationTree.PresentationTreeNode rootNode, Document doc, bool hideAllWarnings = false) {

        //    // Get the root entries according to the PresentationTreeRoots --> done this way because rootNode can not be parsed to eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode
        //    var rootEntries = from tree in doc.GaapPresentationTrees.Values
        //                      where
        //                          tree is PresentationTree && tree.RootEntries.First() is PresentationTreeNode &&
        //                          tree.RootEntries.Any(x => x.Element.Id == rootNode.Element.Id)
        //                      select ((PresentationTree)tree).RootEntries;

        //    // if there are rootentries
        //    if (rootEntries.Any()) {
        //        foreach (PresentationTreeNode node in rootEntries.First()) {
                                    
        //            foreach (var child in GetChildList(node)) {
        //                if (child.Value != null) {
        //                    if (hideAllWarnings || child.Value.SupressWarningMessages) {
        //                        child.Value.UserOptionChanged();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else {
        //        // Look the root up in the ValueTreeGcd
        //        Debug.WriteLine("Seems to be in the GCD");

        //        var crootEntries = from value in doc.ValueTreeGcd.Root.Values
        //                           where
        //                               value.Value.Element.Id == rootNode.Element.Id
        //                           select (value.Value);

        //        foreach (var valueTreeEntry in crootEntries) {
        //            // Should be only one
        //            if (valueTreeEntry.SupressWarningMessages || hideAllWarnings) {
        //                valueTreeEntry.UserOptionChanged();
        //            }

        //        }
        //    }
        //}

        ///// <summary>
        ///// Hide validation warnings in the DisplayValueTreeModel.
        ///// </summary>
        //private void HideValidationWarnings(DisplayValueTreeModel model, bool hideAllWarnings = false) {
        //    if (model.ValueTreeRoot == null) return;
        //    foreach (var valueTreeEntry in model.ValueTreeRoot.Values.Values) {
        //        if (valueTreeEntry.SupressWarningMessages || hideAllWarnings) {
        //            valueTreeEntry.UserOptionChanged();
        //        }

        //    }
        //} 
        #endregion

        #region RemoveValidationInfos
        /// <summary>
        /// Validates all items in this instance.
        /// </summary>
        public void RemoveValidationInfos() {
            foreach (NavigationTreeEntry entry in Children) {
                entry.ValidationWarning = false;
                entry.ValidationError = false;
                RemoveValidationInfos(entry);
            }
        }

        private void RemoveValidationInfos(NavigationTreeEntry startEntry) {
            foreach (NavigationTreeEntry entry in startEntry.Children) {
                entry.ValidationWarning = false;
                entry.ValidationError = false;
                RemoveValidationInfos(entry);
                if (entry.Model != null && entry.Model is DisplayValueTreeModel) {
                    RemoveValidationInfos((entry.Model as DisplayValueTreeModel));
                }
            }
        }

        private void RemoveValidationInfos(DisplayValueTreeModel model) {
            if (model.ValueTreeRoot == null) return;
            foreach (var value in model.ValueTreeRoot.Values.Values) {
                value.ValidationError = false;
                value.ValidationWarning = false;
            }
            model.UpdateChildrenContainsValidationErrorElement();
            model.UpdateChildrenContainsValidationWarningElement();
        }

        #endregion RemoveValidationInfos

        #region GetEnumerator
        public IEnumerator<INavigationTreeEntryBase> GetEnumerator() {
            //// TODO: thats nasty
            //ObservableCollectionAsync<NavigationTreeEntry> ret = new ObservableCollectionAsync<NavigationTreeEntry>();
            //foreach (INavigationTreeEntryBase navigationTreeEntryBase in Children) {
            //    ret.Add(navigationTreeEntryBase as NavigationTreeEntry);
            //}
            return Children.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() { return Children.GetEnumerator(); }
        #endregion GetEnumerator

        #region InitNavigation_EqCh
        private void InitNavigation_EqCh(NavigationTreeEntry parent = null) {
            var textPanel = new XbrlTextPanel();
            NavigationTreeEntry subentry = AddNavigationTreeEntryGAAP(GetLabel("de-gaap-ci_eqCh"), textPanel, parent, xbrlElem:GetElement("de-gaap-ci_eqCh"));
            textPanel.Init("eqCh");

            //HC_EquityChangesStatememt hcEquityChangesStatememt = new HC_EquityChangesStatememt { Header = GetLabel("de-gaap-ci_eqCh") };
            //this.HC_EquityChangesStatememtModel = new HyperCubeModel(this, "ChangesEquityStatement", this.Owner);
            //AddNavigationTreeEntry("Eigenkapitalspiegel", hcEquityChangesStatememt, this.HC_EquityChangesStatememtModel);
        }
        #endregion

    }
}