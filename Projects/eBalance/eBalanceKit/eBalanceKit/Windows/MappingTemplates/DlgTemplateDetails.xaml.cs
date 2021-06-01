// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgTemplateDetails : Window {
        public DlgTemplateDetails(MappingTemplateHead template) {
            InitializeComponent();
            DataContext = new TemplateDetailModel(template, ctlTemplateDetails.DragDropPopup);
        }
    }
}