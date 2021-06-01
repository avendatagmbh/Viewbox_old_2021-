using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for ToolTip on treenode.
    /// </summary>
    /// <author>MGabor Bauer</author>
    /// <since>2012-05-16</since>
    public interface IRightToolTipTreeNode {
        string ToolTip { get; set; }
    }
}
