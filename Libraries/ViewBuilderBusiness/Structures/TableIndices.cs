using System;
using System.Collections.Generic;
using System.Linq;
using AV.Log;
using DbAccess;
using ViewBuilderCommon;
using log4net;

namespace ViewBuilderBusiness.Structures
{
    public class TableIndices
    {
        private readonly ILog _logger = LogHelper.GetLogger();

        #region Constructor

        public TableIndices(string tableName)
        {
            TableName = tableName;
        }

        #endregion Constructor

        #region Properties

        private readonly List<Index> _indices = new List<Index>();
        public string TableName { get; set; }

        public int IndexCount
        {
            get { return _indices.Count; }
        }

        #endregion Properties

        #region Methods

        public void AddIndex(Index index)
        {
            if (!_indices.Contains(index))
                _indices.Add(index);
        }

        public void AddIndexIfNotExistsInDatabase(Index index, IDatabase conn)
        {
            if (!_indices.Contains(index) && !index.Exists(conn))
                _indices.Add(index);
        }

        public void CreateIndices(IDatabase conn)
        {
            if (_indices.Count == 0)
                return;
            foreach (var index in _indices)
            {
                try
                {
                    using (
                        var reader =
                            conn.ExecuteReader("SHOW INDEX FROM " + conn.Enquote(TableName) + " WHERE KEY_NAME = '" +
                                               (index.Name.Length > 63
                                                    ? index.Name.Substring(index.Name.Length - 63)
                                                    : index.Name) + "'"))
                    {
                        if (reader.Read())
                            continue;
                    }
                    var sql = String.Format("ALTER TABLE {0} {1}", conn.Enquote(TableName),
                                            "ADD INDEX " +
                                            (index.Name.Length > 63
                                                 ? index.Name.Substring(index.Name.Length - 63)
                                                 : index.Name) + "(" +
                                            String.Join(",", (from column in index.Columns select conn.Enquote(column))) +
                                            ")");
                    conn.ExecuteNonQuery(sql);
                }
                catch (Exception e)
                {
                    _logger.Error("Error while creating index", e);
                }
            }
        }

        #endregion Methods
    }
}