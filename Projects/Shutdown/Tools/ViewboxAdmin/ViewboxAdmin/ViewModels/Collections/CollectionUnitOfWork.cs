using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Collections
{
    public class CollectionUnitOfWork : NotifyBase, ICollectionsUnitOfWork
    {

        #region Constructor
        public CollectionUnitOfWork(IParameterEditor parameterValueEditor) {
            //inject dependencies
            this.ParameterValueEditor = parameterValueEditor;
            //create collections
            this.DirtyItems = new List<ICollectionModel>();
            this.NewItems = new List<Tuple<ICollectionModel, IParameterModel>>();
            this.DeletedItems = new List<ICollectionModel>();
        }
        #endregion Constructor

        #region Properties
        public IParameterEditor ParameterValueEditor { get; private set; }
        public List<ICollectionModel> DirtyItems { get; set; }
        public List<Tuple<ICollectionModel, IParameterModel>> NewItems { get; set; }
        public List<ICollectionModel> DeletedItems { get; set; }
        

        #region DebugMessage
        private string _DebugMessage;

        public string DebugMessage {
            get { return _DebugMessage; }
            set {
                if (_DebugMessage != value) {
                    _DebugMessage = value;
                    OnPropertyChanged("DebugMessage");
                }
            }
        }
        #endregion DebugMessage
        #endregion

        #region Public methods
        public void MarkAsDirty(ICollectionModel cm) {
            if (ObjectCanBeRegisteredDirty(cm))
            {
                AddDebugMessage("The item with hash :" + cm.GetHashCode() + " marked as dirty");
                DirtyItems.Add(cm);
            }
        }

        public void MarkAsNew(Tuple<ICollectionModel,IParameterModel> collparmap ) {
            AddDebugMessage("The item with hash :" + collparmap.GetHashCode() + " was marked as new");
            NewItems.Add(collparmap);
        }

        public void MarkAsDeleted(ICollectionModel cm) {
            if (NewItems.RemoveAll(a => a.Item1 == cm) != 0) return;
            AddDebugMessage("The item with hash :" + cm.GetHashCode() + "  was marked as deleted");
            DeletedItems.Add(cm);
        }

        public void Commit() {
            AddDebugMessage("Starting database manipulations...");
            AddDebugMessage("Items to be deleted: " + DeletedItems.Count);
            AddDebugMessage("Items to be created: " + NewItems.Count);
            AddDebugMessage("Items to be updated: " + DirtyItems.Count);
            //delete items
            foreach (var collectionModel in DeletedItems) {
                try {
                    ParameterValueEditor.Delete(collectionModel);
                    }
                catch(Exception ex) {
                    AddDebugMessage("Exception during deleting item: "+ex.Message);
                    if(collectionModel.WrappedParamValue ==null) {
                        AddDebugMessage("Probably you wanted to delete a newly created item. Please reload the current profile to delete that item");
                    }
                }
            }

            //create new items
            foreach (var collectionModel in NewItems)
            {
                try {
                    ParameterValueEditor.CreateNew(collectionModel.Item1, collectionModel.Item2);
                }
                catch(Exception e) {
                    AddDebugMessage("Exception during creating item: "+e.Message);
                }
            }

            //update items
            foreach (var collectionModel in DirtyItems)
            {
                try {
                    ParameterValueEditor.Update(collectionModel);
                }
                catch(Exception ex) {
                    AddDebugMessage("Exception during updating item: "+ex.Message);
                }
            }

            DirtyItems.Clear();
            DeletedItems.Clear();
            NewItems.Clear();
            AddDebugMessage("Commiting finished");
        }

        public void Rollback() {
            throw new NotImplementedException();
        }

        #endregion Public methods

        #region Private methods

        private void AddDebugMessage(string message) { DebugMessage += message + (Environment.NewLine); }

        private bool ObjectCanBeRegisteredDirty(ICollectionModel collectionModel) {
            return this.ObjectNotDeleted(collectionModel)
                && this.ObjectNotDirty(collectionModel)
                && this.ObjectNotNew(collectionModel);
        }

        private bool ObjectNotDirty(ICollectionModel collectionModel) {
            return !DirtyItems.Contains(collectionModel);
        }

        private bool ObjectNotDeleted(ICollectionModel collectionModel) {
            return !DeletedItems.Contains(collectionModel);
        }

        private bool ObjectNotNew(ICollectionModel colllectionModel) {
            return null == NewItems.Find(a => a.Item1 == colllectionModel);
        }

        #endregion Private methods

    }
}
