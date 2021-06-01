using SystemDb;
using DbAccess.Structures;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class ColumnDifference
    {
        #region Type enum

        public enum Type
        {
            MissingColumn,
            MultipleColumns,
            DataTypeCollisionText,
            DataTypeCollisionBinary,
            DataTypeCollisionDouble,
            DataTypeCollision,
            NoDifference
        }

        #endregion

        public ColumnDifference(string tableName, string columnName, Type type)
        {
            TableName = tableName;
            ColumnName = columnName;
            DifferenceType = type;
        }

        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public Type DifferenceType { get; set; }
        public DbColumnInfo CustomerColumn { get; set; }
        public IColumn MetadataColumn { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\t\t{1}", ColumnName, DifferenceType);
        }
    }
}