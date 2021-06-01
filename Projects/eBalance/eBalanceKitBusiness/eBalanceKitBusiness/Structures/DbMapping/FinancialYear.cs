// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-12-28
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Globalization;
using DbAccess;
using DbAccess.Structures;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures.DbMapping {
    /// <summary>
    /// This class represents an available document period.
    /// </summary>
    [DbTable("financial_years", Description = "all avaliable financial years", ForceInnoDb = true)]
    public class FinancialYear : INotifyPropertyChanged, IComparable {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region properties

        #region Id
        private int _id;

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id {
            get { return _id; }
            set {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        #endregion

        #region Company
        [DbColumn("company_id", AllowDbNull = false, IsInverseMapping = true)]
        public Company Company { get; set; }
        #endregion

        #region FYear
        private int _fyear;

        [DbColumn("fyear", AllowDbNull = true, Length = 4)]
        public int FYear {
            get { return _fyear; }
            set {
                if (_fyear == value) return;
                _fyear = value;
                OnPropertyChanged("FYear");
            }
        }
        #endregion

        #region IsEnabled
        [DbColumn("is_enabled", AllowDbNull = true)]
        public bool IsEnabled { get; set; }
        #endregion

        #region FiscalYearBegin
        private DateTime? _fiscalYearBegin;

        [DbColumn("fiscal_year_begin", AllowDbNull = true)]
        public DateTime? FiscalYearBegin {
            get { return _fiscalYearBegin; }
            set {
                _fiscalYearBegin = value;
                if (Successor != null) Successor.FiscalYearBeginPrevious = value;
                OnPropertyChanged("FiscalYearBegin");
            }
        }
        #endregion

        #region FiscalYearEnd
        private DateTime? _fiscalYearEnd;

        [DbColumn("fiscal_year_end", AllowDbNull = true)]
        public DateTime? FiscalYearEnd {
            get { return _fiscalYearEnd; }
            set {
                _fiscalYearEnd = value;
                if (Successor != null) Successor.FiscalYearEndPrevious = value;
                OnPropertyChanged("FiscalYearEnd");
            }
        }
        #endregion

        #region BalSheetClosingDate
        private DateTime? _balSheetClosingDate;

        [DbColumn("bal_sheet_closing_date", AllowDbNull = true)]
        public DateTime? BalSheetClosingDate {
            get { return _balSheetClosingDate; }
            set {
                _balSheetClosingDate = value;
                if (Successor != null) Successor.BalSheetClosingDatePreviousYear = value;
                OnPropertyChanged("BalSheetClosingDate");
            }
        }
        #endregion

        #region FiscalYearBeginPrevious
        private DateTime? _fiscalYearBeginPrevious;

        /// <summary>
        /// Gets or sets the begin of the previous fiscal year. This property will 
        /// be automatically set on update/load of the respective actual property.
        /// </summary>
        /// <value>The begin of the previous fiscal year.</value>
        public DateTime? FiscalYearBeginPrevious {
            get { return _fiscalYearBeginPrevious; }
            set {
                _fiscalYearBeginPrevious = value;
                OnPropertyChanged("FiscalYearBeginPrevious");
            }
        }
        #endregion

        #region FiscalYearEndPrevious
        private DateTime? _fiscalYearEndPrevious;

        /// <summary>
        /// Gets or sets the end of the previous fiscal year. This property will 
        /// be automatically set on update/load of the respective actual property.
        /// </summary>
        /// <value>The end of the previous fiscal year.</value>
        public DateTime? FiscalYearEndPrevious {
            get { return _fiscalYearEndPrevious; }
            set {
                _fiscalYearEndPrevious = value;
                OnPropertyChanged("FiscalYearEndPrevious");
            }
        }
        #endregion

        #region BalSheetClosingDatePreviousYear
        private DateTime? _balSheetClosingDatePreviousYear;

        /// <summary>
        /// Gets or sets the closing date of the balance sheet in the previous year. This property 
        /// will be automatically set on update/load of the respective actual property.
        /// </summary>
        /// <value>The closing date of the balance sheet of the previous year.</value>
        public DateTime? BalSheetClosingDatePreviousYear {
            get { return _balSheetClosingDatePreviousYear; }
            set {
                _balSheetClosingDatePreviousYear = value;
                OnPropertyChanged("BalSheetClosingDatePreviousYear");
            }
        }
        #endregion

        #region BalSheetClosingDateString
        /// <summary>
        /// Gets the bal sheet closing date string.
        /// </summary>
        /// <value>The bal sheet closing date string.</value>
        public string BalSheetClosingDateString {
            get {
                return BalSheetClosingDate.HasValue ? BalSheetClosingDate.Value.ToString("dd.MM.yyyy") : "";
            }
        }
        #endregion

        #region Successor
        public FinancialYear Successor { get; set; }
        #endregion

        #region Predecessor
        public FinancialYear Predecessor { get; set; }
        #endregion

        #region DisplayString
        public string DisplayString { get { return FYear.ToString(); } }
        #endregion // DisplayString

        #endregion properties

        #region methods

        #region CompareTo
        public int CompareTo(object obj) {
            if (!(obj is FinancialYear)) return 0;
            return FYear.CompareTo(((FinancialYear) obj).FYear);
        }
        #endregion

        public override string ToString() {
            return FYear.ToString() + ", enabled: " + IsEnabled;
        }

        public void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(this);
            }
        }
        #endregion methods
    }
}