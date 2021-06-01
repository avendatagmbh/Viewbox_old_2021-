using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.Structures
{
    public class MessageBoxActions : EventArgs {
        public MessageBoxActions(Action onYes, Action onNo) {
            this.OnYesClick = onYes;
            this.OnNoClick = onNo;
        }
        public Action OnYesClick { get; private set; }
        public Action OnNoClick { get; private set; }
    }
}
