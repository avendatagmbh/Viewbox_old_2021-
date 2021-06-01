using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SystemDb;
using SystemDb.Compatibility.Viewbuilder.OptimizationRelated;
using AvdCommon.DataGridHelper;
using Utils;
using ViewBuilder.Converters;
using ViewBuilder.Windows;
using ViewBuilder.Windows.Optimizations;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Models {
    public class EditOptimizationsModel : NotifyPropertyChangedBase {
        #region Constructor
        public EditOptimizationsModel(ProfileConfig profile, Window owner) {
            Owner = owner;

            string system = profile.ConnectionManager.DbConfig.DbName.ToLower();
            var optimizations = profile.ViewboxDb.Optimizations.Where(
                (o) => o.Level == 1 && o.Value != null && o.Value.ToLower() == system).ToList();
            if (optimizations.Count > 1)
                throw new InvalidOperationException(string.Format("Es ist mehr als ein System mit dem Namen {0} vorhanden", system));

            Layers = new Layers(profile.ViewboxDb.Languages);
            Root = new Optimization(profile.ViewboxDb.Languages.Count, Layers);
            if(optimizations.Count == 1)
                Root.Children.Add(Optimization.CreateFromViewboxOptimization(optimizations[0],profile.ViewboxDb.Languages.Count, Layers));
            Layers.DetectLayers(Root);
            _profile = profile;
            EditableItems = new ObservableCollectionAsync<Optimization>();
        }
        #endregion Constructor

        #region Properties
        private readonly ProfileConfig _profile;
        public ProfileConfig Profile { get { return _profile; } }
        public Optimization Root { get; set; }
        public Window Owner { get; private set; }
        public Layers Layers { get; private set; }

        #region EditableItems
        private ObservableCollectionAsync<Optimization> _editableItems;

        public ObservableCollectionAsync<Optimization> EditableItems {
            get { return _editableItems; }
            set {
                if (_editableItems != value) {
                    _editableItems = value;
                    OnPropertyChanged("EditableItems");
                }
            }
        }

        #endregion EditableItems
        public bool IsInEditMode { get; set; }
        #endregion Properties

        #region Methods
        public void ManageLayers() {
            ManageLayersModel model = new ManageLayersModel(Layers, _profile.ViewboxDb.Languages);
            DlgManageLayers dlg = new DlgManageLayers() {DataContext = model, Owner = Owner};

            dlg.ShowDialog();
            if(model.Save) {
                Layers.ExtendOptimization(Layers, model.Layers, Root);
                for (int i = 0; i < Layers.Layer.Count; ++i) {
                    Layers.Layer[i].UseLayer = model.Layers.Layer[i].UseLayer;
                    for (int j = 0; j < Layers.Layer[i].Descriptions.Count; ++j)
                        Layers.Layer[i].Descriptions[j] = model.Layers.Layer[i].Descriptions[j];
                }
            }
        
        }

        internal class TempColumn {
            public string Header { get; set; }
            public string PropertyPath { get; set; }
            public IValueConverter TextBlockConverter { get; set; }
            public IValueConverter TextBoxConverter { get; set; }
            public bool IsReadOnly { get; set; }
        }

        public void SetupDataGrid(Optimization opt, DataGrid dataGrid) {
            dataGrid.Columns.Clear();
            //EditableItems.Clear();

            if (opt == null || opt.Parent == null || (opt.Group != null && opt.Group.Type == OptimizationType.System)) {
                EditableItems = null;
                return;
            }
            EditableItems = opt.Parent.Children;
            //Setup columns
            List<TempColumn> columns = new List<TempColumn>() { new TempColumn() { Header = "Wert", PropertyPath = "Value", TextBlockConverter = new StringToNullConverter(), TextBoxConverter = new StringToNullConverter() } };
            for(int i = 0;i < opt.Descriptions.Count; ++i)
                columns.Add(new TempColumn() { PropertyPath = string.Format("Descriptions[{0}]", i), Header = _profile.ViewboxDb.Languages.ElementAt(i).LanguageName, TextBlockConverter = new StringToNullConverter(), TextBoxConverter = new StringToNullConverter() });

            AddColumnsToDataGrid(dataGrid, columns);

        }

        internal static void AddColumnsToDataGrid(DataGrid dataGrid, List<TempColumn> columns) {
            foreach (var column in columns) {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn() {IsReadOnly = false};
                gridColumn.Header = column.Header;
                gridColumn.SortMemberPath = column.PropertyPath;

                //Set cell template
                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof (TextBlock));
                Binding b = new Binding(column.PropertyPath) {Converter = column.TextBlockConverter};
                textBlockFactory.SetValue(TextBlock.TextProperty, b);

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = textBlockFactory;
                gridColumn.CellTemplate = cellTemplate;

                //set edit cell template
                if (!column.IsReadOnly) {
                    FrameworkElementFactory textBoxFactory = new FrameworkElementFactory(typeof (TextBox));
                    textBoxFactory.SetValue(TextBox.TextProperty,
                                            new Binding(column.PropertyPath) {
                                                Mode = BindingMode.TwoWay,
                                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                                Converter = column.TextBoxConverter
                                            });
                    textBoxFactory.SetValue(Control.ForegroundProperty, Brushes.Black);

                    DataTemplate cellEditingTemplate = new DataTemplate();

                    cellEditingTemplate.VisualTree = textBoxFactory;
                    gridColumn.CellEditingTemplate = cellEditingTemplate;
                }

                dataGrid.Columns.Add(gridColumn);
            }
        }

        #region AddOptimizationChild 
        public void AddOptimizationChild(Optimization opt) {
            Debug.Assert(opt.Group != null, "Optimization group should not be null");
            Layer layer;
            try {
                layer = Layers.GetNextLayer(opt.Group.Type);
            } catch (Exception ex) {
                MessageBox.Show(Owner, ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (layer == null) {
                MessageBox.Show(Owner,
                                "Es gibt keine Ebene unter der gewählten - es können dort keine Elemente hinzugefügt werden.",
                                "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            opt.Children.Add(new Optimization(opt.Descriptions.Count, Layers, opt) { Value = "Neu", Group = layer.Group });

        }
        #endregion AddOptimizationChild

        #region Save
        private PopupProgressBar _dlgProgress;
        public void Save() {
            ProgressCalculator progress = new ProgressCalculator();
            progress.DoWork += progress_DoWork;
            progress.RunWorkerCompleted += progress_RunWorkerCompleted;

            _dlgProgress = new PopupProgressBar(){DataContext = progress, Owner=Owner};
            progress.RunWorkerAsync();
            _dlgProgress.ShowDialog();
        }

        void progress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if(e.Error != null)
                MessageBox.Show(_dlgProgress,
                            "Ein Fehler beim Speichern der Optimierungskriterien ist aufgetreten: " +
                            Environment.NewLine + e.Error.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(_dlgProgress,
                            "Optimierungen wurden gespeichert", "", MessageBoxButton.OK, MessageBoxImage.Information);
            _dlgProgress.Close();
        }

        void progress_DoWork(object sender, DoWorkEventArgs e) {
            ProgressCalculator progress = sender as ProgressCalculator;
            progress.Title = "Speichern der Daten";
            progress.SetWorkSteps(2, false);
            progress.Description = "Speichere Optimierungen";
            Optimization.Save(_profile.ViewboxDb, Root, Layers, _profile.ConnectionManager.DbConfig.DbName);
            progress.StepDone();
            progress.Description = "Lade Metadaten neu";
            using (var conn = _profile.ViewboxDb.ConnectionManager.GetConnection()) {
                _profile.ViewboxDb.LoadTables(conn);
            }
            progress.StepDone();
        }
        #endregion Save

        #region AddFinancialYears
        public void AddFinancialYears() {
            if (!Layers.TypeToLayer[OptimizationType.SortColumn].UseLayer) {
                MessageBox.Show(Owner,
                                "Die Geschäftsjahres Ebene wurde nicht aktiviert, somit können keine Geschäftsjahre hinzugefügt werden.",
                                "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Layer layer = Layers.GetPreviousLayer(OptimizationType.SortColumn);
            if (layer == null) {
                MessageBox.Show(Owner,
                                "Es gibt keine übergeordnete Ebene zu den Geschäftsjahren, somit können keine Geschäftsjahre hinzugefügt werden.",
                                "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var optimizations =
                Root.AsBreadthFirstEnumerable(x => x.Children).Where(
                    opt => opt.Group != null && opt.Group.Type == layer.Group.Type).ToList();
            AddFinancialYearsModel model = new AddFinancialYearsModel(optimizations,
                                                                      Layers.TypeToLayer[OptimizationType.SortColumn].
                                                                          Group, _profile.ViewboxDb.Languages, Layers);
            DlgAddFinancialYears dlgAddFinancialYears = new DlgAddFinancialYears() {DataContext = model, Owner = Owner};
            dlgAddFinancialYears.ShowDialog();
        }

        #endregion AddFinancialYears
        
        #endregion Methods
    }
}
