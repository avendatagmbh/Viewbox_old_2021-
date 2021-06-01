// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Interfaces;

namespace Taxonomy {
    public class XbrlDisplayStyle : IComparable{
        internal XbrlDisplayStyle() {
            IsVisible = true;
            Ordinal = 9999;
            ShowBalanceList = true;
        }

        public ITaxonomy Taxonomy { get; internal set; }
        public bool IsVisible { get; set; }
        public int Ordinal { get; set; }
        public bool ShowBalanceList { get; set; }

        public int CompareTo(object obj) {
            var xbrlDisplayStyle = obj as XbrlDisplayStyle;
            return xbrlDisplayStyle != null ? Ordinal.CompareTo(xbrlDisplayStyle.Ordinal) : 0;
        }
    }
}