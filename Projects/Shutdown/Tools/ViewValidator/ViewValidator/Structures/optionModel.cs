using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ViewValidator.Structures {
    public class OptionModel {

        public OptionModel() {
            this.RemoveZeros = true;
        }

        public bool RemoveZeros { get ; set; }

    }
}
