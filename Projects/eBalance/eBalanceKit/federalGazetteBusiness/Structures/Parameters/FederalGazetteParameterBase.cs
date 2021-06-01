// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    
    /// <summary>
    /// Abstract base class for parameters.
    /// Important: The parameters will be shown in the UI in the order they are defined in your child class.
    /// </summary>
    public abstract class FederalGazetteParameterBase : FederalGazetteElementList {
        
        protected FederalGazetteParameterBase() : base() {
            using (var conn = eBalanceKitBusiness.Structures.AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.CreateTableIfNotExists<DbElementValue>();
            }
        }

        protected
            ObservableCollectionAsync<IFederalGazetteElementInfo> OCompanyTypes = new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("1", "Kreditinstitut",
                                                    new List<string>()
                                                    {"genInfo.report.id.specialAccountingStandard.RKV"}),
                new FederalGazetteElementInfoString("2", "Pensionsfond"),
                new FederalGazetteElementInfoString("3", "Finanzierungsdienstleister"),
                new FederalGazetteElementInfoString("4", "Versicherung",
                                                    "genInfo.report.id.specialAccountingStandard.RVV"),
                new FederalGazetteElementInfoString("5", "Rückversicherungsgesellschaft"),
                new FederalGazetteElementInfoString("6", "keine der genannten Gesellschaftsarten",
                                                    new List<string>() {
                                                        "genInfo.report.id.specialAccountingStandard.K",
                                                        "genInfo.report.id.specialAccountingStandard.PBV",
                                                        "genInfo.report.id.specialAccountingStandard.KHBV",
                                                        "genInfo.report.id.specialAccountingStandard.EBV",
                                                        "genInfo.report.id.specialAccountingStandard.WUV",
                                                        "genInfo.report.id.specialAccountingStandard.VUV",
                                                        "genInfo.report.id.specialAccountingStandard.LUF"
                                                    })
            };

        /// <summary>
        /// Loads the values from the database and sets the ParameterArea. It also adds the Parameters to the <see cref="FederalGazetteElementList.Items"/>.
        /// </summary>
        protected void LoadEverything() {
            FederalGazetteElementInfoBase.DisableSaving = true;


            var propertyInfo = this.GetType().GetProperties();
            foreach (var info in propertyInfo) {
                var val = info.GetValue(this, null);
                if (val != null && val is IFederalGazetteElementInfo) {
                    (val as IFederalGazetteElementInfo).ParameterArea = ParameterType;
                    Add(val as IFederalGazetteElementInfo);
                } else {
                    //it's not a IFederalGazetteElementInfo so we don't need to do anything
                    //System.Diagnostics.Debug.Fail("failed");
                }
            }

            LoadValuesFromDatabase();
            OnPropertyChanged("Items");
            FederalGazetteElementInfoBase.DisableSaving = false;
        }

        public abstract Enum.ParameterArea ParameterType { get;}

        private void LoadValuesFromDatabase() {

            using (DbAccess.IDatabase conn = eBalanceKitBusiness.Structures.AppConfig.ConnectionManager.GetConnection()) {
                try {
                    if (eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument == null) {
                        System.Diagnostics.Debug.Fail("No document loaded!!!");
                        return;
                    }
                    var values = conn.DbMapping.Load<DbElementValue>(conn.Enquote("document_id") + "=" + eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.Id + " AND " + conn.Enquote("parameter_type") + "=" + conn.GetValueString(ParameterType.ToString()));
                    foreach (var dbElementValue in values) {
                        SetElementValue(dbElementValue, dbElementValue.ElementValue);
                    }
                }
                catch (Exception exception) {
                    eBalanceKitBase.Structures.ExceptionLogging.LogException(exception);
                    //throw;
                }
            }
        }

        #region helper

        protected abstract void Validate();

        protected bool HasValue(string propertyName) {
            var valueSet = GetElementValue(propertyName);
            return valueSet != null && !string.IsNullOrEmpty(valueSet.ToString());
        }

        protected bool HasValue(IFederalGazetteElementInfo element) {
            var valueSet = element.Value;
            return valueSet != null && !string.IsNullOrEmpty(valueSet.ToString());
        }

        protected bool IsValueSetTo(object valueSet, object value) {
            bool result = false;
            if ((value == null && valueSet != null) || (value != null && valueSet == null)) {
                return result;
            }
            result = (value == valueSet || value.ToString() == valueSet.ToString() || ((valueSet as IFederalGazetteElementInfo) != null && (valueSet as IFederalGazetteElementInfo).Value != null && value.ToString() == (valueSet as IFederalGazetteElementInfo).Value.ToString()));
            return result;
        }

        protected bool IsValueSetTo(string propertyName, object value) {
            if (value is IEnumerable<object>) {
                return IsValueSetTo(propertyName, value as IEnumerable<object>);
            }
            if (value is Array) {
                return IsValueSetTo(propertyName, (value as Array).Cast<object>());
            }
            var valueSet = GetElementValue(propertyName);
            return IsValueSetTo(valueSet, value);
        }

        protected bool IsValueSetTo(IFederalGazetteElementInfo element, object value) {
            if (value is IEnumerable<object>) {
                return IsValueSetTo(element, value as IEnumerable<object>);
            }
            if (value is Array) {
                return IsValueSetTo(element, (value as Array).Cast<object>());
            }
            var valueSet = element.Value;
            return IsValueSetTo(valueSet, value);
        }

        //protected bool IsValueSetToAny(IFederalGazetteElementInfo element, object[] values) {
        //    var valueSet = element.Value;
        //    bool result = false;
        //    foreach (var value in values) {
        //        result = IsValueSetTo(valueSet, value);
        //        if (result) {
        //            break;
        //        }
        //    }
        //    return result;
        //}

        protected bool IsValueSetTo(IFederalGazetteElementInfo element, IEnumerable<object> values) {
            var valueSet = element.Value;
            bool result = false;
            foreach (var value in values) {
                result = IsValueSetTo(valueSet, value);
                if (result) {
                    break;
                }
            }
            return result;
        }

        protected bool IsValueSetTo(string propertyName, IEnumerable<object> values) {
            var valueSet = GetElementValue(propertyName);
            bool result = false;
            foreach (var value in values) {
                result = IsValueSetTo(valueSet, value);
                if (result) {
                    break;
                }
            }
            return result;
        }

        protected object GetElementValue(string propertyName) {
            var result = this.GetType().GetProperty(propertyName).GetValue(this, null);
            return result;
        }

        protected void SetElementValue(DbElementValue dbValue, object value) {

            var propertyInfo = this.GetType().GetProperty(dbValue.ElementName);

            if (propertyInfo == null) {
                System.Diagnostics.Debug.Fail("propertyInfo == null");
                return;
            }
            
            
            var valueSet = propertyInfo.GetValue(this, null);
            if (valueSet is FederalGazetteElementSelectionBase) {
                var v = valueSet as FederalGazetteElementSelectionBase;
                v.SetDbValue(dbValue);
                v.SelectedOption =
                    v.Options.FirstOrDefault(
                        option => option.Value == value || (option.Value != null && value != null && option.Value.ToString() == value.ToString()));
                //v.SelectedOption = value == null ? null :
            }
            else {
                //System.Diagnostics.Debug.Fail("Check whats the dataType of valueSet");
                var v = (valueSet as FederalGazetteElementInfoBase);
                if (v != null) {
                    v.SetDbValue(dbValue);
                    v.Value = value;
                }
            }
        }

        protected void SetElementValue(string propertyName, object value) {
            var propertyInfo = this.GetType().GetProperty(propertyName);
            var valueSet = propertyInfo.GetValue(this, null);
            //if (value is IFederalGazetteElementInfo) {
            if (valueSet is FederalGazetteElementSelectionBase) {
                var v = valueSet as FederalGazetteElementSelectionBase;
                v.SelectedOption = value == null ? null :
                    v.Options.FirstOrDefault(
                        option => option.Value == value || option.Value.ToString() == value.ToString());

            }
            propertyInfo.SetValue(propertyInfo, value, null);
            //this.GetType().GetProperty(propertyName).SetValue(propertyInfo, value, null).GetValue(this, null);
        }

        #endregion




    }
}