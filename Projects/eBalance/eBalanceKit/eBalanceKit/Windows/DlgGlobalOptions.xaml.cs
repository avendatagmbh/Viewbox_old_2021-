using System.Windows;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgGlobalOptions.xaml
    /// </summary>
    public partial class DlgGlobalOptions : Window {
        public DlgGlobalOptions() {
            InitializeComponent();
        }
        public DlgGlobalOptions(object mainWindowModel) {
            DataContext = new Models.GlobalOptionsModel(mainWindowModel as Models.MainWindowModel);
            InitializeComponent();
        }
    }
}
