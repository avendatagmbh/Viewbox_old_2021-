// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Taxonomy;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKit.Structures;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.Models {
    internal class NavigationTreeMappingTemplates : IEnumerable<NavigationTreeEntryBase> {
        public NavigationTreeMappingTemplates(MappingTemplateHead template, IEnumerable<MappingHeaderGui> assignments, Popup dragDropPopup) {
            Template = template;
            Init(assignments, dragDropPopup);
        }

        #region properties
        private MappingTemplateHead Template { get; set; }

        #region Children
        private readonly ObservableCollection<NavigationTreeEntryBase> _children =
            new ObservableCollection<NavigationTreeEntryBase>();

        public IEnumerable<NavigationTreeEntryBase> Children { get { return _children; } }
        #endregion Children;

        #endregion properties

        #region methods
        private void Init(IEnumerable<MappingHeaderGui> assignments, Popup dragDropPopup) {
            foreach (
                var ptree in MappingTemplatePresentationTrees.Create(Template, assignments).Where(
                    ptree => ptree.Role.Style.IsVisible && ptree.Role.Style.ShowBalanceList)) {
                switch (ptree.Role.Id) {
                    case "role_detailedInformation":
                    case "role_transfersCommercialCodeToTax":
                        continue;

                    default:
                        AddNavigationTreeStyle(ptree, dragDropPopup);
                        break;
                }
            }

            _children.First().IsSelected = true;
        }
        #endregion methods

        #region AddNavigationTreeStyle
        private void AddNavigationTreeStyle(IPresentationTree ptree, Popup dragDropPopup) {
            var treeView = new CtlTemplateTreeView {DragDropPopup = dragDropPopup};
            
            AddNavigationTreeEntry(
                ptree.Role.Name,
                treeView, 
                ptree);
        }
        #endregion AddNavigationTreeStyle

        #region AddNavigationTreeEntry
        private void AddNavigationTreeEntry(
            string header,
            UIElement elem,
            object dataContext,
            IElement xbrlElem = null) {
            ((FrameworkElement)elem).DataContext = dataContext;

            var entry = new NavigationTreeEntryBase {
                Header = header,
                Content = elem,
                XbrlElem = xbrlElem,
                Model = dataContext,
                IsVisible = true
            };

            _children.Add(entry);
        }
        #endregion AddNavigationTreeEntry

        #region GetEnumerator
        public IEnumerator<NavigationTreeEntryBase> GetEnumerator() { return Children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Children.GetEnumerator(); }
        #endregion GetEnumerator

    }
}