using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    public class ImportedValuesDetailTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            if (item is ITransactionGroup) return element.FindResource("TransactionGroupItem") as DataTemplate;
            if (item is IReconciliationTransaction) return element.FindResource("ImportedValuesReconciliationTransaction") as DataTemplate;
            return null;
        }
    }
}
