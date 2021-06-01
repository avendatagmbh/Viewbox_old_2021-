// --------------------------------------------------------------------------------
// author: Lajos Szoke
// since: 2011-12-12
// --------------------------------------------------------------------------------

namespace eBalanceKitBase.Structures
{
    /// <summary>
    /// Interface for the easy usage of the SourceLocker class
    /// </summary>
    public interface ILockableSource
    {
        #region Methods
        /// <summary>
        /// It locks the sources
        /// </summary>
        /// <param name="parameters"></param>
        void LockSource(LockableSourceParameter parameters);

        /// <summary>
        /// It unlocks the sources
        /// </summary>
        /// <param name="parameters"></param>
        void UnLockSource(LockableSourceParameter parameters);
        #endregion Methods
    }
}
