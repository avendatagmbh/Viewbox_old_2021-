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
using AvdCommon.DataGridHelper;
using ScreenshotAnalyzer.Models;

namespace ScreenshotAnalyzer.Windows {
    /// <summary>
    /// Interaktionslogik für DlgTextTable.xaml
    /// </summary>
    public partial class DlgTextTable : Window {
        public DlgTextTable() {
            InitializeComponent();
        }

        TextTableModel Model { get { return DataContext as TextTableModel; } }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model == null) return;
            DataGridCreater.CreateColumns(dgvTextTable, Model.RecognitionResult.TextTable);
        }

    }
}
