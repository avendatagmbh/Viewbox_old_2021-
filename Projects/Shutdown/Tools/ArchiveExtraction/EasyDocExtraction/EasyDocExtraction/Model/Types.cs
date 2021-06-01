using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyDocExtraction.Model
{
    public enum FieldType:byte
    {
        SystemField = 0,
        CustomField = 1
    }
    [Flags]
    public enum FieldFormat:byte
    {
        None = 0x0,
        IDX = 0x1,
        CAT = 0x2,
        TXT = 0x4,
        DEF = 0x8,
        QRY = 0x16
    }
}
