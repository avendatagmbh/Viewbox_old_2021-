using AvdCommon.DataGridHelper.Interfaces;

namespace AvdCommon.DataGridHelper
{
    public class DataRowEntry : IDataRowEntry
    {
        public object Value;

        public DataRowEntry(object value)
        {
            Value = value;
        }

        #region IDataRowEntry Members

        public string DisplayString
        {
            get { return Value == null ? string.Empty : Value.ToString(); }
        }

        #endregion
    }
}