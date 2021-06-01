// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.Models {
    /// <summary>
    /// This class contains the accounts, which should be dragged from the list view into the tree view or vice versa.
    /// </summary>
    internal class TemplateGuiDragDropData {
        public TemplateGuiDragDropData(List<MappingLineGui> items) { Items = items; }
        public List<MappingLineGui> Items { get; private set; }
        public bool AllowDrop { get; set; }
    }
}