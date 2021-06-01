using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using eBalanceKitBusiness;

namespace eBalanceKit.TemplateSelectors {

    class BalanceListTemplateSelector : DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            var cp = container as ContentPresenter;
            if (cp.Content is IAccount) {
                return cp.FindResource("BalanceListAccount") as DataTemplate;

            } if (cp.Content is IAccountGroup) {
                return cp.FindResource("BalanceListAccountGroup") as DataTemplate;

            } if (cp.Content is ISplittedAccount) {
                return cp.FindResource("BalanceListSplittedAccount") as DataTemplate;

            } else {
                // should not happen
                throw new Exception("Balance list entry type is not implemented: " + cp.Content.GetType().Name);
            }

        }
    }

    class BalanceListAssignedTemplateSelector : DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            var cp = container as ContentPresenter;
            if (cp.Content is IAccount) {
                return cp.FindResource("BalanceListAssignedAccount") as DataTemplate;

            } if (cp.Content is IAccountGroup) {
                return cp.FindResource("BalanceListAssignedAccountGroup") as DataTemplate;

            } if (cp.Content is ISplittedAccount) {
                return cp.FindResource("BalanceListAssignedSplittedAccount") as DataTemplate;

            } else {
                // should not happen
                throw new Exception("Balance list entry type is not implemented: " + cp.Content.GetType().Name);
            }

        }
    }

    class BalanceListDragDropTemplateSelector : DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            var cp = container as ContentPresenter;
            if (cp.Content is IAccount) {
                return cp.FindResource("BalanceListDragDropAccount") as DataTemplate;

            } if (cp.Content is IAccountGroup) {
                return cp.FindResource("BalanceListDragDropAccountGroup") as DataTemplate;

            } if (cp.Content is ISplittedAccount) {
                return cp.FindResource("BalanceListDragDropSplittedAccount") as DataTemplate;

            } else {
                // should not happen
                throw new Exception("Balance list entry type is not implemented: " + cp.Content.GetType().Name);
            }
        }

    }


    class EditBalanceListAccountGroupTemplateSelector : DataTemplateSelector {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            if (container is TreeViewItem) {
                var c = container as TreeViewItem;
                if (c.Header is IAccountGroup) return c.FindResource("AccountGroup") as DataTemplate;
                else if (c.Header is IAccount) return c.FindResource("Account") as DataTemplate;
                else throw new Exception("Not implemented: " + c.Header.GetType().Name);
            
            } else if (container is ContentPresenter) {
                var c = container as ContentPresenter;
                if (c.Content is IAccountGroup) return c.FindResource("AccountGroup") as DataTemplate;
                else if (c.Content is IAccount) return c.FindResource("Account") as DataTemplate;
                else throw new Exception("Not implemented: " + c.Content.GetType().Name);
            
            } else {
                throw new Exception("Not implemented exception.");
            }
        }
    }
}
