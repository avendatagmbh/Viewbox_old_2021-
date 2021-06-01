using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbSearchDatabase.Structures;

namespace DbSearchLogic.SearchCore.Structures {

    public class ConfigSearchParams : IConfigSearchParams {
        /// <summary>
        /// Konstruktor.
        /// </summary>
        public ConfigSearchParams(ConfigSearchParams parentParams = null) {
            ParentParams = parentParams;
            if (parentParams == null) {
                CaseIgnore = true;
                InStringSearch = true;
                StringSearch = false;
                SearchRoundedValues = false;
                NumericPrecision = 2;
                PrimaryInterpretationLanguage = "Englisch";
            }
            else {
                CaseIgnore = null;
                InStringSearch = null;
                StringSearch = null;
                SearchRoundedValues = null;
            }
        }

        //Used for columns which have the global table params as a parent
        private ConfigSearchParams ParentParams { get; set; }

        #region CaseIgnore
        private bool? _caseIgnore;
        public bool? CaseIgnore { get { return _caseIgnore; } set { _caseIgnore = value; } }
        public bool UseCaseIgnore {
            get {
                if (ParentParams != null && !_caseIgnore.HasValue) return ParentParams.UseCaseIgnore;
                return _caseIgnore != null && _caseIgnore.Value;
            }
            set { _caseIgnore = value; }
        }
        #endregion CaseIgnore

        #region InStringSearch
        /// <summary>
        /// Gibt an, ob eine exakte Suche verwendet werden soll.
        /// </summary>
        private bool? _inStringSearch;
        public bool? InStringSearch {get { return _inStringSearch; }set { _inStringSearch = value; } }
        public bool UseInStringSearch {
            get {
                if (ParentParams != null && !_inStringSearch.HasValue) return ParentParams.UseInStringSearch;
                return _inStringSearch != null && _inStringSearch.Value;
            }
            set { _inStringSearch = value; }
        }
        #endregion InStringSearch

        #region StringSearch
        /// <summary>
        /// Gibt an, ob andere Datentypen auch als Strings gesucht werden sollen.
        /// </summary>
        private bool? _stringSearch;
        public bool? StringSearch { get { return _stringSearch; } set { _stringSearch = value; } }
        public bool UseStringSearch {
            get {
                if (ParentParams != null && !_stringSearch.HasValue) return ParentParams.UseStringSearch;
                return _stringSearch != null && _stringSearch.Value;
            }
            set { _stringSearch = value; }
        }
        #endregion StringSearch

        #region PrimaryInterpretationLanguage
        /// <summary>
        /// Gibt die primäre Sprache für die Interpretation der Werte an.
        /// </summary>
        private string _primaryInterpretationLanguage;
        public string PrimaryInterpretationLanguage {
            get {
                if (ParentParams != null && _primaryInterpretationLanguage == null) return ParentParams.PrimaryInterpretationLanguage;
                return _primaryInterpretationLanguage;
            }
            set { _primaryInterpretationLanguage = value; }
        }
        #endregion  PrimaryInterpretationLanguage

        #region SearchRoundedValues
        /// <summary>
        /// Gets or sets a value indicating whether [search rounded values].
        /// </summary>
        /// <value><c>true</c> if [search rounded values]; otherwise, <c>false</c>.</value>
        private bool? _searchRoundedValues;
        public bool? SearchRoundedValues { get { return _searchRoundedValues; } set { _searchRoundedValues = value; } }
        public bool UseSearchRoundedValues {
            get {
                if (ParentParams != null && !_searchRoundedValues.HasValue) return ParentParams.UseSearchRoundedValues; 
                return _searchRoundedValues.HasValue && _searchRoundedValues.Value;
            }
            set {_searchRoundedValues = value; }
        }

        #endregion SearchRoundedValues

        #region NumericPrecision
        /// <summary>
        /// Gets or sets the numeric precision.
        /// </summary>
        /// <value>The numeric precision.</value>
        private int? _numericPrecision;
        public int NumericPrecision {
            get {
                if (ParentParams != null && !_numericPrecision.HasValue) return ParentParams.NumericPrecision;
                return _numericPrecision.Value;
            }
            set { _numericPrecision = value; }
        }

        #endregion NumericPrecision
        public StringComparison StringComparison { get { return UseCaseIgnore ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture; } }

        public DbConfigSearchParams ToDbSearchParams() {
            return new DbConfigSearchParams() {
                                                  CaseIgnore = CaseIgnore,
                                                  InStringSearch = InStringSearch,
                                                  SearchRoundedValues = SearchRoundedValues,
                                                  StringSearch = StringSearch
                                              };
        }

        public void ToDbSearchParams(DbConfigSearchParams dbParams) {
            dbParams.CaseIgnore = CaseIgnore;
            dbParams.InStringSearch = InStringSearch;
            dbParams.SearchRoundedValues = SearchRoundedValues;
            dbParams.StringSearch = StringSearch;
        }

        public void FromDbSearchParams(DbConfigSearchParams dbParams) {
            CaseIgnore = dbParams.CaseIgnore;
            InStringSearch = dbParams.InStringSearch;
            SearchRoundedValues = dbParams.SearchRoundedValues;
            StringSearch = dbParams.StringSearch;
        }

        public void FromXml(XmlReader reader) {
            DbConfigSearchParams dbParams = new DbConfigSearchParams();
            if(dbParams.FromXml(reader))
                FromDbSearchParams(dbParams);
        }

        public void ToXml(XmlWriter writer) {
            ToDbSearchParams().ToXml(writer);
        }
    }
}
