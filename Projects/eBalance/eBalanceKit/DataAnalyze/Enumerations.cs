namespace DataAnalyze {
    // List of available results that could be returned from the analysing module.
    public enum Result {
        True,
        False,
        Indeterminable
    }

    // List of available strategies that can process the datas.
    public enum Strategy {
        IsPositiveOrZero,
        IsNegativeOrZero,
        IsEmpty,
        IsNumeric,
        Dummy
    }

    // List of available tresholds.
    public enum Treshold {
        EveryDataShouldBeSame,
        OnlyTrueAccepted,
        OnlyFalseAccepted
    }

    /// <summary>
    /// Possible causes of import failure.
    /// </summary>
    public enum ProblemType {
        DuplicatedPrimaryKey,
        RequestedFieldIsMissing,
        WholeRowIsEmpty,
        NumericFieldIsNotNumeric
    }

    /// <summary>
    /// List of problem severity.
    /// </summary>
    public enum Severity {
        Trace,          // Not used in this project
        Debug,          // Not used in this project
        Information,    // Skip
        Warning,        // Notify
        Error,          // Skip + Notify
        Fatal           // Not used in this project
    }
}