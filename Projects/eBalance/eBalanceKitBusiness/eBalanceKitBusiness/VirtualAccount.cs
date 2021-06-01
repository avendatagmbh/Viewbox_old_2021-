// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DbAccess;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Class for virtual accounts that reference the value of special taxonomy positions. Needed eg. for "Bilanzgewinn"  
    /// </summary>
    [DbTable("virtual_accounts", Description = "virtual balance list accounts", ForceInnoDb = true)]
    internal class VirtualAccount : Account, IBalanceListEntry {
        /// <summary>
        /// Class for virtual accounts that reference the value of special taxonomy positions. Needed eg. for "Bilanzgewinn" 
        /// </summary>
        /// <param name="taxonomyPosition">The source position in the BalanceList.Document.ValueTreeMain that is connected to this account</param>
        public VirtualAccount(string taxonomyPosition, BalanceList balanceList) {
            TaxonomyPosition = taxonomyPosition;
            BalanceList = balanceList;
            //SortIndex = taxonomyPosition;
            
            //Init();   
        }

        private void Init() {
            if (BalanceList.Document.ValueTreeMain != null) {
                TaxonomyValue = BalanceList.Document.ValueTreeMain.GetValue(TaxonomyPosition);
                Debug.Assert(TaxonomyValue is XbrlElementValue_Monetary);
            } else {
                BalanceList.Document.PropertyChanged += Document_PropertyChanged;
            }
        }

        ~VirtualAccount() {
            if(BalanceList != null && BalanceList.Document != null) BalanceList.Document.PropertyChanged -= Document_PropertyChanged;
        }

        void Document_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "ValueTreeMain" || e.PropertyName.Equals("MainTaxonomyKind")) {
                if (BalanceList.Document.ValueTreeMain != null) {
                    TaxonomyValue = BalanceList.Document.ValueTreeMain.GetValue(TaxonomyPosition);
                }

                List<IAccount> availableAccounts = new List<IAccount>();
                foreach (VirtualAccount account in this.BalanceList.Accounts) {
                    if (account.CheckAvailability(sender as Document)) {
                        availableAccounts.Add(account);
                    }
                }
                BalanceList.IsHidden = !availableAccounts.Any();
                BalanceList.SetEntries(availableAccounts, null, null);
                OnPropertyChanged("BalanceListsVisible");
                //BalanceList.FirePropertyChangedEventsForDisplayedItems();
                BalanceList.UpdateFilter();
            }
        }


        public VirtualAccount() { 
            //Amount = 0;
        }


        public override BalanceList BalanceList {
            get {
                return base.BalanceList;
            }
            set {
                base.BalanceList = value;
                if(BalanceList != null) Init();
            }
        }

        public new Taxonomy.IElement AssignedElement {
            get { return base.AssignedElement; }
            set {
                if (value == null || AssignmentRestriction.AllowedAssignmentPositions == null || AssignmentRestriction.AllowedAssignmentPositions.Any(restriction => value.Name.StartsWith(restriction))) {
                    base.AssignedElement = value;
                } else {
                    BalanceList.Document.RemoveAssignment(this);
                    RemoveFromParents();
                }
            }
        }

        [DbColumn("assigned_element_id", AllowDbNull = true)]
        public new int AssignedElementId {
            get { return base.AssignedElementId; }
            set {
                if (value == 0) {
                    base.AssignedElementId = value;
                    return;
                }

                var elementId = BalanceList == null ? null : BalanceList.Document.TaxonomyIdManager.GetElement(value);

                if (CheckAssignmentAllowed(elementId as IPresentationTreeNode)) {
                    base.AssignedElementId = value;
                } else {
                    BalanceList.Document.RemoveAssignment(this);
                    //TaxonomyIdManager.RemoveElementAssignment(this);
                    RemoveFromParents();
                    throw new AssignmentNotAllowedException(new string[] { this.Name, AssignmentRestriction.AllowedAssignmentPositionVerbal});
                }
            }
        }
        
        [DbColumn("taxonomy_source_position")]
        public string TaxonomyPosition { get; set; }

        #region TaxonomyValue
        private IValueTreeEntry _taxonomyValue;

        public IValueTreeEntry TaxonomyValue {
            get { return _taxonomyValue; }
            set {
                if (_taxonomyValue != value) {
                    _taxonomyValue = value;
                    OnPropertyChanged("TaxonomyValue");
                    OnPropertyChanged("Amount");
                    OnPropertyChanged("DisplayString");
                    OnPropertyChanged("Value");
                    OnPropertyChanged("DecimalValue");
                    OnPropertyChanged("ValueDisplayString");
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Label");
                }
            }
        }
        #endregion TaxonomyValue
        /// <summary>
        /// Get the DecimalValue of the TaxonomyPosition
        /// </summary>
        public override decimal Amount {
            get {
                //if (TaxonomyValue == null ){
                //    if (BalanceList == null || BalanceList.Document.ValueTreeMain == null) { return 0; }
                //    TaxonomyValue = BalanceList.Document.ValueTreeMain.GetValue(TaxonomyPosition);
                //}
                return TaxonomyValue == null ? 0 : TaxonomyValue.DecimalValue;
            }
            set {
                // do nothing
                OnPropertyChanged("Amount");    
                OnPropertyChanged("DecimalValue");
                OnPropertyChanged("ValueDisplayString");
            } 
        }

        public new bool IsVisible {
            get { return true; }
            set {
                // do nothing
            }
        }

        [DbColumn("hidden", AllowDbNull = false)]
        public new bool IsHidden { get { return false; } set { } }

        /// <summary>
        /// Get the explanation that it's a virtual account for the regarding TaxonomyPosition
        /// </summary>
        public new string Comment {
            get {
                return string.Format(ResourcesBalanceList.VirtualAccountComment,
                    TaxonomyValue == null ? "unknown" : TaxonomyValue.Element.Label);
            }
            set {
                // do nothing
            }
        }
        
        /// <summary>
        /// returns the DbValue.ElementId of the regarding TaxonomyPositionValue
        /// </summary>
        public override string Number {
            get { return TaxonomyValue == null ? string.Empty : TaxonomyValue.DbValue.ElementId + ""; }
            set {
                // do nothing
                OnPropertyChanged("Number");
            }
        }

        /// <summary>
        /// Get the DisplayString of the regarding TaxonomyPosition
        /// </summary>
        public override string Name {
            get {
                return TaxonomyValue == null ? ResourcesBalanceList.VirtualAccountDefaultName + " " + TaxonomyPosition : TaxonomyValue.Element.Label;
            }
            set {
                // do nothing
            }
        }

        public VirtualAccountConfiguration // Get the configuration for this account
            AssignmentRestriction {
            get {
                return (from elem in VirtualBalanceListAndAccountManager.CreateDefaultTaxonomyElements()
                        where elem.SourcePosition.Equals(TaxonomyPosition)
                        select elem).FirstOrDefault();
            }
        }

        /// <summary>
        /// Checks if an assignment that is in process is allowed. If not allowed it will fire an AssignmentNotAllowedException.
        /// </summary>
        /// <param name="assignedNode">The destination IPresentationTreeNode where this account should be assigned to.</param>
        /// <returns>Is it allowed?</returns>
        public override bool CheckAssignmentAllowed(IPresentationTreeNode assignedNode) {

            if (assignedNode == null) {
                return true;
            }

            if (AssignmentRestriction == null) {
                // We don't have informations so we don't need to allow this assignments
                return false;
            }
            
            if (AssignmentRestriction.AllowedAssignmentPositions.Any(allowedAssignmentPosition => assignedNode.Element.Name.StartsWith(allowedAssignmentPosition))) {
                // and the assignment destination is valid
                return true;
            }
            
            throw new AssignmentNotAllowedException(new string[] { this.Name, AssignmentRestriction.AllowedAssignmentPositionVerbal });
            //return false;
        }


        public bool CheckAvailability(Document doc = null) {
            if (doc == null) {
                doc = BalanceList.Document;
            }

            if (doc == null) {
                doc = DocumentManager.Instance.CurrentDocument;
            }

            if (doc == null) {
                return true;
            }

            if (doc.GaapPresentationTrees != null) {

                bool containsSource = false;
                bool containsDestination = false;

                foreach (var presentaionTree in doc.GaapPresentationTrees.Values) {

                    // does it contain a tree entry that starts with one of the allowed assignment positions (eg. assignment only to balance but no balance in current doc)
                    if (!containsDestination) {
                        containsDestination =
                            presentaionTree.Nodes.Any(
                                presentaionTreeNode =>
                                AssignmentRestriction.AllowedAssignmentPositions.Any(
                                    allowedAssignmentPosition =>
                                    presentaionTreeNode.Element.Name.StartsWith(allowedAssignmentPosition)));
                    }

                    // is the calculation source for this account part of this GaapPresentationTree
                    if (!containsSource) {
                        containsSource =
                            presentaionTree.Nodes.Any(
                                valueTreeMainEntry =>
                                valueTreeMainEntry.Element.Id.Equals(AssignmentRestriction.SourcePosition));
                    }
                }
                if (!containsDestination || !containsSource) {
                    return false;
                }
            }
            else {
                doc.PropertyChanged -= Document_PropertyChanged;
                doc.PropertyChanged += Document_PropertyChanged;
            }

            return true;
        }


    }

    /// <summary>
    /// A BalanceList with only virtual accounts. Has a fix Name and Comment
    /// </summary>
    [DbTable("balance_lists", Description = "imported balance lists", ForceInnoDb = true)]
    internal class VirtualBalanceList: BalanceList {
        public VirtualBalanceList() {
            Init();
        }

        
        public VirtualBalanceList(IEnumerable<VirtualAccount> accounts) : this() {
            this.SetEntries(accounts, null, null);
        }

        private void Init() {
            Name = ResourcesBalanceList.VirtualBalanceListName;
            Comment = ResourcesBalanceList.VirtualBalanceListComment;
            Source = "Taxonomy";
        }
    }

    #region maybe for later (calculated virtual accounts)


    //public enum VirtualAccountCalculations {
    //    YCase1
    //}

    //public class vAccountCalculation {
    //    public VirtualAccountCalculations Identifier;
    //    public List<string> ReferencedTaxonomyPositions;
    //    public string Value;
    //    public Func<string> Func;
    //}


    //public enum VirtualAccountCalculations {
    //    YCase1
    //}

    //internal class VirtualAccountCalculated : VirtualAccount {
    //    #region Amount
    //    private decimal _amount;
    //    public override decimal Amount { get { return _amount; } set { base.Amount = value; } } 
    //    #endregion

    //    public VirtualAccountCalculations Identifier;
    //    public List<string> ReferencedTaxonomyPositions { get; set; }
    //    public string Value { get; set; }
    //    public Func<string> Func;

    //} 
    #endregion

    internal static class VirtualBalanceListAndAccountManager {
        
        /// <summary>
        /// Eventhandler to fire PropertyChanged if the source taxonomy value has changed.
        /// </summary>
        public static void MonetaryValuePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (!e.PropertyName.Equals("Value") || 
                    Document == null || 
                    !CreateDefaultTaxonomyElements().Any(x => x.SourcePosition.Equals((sender as XbrlElementValue_Monetary).Element.Id))) {
                return;
            }
            // Load the virtual balance list
            foreach (IBalanceList balanceList in Document.BalanceLists.Where(x => x is VirtualBalanceList || !x.IsImported)) {
                // load the account for this (the changed) taxonomy position
                var account =
                    balanceList.Accounts.FirstOrDefault(
                        entry =>
                        entry is VirtualAccount &&
                        (entry as VirtualAccount).TaxonomyPosition == (sender as XbrlElementValue_Monetary).Element.Id);

                if (account != null) {
                    // dummy set to call the PropertyChanged events
                    account.Amount = 0;
                }
            }
        }

        
        /// <summary>
        /// Load the account informations for the specified balance list.
        /// </summary>
        /// <param name="document">The current document that contains the balance list and the accounts.</param>
        /// <param name="balanceList">The balance list that contains the accounts.</param>
        public static void InitBalanceListEntries(Document document, IBalanceList balanceList) {
            // keep the document as current document
            Document = document;

            List<VirtualAccount> accounts;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {

                // load accounts
                accounts = conn.DbMapping.Load<VirtualAccount>(conn.Enquote("balance_list_id") + "=" + balanceList.Id);
            }
            
            // get only the accounts that are valid for the current document (taxonomy ...)
            var availableAccounts = accounts.Where(account => account.CheckAvailability(document)).ToList();

            if (!availableAccounts.Any()) {
                balanceList.IsHidden = true;
                return;
            }

            balanceList.IsHidden = false;
            // init assigned elements
            document.TaxonomyIdManager.AssignElements(availableAccounts);
            
            // set balance list for accounts
            foreach (var account in availableAccounts) {
                account.BalanceList = balanceList as BalanceList;//as VirtualBalanceList;
                account.DoDbUpdate = true;
            }


            balanceList.Name = ResourcesBalanceList.VirtualBalanceListName;
            balanceList.Comment = ResourcesBalanceList.VirtualBalanceListComment;
            // init balance list entries
            balanceList.SetEntries(availableAccounts, null, null);

        }

        private static VirtualBalanceList BalanceList { get; set; }

        //<remark> 
        // get the value from the Income Statement (GUV) "de-gaap-ci_is.netIncome" and assign it per default to the position in the balance "de-gaap-ci_bs.eqLiab.equity.netIncome"
        // allow only assignments to the liability part of the balance (Passiva) "bs.eqLiab" and explain the situation by extending the AssignmentNotAllowedException "only allowed to assign to..."
        // with the entry VirtualAccountAssignmentAllowedTo_bs_eqLiab in ResourcesBalanceList
        //</remark>
        static readonly VirtualAccountConfiguration DefautlVirtualAccount = new VirtualAccountConfiguration("de-gaap-ci_is.netIncome", "de-gaap-ci_bs.eqLiab.equity.netIncome", new[]{"bs.eqLiab", "bs.ass.accountingConvenience"}, "VirtualAccountAssignmentAllowedTo_bs_eqLiab");

        static readonly List<VirtualAccountConfiguration> DefaultVirtualAccountList = new List<VirtualAccountConfiguration> {DefautlVirtualAccount};

        public static List<VirtualAccountConfiguration> CreateDefaultTaxonomyElements() { return DefaultVirtualAccountList; }

        ///// <summary>
        ///// List of VirtualAccountConfiguration that contain "Source", "Default Assignment Destination" and "Allowed Assignment Positions" (= Start of the Element.Id) and "allowed assignment position verbal"
        ///// </summary>
        //public static List<VirtualAccountConfiguration> TaxonomyElements {
        //    get {
        //        return new List<VirtualAccountConfiguration>
        //               {
        //                   DefautlVirtualAccount
        //                   //new VirtualAccountConfiguration("de-gaap-ci_is.netIncome", "de-gaap-ci_bs.eqLiab.equity.netIncome", new[]{"bs.eqLiab", "bs.ass.accountingConvenience"}, ResourcesBalanceList.VirtualAccountAssignmentAllowedTo_bs_eqLiab)
        //               };
        //    }
        //}
        
        #region maybe for later (calculated virtual accounts)
		
        ////private static Dictionary<VirtualAccountCalculations, Func<string>> CalculationElements {
        ////    get {
        ////        return new Dictionary<VirtualAccountCalculations, Func<string>>
        ////               {
        ////                   {VirtualAccountCalculations.YCase1, CalculateForYCase1}
        ////               };
        ////    }
        ////}

        //private static HashSet<VirtualAccountCalculated> CalculatedElements;

        ////private static Dictionary<VirtualAccountCalculations, Tuple<VirtualAccountCalculations, List<string>, string, Func<string>>> CalculatedElements {
        ////    //get {
        ////    //    return new Dictionary<VirtualAccountCalculations, string>() {
        ////    //        {VirtualAccountCalculations.YCase1, CalculateForYCase1()}
        ////    //    };
        ////    //}
        ////    get; set; }

        ////private static Tuple<VirtualAccountCalculations, List<string>, string, Func<string>> x {
        ////    //get {
        ////    //    return new Tuple<VirtualAccountCalculations, List<string>, string, Func<string>>() {
        ////    //        VirtualAccountCalculations.
        ////    //    }
        ////    //}
        ////    get; set; }

        //private static void initCalculations() {
        //    //var cc =
        //    //    new Tuple<VirtualAccountCalculations, List<string>, string, Func<string>>(
        //    //        VirtualAccountCalculations.YCase1, new List<string>() {"xy", "dw"}, null, CalculateForYCase1);
        //    //CalculatedElements.Add(VirtualAccountCalculations.YCase1, cc);

        //    //foreach (Tuple<VirtualAccountCalculations, List<string>, string, Func<string>> tuple in CalculatedElements.Values) {
        //    //    tuple.Item4();
        //    //}
        //    CalculatedElements = new HashSet<VirtualAccountCalculated>();
        //    CalculatedElements.Add(new VirtualAccountCalculated()
        //                           {Identifier = VirtualAccountCalculations.YCase1, Func = CalculateForYCase1});
        //}



        //private static decimal TryGetElementValue(string element) {
        //    if(!Document.ValueTreeMain.Root.Values.ContainsKey(element))
        //        return 0;
        //    if (!Document.ValueTreeMain.Root.Values[element].HasValue)
        //        return 0;

        //    return Document.ValueTreeMain.Root.Values[element].DecimalValue;
        //}

        //private static string CalculateForYCase1() {
        //    string result = string.Empty;
        //    var y = CalculatedElements.FirstOrDefault(x => x.Identifier == VirtualAccountCalculations.YCase1);// .ToLookup() [VirtualAccountCalculations.YCase1].Item3 = result;
        //    if (y == null) {
        //        return null;
        //    }
        //    y.ReferencedTaxonomyPositions = new List<string>() {
        //        "de-gaap-ci_bs.ass.deficitNotCoveredByCapital.privateAccountSP.netIncome",
        //        "de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossUnlimitedLiablePartnerS.netIncome",
        //        "de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossLimitedLiablePartnerS.netIncome",
        //        "de-gaap-ci_bs.eqLiab.equity.subscribed.privateAccountSP.netIncome",
        //        "de-gaap-ci_bs.eqLiab.equity.subscribed.unlimitedLiablePartnerS.netIncome",
        //        "de-gaap-ci_bs.eqLiab.equity.subscribed.limitedLiablePartnerS.netIncome",
        //        "de-gaap-ci_bs.eqLiab.equity.netIncome",
        //        "de-gaap-ci_is.netIncome"
        //    };
            
        //    var number1 = TryGetElementValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.privateAccountSP.netIncome");
        //    // div
        //    var number2 =
        //        TryGetElementValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossUnlimitedLiablePartnerS.netIncome");
        //    // div
        //    var number3 =
        //        TryGetElementValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossLimitedLiablePartnerS.netIncome");
        //    // +
        //    var number4 = TryGetElementValue("de-gaap-ci_bs.eqLiab.equity.subscribed.privateAccountSP.netIncome");
        //    // +
        //    var number5 = TryGetElementValue("de-gaap-ci_bs.eqLiab.equity.subscribed.unlimitedLiablePartnerS.netIncome");
        //    // +
        //    var number6 = TryGetElementValue("de-gaap-ci_bs.eqLiab.equity.subscribed.limitedLiablePartnerS.netIncome");
        //    // +
        //    var number7 = TryGetElementValue("de-gaap-ci_bs.eqLiab.equity.netIncome");
        //    // =
        //    var shouldBe = TryGetElementValue("de-gaap-ci_is.netIncome");

        //    var calcResult = decimal.Round((number1 / number2 / number3 + number4 + number5 + number6 + number7), 2);

        //    result = calcResult + string.Empty;

        //    y.Value = result;
        //    return result;
        //}

	#endregion

        public static Document Document{ get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="progressInfo">at the moment not used, default = null</param>
        /// <param name="doAssignments">Assign the new created virtual accounts? (default=true)</param>
        /// <returns></returns>
        public static bool GenerateAndAssignVirtualBalanceList(Document document, ProgressInfo progressInfo = null, bool doAssignments = true) {
            Document = document;

            try {
                var tupleList = CreateAccounts(document);
                if (doAssignments) {
                    document.AddAccountAssignments(tupleList, null, BalanceList);
                    
                }
                return true;
            }
            catch (Exception) {
                return false;
                //throw;
            }
        }

        /// <summary>
        /// Formerly known as CreateTupelList. Creates all virtual accounts for all taxonomies.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>List of tuple that can be given to the methode document.AddAccountAssignments</returns>
        private static List<Tuple<IBalanceListEntry, string>> CreateAccounts(Document document = null) {
            Init(document);

            List<Tuple<IBalanceListEntry, string>> tupList = new List<Tuple<IBalanceListEntry, string>>();
            List<IAccount> accounts = new List<IAccount>();
            // create new virtual accounts for each position that is relevant for the current document
            foreach (var entry in CreateDefaultTaxonomyElements()) {

                var account = new VirtualAccount(entry.SourcePosition, BalanceList);
                tupList.Add(new Tuple<IBalanceListEntry, string>(account, entry.DefaultAssignmentPosition));
                account.DoDbUpdate = true;
                account.Save();
                accounts.Add(account);
            }
            BalanceList.SetEntries(accounts, null, null);
            return tupList;
        }

        ///// <summary>
        ///// This mehode will check if the virtual account for this elementId will be available.
        ///// </summary>
        ///// <param name="elementId">An KeyValuePair with the calculation source and assignment destination information.</param>
        ///// <param name="doc">The current document with loaded taxonomies.</param>
        ///// <returns>Should it be added to the VirtualBalanceList</returns>
        //private static bool CheckStatus(KeyValuePair<string, string> elementId, Document doc = null) {
        //    if (doc == null) {
        //        doc = Document;
        //    }
        //    if (doc == null) {
        //        return true;
        //    }
        //    // we need the document


        //    // Check the stuff that is at the moment just checked in the taxonomy validation

        //    // if Key is in taxonomy but value is not than unassign account
        //    if (doc.ValueTreeMain != null && (StaticCompanyMethods.IsRKV(doc) || StaticCompanyMethods.IsRVV(doc))) {
        //        return false;
        //    }

        //    return true;
        //}
        
        /// <summary>
        /// Create the new VirtualBalanceList.
        /// </summary>
        /// <param name="document"> </param>
        private static void Init(Document document = null) {
            BalanceList = new VirtualBalanceList {Document = document, DoDbUpdate = true};
            BalanceListManager.AddBalanceList(document, BalanceList);
        }
    }
    
    //List of Tuples that contain "Source", "Default Assignment Destination" and "Allowed Assignment Position" (= Start of the Element.Id) and "allowed assignment position verbal"
    public class VirtualAccountConfiguration {
        
        public VirtualAccountConfiguration(string source, string defaultAssignment, string allowedAssignmentPosition, string allowedAssignmentVerbal) {
            SourcePosition = source;
            DefaultAssignmentPosition = defaultAssignment;
            AllowedAssignmentPositions = new string[]{allowedAssignmentPosition};
            _allowedAssignmentPositionVerbal = allowedAssignmentVerbal;
        }
        
        public VirtualAccountConfiguration(string source, string defaultAssignment, IEnumerable<string> allowedAssignmentPositions, string allowedAssignmentVerbal) {
            SourcePosition = source;
            DefaultAssignmentPosition = defaultAssignment;
            AllowedAssignmentPositions = allowedAssignmentPositions;
            _allowedAssignmentPositionVerbal = allowedAssignmentVerbal;
        }
        
        public string SourcePosition;
        public string DefaultAssignmentPosition;
        public IEnumerable<string> AllowedAssignmentPositions;
        private string _allowedAssignmentPositionVerbal;

        public string AllowedAssignmentPositionVerbal { get { return ResourcesBalanceList.ResourceManager.GetString(_allowedAssignmentPositionVerbal); } set { _allowedAssignmentPositionVerbal = value; } }
    }
}