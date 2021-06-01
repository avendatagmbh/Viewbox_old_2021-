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
using System.Windows.Shapes;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    /// <summary>
    /// Interaktionslogik für DlgEditTemplate.xaml
    /// </summary>
    public partial class DlgEditTemplate : Window {
        public DlgEditTemplate(MappingTemplateHead template) {
            InitializeComponent();
            DataContext = template;
        }
    }
}
