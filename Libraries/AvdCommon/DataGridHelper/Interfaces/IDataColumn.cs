using System.Drawing;

namespace AvdCommon.DataGridHelper.Interfaces
{
    public interface IDataColumn
    {
        string Name { get; set; }
        Color Color { get; set; }
    }
}