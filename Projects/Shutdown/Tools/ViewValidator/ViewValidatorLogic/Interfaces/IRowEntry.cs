using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewValidatorLogic.Interfaces {
    public enum RowEntryType {
        Normal,
        KeyEntry,
        Mismatch //Used for RowDifferences, when two entries are not equal
    }

    public interface IRowEntry {
        string DisplayString { get; }
        string RuleDisplayString { get; }
        RowEntryType RowEntryType { get; set; }
    }
}
