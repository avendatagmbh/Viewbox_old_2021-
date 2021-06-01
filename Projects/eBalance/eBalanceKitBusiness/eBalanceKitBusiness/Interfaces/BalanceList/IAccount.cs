using Taxonomy.Interfaces.PresentationTree;

namespace eBalanceKitBusiness {

    public interface IAccount : IBalanceListEntry {
        IAccountGroup AccountGroup { get; set; }
        bool IsUserDefined { get; set; }
        ISplitAccountGroup CreateSplitAccountGroup();
        /// <summary>
        /// Checks if an assignment that is in process is allowed. Requiered for virtual accounts which fire an AssignmentNotAllowedException.
        /// </summary>
        /// <param name="assignedNode">The destination IPresentationTreeNode where this account should be assigned to.</param>
        /// <returns>Is it allowed?</returns>
        /// <remarks>
        /// This function has been implemented because otherwise we could get an ininite loop while trying to assign an virtual account 
        /// to a position that is calculation base for this virtual account.
        /// </remarks>
        /// <Author>Sebastian Vetter</Author>
        bool CheckAssignmentAllowed(IPresentationTreeNode assignedNode);

        bool IsAssignedToReferenceList { get; set; }
    }
}
