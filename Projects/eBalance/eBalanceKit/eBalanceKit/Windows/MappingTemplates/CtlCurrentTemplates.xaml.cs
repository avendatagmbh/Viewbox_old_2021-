// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKit.Windows.MappingTemplates.Models;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class CtlCurrentTemplates : UserControl {
        public CtlCurrentTemplates() { InitializeComponent(); }
        private DlgTemplatesModel Model { get { return DataContext as DlgTemplatesModel; } }
    }
}