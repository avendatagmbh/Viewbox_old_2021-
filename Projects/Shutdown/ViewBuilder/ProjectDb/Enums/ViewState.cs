namespace ProjectDb.Enums
{
    /// <summary>
    ///   Enumeration of all avaliable view states.
    /// </summary>
    public enum ViewStates
    {
        /// <summary>
        ///   The view is identical to the viewscript version.
        /// </summary>
        OK,

        /// <summary>
        ///   The viewscript has been changed.
        /// </summary>
        Changed,

        /// <summary>
        ///   The assosiated viewscript file could not be found.
        /// </summary>
        Unknown
    }
}