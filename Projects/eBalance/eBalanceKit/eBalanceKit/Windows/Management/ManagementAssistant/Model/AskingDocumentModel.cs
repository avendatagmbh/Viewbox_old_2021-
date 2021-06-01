// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-04
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.Commands;
using eBalanceKit.Windows.Management.ManagementAssistant.Add;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingDocumentModel : AskingModelBase, IAskingModel {

        //public AskingDocumentModel(Window window)
        //    : base(window) {
        //    CmdNo  = new DelegateCommand(o => true, o => No());
        //    CmdYes= new DelegateCommand(o => true, o => Yes());
        //    ObjectType = ObjectTypes.Report;
        //}

        public AskingDocumentModel(Window window, IEnumerable<INamedObject> objectCollection):base(window) {
            // ToDo handle objectCollection (filtered documents)
            CmdNo  = new DelegateCommand(o => true, o => No());
            CmdYes= new DelegateCommand(o => true, o => Yes());
            ObjectType = ObjectTypes.Report;
            _objectCollection = objectCollection;
            //if (_objectCollection.Any()) {
            //    AddReport();
            //}
        }

        private IEnumerable<INamedObject> _objectCollection;

        #region Implementation of IAskingModel
        public DelegateCommand CmdYes { get; set; }

        public DelegateCommand CmdNo { get; set; }

        public override sealed ObjectTypes ObjectType { get; set; }
        public string Question { get { return _objectCollection.Any() ? QuestionIfExisting: QuestionGenerating; } }

        public new string QuestionIfExisting {
            get {
                return
                    string.Format(
                        ResourcesManamgement.QuestionExistingReport,
                        ResourcesMain.ResourceManager.GetString(ObjectType.ToString()));
            }
        }

        #endregion

        private void Yes() {
            if (_objectCollection == null || !_objectCollection.Any()) {
                AddReport();
            } else {
                SelectReport();
            }
        }

        private void No() {
            if (_objectCollection != null && _objectCollection.Any()) {
                AddReport();
            } else {
                Result = null;
            }
        }

        private void SelectReport() {
            DlgSelectObject dlg = new DlgSelectObject();
            dlg.DataContext = new Model.SelectionModel(dlg, _objectCollection, ObjectTypes.Report);

            ShowWindow(dlg);
        }

        public Add.Models.AddObjectModel AddObjectModel { get; set; }

        private void AddReport() {
            DlgAddReport dlg = new DlgAddReport();
            AddObjectModel = new Add.Models.AddObjectModel(dlg, ObjectTypes.Report);
            dlg.DataContext = AddObjectModel;

            ShowWindow(dlg);
        }
    }
}