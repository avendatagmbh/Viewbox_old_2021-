// -----------------------------------------------------------
// Created by Benjamin Held - 22.08.2011 15:00:16
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using AvdCommon.Rules;
using ViewValidatorLogic.Config;
using ViewValidatorLogic.Manager;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Profile {
    public class ProfileModel {
        #region Constructor
        public ProfileModel(ProfileConfig profileConfig) {
            this.Profile = profileConfig;
            DatabaseModel = new DatabaseModel(profileConfig.ValidationSetup.DbConfigValidation, profileConfig.ValidationSetup.DbConfigView);
            //SelectedColumnMappingModel = new ColumnMappingModel();
            TableMappingModel = new TableMappingModel(profileConfig.ValidationSetup.TableMappings, profileConfig.ValidationSetup.DbConfigView, profileConfig.ValidationSetup.DbConfigValidation);
            TableToColumnMapping = new Dictionary<TableMapping, ColumnMappingModel>();
            //this.TableMappingToColumnMapping = new Dictionary<TableMapping, ColumnMappingModel>();
            TableMappingModel.TableMappingChangedEvent += new EventHandler<TableMappingModel.TableMappingChangedEventArgs>(TableMappingModel_MappingChangedEvent);
            //StoredProcedureModel = new StoredProcedureModel();

            ConfigToGui(profileConfig.ValidationSetup);
            
        }

        void TableMappingModel_MappingChangedEvent(object sender, TableMappingModel.TableMappingChangedEventArgs e) {
            if (TableToColumnMapping.ContainsKey(e.Mapping)) {
                if (e.Removed)
                    TableToColumnMapping.Remove(e.Mapping);
            } else if (!e.Removed) {
                TableToColumnMapping[e.Mapping] = new ColumnMappingModel(e.Mapping, DatabaseModel.ConfigDest);
            }
        }

        #endregion

        #region Properties
        public DatabaseModel DatabaseModel { get; set; }
        public ProfileConfig Profile { get; set; }
        public ColumnMappingModel SelectedColumnMappingModel { get; set; }
        public TableMapping SelectedTableMapping { get; set; }
        public Dictionary<TableMapping, ColumnMappingModel> TableToColumnMapping { get; set; }
        //public Dictionary<TableMapping, ColumnMappingModel> TableMappingToColumnMapping { get; set; }
        public TableMappingModel TableMappingModel { get; set; }
        //public StoredProcedureModel StoredProcedureModel { get; set; }

        #region TableInfoNotInGui
        class TableInfoNotInGui {
            public bool Used { get; set; }
            public RuleSet RuleSet { get; set; }
        }

        Dictionary<string, TableInfoNotInGui> _tableInfosNotInGui = new Dictionary<string, TableInfoNotInGui>();

        private Dictionary<string, TableInfoNotInGui> TableInfosNotInGui {
            get { return _tableInfosNotInGui; }
            set { _tableInfosNotInGui = value; }
        }
        #endregion
        #endregion

        #region Methods
        #region ConfigToGui
        private void ConfigToGui(ValidationSetup setup) {
            //if (setup.DbConfigValidation != null) this.DatabaseModel.ConfigSource = setup.DbConfigValidation;
            //if(setup.DbConfigView != null) this.DatabaseModel.ConfigDest = setup.DbConfigView;
            foreach (var tableMapping in setup.TableMappings) {
                //TableMappingModel.AddMapping(tableMapping);
                ColumnMappingModel columnMappingModel = new ColumnMappingModel(tableMapping, setup.DbConfigView);

                //foreach (var columnMapping in tableMapping.ColumnMappings) {
                //    columnMappingModel.AddMapping(new Mapping() { Source = columnMapping.Source, Destination = columnMapping.Destination });
                //}
                //if (tableMapping.TableValidation.KeyEntriesColumnNames.Count != tableMapping.TableView.KeyEntriesColumnNames.Count)
                //    throw new Exception("Es gibt bei den Tabellen unterschiedlich viele Key Entries.");
                //columnMappingModel.Sort.Clear();
                //for (int i = 0; i < tableMapping.TableValidation.KeyEntriesColumnNames.Count; ++i) {
                //    columnMappingModel.Sort.Add(new Mapping(tableMapping.TableValidation.KeyEntriesColumnNames[i], tableMapping.TableView.KeyEntriesColumnNames[i]));
                //}
                //columnMappingModel.FilterValidation = tableMapping.TableValidation.Filter.FilterString;
                //columnMappingModel.FilterView = tableMapping.TableView.Filter.FilterString;
                columnMappingModel.FilterValidation = tableMapping.TableValidation.Filter;
                columnMappingModel.FilterView = tableMapping.TableView.Filter;

                this.TableToColumnMapping[tableMapping] = columnMappingModel;
                //this.TableMappingToColumnMapping[tableMapping] = columnMappingModel;
                //Save which table were used and the ruleset as this is not shown in the gui
                if (this.TableInfosNotInGui.ContainsKey(tableMapping.UniqueName)) {
                    this.TableInfosNotInGui[tableMapping.UniqueName].Used = tableMapping.Used;
                    this.TableInfosNotInGui[tableMapping.UniqueName].RuleSet = tableMapping.Rules;
                } else {
                    this.TableInfosNotInGui[tableMapping.UniqueName] = new TableInfoNotInGui() { Used = tableMapping.Used, RuleSet = tableMapping.Rules };
                }
                
            }
        }
        #endregion

        internal void SaveProfile() {
            ProfileManager.Save(Profile);
        }

        public void RemoveTableMapping(TableMapping tableMapping){
            this.TableInfosNotInGui.Remove(tableMapping.UniqueName);
            this.TableMappingModel.RemoveMapping(tableMapping);
            TableToColumnMapping.Remove(tableMapping);
        }
        #endregion Methods

    }
}
