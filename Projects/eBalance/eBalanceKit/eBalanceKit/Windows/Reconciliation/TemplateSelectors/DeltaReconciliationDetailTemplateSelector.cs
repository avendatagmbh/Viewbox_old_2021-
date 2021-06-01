// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    public class DeltaReconciliationDetailTemplateSelector : DataTemplateSelector{
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            if (item is ITransactionGroup) return element.FindResource("TransactionGroupItem") as DataTemplate;
            if (item is IReconciliationTransaction) return element.FindResource("DeltaReconciliationTransaction") as DataTemplate;
            return null;
        }
    }
}