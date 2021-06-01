using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewAssistantBusiness.Models
{
    public class TableCollection
    {

        public MainModel MainModel { get; set; }

        public TableCollection(MainModel model)
        {
            MainModel = model;
        }

        private readonly Dictionary<string, TableModel> _tables = new Dictionary<string, TableModel>();
        private readonly Dictionary<string, TableModel> _sourceTables = new Dictionary<string, TableModel>();
        private readonly Dictionary<string, TableModel> _viewboxTables = new Dictionary<string, TableModel>();
        private readonly Dictionary<string, TableModel> _finalTables = new Dictionary<string, TableModel>();

        public Dictionary<string, TableModel> Tables { get { return _tables; } }
        public Dictionary<string, TableModel> SourceTables { get { return _sourceTables; } }
        public Dictionary<string, TableModel> ViewboxTables { get { return _viewboxTables; } }
        public Dictionary<string, TableModel> FinalTables { get { return _finalTables; } }

        private readonly Object padlock = new Object();

        public TableModel GetTable(string name)
        {
            lock (padlock)
                lock (_tables)
                {
                    var nameLower = name.ToLower();
                    if (_tables.ContainsKey(nameLower))
                        return _tables[nameLower];
                    var table = new TableModel(MainModel) { Name = nameLower };
                    _tables.Add(table.Name, table);
                    return table;
                }
        }

        public void AddSourceTable(TableModel table)
        {
            lock (padlock)
                lock (_tables)
                    lock (_sourceTables)
                    {
                        if (!_sourceTables.ContainsKey(table.Name))
                        {
                            _sourceTables.Add(table.Name, table);
                            table.RenamerClicked -= table_RenamerClicked;
                            table.RenamerClicked += table_RenamerClicked;
                        }
                        table.IsInSource = true;
                    }
        }

        public void RemoveSourceTable(TableModel table)
        {
            lock (padlock)
                lock (_tables)
                    lock (_sourceTables)
                    {
                        if (_sourceTables.ContainsKey(table.Name))
                        {
                            _sourceTables.Remove(table.Name);
                            table.RenamerClicked -= table_RenamerClicked;
                        }
                        table.IsInSource = false;
                    }
        }

        void table_RenamerClicked(object sender, IRenameable model)
        {
            OnRenamerClicked(sender, model);
        }

        public void AddViewboxTable(TableModel table)
        {
            lock (padlock)
                lock (_tables)
                    lock (_viewboxTables)
                    {
                        if (!_viewboxTables.ContainsKey(table.Name))
                        {
                            _viewboxTables.Add(table.Name, table);
                            if (table is IViewboxLocalisable)
                            {
                                (table as IViewboxLocalisable).ConfigureLocalisationTextsClicked -= TableCollection_ConfigureTableTextsClicked;
                                (table as IViewboxLocalisable).ConfigureLocalisationTextsClicked += TableCollection_ConfigureTableTextsClicked;       
                            }
                        }
                        table.IsInViewbox = true;
                    }
        }

        void TableCollection_ConfigureTableTextsClicked(object sender, IViewboxLocalisable table)
        {
            OnConfigureTableTextsClicked(sender, table);
        }

        public event ConfigureLocalisationTextsClicked ConfigureTableTextsClicked;

        public void OnConfigureTableTextsClicked(object sender, IViewboxLocalisable model)
        {
            if (ConfigureTableTextsClicked != null)
            {
                ConfigureTableTextsClicked(sender, model);
            }
        }

        public event RenamerClicked RenamerClicked;

        public void OnRenamerClicked(object sender, IRenameable model)
        {
            if (RenamerClicked != null)
            {
                RenamerClicked(sender, model);
            }
        }


        public void AddFinalTable(TableModel table)
        {
            lock (padlock)
                lock (_tables)
                    lock (_finalTables)
                    {
                        if (!_finalTables.ContainsKey(table.Name))
                            _finalTables.Add(table.Name, table);
                        table.IsInFinal = true;
                    }
        }

        public void ClearSourceTables()
        {
            lock (padlock)
                lock (_tables)
                    lock (_finalTables)
                        lock (_viewboxTables)
                            lock (_sourceTables)
                            {
                                foreach (var table in _sourceTables.Values)
                                {
                                    table.ClearSourceInfos();
                                    if (!_finalTables.ContainsKey(table.Name) && !_viewboxTables.ContainsKey(table.Name))
                                    {
                                        _tables.Remove(table.Name);
                                    }
                                }
                                _sourceTables.Clear();
                            }
        }


        public void ClearViewboxTables()
        {
            lock (padlock)
                lock (_tables)
                    lock (_finalTables)
                        lock (_viewboxTables)
                            lock (_sourceTables)
                            {
                                foreach (var table in _viewboxTables.Values)
                                {
                                    table.ClearViewboxInfos();
                                    if (!_sourceTables.ContainsKey(table.Name) && !_finalTables.ContainsKey(table.Name))
                                    {
                                        _tables.Remove(table.Name);
                                    }
                                }
                                _viewboxTables.Clear();
                            }
        }

        public void ClearFinalTables()
        {
            lock (padlock)
                lock (_tables)
                    lock (_finalTables)
                        lock (_viewboxTables)
                            lock (_sourceTables)
                            {
                                foreach (var table in _finalTables.Values)
                                {
                                    table.ClearFinalInfos();
                                    if (!_sourceTables.ContainsKey(table.Name) && !_viewboxTables.ContainsKey(table.Name))
                                    {
                                        _tables.Remove(table.Name);
                                    }
                                }
                                _finalTables.Clear();
                            }
        }
    }
}
