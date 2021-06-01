// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.MappingTemplate {
    public class CreateTemplateModel : TemplateModelBase {

        public CreateTemplateModel(Document document, MappingTemplateHead template)
            : base(document, template) { }
    }
}