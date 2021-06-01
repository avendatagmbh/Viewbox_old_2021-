using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;

namespace ViewboxAdmin.ViewModels.MergeDataBase
{
    internal interface ILoaderHack {
        ILanguageCollection Languages { get; set; }
        List<Relation> LoadRelationFromTable();
        List<ParameterValue> LoadParameterValue();
        List<ParameterCollectionMapping> LoadParameterCollectionFromTable();
        List<NewLogActionMerge> LoadNewLogAction();
        List<TableObjectSchemeMapping> LoadTableObjectSchemeMapping();
        List<TableOriginalName> TableOriginalName();
        List<TableArchiveInformation> TableArchiveInformation();
        List<Info> Info();
        List<Note> Note();
        List<About> About();
        List<HistoryParameterValue> UserIssueParameterHistory();
        List<UserOptimizationSettings> UserOptimizationSettings();
    }

    class LoaderHack : ILoaderHack {
        private DbAccess.IDatabase iDatabase;

        public ILanguageCollection Languages { get; set; }

        public LoaderHack(DbAccess.IDatabase iDatabase) {
            // TODO: Complete member initialization
            this.iDatabase = iDatabase;
        }

        public List<Relation> LoadRelationFromTable() {
            var list = new List<Relation>();
            using(iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<Relation>();
            }
            return list;
        }

        public List<ParameterValue> LoadParameterValue()
        {
            var list = new List<ParameterValue>();
            using (iDatabase)
            {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<ParameterValue>();

                var listParams = iDatabase.DbMapping.Load<ParameterValueSetText>();
                foreach (var t in listParams)
                {
                    foreach (ParameterValue lst in list) {
                        if(lst.Id==t.RefId) 
                            //lst.SetDescription(t.Text, Languages[t.CountryCode]);
                            if (Languages.Any(l => l.CountryCode == t.CountryCode)) lst.SetDescription(t.Text, Languages[t.CountryCode]);
                    }
                    //list[t.RefId-1].SetDescription(t.Text, Languages[t.CountryCode]);
                }

            }

            return list;
        }

        public List<ParameterCollectionMapping> LoadParameterCollectionFromTable()
        {
            var list = new List<ParameterCollectionMapping>();
            using (iDatabase)
            {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<ParameterCollectionMapping>();
            }
            return list;
        }

        public List<NewLogActionMerge> LoadNewLogAction()
        {
            var list = new List<NewLogActionMerge>();
            using (iDatabase)
            {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<NewLogActionMerge>();
            }
            return list;
        }

        public List<TableObjectSchemeMapping> LoadTableObjectSchemeMapping()
        {
            var list = new List<TableObjectSchemeMapping>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<TableObjectSchemeMapping>();
            }
            return list;
        }

        public List<TableOriginalName> TableOriginalName()
        {
            var list = new List<TableOriginalName>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<TableOriginalName>();
            }
            return list;
        }

        public List<TableArchiveInformation> TableArchiveInformation()
        {
            var list = new List<TableArchiveInformation>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<TableArchiveInformation>();
            }
            return list;
        }

        public List<Info> Info()
        {
            var list = new List<Info>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<Info>();
            }
            return list;
        }

        public List<Note> Note()
        {
            var list = new List<Note>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<Note>();
            }
            return list;
        }

        public List<About> About()
        {
            var list = new List<About>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<About>();
            }
            return list;
        }

        public List<HistoryParameterValue> UserIssueParameterHistory()
        {
            var list = new List<HistoryParameterValue>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<HistoryParameterValue>();
            }
            return list;
        }

        public List<UserOptimizationSettings> UserOptimizationSettings() {
            var list = new List<UserOptimizationSettings>();
            using (iDatabase) {
                iDatabase.Open();
                list = iDatabase.DbMapping.Load<UserOptimizationSettings>();
            }
            return list;
            
        }
    }
}
