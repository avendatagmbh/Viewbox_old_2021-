using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBase.Interfaces {
    public interface INavigationTree {
        INavigationTreeEntryBase NavigationTreeReport { get; set; }
    }
}
