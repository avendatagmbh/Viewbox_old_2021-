using System;

namespace AvdCommon.Rules.SortRules
{
    internal class RuleSortDateTime : SortRule
    {
        public RuleSortDateTime()
        {
            Name = "Sortiere als Datum";
            UniqueName = "SortDateTime";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override int Sort(string v1, string v2)
        {
            DateTime value1, value2;
            if (DateTime.TryParse(v1, out value1) && DateTime.TryParse(v2, out value2))
            {
                return value1.CompareTo(value2);
            }
            return base.Sort(v1, v2);
        }
    }
}