// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.Models {
    internal class TemplateDetailModel {
        public TemplateDetailModel(MappingTemplateHead template, Popup dragDropPopup) {
            Template = template;

            foreach (var assignment in Template.Assignments)
                _assignments.Add(new MappingHeaderGui(assignment));

            NavigationTree = new NavigationTreeMappingTemplates(template, Assignments, dragDropPopup);
        }

        #region Assignments
        private readonly ObservableCollection<MappingHeaderGui> _assignments = new ObservableCollection<MappingHeaderGui>();
        public IEnumerable<MappingHeaderGui> Assignments { get { return _assignments; } }
        #endregion Assignments

        public MappingTemplateHead Template { get; private set; }
        public NavigationTreeMappingTemplates NavigationTree { get; private set; }
    }
}