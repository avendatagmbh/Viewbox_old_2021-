namespace Utils.Commands
{
    /// <summary>
    ///   Defines command actions.
    /// </summary>
    public enum CommandAction
    {
        /// <summary>
        ///   No action.
        /// </summary>
        None,

        /// <summary>
        ///   Disable the element if the command can't be executed.
        /// </summary>
        Disable,

        /// <summary>
        ///   Hide the element if the command can't be executed.
        /// </summary>
        Hide
    }
}