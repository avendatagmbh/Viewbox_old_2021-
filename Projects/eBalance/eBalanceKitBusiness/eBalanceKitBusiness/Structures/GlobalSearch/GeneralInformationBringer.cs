using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Taxonomy;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Interfaces;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    internal class GeneralInformationBringer : SpecialBringerIntoViewStrategy {

        /// <summary>
        /// If the searched element inside General information, this method brings the searched element into view.
        /// </summary>
        /// <param name="presentationTreeEntryPath">the searched element's path</param>
        /// <param name="navigationTreeEntry">the general information navigation tree node</param>
        /// <param name="setSelectedEntry">The setter of MainWindowModel's selected navigation tree property</param>
        public override void BringIntoView(List<ISearchableNode> presentationTreeEntryPath,
                                           INavigationTreeEntryBase navigationTreeEntry, Action<INavigationTreeEntryBase> setSelectedEntry) {
            if (presentationTreeEntryPath[0].Element.Id == "de-gcd_genInfo" &&
                presentationTreeEntryPath[1].Element.Id == "de-gcd_genInfo.report" &&
                presentationTreeEntryPath[2].Element.Id == "de-gcd_genInfo.report.id" &&
                presentationTreeEntryPath[3].Element.Id == "de-gcd_genInfo.report.id.accordingTo") {
                RecursiveClose(navigationTreeEntry);
                if (!navigationTreeEntry.IsExpanded)
                    navigationTreeEntry.IsExpanded = true;
                if (!navigationTreeEntry.Children[1].IsExpanded)
                    navigationTreeEntry.Children[1].IsExpanded = true;
                if (!navigationTreeEntry.Children[1].Children[1].IsExpanded)
                    navigationTreeEntry.Children[1].Children[1].IsExpanded = true;
                //Dispatcher.CurrentDispatcher.BeginInvoke(
                //    (Action) delegate { navigationTreeEntry.Children[1].Children[1].IsSelected = true; },
                //    DispatcherPriority.SystemIdle);
                navigationTreeEntry.Children[1].Children[1].IsSelected = true;
                setSelectedEntry(navigationTreeEntry.Children[1].Children[1]);
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    (Action) (() => presentationTreeEntryPath.Last().ScrollIntoView(presentationTreeEntryPath)),
                    DispatcherPriority.SystemIdle);
                return;
            }
            if (presentationTreeEntryPath[0].Element.Id != "de-gcd_genInfo" ||
                presentationTreeEntryPath[1].Element.Id != "de-gcd_genInfo.company" ||
                presentationTreeEntryPath[2].Element.Id != "de-gcd_genInfo.company.id") {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    (Action) (() => presentationTreeEntryPath.Last().ScrollIntoView(presentationTreeEntryPath)),
                    DispatcherPriority.SystemIdle);
                return;
            }
            RecursiveClose(navigationTreeEntry);
            //setSelectedEntry(navigationTreeEntry);
            if (!navigationTreeEntry.IsExpanded) {
                navigationTreeEntry.IsExpanded = true;
            }
            INavigationTreeEntryBase companyId = navigationTreeEntry.Children[2];
            if (!companyId.IsExpanded)
                companyId.IsExpanded = true;
            // DEVNOTE: hack
            // the level under company id is artificial. It's a switch to diference between the 8 node under company id.
            // hard to test if everything is ok.
            switch (presentationTreeEntryPath[3].Element.Id) {
                case "de-gcd_genInfo.company.id.name":
                case "de-gcd_genInfo.company.id.legalStatus":
                case "de-gcd_genInfo.company.id.legalStatus.formerStatus":
                case "de-gcd_genInfo.company.id.legalStatus.dateOfLastChange":
                case "de-gcd_genInfo.company.id.location":
                    if (!companyId.Children[0].IsExpanded)
                        companyId.Children[0].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[0].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[0]);
                    companyId.Children[0].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.idNo":
                    if (!companyId.Children[1].IsExpanded)
                        companyId.Children[1].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[1].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[1]);
                    companyId.Children[1].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.shareholder":
                    if (!companyId.Children[2].IsExpanded)
                        companyId.Children[2].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[2].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[2]);
                    companyId.Children[2].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.Incorporation":
                    if (!companyId.Children[3].IsExpanded)
                        companyId.Children[3].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[3].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[3]);
                    companyId.Children[3].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.stockExch":
                    if (!companyId.Children[4].IsExpanded)
                        companyId.Children[4].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[4].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[4]);
                    companyId.Children[4].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.contactAddress":
                    if (!companyId.Children[5].IsExpanded)
                        companyId.Children[5].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[5].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[5]);
                    companyId.Children[5].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.lastTaxAudit":
                case "de-gcd_genInfo.company.id.sizeClass":
                case "de-gcd_genInfo.company.id.business":
                case "de-gcd_genInfo.company.id.CompanyStatus":
                case "de-gcd_genInfo.company.id.FoundationDate":
                case "de-gcd_genInfo.company.id.internet":
                case "de-gcd_genInfo.company.id.industry":
                case "de-gcd_genInfo.company.id.taxGroupKstEst":
                case "de-gcd_genInfo.company.id.companyLogo":
                case "de-gcd_genInfo.company.id.bankAcct":
                case "de-gcd_genInfo.company.id.comingfrom":
                    if (!companyId.Children[6].IsExpanded)
                        companyId.Children[6].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[6].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[6]);
                    companyId.Children[6].IsSelected = true;
                    break;
                case "de-gcd_genInfo.company.id.parent":
                    if (!companyId.Children[7].IsExpanded)
                        companyId.Children[7].IsExpanded = true;
                    //Dispatcher.CurrentDispatcher.BeginInvoke(
                    //    (Action) delegate { companyId.Children[7].IsSelected = true; }, DispatcherPriority.SystemIdle);
                    setSelectedEntry(companyId.Children[7]);
                    companyId.Children[7].IsSelected = true;
                    break;
                default:
                    Debug.Fail("The location of the node is not exactly known");
                    break;
            }
            Dispatcher.CurrentDispatcher.BeginInvoke(
                (Action) (() => presentationTreeEntryPath.Last().ScrollIntoView(presentationTreeEntryPath)),
                DispatcherPriority.SystemIdle);
        }
        
        /// <summary>
        /// Depth first traversal of the navigation tree. Closes every node. Used if we want to bring something into view manually.
        /// </summary>
        /// <param name="navigationTreeEntry">the root of closing</param>
        private void RecursiveClose(INavigationTreeEntryBase navigationTreeEntry) {
            foreach (INavigationTreeEntryBase child in navigationTreeEntry.Children) {
                RecursiveClose(child);
            }
            navigationTreeEntry.IsExpanded = false;
            navigationTreeEntry.IsSelected = false;
        }
    }
}