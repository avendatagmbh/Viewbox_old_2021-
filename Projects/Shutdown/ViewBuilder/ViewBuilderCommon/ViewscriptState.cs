namespace ViewBuilderCommon
{
    /// <summary>
    ///   Enumeration of all avaliable viewscript states.
    /// </summary>
    public enum ViewscriptStates
    {
        /// <summary>
        ///   The view is ready to be created.
        /// </summary>
        Ready,

        /// <summary>
        ///   The neccesary indizes are beeing created.
        /// </summary>
        CreatingIndex,

        /// <summary>
        ///   An error occured during the create index process.
        /// </summary>
        CreateIndexError,

        /// <summary>
        ///   The view table is beeing created.
        /// </summary>
        CreatingTable,

        /// <summary>
        ///   An error occured during the create table process.
        /// </summary>
        CreateTableError,

        /// <summary>
        ///   The view table is copying to the data-db.
        /// </summary>
        CopyingTable,

        /// <summary>
        ///   An error occured during the copy table process.
        /// </summary>
        CopyError,

        /// <summary>
        ///   The view creation has been finished.
        /// </summary>
        Completed,

        /// <summary>
        ///   The view creation has warnings.
        /// </summary>
        Warning,

        /// <summary>
        ///   Getting index information for the current viewtable which will be visible within viewbox
        /// </summary>
        GettingIndexInfo,

        /// <summary>
        ///   An error occured while getting index information
        /// </summary>
        GettingIndexInfoError,

        /// <summary>
        ///   Generating distinct values for parameters which will be visualized in a popup window within viewbox as a value picker
        /// </summary>
        GeneratingDistinctValues,

        /// <summary>
        ///   An error occured while generating distinct values
        /// </summary>
        GeneratingDistinctValuesError,

        /// <summary>
        ///   Validating the existence of procedure parameters
        /// </summary>
        CheckingProcedureParameters,

        /// <summary>
        ///   An error occured while validating the existence of procedure parameters
        /// </summary>
        CheckingProcedureParametersError,

        /// <summary>
        ///   Validating the existence of report parameters
        /// </summary>
        CheckingReportParameters,

        /// <summary>
        ///   An error occured while validating the existence of report parameters
        /// </summary>
        CheckingReportParametersError,

        /// <summary>
        ///   Validating where condition
        /// </summary>
        CheckingWhereCondition,

        /// <summary>
        ///   An error occured while validating where condition
        /// </summary>
        CheckingWhereConditionError,
    }
}