using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;
using DbAccess.Structures;
using Utils;
using ViewBuilderCommon;

namespace ViewBuilderBusiness.Structures
{
    public class RelationCreater
    {
        #region Constructor

        public RelationCreater(SystemDb.SystemDb viewboxDb, string systemName, List<IRelationDatabaseObject> relations,
                               DbConfig baseDbConfig)
        {
            _systemName = systemName;
            _viewboxDb = viewboxDb;
            _relations = relations;
            _baseDbConfig = baseDbConfig;
        }

        #endregion Constructor

        #region Properties

        private readonly DbConfig _baseDbConfig;
        private readonly List<IRelationDatabaseObject> _relations;

        private readonly Dictionary<string, TableIndices> _tableToIndices =
            new Dictionary<string, TableIndices>(StringComparer.InvariantCultureIgnoreCase);

        private readonly SystemDb.SystemDb _viewboxDb;
        private string _systemName;

        public int TotalTables
        {
            get { return _tableToIndices.Count; }
        }

        public int TotalIndexCount
        {
            get { return _tableToIndices.Sum(pair => pair.Value.IndexCount); }
        }

        public long TotalRowCount { get; private set; }

        #endregion Properties

        #region Methods

        #region CheckIndizes

        public void CheckIndizes(ProgressCalculator progress)
        {
            progress.Title = "Analysiere zu erstellende Indizes";
            Dictionary<int, List<IRelationDatabaseObject>> relationIdToRelations =
                new Dictionary<int, List<IRelationDatabaseObject>>();
            foreach (var relation in _relations)
            {
                List<IRelationDatabaseObject> list;
                if (!relationIdToRelations.TryGetValue(relation.RelationId, out list))
                {
                    list = new List<IRelationDatabaseObject>();
                    relationIdToRelations[relation.RelationId] = list;
                }
                list.Add(relation);
            }
            progress.SetWorkSteps(relationIdToRelations.Count, false);
            using (var conn = ConnectionManager.CreateConnection(_baseDbConfig))
            {
                conn.Open();
                foreach (var relationList in relationIdToRelations.Values)
                {
                    AddIndexFromRelationList(conn, relationList, true);
                    AddIndexFromRelationList(conn, relationList, false);
                    progress.StepDone();
                }
            }
        }

        #endregion CheckIndizes

        public void CreateIndices(ProgressCalculator progress)
        {
            progress.Title = "Erstelle Indizes";
            progress.SetWorkSteps(TotalIndexCount, false);
            using (var conn = ConnectionManager.CreateConnection(_baseDbConfig))
            {
                conn.Open();
                conn.SetHighTimeout();
                foreach (var tableIndex in _tableToIndices.Values)
                {
                    progress.Description = string.Format("Erstelle {0} Indizes auf Tabelle {1}", tableIndex.IndexCount,
                                                         tableIndex.TableName);
                    tableIndex.CreateIndices(conn);
                    progress.StepsDone(tableIndex.IndexCount);
                }
            }
        }

        private void AddIndexFromRelationList(IDatabase conn, IEnumerable<IRelationDatabaseObject> relationList,
                                              bool source)
        {
            Index index = new Index();
            ITableObject table = null;
            foreach (var relation in relationList)
            {
                IColumn column = _viewboxDb.Columns[source ? relation.ParentId : relation.ChildId];
                if (table == null) table = column.Table;
                else if (table != column.Table)
                    throw new InvalidOperationException(
                        string.Format(
                            "Bei der Relationsnummer {0} gibt es unterschiedliche {1}-Tabellen - dies ist nicht zulässig.",
                            relation.RelationId, source ? "Ausgangs" : "Ziel"));
                index.Table = table.TableName;
                index.AddColumn(column.Name);
            }


            TableIndices tableIndices;
            if (!_tableToIndices.TryGetValue(table.TableName, out tableIndices))
            {
                tableIndices = new TableIndices(table.TableName);
                _tableToIndices[table.TableName] = tableIndices;
                TotalRowCount += table.RowCount;
            }
            tableIndices.AddIndexIfNotExistsInDatabase(index, conn);
        }

        #endregion Methods
    }
}