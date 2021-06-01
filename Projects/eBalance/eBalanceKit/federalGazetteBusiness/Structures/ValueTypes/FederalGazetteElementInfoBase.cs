using System;
using System.Collections.Generic;
using System.Diagnostics;
using DbAccess;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;

namespace federalGazetteBusiness.Structures.ValueTypes
{
    //[DbTable("federalGazetteElementValues")]
    public abstract class FederalGazetteElementInfoBase : Utils.NotifyPropertyChangedBase, IFederalGazetteElementInfo {
        private FederalGazetteElementInfoBase() {
            TaxonomyElements = new List<string>();
            IsVisible = true;
        }

        protected FederalGazetteElementInfoBase(string id, string elementName, bool isAllowed = true) : this() {
            Id = id;
            //Caption = elementName;
            ElementName = elementName;
            IsAllowed = isAllowed;
        }

        protected FederalGazetteElementInfoBase(string id, string elementName, string taxonomyElementId, bool isAllowed = true)
            : this(id, elementName, isAllowed) {
            TaxonomyElements = new List<string>{taxonomyElementId};
        }

        protected FederalGazetteElementInfoBase(string id, string elementName, IEnumerable<string> taxonomyElementIds, bool isAllowed = true)
            : this(id, elementName, isAllowed) {
            TaxonomyElements = taxonomyElementIds;
        }

        public abstract FederalGazetteElementType Type { get; }

        //[DbColumn("Id", AllowDbNull = true)]
        public string Id { get; set; }

        public virtual string Caption {
            get {
                return
                    eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.ResourceManager.GetString("Parameter_" +
                                                                                                        ElementName) ??
                    "caption for " + ElementName + " not yet available in the ResourcesFederalGazette";
            }
        }

        public string ElementName { get; private set; }

        //[DbColumn("DocumentId", AllowDbNull = false)]
        //public int DocumentId { get; set; }



        #region IsNullable
        private bool _isNullable;

        public bool IsNullable {
            get { return _isNullable; }
            set {
                if (_isNullable != value) {
                    _isNullable = value;
                    OnPropertyChanged("IsNullable");
                }
            }
        }
        #endregion // IsNullable

        #region Value
        private object _value;
        //[DbColumn("value", AllowDbNull = true)]
        public object Value {
            get { return _value; }
            set {
                if (_value != value) {
                    _value = value;
                    OnPropertyChanged("Value");
                    SaveValue();
                }
            }
        }
        #endregion // Value

        #region IsAllowed
        private bool _isAllowed;

        public bool IsAllowed {
            get { return _isAllowed; }
            set {
                if (_isAllowed != value) {
                    _isAllowed = value;
                    OnPropertyChanged("IsAllowed");
                }
            }
        }
        #endregion // IsAllowed

        #region IsVisible
        private bool _isVisible;

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }
        #endregion // IsVisible

        public static bool DisableSaving { get; set; }

        public Enum.ParameterArea ParameterArea { get; set; }

        public IEnumerable<string> TaxonomyElements { get; set; }

        public override string ToString() { return ElementName; }

        private DbElementValue DbValue { get; set; }

        public void SetDbValue(DbElementValue elementValue) { DbValue = elementValue; }

        private void SaveValue(object value, bool useTransaction = false) {
            //eBalanceKitBusiness.Manager.ValueManager.SaveValues(new[]{value});

            if (DisableSaving) {
                return;
            }

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                if (useTransaction) {
                    conn.BeginTransaction();
                }
                try {
                    conn.DbMapping.Save(value);
                    if (useTransaction && conn.HasTransaction()) {
                        conn.CommitTransaction();
                    }
                }
                catch (Exception ex) {
                    if (useTransaction && conn.HasTransaction()) {
                        conn.RollbackTransaction();
                    }
                    throw new Exception(ex.Message, ex);
                }
            }


            //using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
            //    try {
            //        Debug.Assert(value != null, "value != null");
            //        conn.DbMapping.Save(value);

            //        //if (tuple.Item2 != null) LogManager.Instance.UpdateValue(tuple.Item2);

            //    }
            //    catch (Exception ex) {
            //        Debug.Fail("exception: " + ex.Message);
            //        ExceptionLogging.LogException(ex);
            //        //OnError(ex.Message);
            //        // TODO: handle error event
            //    }
            //}
        }

        /// <summary>
        /// Saves the current Value in the database (DbValue).
        /// Is overridden for <see cref="FederalGazetteElementInfoString"/> because we don't need to save this values for now.
        /// </summary>
        protected virtual void SaveValue() {

            if (DisableSaving) {
                return;
            }
            if (DbValue == null) {
                DbValue = new DbElementValue() { ElementName = ElementName, ElementValue = Value == null ? null : Value.ToString(), DocumentId = eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.Id, ParameterType = ParameterArea.ToString()};
                
                //seems like it's a new DataSet that not has been saved yet
                //Debug.Fail("DbValue was NULL");
                //return;
            }
            DbValue.ElementValue = Value == null ? null : Value.ToString();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                
                try {
                    conn.DbMapping.Save(DbValue);
                }
                catch (Exception ex) {
                    
                    throw new Exception(ex.Message, ex);
                }
            }
        }

    }
}