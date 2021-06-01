using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels
{
    public interface IDispatcher {
        void Invoke(Action action);
    }
}
