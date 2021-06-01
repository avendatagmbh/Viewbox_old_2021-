using System;
using System.Collections.Generic;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class TableDifference
    {
        #region Type enum

        public enum Type
        {
            MissingTable,
            ColumnDifferences,
            NoDifferences
        }

        #endregion

        public TableDifference(string tableName, Type type)
        {
            TableName = tableName;
            DifferenceType = type;
            ColumnDifferences = new List<ColumnDifference>();
        }

        public string TableName { get; set; }
        public Type DifferenceType { get; set; }
        public IList<ColumnDifference> ColumnDifferences { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}\t\t{1}\r\n{2}", TableName, DifferenceType,
                                 string.Join(Environment.NewLine, ColumnDifferences));
        }
    }
}