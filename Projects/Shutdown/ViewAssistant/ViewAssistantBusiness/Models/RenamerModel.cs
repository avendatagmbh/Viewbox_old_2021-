using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Base.Localisation;
using Utils;
using DbAccess.Structures;
using SystemDb;
using System.Windows.Input;
using DbAccess;
using ViewAssistant.Controls;

namespace ViewAssistantBusiness.Models
{
    public delegate void RenamerEventHandler(object sender);

    public class RenamerModel : NotifyPropertyChangedBase
    {
        private MainModel mainModel { get; set; }

        private IRenameable columnOrTableObject { get; set; }

        private string fromName;
        public string FromName
        {
            get { return fromName; }

            set
            {
                fromName = value;
                OnPropertyChanged("FromName");
            }
        }

        private string toName;
        public string ToName
        {
            get { return toName; }

            set
            {
                toName = value;
                OnPropertyChanged("ToName");
            }
        }

        public RenamerModel(IRenameable columnOrTableObject, MainModel mainModel)
        {
            this.columnOrTableObject = columnOrTableObject;
            FromName = columnOrTableObject.Name;
            ToName = FromName;
            this.mainModel = mainModel;
        }

        public ICommand SaveCommand { get { return new RelayCommand(SaveExecuted); } }

        private void SaveExecuted(object sender)
        {
            using (var conn = mainModel.CurrentProfile.SourceConnection.CreateConnection())
            {
                Save(conn);
            }
        }

        private void Save(IDatabase db)
        {
            if (!FromName.Equals(ToName) && !mainModel.TableCollection.SourceTables.ContainsKey(ToName))
            {
                if (columnOrTableObject is ColumnModel)
                {
                    var column = columnOrTableObject as ColumnModel;
                    db.RenameColumn(column.TableModel.Name, FromName, ToName);
                    columnOrTableObject.Name = ToName;
                    column.SourceError = ValidateTableOrColumnName(ToName) ? null : ResourcesCommon.ContainsSpecCharOrWhiteSpace;
                }
                else if (columnOrTableObject is TableModel)
                {
                    var table = columnOrTableObject as TableModel;

                    var index = mainModel.SourceTables.IndexOf(table);
                    mainModel.TableCollection.RemoveSourceTable(table);
                    mainModel.SourceTables.Remove(table);
                    table.IsInViewbox = false;
                    table.IsInFinal = false;
                    if (mainModel.TableCollection.ViewboxTables.All(v => v.Value.Name != FromName) &&
                        mainModel.TableCollection.FinalTables.All(v => v.Value.Name != FromName))
                    {
                        mainModel.TableCollection.Tables.Remove(table.Name.ToLower());
                    }

                    var newTable = mainModel.TableCollection.GetTable(ToName);
                    mainModel.TableCollection.AddSourceTable(newTable);
                    mainModel.SourceTables.Insert(index, newTable);
                    newTable.SourceRowCount = table.SourceRowCount;
                    newTable.CopySourceColumns(table);

                    newTable.SourceChecked = table.SourceChecked;
                    newTable.IsSelected = table.IsSelected;

                    db.RenameTable(FromName, ToName);
                    newTable.Name = ToName;
                    newTable.SourceWarning = "";

                    newTable.SourceError = ValidateTableOrColumnName(ToName) ? null : ResourcesCommon.ContainsSpecCharOrWhiteSpace;

                    if (mainModel.TableCollection.ViewboxTables.Select(w => w.Value).Any(value => value.Name == ToName))
                    {
                        newTable.IsInViewbox = true;
                        foreach (var columns in newTable.SourceColumns)
                        {
                            if (newTable.ViewboxColumns.Any(w => w.Name == columns.Name))
                            {
                                columns.IsInViewbox = true;
                            }
                        }
                    }

                    if (mainModel.TableCollection.FinalTables.Select(w => w.Value).Any(value => value.Name == ToName))
                    {
                        newTable.IsInFinal = true;
                    }

                    if (!newTable.IsInViewbox || newTable.SourceViewboxDiff())
                    {
                        newTable.SourceWarning = "Database need to be migrated to viewbox";
                    }

                    mainModel.UpdateRenaming();
                }
            }
            OnSaveFinished(this);
        }

        public void AutoRename(IDatabase db, RenamerSettingsEventArgs settings)
        {
            foreach (var item in settings.ReplaceFromTo.Where(p => !String.IsNullOrEmpty(p.FromText))) //Replace string parts to anothers.
            {
                ToName = ToName.Replace(item.FromText, item.ToText);
            }

            Save(db);
        }

        public event RenamerEventHandler SaveFinished;
        private void OnSaveFinished(object sender)
        {
            if (SaveFinished != null)
                SaveFinished(sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Is valid?</returns>
        public bool ValidateTableOrColumnName(string name)
        {
            Regex myregex = new Regex(@"[^a-zA-Z0-9\-\\/(),_\s]+");

            if (myregex.IsMatch(name) || name.Contains(" "))
                return false;

            return true;
        }
    }
}
