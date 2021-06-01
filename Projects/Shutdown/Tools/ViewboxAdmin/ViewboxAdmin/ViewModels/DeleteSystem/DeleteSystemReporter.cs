/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-12      fully unit test covered class for deleting a complete system from metadatabase. 
 * 
 * KNOWN ISSUES:
 * very poor performance, because of the step by step column removing.
 * version changes can make severe problems to keep this strategy up-to-data.
 * the test are rather hard to maintain, especially setting up the mocks, however this fact arises from the structure of the systemDB class to some extent
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SystemDb;
using ViewboxAdmin.CustomEventArgs;

namespace ViewboxAdmin.ViewModels
{
    public class DeleteSystemReporter : StatusReporterBase, IDeleteSystemStrategy
    {
        public DeleteSystemReporter(SystemDb.ISystemDb systemDb) {
            if (systemDb == null)
            {
                throw new ArgumentNullException("The systemDB cannot be null");
            }
            this._systemDb = systemDb;
        }
        /// <summary>
        /// removing a system from the metadatabase
        /// </summary>
        /// <param name="optimizationid">a systemtype optimization to remove permanently</param>
        public void DeleteSystemFromMetaDataBase(int optimizationid) {
            OnStart();
            //check if it is a system
            if (IsInvalid(optimizationid)) return;
            string valuestring = SystemDb.Optimizations[optimizationid].FindValue(OptimizationType.System);

            //collecting tables
            List<int> tableObjectIdCollection = new List<int>();
            List<ITableObject> tableObjectCollection = new List<ITableObject>();
                CollectTablesToRemove(tableObjectIdCollection,tableObjectCollection,valuestring);
            OnProgress(5);

            //collecting columns
            List<int> columnidcollection = new List<int>();
            CollectColumnsToRemove(columnidcollection, tableObjectIdCollection);
            OnProgress(10);

            //collecting parameters
            List<int> parametercollection = new List<int>();
            CollectParametersToRemove(parametercollection, tableObjectIdCollection);
            OnProgress(15);

            if (columnidcollection.Count!=0 && tableObjectCollection.Count!=0) {
                
            
            try {
                RemoveRelations(valuestring);
                OnProgress(10);
                RemoveColumnReferences(columnidcollection, tableObjectIdCollection);
                OnProgress(50);
                RemoveTableReferences(tableObjectIdCollection,tableObjectCollection);
                OnProgress(80);
                RemoveOptimizations(optimizationid);
                OnProgress(90);
                RemoveParameters(parametercollection);
                OnProgress(99);
            } catch(Exception e) {
                OnDebug("The following exception has been occured: " + e.Message);
            }
                
            }
            else {
                OnDebug("There are no columns or tables...");
            }
            OnCompleted();
        }

        private void RemoveParameters(List<int> parametercollection) {

            var parameterremovactions = new Dictionary<Action<int>, string>() {
                {SystemDb.DeleteParameters, "deleting parameters..."},
                {SystemDb.DeleteParameterCollection, "deleting parameter collections..."},
                {SystemDb.DeleteParameterText, "deleting parameter texts..."}
            };

            foreach (var action in parameterremovactions) {
                _stopwatch.Restart();
                OnDebug(action.Value);
                foreach (var parameters in parametercollection)
                {
                    action.Key(parameters);
                }
                OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds); 
              }
        }

        private void RemoveRelations(string valuestring) {
            SystemDb.DeleteRelations(valuestring);
            OnDebug("Relations are deleted");
        }

        private void RemoveOptimizations(int optimizationid) {
            SystemDb.RemoveOptimizationFromAllTables(optimizationid);
            OnDebug("Optimization has been removed successfully");
        }

        private void RemoveTableReferences(List<int> tableidcollectio,List<ITableObject> tableobjectcollection ) {
            // dictionary for holding the submethods
            var tableremoveactions = new Dictionary<Action<int>, string>() {
                //{SystemDb.DeleteTableObject, "Deleting tables..."},
                {SystemDb.DeleteTableArchiveInfo,"Deleting table archive info..."},
                {SystemDb.DeleteTableRoles,"Delete table-roles rights..."},
                {SystemDb.DeleteTableOriginalName,"Deleting table original names..."},
                {SystemDb.DeleteTableSchemes,"Deleting table schemes..."},
                //{SystemDb.DeleteTableText,"Deleting table texts..."},
                {SystemDb.DeleteTableUsers,"Deleting table users..."},
                {SystemDb.DeleteTableOrder,"Deleting table orders..."},
                {SystemDb.DeleteUserTableSettings,"Deleting user table settings..."},
                {SystemDb.DeleteIssues,"Deleting issues..."}
                //{SystemDb.DeleteOrderArea,"Deleting order areas..."}
            };
            OnDebug("delelting tableobject at once");
            _stopwatch.Restart();
            SystemDb.DeleteTableObjects(tableobjectcollection);
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);

            OnDebug("delelting tabletext at once");
            _stopwatch.Restart();
            SystemDb.DeleteTableTexts(tableidcollectio);
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);

            OnDebug("deleting orderarea at once");
            _stopwatch.Restart();
            SystemDb.DeleteOrderArea(tableidcollectio);
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);



            foreach (var action in tableremoveactions) {
                _stopwatch.Restart();
                OnDebug(action.Value);
                foreach (var tables in tableidcollectio) {
                    action.Key(tables);
                }
                OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);
            }
        }

        private bool IsInvalid(int optimizationid) {
            IOptimization opt = SystemDb.Optimizations[optimizationid];
            if (opt == null || opt.Group.Type != OptimizationType.System) {
                OnDebug("The optimization with given ID is not exist, or not a system");
                OnCrashed();
                return true;
            }
            return false;
        }

        private void CollectColumnsToRemove(List<int> columnIdToRemove, List<int> tableidcollectio) {
            foreach (IColumn column in SystemDb.Columns) {
                if (tableidcollectio.Contains(column.Table.Id))
                    columnIdToRemove.Add(column.Id);
            }
            OnDebug(columnIdToRemove.Count.ToString() + " columns have been found... ");
        }

        private void CollectTablesToRemove(List<int> tableIdCollection,List<ITableObject> tablecollection , string valuestring) {
            foreach (var obj in SystemDb.Objects) {
                if (obj.Database == valuestring) {
                    tableIdCollection.Add(obj.Id);
                    tablecollection.Add(obj);
                }
            }
            foreach(var obj in SystemDb.GetFakeTableObjects(valuestring))
            {
                tableIdCollection.Add(obj.Id);
                tablecollection.Add(obj);
            }
            OnDebug(tableIdCollection.Count.ToString() + " tables have been found... ");
        }

        private void CollectParametersToRemove(List<int> parameterIdCollection, List<int> tableIdCollection) {
            foreach (var i in SystemDb.Issues)
            {
                if (tableIdCollection.Contains(i.Id))
                {
                    foreach (var p in i.Parameters)
                        parameterIdCollection.Add(p.Id);
                }
            }
        }

        private void RemoveColumnReferences(List<int> columnIdCollection,List<int> tableId ) {

            var columnremovactions = new Dictionary<Action<int>, string>() {
                //{SystemDb.DeleteColumn,"Deleting columns..."},
                //{SystemDb.DeleteRoleColumn,"Deleting role columns..."},
                //{SystemDb.DeleteColumnTexts,"Deleting column texts..."},
                //{SystemDb.DeleteUsersColumn,"Deleting users columns..."},
                //{SystemDb.DeleteColumnOrder,"Deleting columns orders..."},
                //{SystemDb.DeleteUserColumnSettings,"Deleting user column settings..."}
            };

            _stopwatch.Restart();
            OnDebug("Delete columns at once...");
            SystemDb.DeleteColumns(columnIdCollection);
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);

            _stopwatch.Restart();
            OnDebug("Delete column texts at once...");
            SystemDb.DeleteColumnTexts(columnIdCollection);
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);

            //role column rights
            _stopwatch.Restart();
            OnDebug("Delete role column rights");
            List<int> idlist = new List<int>();
            foreach (var item in SystemDb.RoleColumnRights) {
                int id = item.Item2.Id;
                if (columnIdCollection.Contains(id)) {
                    idlist.Add(id);
                }
            }

            foreach (var i in idlist) {
                SystemDb.DeleteRoleColumn(i);
            }
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);


            // user column rights
            _stopwatch.Restart();
            OnDebug("Delete user column rights");
            idlist = new List<int>();
            foreach (var item in SystemDb.UserColumnRights)
            {
                int id = item.Item2.Id;
                if (columnIdCollection.Contains(id))
                {
                    idlist.Add(id);
                }
            }

            foreach (var i in idlist)
            {
                SystemDb.DeleteUsersColumn(i);
            }
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);



            //column order
            _stopwatch.Restart();
            OnDebug("Delete column order settings");
            List<IUserColumnOrderSettings> columnorderlist = new List<IUserColumnOrderSettings>();
            foreach (var columnorder in SystemDb.UserColumnOrderSettings)
            {

                if (tableId.Contains(columnorder.TableObject.Id))
                {
                    columnorderlist.Add(columnorder);
                }
            }

            foreach (var userColumnOrderSettingse in columnorderlist) {
                SystemDb.DeleteColumnOrder(userColumnOrderSettingse.TableObject.Id);
            }
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);

            //user column visibility settings

            _stopwatch.Restart();
            OnDebug("Delete user column visibility settings");
            List<IUserColumnSettings> columnvisibilitylist = new List<IUserColumnSettings>();
            foreach (var columvis in SystemDb.UserColumnSettings)
            {

                if (columnIdCollection.Contains(columvis.Column.Id))
                {
                    columnvisibilitylist.Add(columvis);
                }
            }

            foreach (var userColumnSettingse in columnvisibilitylist)
            {
                SystemDb.DeleteUserColumnSettings(userColumnSettingse.Column.Id);
            }
            OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);







            foreach (var action in columnremovactions)
            {
                _stopwatch.Restart();
                OnDebug(action.Value);
                foreach (var column in columnIdCollection)
                {
                    action.Key(column);
                }
                OnDebug("Ellapsed seconds: " + _stopwatch.Elapsed.TotalSeconds);
            }
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private ISystemDb _systemDb;
        public ISystemDb SystemDb { get { return _systemDb; } }
        
    }
}
