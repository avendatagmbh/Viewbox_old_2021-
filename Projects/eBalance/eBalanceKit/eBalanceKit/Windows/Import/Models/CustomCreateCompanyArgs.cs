// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;

namespace eBalanceKit.Windows.Import.Models {
    public class CustomCreateCompanyArgs : System.EventArgs {
        public CustomCreateCompanyArgs(List<Tuple<DataTable, List<int>>> companyTableList) {
            CompanyTableList = companyTableList;
            ErrorList = new List<Exception>();
        }

        public List<Exception> ErrorList { get; set; }
        public List<Tuple<DataTable, List<int>>> CompanyTableList { get; private set; }
    }
}