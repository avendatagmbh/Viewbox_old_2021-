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
using Business.Interfaces;
using Base.Localisation;
using Config.Interfaces.DbStructure;
using Business;
using TransDATA.Models;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlImportDbStructure.xaml
    /// </summary>
    public partial class CtlImportDbStructure : UserControl {
        public CtlImportDbStructure() { InitializeComponent(); }

        ImportDbStructureModel Model {
            get { return DataContext as ImportDbStructureModel; }
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { Model.Cancel(); }


    }
}
