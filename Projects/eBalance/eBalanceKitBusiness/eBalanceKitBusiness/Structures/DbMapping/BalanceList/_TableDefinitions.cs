using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using eBalanceKitBusiness.Manager;
using System.Xml;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {

    /*
     * Table and column definitions for balance list depending tables.
     * author: Mirko Dibbert
     */

    #region BalanceList
    [DbTable("balance_lists", Description = "imported balance lists", ForceInnoDb = true)]
    internal partial class BalanceList {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region Document
        [DbColumn("document_id", IsInverseMapping = true)]
        public Document Document { get; set; }
        #endregion

        #region Comment
        public string _comment;

        [DbColumn("comment", Length = 1024, AllowDbNull = true)]
        public string Comment {
            get { return _comment; }
            set {
                _comment = value;
                Save();

                OnPropertyChanged("Comment");
            }
        }
        #endregion

        #region Name
        [DbColumn("name", Length = 40, AllowDbNull = true)]
        public string Name {
            get { return _name; }
            set {
                if (value != null && value.Length > 40) _name = value.Substring(0, 39);
                else _name = value;
                Save();

                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayString");
            }
        }

        private string _name;
        #endregion

        #region ImportedFromId
        [DbColumn("imported_from_id")]
        public int ImportedFromId {
            get { return _importedFromId; }
            set {
                if (_importedFromId != value) {
                    _importedFromId = value;
                    if (_importedFromId > 0) {
                        ImportedFrom = UserManager.Instance.GetUser(_importedFromId);
                    }
                }
            }
        }

        private int _importedFromId;
        #endregion

        #region ImportDate
        [DbColumn("import_date")]
        public DateTime? ImportDate {
            get { return _importDate; }
            set {
                if (_importDate != value) {
                    _importDate = value;
                    OnPropertyChanged("ImportDate");
                    OnPropertyChanged("ImportDescription");
                }
            }
        }

        private DateTime? _importDate;
        #endregion

        #region Source
        /// <summary>
        /// Name of the source file.
        /// </summary>
        [DbColumn("source", Length = 2048)]
        public string Source {
            get { return _source; }
            set {
                if (_source != value) {
                    _source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        private string _source;
        #endregion

        public string ToXml() {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("Bl");
            doc.AppendChild(root);
            XmlAttribute attr = doc.CreateAttribute("Name");
            attr.InnerText = Name;
            root.Attributes.Append(attr);
            attr = doc.CreateAttribute("Comment");
            attr.InnerText = Comment;
            root.Attributes.Append(attr);
            attr = doc.CreateAttribute("Source");
            attr.InnerText = Source;
            root.Attributes.Append(attr);
            return doc.OuterXml;
        }
    }
    #endregion BalanceList
       
    #region Account
    [DbTable("accounts", Description = "imported balance list accounts", ForceInnoDb = true)]
    internal partial class Account : BalanceListEntryBase {
        #region Amount
        [DbColumn("amount", AllowDbNull = false)]
        public override decimal Amount {
            get { return _amount; }
            set {
                _amount = value;
                if (DoDbUpdate) Save();
                OnPropertyChanged("Amount");
            }
        }
        public decimal _amount;
        #endregion

        #region AccountGroup
        [DbColumn("group_id", AllowDbNull = true, IsInverseMapping = true)]
        public long AccountGroupId { get; set; }
        #endregion

        #region IsUserDefined
        [DbColumn("user_defined", AllowDbNull = false)]
        public bool IsUserDefined { get; set; }
        #endregion

    }
    #endregion Account

    
    #region AccountGroup
    [DbTable("account_groups", ForceInnoDb = true)]
    internal partial class AccountGroup : BalanceListEntryBase {
        #region Number
        public override string Number {
            get { return base.Number; }
            set {
                base.Number = value;
                ValidateNumber();
            }
        }
        #endregion

        #region Name
        public override string Name {
            get { return base.Name; }
            set {
                base.Name = value;
                ValidateName();
            }
        }
        #endregion
    }
    #endregion AccountGroup

    
    #region SplittedAccount
    [DbTable("splitted_accounts", Description = "splitted balance list accounts", ForceInnoDb = true)]
    internal partial class SplittedAccount : BalanceListEntryBase {        
        #region Amount
        [DbColumn("amount", AllowDbNull = false)]
        public override decimal Amount {
            get { return _amount; }
            set {
                _amount = Math.Round(Convert.ToDecimal(value), 2, MidpointRounding.AwayFromZero);
                if (SplitAccountGroup != null) (SplitAccountGroup as SplitAccountGroup).RecomputateAmountSum();
                if (DoDbUpdate) Save();

                OnPropertyChanged("ValueDisplayString");
                OnPropertyChanged("Amount");

                Validate();
            }
        }

        public decimal _amount;
        #endregion

        #region AmountPercent
        [DbColumn("amount_percent", AllowDbNull = true)]
        public decimal? AmountPercent {
            get { return _amountPercent; }
            set {
                if (value.HasValue) _amountPercent = Math.Round(value.Value, 8, MidpointRounding.AwayFromZero);
                else _amountPercent = null;

                // update percent sum
                if (SplitAccountGroup != null && _amountPercent.HasValue) {
                    
                    // limit percent value
                    if (_amountPercent.Value < 0) _amountPercent = 0;
                    else if (_amountPercent.Value > 100) _amountPercent = 100;

                    ComputeAmount();

                    (SplitAccountGroup as SplitAccountGroup).RecomputatePercentSum();
                }

                OnPropertyChanged("AmountPercent");
            }
        }
        private decimal? _amountPercent;
        #endregion AmountPercent

        #region SplitAccountGroupId
        [DbColumn("split_group_id", Length = 1, AllowDbNull = true)]
        public long SplitAccountGroupId { get; private set; }
        #endregion

        #region Number
        public override string Number {
            get { return base.Number; }
            set {
                base.Number = value;
                Validate();
            }
        }
        #endregion

        #region Name
        public override string Name {
            get { return base.Name; }
            set {
                base.Name = value;
                Validate();
            }
        }
        #endregion

    }
    #endregion SplittedAccount

    
    #region SplitAccountGroup
    [DbTable("split_account_groups", ForceInnoDb = true)]
    internal partial class SplitAccountGroup {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id {
            get { return _id; }
            set { 
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        internal long _id;
        #endregion

        #region BalanceList
        [DbColumn("balance_list_id", AllowDbNull = false, IsInverseMapping = true)]
        public BalanceList BalanceList { get; set; }
        #endregion

        #region Account
        [DbColumn("account_id", AllowDbNull = false)]
        public long AccountId { get; set; }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", AllowDbNull = true, Length = 1000)]
        public string Comment {
            get { return _comment; }
            set {
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        #endregion

        #region ValueInputMode
        ValueInputMode _valueInputMode = ValueInputMode.Absolute;

        [DbColumn("value_input_mode")]
        public ValueInputMode ValueInputMode {
            get { return _valueInputMode; }
            set {
                _valueInputMode = value;
                OnPropertyChanged("ValueInputMode");
                switch (_valueInputMode) {
                    case DbMapping.BalanceList.ValueInputMode.Absolute:
                        // reset percent values
                        //foreach (var item in Items)
                        //    (item as SplittedAccount).AmountPercent = null;

                        // calculate the absolute ammounts from the relative one
                        foreach (SplittedAccount account in Items) {
                            if (account.AmountPercent.HasValue) {
                                // compute relative amount value
                                var tmp = Account.Amount * account.AmountPercent.Value / 100;
                                decimal amount = Math.Round(tmp, 2, MidpointRounding.AwayFromZero);

                                if (account.CorrectionValue.HasValue) amount += (decimal)account.CorrectionValue.Value / 100;

                                account.Amount = amount;
                            }
                        }

                        RecomputatePercentSum();
                        foreach (var item in Items) item.Validate();
                        Validate();
                        break;

                    case DbMapping.BalanceList.ValueInputMode.Relative:
                        // reset absolute values
                        //foreach (var item in Items)
                        //    (item as SplittedAccount).Amount = 0;


                        // calculate the relative ammounts from the relative absolute one
                        foreach (SplittedAccount account in Items) {
                            // compute relative amount value
                            var amountPercent = account.Amount * 100 / Account.Amount;
                            amountPercent = Math.Round(amountPercent, 2, MidpointRounding.AwayFromZero);

                            var tmp = Account.Amount * amountPercent / 100;
                            decimal amount = Math.Round(tmp, 2, MidpointRounding.AwayFromZero);

                            if (account.Amount != amount) {
                                account.CorrectionValue = Convert.ToInt32((account.Amount*100) - (amount*100));
                            }

                            account.AmountPercent = amountPercent;
                        }

                        RecomputateAmountSum();
                        foreach (var item in Items) item.Validate();
                        Validate();
                        break;
                }
            }
        }
        #endregion
    }
    #endregion SplitAccountGroup
}
