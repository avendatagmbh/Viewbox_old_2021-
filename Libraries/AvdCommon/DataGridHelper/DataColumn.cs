using System.Drawing;
using AvdCommon.DataGridHelper.Interfaces;

namespace AvdCommon.DataGridHelper
{
    public class DataColumn : IDataColumn
    {
        public DataColumn(string name)
        {
            Name = name;
        }

        #region IDataColumn Members

        public string Name { get; set; }
        public Color Color { get; set; }

        #endregion
    }
}