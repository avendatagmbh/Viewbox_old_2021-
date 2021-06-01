// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using Taxonomy;

namespace eBalanceKitBusiness.MappingTemplate {
    public class UpdateTemplateResultListEntry {
        internal UpdateTemplateResultListEntry(string account, string type, IElement previousAssignment) {
            Account = account;
            Type = type;
            PreviousAssignment = previousAssignment;
        }

        public string Account { get; private set; }
        public string Type { get; private set; }
        public IElement PreviousAssignment { get; private set; }
    }
}