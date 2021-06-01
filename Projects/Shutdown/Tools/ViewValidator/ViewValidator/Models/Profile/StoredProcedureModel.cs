
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using AvdCommon.Logging;
using DbAccess;
using DbAccess.Structures;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.InitialSetup.StoredProcedures;

namespace ViewValidator.Models.Profile {
    public class StoredProcedureModel : INotifyPropertyChanged{
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Constructor
        public StoredProcedureModel(StoredProcedure storedProcedure, DbConfig dbConfigView) {
            StoredProcedure = storedProcedure;
            DbConfigView = dbConfigView;
        }
        #endregion
        #region Properties
        public StoredProcedure StoredProcedure { get; set; }
        public DbConfig DbConfigView { get; set; }
        private string ViewTableName { get; set; }
        #endregion

        #region Methods

        //Retrieves table id from system table, uses the table id to find a possible stored procedure, uses the procedure id to find the procedure arguments
        public bool CheckForDynamicView(TableMapping tableMapping) {
            StoredProcedure.Clear();

            ViewTableName = tableMapping.TableView.Name;

            DbConfig systemDbConfig = (DbConfig) DbConfigView.Clone();
            systemDbConfig.DbName = DbConfigView.DbName + "_system";

            using (IDatabase conn = ConnectionManager.CreateConnection(systemDbConfig)) {
                conn.Open();
                object tableIdObject = conn.ExecuteScalar("SELECT table_id FROM " + conn.Enquote("table") + " WHERE name=" + conn.GetSqlString(ViewTableName));
                long tableId;
                if (tableIdObject == null || !Int64.TryParse(tableIdObject.ToString(), out tableId))
                    throw new InvalidOperationException("Could not retrieve table id for view table " + ViewTableName);

                long procId;
                string name;
                bool doClientSplit, doCompCodeSplit, doFYearSplit;
                if (!conn.TableExists("procedures")) {
                    return false;
                }
                using (var reader = conn.ExecuteReader("SELECT id,name,DoClientSplit,DoCompCodeSplit,DoFYearSplit FROM " + conn.Enquote("procedures")+" WHERE table_id=" + tableId)) {
                    if (!reader.Read()) return false;
                    procId = Convert.ToInt64(reader["id"]);
                    name = reader["name"].ToString();
                    doClientSplit = Convert.ToBoolean(reader["DoClientSplit"]);
                    doCompCodeSplit = Convert.ToBoolean(reader["DoCompCodeSplit"]);
                    doFYearSplit = Convert.ToBoolean(reader["DoFYearSplit"]);

                }
                //StoredProcedure = new StoredProcedure(procId, name);
                StoredProcedure.Id = procId;
                StoredProcedure.Name = name;

                if(doClientSplit)
                    StoredProcedure.AddArgument(new ProcedureArgument() {Name = "Mandant",Type="text",Ordinal = Int32.MaxValue-3} );
                if (doCompCodeSplit)
                    StoredProcedure.AddArgument(new ProcedureArgument() { Name = "Buchungskreis", Type = "text", Ordinal = Int32.MaxValue - 2 });
                if (doFYearSplit)
                    StoredProcedure.AddArgument(new ProcedureArgument() { Name = "Finanzjahr", Type = "text", Ordinal = Int32.MaxValue - 1 });

                using (var reader = conn.ExecuteReader("SELECT id,ordinal,name,description,type FROM " + conn.Enquote("procedure_params") + " WHERE procedure_id=" + procId)) {
                    while (reader.Read()) {
                        ProcedureArgument arg = new ProcedureArgument() {
                                                                            Name = reader["name"].ToString(),
                                                                            Description = reader["description"].ToString(),
                                                                            Id = Convert.ToInt64(reader["id"]),
                                                                            Type = reader["type"].ToString(),
                                                                            Ordinal = Convert.ToInt64(reader["ordinal"])
                                                                        };

                        StoredProcedure.AddArgument(arg);
                    }
                }
                //tableMapping.StoredProcedure = StoredProcedure;
                //StoredProcedure = tableMapping.StoredProcedure;
                OnPropertyChanged("StoredProcedure");
            }
            return true;
        }

        public void CallProcedure(Window owner) {
            try{
                StoredProcedure.Call(DbConfigView);
                using (IDatabase conn = ConnectionManager.CreateConnection(DbConfigView)) {
                    conn.Open();
                    long rows = (long) conn.ExecuteScalar("SELECT COUNT(*) FROM " + conn.Enquote(ViewTableName));
                    MessageBox.Show(owner,
                                    "Stored Procedure erfolgreich eingespielt. Es befinden sich nun " + rows +
                                    " Datensätze in der Datenbank.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception ex) {
                MessageBox.Show(owner, "Ein Fehler ist aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
