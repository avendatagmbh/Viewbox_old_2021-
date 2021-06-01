using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidator.Models.Rules {
    class RowEntryDetailsModel {
        public RowEntryDetailsModel(IRowEntry rowEntry) {
            this.RowEntry = rowEntry;
        }

        public IRowEntry RowEntry { get; set; }

        public string RuleDisplayStringHex { get { return StringToHex(RowEntry.RuleDisplayString); } }
        public string DisplayStringHex { get { return StringToHex(RowEntry.DisplayString); } }

        private string StringToHex(string hexstring) {
            var sb = new StringBuilder();
            foreach (char t in hexstring)
                sb.Append("0x" + System.Convert.ToInt32(t).ToString("x") + " ");
            return sb.ToString();
        }
    }
}
