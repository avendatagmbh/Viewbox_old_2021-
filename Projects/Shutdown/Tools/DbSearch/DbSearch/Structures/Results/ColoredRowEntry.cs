using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using AvdCommon.DataGridHelper.Interfaces;

namespace DbSearch.Structures.Results {
    class ColoredRowEntry : IDataRowEntry{
        public ColoredRowEntry(object value, Brush background = null) {
            _displayString = value == null ? string.Empty : value.ToString();
            Background = background ?? Brushes.White;
        }

        private string _displayString;
        public string DisplayString {
            get { return _displayString; }
        }

        public Brush Background { get; set; }
    }
}
