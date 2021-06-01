using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBase.Structures
{
    /// <summary>
    /// Class for ILockableSource parameter
    /// </summary>
    public class LockableSourceParameter
    {
        #region Properties

        public ProgressInfo ProgressInfo { get; set; }
        public bool LockOnlySelectedEntry { get; set; }

        #endregion

        #region Methods
        public LockableSourceParameter(ProgressInfo progressInfo = null, bool lockOnlySelectedEntry = true)
        {
            ProgressInfo = progressInfo;
            LockOnlySelectedEntry = lockOnlySelectedEntry;
        }
        #endregion Methods
    }
}
