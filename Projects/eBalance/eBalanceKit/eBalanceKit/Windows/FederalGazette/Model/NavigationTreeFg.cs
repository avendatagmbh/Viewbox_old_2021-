// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Taxonomy;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKit.Controls;
using eBalanceKitResources.Localisation;
using eBalanceKit.Models;
using eBalanceKit.Structures;
using eBalanceKit.Windows.FederalGazette.FederalGazetteCtls;
using eBalanceKitBusiness.FederalGazette.Model;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Linq;

namespace eBalanceKit.Windows.FederalGazette.Model {
    internal class NavigationTreeFg : INotifyPropertyChanged, INotifyPropertyChanging, IEnumerable<NavigationTreeEntryFg>
    {

        #region Constructor
        public NavigationTreeFg(ObjectWrapper<Document> documentWrapper)
        {
            DocumentWrapper = documentWrapper;
            DocumentWrapper.PropertyChanged += new PropertyChangedEventHandler(DocumentWrapper_PropertyChanged);
            DocumentWrapper.PropertyChanging += new PropertyChangingEventHandler(DocumentWrapper_PropertyChanging);
            if (DocumentWrapper.Value != null) DocumentWrapper.Value.AssignedTaxonomyInfoChanged += new System.EventHandler<eBalanceKitBusiness.EventArgs.AssignedTaxonomyInfoChangedEventArgs>(Value_AssignedTaxonomyInfoChanged);
            ValueTreeWrapperGaapFg= new ValueTreeWrapper();
        }

        void Value_AssignedTaxonomyInfoChanged(object sender, eBalanceKitBusiness.EventArgs.AssignedTaxonomyInfoChangedEventArgs e)
        {
            InitNavigationReport();   
        }

        void DocumentWrapper_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (DocumentWrapper.Value != null) 
                DocumentWrapper.Value.AssignedTaxonomyInfoChanged -= Value_AssignedTaxonomyInfoChanged;
        }


        void DocumentWrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Value")) {
                NavigationTreeReport.IsVisible = DocumentWrapper.Value != null;
                if (DocumentWrapper.Value != null) {
                    ((FrameworkElement) NavigationTreeReport.Content).DataContext = DocumentWrapper.Value;
                    ValueTreeWrapperGaapFg.ValueTreeRoot = DocumentWrapper.Value.ValueTreeMain.Root;
                    InitNavigationReport();
                    DocumentWrapper.Value.AssignedTaxonomyInfoChanged += Value_AssignedTaxonomyInfoChanged;
                }
            }
        }

        #endregion

        #region properties
        public IEnumerator<NavigationTreeEntryFg> GetEnumerator() { return Children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Children.GetEnumerator(); }

        public NavigationTreeEntryFg NavigationTreeReport { get; set; }
        private ObjectWrapper<Document> DocumentWrapper { get; set; }
        private Window Owner { get; set; }
        private ValueTreeWrapper ValueTreeWrapperGaapFg { get; set; }

        private readonly ObservableCollection<NavigationTreeEntryFg> _children =
            new ObservableCollection<NavigationTreeEntryFg>();

        public IEnumerable<NavigationTreeEntryFg> Children { get { return _children; } }
        #endregion

        #region Methods
        public void InitNavigation(FederalGazetteMainModel model) {
            InitNavigationManagement(model);
            
            NavigationTreeReport = new NavigationTreeEntryFg { Header = "Report", Content = null, IsVisible = true };
            _children.Add(NavigationTreeReport);

        }

        public void InitNavigationReport() {

            NavigationTreeEntryFg root = NavigationTreeReport;
            ITaxonomy taxonomy = DocumentWrapper.Value.MainTaxonomy;

            foreach (var ptree in taxonomy.PresentationTrees.Where(ptree => ptree.Role.Style.IsVisible))
            {
                AddNavigationTreeStyle(root, ptree);
            }

        }

        public void InitNavigationManagement(FederalGazetteMainModel model) {
            NavigationTreeEntryFg entry, subEntry;
            entry = new NavigationTreeEntryFg {Header = "eBundesanzeiger", Content = null, IsVisible = true};
            _children.Add(entry);

            //client management 
            var contentClient = new CtlFederalGazetteClient(model);
            subEntry = new NavigationTreeEntryFg {Header = "Client", Content = contentClient, IsVisible = true};
            entry.AddChildren(subEntry);

            var contentSettings = new CtlFederalGazetteReportSettings(model);
            subEntry = new NavigationTreeEntryFg {Header = "Einstellung", Content = contentSettings, IsVisible = true};
            entry.AddChildren(subEntry);

            entry.AddChildren(subEntry);

        }

        public void AddNavigationTreeStyle(NavigationTreeEntryFg root, IPresentationTree ptree) {
            var model = new TaxonomyViewModel(Owner, DocumentWrapper);
            model.Elements = DocumentWrapper.Value.MainTaxonomy.Elements;
            model.RoleURI = ptree.Role.RoleUri;

            AddNavigationTreeEntry(ptree.Role.Name, new CtlTaxonomyTreeView(), dataContext: model, parent: root,
                                   showBalanceList: ptree.Role.Style.ShowBalanceList);
        }


        public NavigationTreeEntryFg AddNavigationTreeEntry(string header,
                                                            UIElement uiElement, object dataContext,
                                                            NavigationTreeEntryFg parent = null,
                                                            IElement xbrlElement = null,
                                                            bool showBalanceList = true) {

            ((FrameworkElement) uiElement).DataContext = dataContext;
            var entry = new NavigationTreeEntryFg();
            entry.Header = header;
            entry.Content = uiElement;
            entry.XbrlElement = xbrlElement;
            entry.Model = dataContext;
            entry.ShowBalanceList = showBalanceList;

            if (parent == null) _children.Add(entry);
            else {
                parent.Children.Add(entry);
                entry.Parent = parent;
            }
            return entry;
        }


        public void Validate() {
            foreach (NavigationTreeEntryFg entryFg in Children) {
                entryFg.Validate();
            }
        }
        #endregion

        #region EventHandler

        private event PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { _propertyChanged += value; } 
            remove { _propertyChanged -= value; }
        }

        private event PropertyChangingEventHandler _propertyChanging;
        public event PropertyChangingEventHandler PropertyChanging {
            add { _propertyChanging += value; } 
            remove { _propertyChanging -= value; }
        }
        protected void OnPropertyChanged(string propertyName) {
            if (_propertyChanged != null) 
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanging(string propertyName) {
            if (_propertyChanging != null) 
                _propertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public void ClearAllEventHandlers() {
            _propertyChanged = null;
            _propertyChanging = null;
        }
        #endregion


    }
}