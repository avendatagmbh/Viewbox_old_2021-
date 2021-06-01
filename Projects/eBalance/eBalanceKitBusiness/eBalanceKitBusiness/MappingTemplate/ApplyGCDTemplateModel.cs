using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Collections.Generic;
using PdfGenerator;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Document = eBalanceKitBusiness.Structures.DbMapping.Document;

namespace eBalanceKitBusiness.MappingTemplate
{
    public class ApplyGCDTemplateModel : TemplateModelBase
    {
        public ApplyGCDTemplateModel(Document document, MappingTemplateHeadGCD template)
            : base(document, template)
        {

            AssignmentErrors = new List<AssignmentErrorListEntry>();
        }
        /*
        public AccountSplittingModel SplittingModel { get; set; }

        public AccountGroupingModel GroupingModel { get; set; }

        protected override void UpdateBalanceListCollection()
        {
            base.UpdateBalanceListCollection();
            SplittingModel = new AccountSplittingModel(this);
            GroupingModel = new AccountGroupingModel(this);
            OnPropertyChanged("SplittingModel");
            OnPropertyChanged("GroupingModel");
        }
         */

        public bool ReplaceAutoComputeEnabledFlag { get; set; }
        public bool ReplaceSendAccountBalanceFlag { get; set; }
        public bool ReplaceIgnoreWarningMessageFlag { get; set; }

        public List<AssignmentErrorListEntry> AssignmentErrors { get; private set; }

        public void SaveResults(string fileName)
        {

        }
    }
}
