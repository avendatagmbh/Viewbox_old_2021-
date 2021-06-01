// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DbAccess;
using Utils;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping {
    [DbTable("systems", ForceInnoDb = true)]
    public class System : NotifyPropertyChangedBase, IComparable, ILoggableObject, INamedObject {

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        #region events
        public event LogHandler NewLog;
        private void OnNewLog(string element, object oldValue, object newValue) { if (!IsTempObject && NewLog != null) NewLog(this, new LogArgs(element, oldValue, newValue)); }
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

        #region Name
        private string _name;

        [DbColumn("name", AllowDbNull = true, Length = 128)]
        [DbUniqueKey]
        public string Name {
            get { return _name; }
            set {
                var val = value == null ? null : value.Trim();
                if (_name == val) return;
                
                // log changed value
                OnNewLog("Name", _name, val);

                _name = StringUtils.Left(val, 128); ;
                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", AllowDbNull = true, Length = 256)]
        public string Comment {
            get { return _comment; }
            set {
                var val = value == null ? null : value.Trim();
                if (_comment == val) return;

                // log changed value
                OnNewLog("Comment", _comment, val);

                _comment = StringUtils.Left(value, 512);
                OnPropertyChanged("Comment");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region IsTempObject
        private bool _isTempObject;

        /// <summary>
        /// Indicates if the instance has been created with the CreateTempObject method. Temp objects will not be persistated to database or create any log entries.
        /// </summary>
        public bool IsTempObject {
            get { return _isTempObject; }
            private set {
                if (_isTempObject == value) return;
                _isTempObject = value;
                OnPropertyChanged("IsTempObject");
            }
        }
        #endregion // IsTempObject

        #region DisplayString
        public string DisplayString {
            get {
                if (string.IsNullOrEmpty(Name)) return "<" + ResourcesCommon.EmptyName + ">";
                string result = Name;
                if (!string.IsNullOrEmpty(Comment)) result += " (" + Comment + ")";
                return result;
            }
        }
        #endregion

        #region ValidationErrorMessages
        private readonly ObservableCollectionAsync<string> _validationErrorMessages = new ObservableCollectionAsync<string>();
        public IEnumerable<string> ValidationErrorMessages { get { return _validationErrorMessages; } }
        #endregion

        #region IsValid
        private bool _isValid;
        public bool IsValid {
            get { return _isValid; }
            private set {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        #endregion

        #region HasAnyAssignedDocument
        private bool _hasAnyAssignedDocument = false;
        public bool HasAnyAssignedDocument {
            get {
                return _hasAnyAssignedDocument;
            }
        }

        #endregion
        #endregion properties

        #region methods

        public int CompareTo(object obj) {
            if (!(obj is System)) return 0;
            var system = (System) obj;
            return Name == null
                       ? (system.Name == null ? 0 : 1)
                       : (system.Name == null ? -1 : Name.CompareTo(system.Name));
        }

        public override string ToString() { return Name; }

        public void Validate() {
            _validationErrorMessages.Clear();
            if (String.IsNullOrEmpty(Name)) _validationErrorMessages.Add(ResourcesManamgement.ErrMsgNoSystemName);
            else if (SystemManager.Instance.Exists(this)) _validationErrorMessages.Add(ResourcesManamgement.ErrMsgDuplicateSystemName);
            IsValid = _validationErrorMessages.Count == 0;
        }

        /// <summary>
        /// Creates a temporary copy of this instance, which could be used as binding object for dialog windows with cancel option.
        /// </summary>
        /// <returns></returns>
        public System CreateTempObject() { return new System {IsTempObject = true, Id = Id, Name = Name, Comment = Comment}; }

        /// <summary>
        /// Copies the values from the specified system and updates the persistated copy of this instance.
        /// </summary>
        /// <param name="system">System, from which the values should be copied.</param>
        public void UpdateValues(System system) {
            Name = system.Name;
            Comment = system.Comment;
            SystemManager.Instance.UpdateSystem(this);
        }

        public void NotifyAssignedReportChanged() {
            semaphore.Wait();
            try {
                _hasAnyAssignedDocument = DocumentManager.Instance.Documents.Any(d => d.SystemId == this.Id);
                OnPropertyChanged("HasAnyAssignedDocument");
            } finally {
                semaphore.Release();
            }
        }
        
        #endregion methods
    }
}