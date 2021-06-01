using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels
{
    public interface IItemWrapperStructure
    {
        string Text { get; set; }
        string Name { get; }
        int Id { get; }
    }
}
