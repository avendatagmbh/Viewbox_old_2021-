using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.InitialSetup.StoredProcedures;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für CtlStoredProcedure.xaml
    /// </summary>
    public partial class CtlStoredProcedure : UserControl {
        StoredProcedureModel Model { get { return DataContext as StoredProcedureModel; } }

        public CtlStoredProcedure() {
            InitializeComponent();
        }

        public void FillParameterGrid(TableMapping tableMapping) {
            StoredProcedure proc = tableMapping.StoredProcedure;

            parameterGrid.Children.Clear();
            parameterGrid.RowDefinitions.Clear();
            foreach (var param in proc.Arguments) {
                parameterGrid.RowDefinitions.Add(new RowDefinition(){Height=GridLength.Auto});
            }

            
            var collection = from arg in proc.Arguments orderby arg.Ordinal select arg;

            int row = 0;
            foreach (var arg in collection) {
                Label label = new Label();
                label.Content = arg.Name + " (" + arg.Description + "," + arg.Type + ")";
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                parameterGrid.Children.Add(label);

                TextBox textBox = new TextBox();
                int id = proc.Arguments.IndexOf(arg);
                textBox.SetBinding(TextBox.TextProperty, new Binding(string.Format("StoredProcedure.Arguments[{0}].Value", id)){Mode = BindingMode.TwoWay});
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, 2);
                parameterGrid.Children.Add(textBox);

                ++row;
            }

            //for (int i = 0; i < proc.Arguments.Count; ++i) {
            //    ProcedureArgument param = proc.Arguments[i];

            //    Label label = new Label();
            //    label.Content = param.Name;
            //    Grid.SetRow(label, i);
            //    Grid.SetColumn(label, 0);
            //    parameterGrid.Children.Add(label);

            //    TextBox textBox = new TextBox();
            //    Grid.SetRow(textBox, i);
            //    Grid.SetColumn(textBox, 2);
            //    parameterGrid.Children.Add(textBox);


            //}
        }

        private void btnCallProcedure_Click(object sender, RoutedEventArgs e) {

            Model.CallProcedure(UIHelpers.TryFindParent<Window>(this));
        }
    }
}
