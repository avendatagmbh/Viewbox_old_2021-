// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class SelectObjectModel : Utils.NotifyPropertyChangedBase {
        public SelectObjectModel(IEnumerable<object> objects, ObjectTypes objectType) {
            ObjectType = objectType;
            
            AvailableObjects = SetDefaults(objects.ToList());
            VisibleObjects = AvailableObjects;
        }

        private IEnumerable<object> SetDefaults(IEnumerable<object> availableObjects) {
            string placeHolder = ">>> {0} <<<";
            var result = availableObjects.ToList();

            switch (ObjectType) {
                case ObjectTypes.Company:
                    if(DefaultEntry == null) DefaultEntry = new eBalanceKitBusiness.Structures.DbMapping.Company() { Name = string.Format(placeHolder, ResourcesManamgement.DlgAddCompanyCaption) };
                    SelectedObject = DefaultEntry;
                    break;
                case ObjectTypes.System:
                    if (DefaultEntry == null) DefaultEntry = new eBalanceKitBusiness.Structures.DbMapping.System() { Name = string.Format(placeHolder, ResourcesManamgement.DlgAddSystemCaption) };
                    SelectedObject = DefaultEntry;
                    break;
                case ObjectTypes.FinancialYear:
                    SelectedObject = result.OfType<eBalanceKitBusiness.Structures.DbMapping.FinancialYear>().FirstOrDefault
                            (fy => fy.FYear == DateTime.UtcNow.Year);
                    break;
                default:
                    DefaultEntry = "Neues unbekanntest Objekt";
                    Debug.Fail("Unknown object");
                    break;
            }

            OnPropertyChanged("DefaultEntry");
            OnPropertyChanged("SelectedObject");

            if (DefaultEntry != null && result != null && !result.Contains(DefaultEntry)) {
                result.Add(DefaultEntry);

                return
                    result.OrderBy(delegate(object obj) {
                        if (obj is INamedObject)
                            return (obj as INamedObject).Name;
                        if (obj is eBalanceKitBusiness.Structures.DbMapping.FinancialYear)
                            return
                                (obj as eBalanceKitBusiness.Structures.DbMapping.FinancialYear).FYear.ToString(
                                    CultureInfo.InvariantCulture);
                        return obj;
                    }).ToList();
                //availableObjects.OrderBy(
                //    obj =>
                //    (obj is INamedObject)
                //        ? (obj as INamedObject).Name
                //        : (obj as eBalanceKitBusiness.Structures.DbMapping.FinancialYear).FYear.ToString(
                //            CultureInfo.InvariantCulture)).ToList();
            }

            return result;
        }

        public bool EnableCondition { get { return true; } }

        #region AvailableObjects
        private IEnumerable<object> _availableObjects;

        public IEnumerable<object> AvailableObjects {
            get { return _availableObjects; }
            set {
                if (_availableObjects != value) {
                    _availableObjects = SetDefaults(value.ToList());
                    OnPropertyChanged("AvailableObjects");
                    OnPropertyChanged("DisplayMemberPath");
                    OnPropertyChanged("IsCreateNewObjectSelected");
                }
            }
        }
        #endregion AvailableObjects


        public IEnumerable<object> VisibleObjects { get; set; }

        #region SelectedObject
        private object _selectedObject;

        public object SelectedObject {
            get { return _selectedObject; }
            set {
                if (_selectedObject != value) {
                    _selectedObject = value;
                    OnPropertyChanged("SelectedObject");
                    OnPropertyChanged("IsCreateNewObjectSelected");
                }
            }
        }
        #endregion SelectedObject

        public ObjectTypes ObjectType { get; set; }
        public string ObjectTypeString { get { return ResourcesMain.ResourceManager.GetString(ObjectType.ToString()); } }

        public object DefaultEntry { get; set; }
        public string DisplayMemberPath {
            get { return ObjectType == ObjectTypes.FinancialYear ? "FYear" : "Name"; }
        }


        public bool IsCreateNewObjectSelected { get { return SelectedObject == DefaultEntry; } }
    }
}