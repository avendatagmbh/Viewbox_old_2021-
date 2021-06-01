using System;

namespace DbSearchLogic.SearchCore.SearchMatrix
{
    [Flags]
    public enum SearchValueMatrixCellType {
        None = 0,
        UseCell = 0x1,
        IsNumeric = 0x2,
        IsInteger = 0x4,
        IsUnsignedInteger = 0x8,
        IsDateTime = 0x10,
        IsText = 0x20
    }
}
