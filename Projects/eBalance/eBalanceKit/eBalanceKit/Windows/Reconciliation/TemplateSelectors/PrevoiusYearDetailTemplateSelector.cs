// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    public class PrevoiusYearDetailTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            if (item is ITransactionGroup) return element.FindResource("TransactionGroupItem") as DataTemplate;
            if (item is IReconciliationTransaction) {
                var transaction = item as IReconciliationTransaction;
                switch (transaction.Reconciliation.ReconciliationType) {
                    case ReconciliationTypes.PreviousYearValues:
                        return element.FindResource("PreviousYearTransaction") as DataTemplate;

                    case ReconciliationTypes.AuditCorrectionPreviousYear:
                        return element.FindResource("AuditCorrectionPreviousYearTransaction") as DataTemplate;

                    default:
                        throw new ArgumentOutOfRangeException();
                }                
            }
            return null;
        }
    }
}