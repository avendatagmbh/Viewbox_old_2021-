using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SKR2MappingTemplate
{
    public class MappingData
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string TaxonomyName { get; set; }

        public MappingData(string accountNumber, string accountName, string taxonomyName)
        {
            AccountNumber = accountNumber;
            AccountName = accountName;
            TaxonomyName = taxonomyName;
        }
    }
}
