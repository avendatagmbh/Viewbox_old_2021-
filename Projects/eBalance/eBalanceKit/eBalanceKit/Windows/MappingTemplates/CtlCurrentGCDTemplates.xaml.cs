using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKit.Windows.MappingTemplates.Models;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for CtlCurrentGCDTemplates.xaml
    /// </summary>
    public partial class CtlCurrentGCDTemplates : UserControl
    {
        public CtlCurrentGCDTemplates()
        {
            InitializeComponent();
        }
         private DlgGCDTemplatesModel Model { get { return DataContext as DlgGCDTemplatesModel; } }
    }
}
