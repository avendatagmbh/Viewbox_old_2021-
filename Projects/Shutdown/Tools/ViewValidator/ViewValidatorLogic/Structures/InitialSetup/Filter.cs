// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:40
// Copyright AvenDATA 2011
// -----------------------------------------------------------

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class Filter {
        public string FilterString { get; set; }
        public string DisplayString { get { return string.IsNullOrEmpty(FilterString) ? "Keiner" : FilterString; } }

        public Filter(string filter) {
            this.FilterString = filter;
        }

        public string GetFilterSQL() {
            if (string.IsNullOrEmpty(FilterString.Replace(" ", "").Replace("\n", "").Replace("\r", ""))) return "";
            if (FilterString.ToUpper().StartsWith("WHERE")) return FilterString;
            else return "WHERE " + FilterString;
        }
    }
}
