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
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant {
    /// <summary>
    /// Interaktionslogik für DlgManagementAssistant.xaml
    /// </summary>
    public partial class DlgManagementAssistant : Window {
        public DlgManagementAssistant() {
            InitializeComponent();
            DataContext = new Model.ManagementAssistantModel(this);
        }

        private void assistantControl_Next(object sender, RoutedEventArgs e) {
            //var model = (DataContext as Model.ManagementAssistantModel);
            //if (model != null && assistantControl.SelectedIndex == 0) {
            //    model.Reset();
            //}
            //if (model != null && assistantControl.SelectedIndex == 4) {
            //    if (!model.SetFinancialYear()) {
            //        assistantControl.NavigateBack();
            //        MessageBox.Show(string.Format(ResourcesManamgement.InsufficentRightsForSelected, ResourcesMain.FinancialYear), ResourcesCommon.InsufficientRights, MessageBoxButton.OK, MessageBoxImage.Error);
            //    } else {
            //        //assistantControl.NavigateNext();
            //    }
            //}

            //if (model != null && (assistantControl.SelectedItem is TabItem) && (model.AvailableDocuments == null || !model.AvailableDocuments.Any()) && assistantControl.SelectedIndex > 0) {

            //    var child = Utils.UIHelpers.FindVisualChild<CtlYesNo>(assistantControl.SelectedItem as TabItem); //.DataContext is Model.AskingDocumentModel
            //    if (child != null && child.DataContext is Model.AskingDocumentModel) {
            //        (child.DataContext as Model.AskingDocumentModel).CmdYes.Execute(null);
            //    }
            //    Control ctl = assistantControl.SelectedItem as TabItem;
            //    child = FindChild<CtlYesNo>(ctl);

            //    if (child != null && child.DataContext is Model.AskingDocumentModel) {
            //        (child.DataContext as Model.AskingDocumentModel).CmdYes.Execute(null);
            //    }

            //}

        }

        T FindChild<T>(DependencyObject control) where T : DependencyObject {

            if (control is T) {
                return (T) control;
            }

            if (control is TabItem) {
                return FindChild<T>((control as TabItem).Content as FrameworkElement);
                    }
            if (control is Grid) {
                return FindChild<T>((control as Grid).Children);
            }
            
            return default(T);
        }

        T FindChild<T>(IEnumerable<DependencyObject> start) where T : DependencyObject {
            T result = default(T);
            foreach (var control in start) {
                result = FindChild<T>(control);
                if (!result.Equals(default(T))) {
                    break;
                }
            }
            return result;
        }

        T FindChild<T>(UIElementCollection start) where T : DependencyObject {
            T result = null;
            foreach (var control in start) {
                result = FindChild<T>(control as DependencyObject);
                if (result != null) {
                    break;
                }
            }
            return result;
        }
    }
}
