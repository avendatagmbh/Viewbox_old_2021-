using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public interface ILockable
    {
        bool Locked { get; set; }
    }
}
