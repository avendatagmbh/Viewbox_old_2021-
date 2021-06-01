
namespace ViewValidatorLogic.Interfaces {
    public interface IRow {
        IRowEntry this[int index] { get; }
        int ColumnCount { get; }
        string HeaderOfColumn(int index);
    }
}
