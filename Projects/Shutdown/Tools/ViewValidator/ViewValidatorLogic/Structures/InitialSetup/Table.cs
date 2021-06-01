// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:43
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using DbAccess;
using DbAccess.Structures;

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class Table {
        public string Name { get; private set; }
        public Filter Filter { get; set; }
        public List<DbColumnInfo> ColumnInfos { get; set; }
        //Indices of key entries
        public List<int> KeyEntries { get; set; }
        public DbConfig DbConfig { get; set; }

        public Table(string name, DbConfig dbConfig){
            Name = name;
            Filter = new Filter("");
            KeyEntries = new List<int>();
            DbConfig = dbConfig;
        }

        public void ReadTableStructure(IDatabase conn, ObservableCollection<ColumnMapping> mappings, int sourceOrDestIndex, ObservableCollection<ColumnMapping> keyEntryMappings) {
            ColumnInfos = conn.GetColumnInfos(Name);
            KeyEntries.Clear();
            //If there are key entries as strings, then find the corresponding indices of the columns
            if (keyEntryMappings.Count != 0) {
                foreach (var keyEntryMapping in keyEntryMappings) {
                    for (int i = 0; i < mappings.Count; ++i)
                        if (mappings[i].GetColumnName(0).ToLower() == keyEntryMapping.GetColumnName(0).ToLower() &&
                            mappings[i].GetColumnName(1).ToLower() == keyEntryMapping.GetColumnName(1).ToLower())
                            this.KeyEntries.Add(i);
                }
            }

            //Console.WriteLine("Table: " + Name);
            //foreach (var columnInfo in ColumnInfos)
            //    Console.WriteLine(columnInfo.Name + ": " + columnInfo.Type.ToString() +", " + columnInfo.OriginalType.ToString());
        }
    }
}
