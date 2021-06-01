// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Commands;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class SelectionModel : Utils.NotifyPropertyChangedBase {
        //public SelectionModel(IEnumerable<INamedObject> objects) {
        //    Objects = objects;
        //    VisibleObjects = objects;
        //}

        public SelectionModel(System.Windows.Window owner, IEnumerable<INamedObject> objects, ObjectTypes objectType) {
            Objects = objects;
            VisibleObjects = Objects;
            Owner = owner;
            CmdOk = new DelegateCommand(o => true, o => { Owner.DialogResult = true; Owner.Close();  });
            ObjectType = objectType;
        }

        public IEnumerable<INamedObject> Objects { get; set; }
        public System.Windows.Window Owner { get; set; }
        private ObjectTypes ObjectType { get; set; }
        public string WindowTitle {
            get {
                //string type = "";
                //switch(ObjectType) {
                //    case ObjectTypes.System:
                //        type = "System";
                //        break;
                //    case ObjectTypes.Company:
                //        type = "Unternehmen";
                //        break;
                //    case ObjectTypes.Report:
                //        type = "Bericht";
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
                return string.Format(ResourcesManamgement.SelectionOfAvailable, ResourcesMain.ResourceManager.GetString(ObjectType.ToString()));
            }
        }

        #region SelectedObject
        private INamedObject _selectedObject;

        public INamedObject SelectedObject {
            get { return _selectedObject; }
            set {
                if (_selectedObject != value) {
                    _selectedObject = value;
                    OnPropertyChanged("SelectedObject");
                }
            }
        }
        #endregion SelectedObject

        #region VisibleObjects
        private IEnumerable<INamedObject> _visibleObjects;

        public IEnumerable<INamedObject> VisibleObjects {
            get { return _visibleObjects; }
            set {
                if (_visibleObjects != value) {
                    _visibleObjects = value;
                    OnPropertyChanged("VisibleObjects");
                }
            }
        }
        #endregion VisibleObjects

        #region Filter
        private string _filter;

        public string Filter {
            get { return _filter; }
            set {
                if (_filter != value.ToLower()) {
                    _filter = value.ToLower();
                    OnPropertyChanged("Filter");

                    VisibleObjects = (from obj in Objects
                                      where
                                       obj.Name != null && obj.Name.ToLower().Contains(_filter)
                                      select obj).ToList();
                }
            }
        }
        #endregion Filter

        protected void GoForward() {
            var dlgManagementAssistant = Utils.UIHelpers.TryFindParent<DlgManagementAssistant>(Owner);
            if (dlgManagementAssistant != null)
                dlgManagementAssistant.assistantControl.NavigateNext();
        }

        public DelegateCommand CmdOk { get; set; }
    }
}