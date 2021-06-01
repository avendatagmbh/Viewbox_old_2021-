/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2012-01-18      initial implementation
 *************************************************************************************************************/

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
using AvdCommon.DataGridHelper.Interfaces;
using eBalanceKit.Models.Assistants;
using System.Windows.Controls.Primitives;
using eBalanceKitBusiness.Import;
using System.Reflection;

namespace eBalanceKit.Controls.BalanceList {
    
    /// <summary>
    /// Interaktionslogik für BalListImpAssistColumnSelection.xaml
    /// </summary>
    public partial class BalListImpAssistPageColumnSelection : BalListImpAssistPageBase {

        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImpAssistPageColumnSelection"/> class.
        /// </summary>
        public BalListImpAssistPageColumnSelection() {
            InitializeComponent();
            preview.ColumnSelected += new EventHandler(preview_ColumnSelected);

            //txtHeader.DataContext = this;
            //txtHeader.SetBinding(TextBlock.TextProperty, "Caption");
        }

        BalListImportAssistantModel Model {
            get { return this.DataContext as BalListImportAssistantModel; }
        }

        void preview_ColumnSelected(object sender, System.EventArgs e) {
            BalanceListImportConfig config = this.Model.Importer.Config;
            config.ColumnDict[SelectedColumnBinding] = preview.SelectedColumn;
            foreach (IDataColumn dataColumn in Model.Importer.PreviewData.Columns) {
                dataColumn.Color = System.Drawing.Color.White;
            }
            var elements = from keyVal in Model.Importer.Config.ColumnDict
                           where !string.IsNullOrEmpty(keyVal.Value)
                           select keyVal;
            foreach (var keyVal in elements) {
                foreach (IDataColumn column in Model.Importer.PreviewData.Columns) {
                    if (column.Name == keyVal.Value) {
                        column.Color = config.ColorDict[keyVal.Key];
                        break;
                    }
                }
            }
            config.OnPropertyChanged("ColumnDict");
            OnColumnSelected();
        }

        public event EventHandler ColumnSelected;
        private void OnColumnSelected() {
            if (ColumnSelected != null) ColumnSelected(this, new System.EventArgs());
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Validate() {
            BalanceListImportConfig config = this.Model.Importer.Config;
            //if (string.IsNullOrEmpty(preview.SelectedColumn)) {
            if (!config.ColumnDict.ContainsKey(this.SelectedColumnBinding) || string.IsNullOrEmpty(config.ColumnDict[this.SelectedColumnBinding])) {
                txtWarning.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            txtWarning.Visibility = System.Windows.Visibility.Collapsed; 
            return true;
        }

        public string SelectedColumnBinding {
            get { return (string)GetValue(SelectedColumnBindingProperty); }
            set { 
                SetValue(SelectedColumnBindingProperty, value);
                txtSelectedColumn.SetBinding(
                    TextBlock.TextProperty,
                    new Binding("Importer.Config.ColumnDict[" + value + "]") { Converter = new Converters.StringToColDescriptionConverter() });
            }
        }
        
        // Using a DependencyProperty as the backing store for SelectedColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColumnBindingProperty =
            DependencyProperty.Register("SelectedColumnBinding", typeof(string), typeof(ImportDataPreview), new UIPropertyMetadata(null));



        public string Caption {
            get { return eBalanceKitResources.Localisation.ResourcesBalanceList.ColumnChoice + (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(BalListImpAssistPageColumnSelection), new UIPropertyMetadata("???"));

        
    }
}
