// -----------------------------------------------------------
// Created by Benjamin Held - 30.08.2011 10:27:03
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using ViewValidator.Controls.Datagrid;
using ViewValidator.Models.Datagrid;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Factories {
    public class HeaderFactory {
        //private static Image validationImage, viewImage;
        //private static Image[] images;

        private static Image LoadImage(string source, int width) {
            Image result = new Image();
            if(width != 0) result.Width = width;
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri("pack://application:,,,/ViewValidator;component/" + source);
            logo.EndInit();
            result.Source = logo;

            return result;
        }

        static HeaderFactory() {
            //validationImage = LoadImage("Resources/ValidationDatabase.png", 16);
            //viewImage = LoadImage("Resources/ViewDatabase.png", 16);

            //images = new Image[2] { validationImage, viewImage };
        }

        public static UserControl GetGridViewHeader(int which, Column column) {
            CtlColumnHeader control = new CtlColumnHeader();
            control.DataContext = new ColumnHeaderModel(column, which, control);
            return control;
        }

        class DataGridPair {
            public DataGrid Grid { get; set; }
            public int Which { get; set; }

            public override bool Equals(object obj) {
                if (obj == null) return false;
                DataGridPair other = obj as DataGridPair;
                if (other == null) return false;
                return Grid == other.Grid && Which == other.Which;
            }

            public override int GetHashCode() {
                return Grid.GetHashCode() ^ Which;
            }
        }

        public static ColumnHeaderModel HeaderToHeaderModel(object header) {
            return (ColumnHeaderModel)header;
        }


        private static List<ColumnMapping> _columnMappings = new List<ColumnMapping>();
        private static List<DataGridPair> _dataGrids = new List<DataGridPair>();
        static void columnMapping_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            ColumnMapping mapping = sender as ColumnMapping;
            foreach (var pair in _dataGrids) {
                foreach (var column in pair.Grid.Columns)
                    if (HeaderToHeaderModel(column.Header).Column == mapping.GetColumn(pair.Which))
                        column.Visibility = mapping.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void RegisterForColumnVisibilityHandling(DataGrid grid, int index, ColumnMapping columnMapping) {
            if (!_columnMappings.Contains(columnMapping)) {
                _columnMappings.Add(columnMapping);
                columnMapping.PropertyChanged += columnMapping_PropertyChanged;
            }
            DataGridPair pair = new DataGridPair() {Grid = grid, Which = index};
            if (!_dataGrids.Contains(pair)) {
                _dataGrids.Add(pair);
            }
        }

        public static void SetHeader(DataGridTextColumn column, ColumnMapping columnMapping, int index) {
            Style headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(Control.TemplateProperty, Application.Current.TryFindResource("ColumnHeaderTemplate")));
            column.HeaderStyle = headerStyle;

            column.Header = new ColumnHeaderModel(columnMapping.GetColumn(index), index, null);
        }
    }
}
