// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures.Results;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures.Config {
    public class Table : NotifyPropertyChangedBase{
        #region Constructor
        public Table(Profile profile, IDbTable dbTable) {
            DbTable = dbTable;
            Profile = profile;

            Groups = new ObservableCollectionAsync<ScreenshotGroup>();
            foreach(var dbScreenshotGroup in dbTable.ScreenshotGroups)
                Groups.Add(new ScreenshotGroup(dbScreenshotGroup));

            if (dbTable.ScreenshotGroups.Count == 0)
                Groups.Add(new ScreenshotGroup(DatabaseObjectFactory.CreateScreenshotGroup(dbTable)));

        }
        #endregion Constructor

        #region Properties

        internal IDbTable DbTable { get; set; }
        public Profile Profile { get; private set; }
        private RecognitionResult _recognitionResult;
        public RecognitionResult RecognitionResult {
            get { return _recognitionResult; }
            set {
                if (_recognitionResult != value) {
                    _recognitionResult = value;
                    OnPropertyChanged("RecognitionResult");
                }
            }
        }

        public string TableName {
            get { return DbTable.TableName; }
            set {
                DbTable.TableName = value;
                OnPropertyChanged("TableName");
            }
        }

        public string Comment {
            get { return DbTable.Comment; }
            set {
                DbTable.Comment = value;
                OnPropertyChanged("Comment");
            }
        }

        public ObservableCollectionAsync<ScreenshotGroup> Groups { get; private set; }

        public string DisplayString {
            get { return Name; }
        }

        #region Name
        //private string _name;
        public string Name {
            get { return DbTable.Name; }
            set {
                if (Name != value) {
                    DbTable.Name = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged("DisplayString");
                }
            }
        }


        #endregion Name

        #endregion
    }
}
