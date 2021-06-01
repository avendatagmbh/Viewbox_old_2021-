using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures.Config;
using ScreenshotAnalyzerBusiness.Structures.Results;
using Utils;

namespace ScreenshotAnalyzer.Models.Results {
    public class TextRecognitionResultModel : NotifyPropertyChangedBase{
        public TextRecognitionResultModel(MainWindowModel model) {
            MainWindowModel = model;
            MainWindowModel.PropertyChanged += MainWindowModel_PropertyChanged;
        }

        public TextRecognitionResultModel(RecognitionResult recognitionResult) {
            RecognitionResult = recognitionResult;
        }

        void MainWindowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedTable") {
                if (MainWindowModel.SelectedTable == null) RecognitionResult = null;
                else {
                    RecognitionResult = MainWindowModel.SelectedTable.RecognitionResult;
                    MainWindowModel.SelectedTable.PropertyChanged -= SelectedTable_PropertyChanged;
                    MainWindowModel.SelectedTable.PropertyChanged += SelectedTable_PropertyChanged;
                }
            }
        }

        void SelectedTable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "RecognitionResult") {
                if (((Table)sender) == MainWindowModel.SelectedTable) {
                    RecognitionResult = MainWindowModel.SelectedTable.RecognitionResult;
                }
            }
        }

        #region Properties
        private MainWindowModel MainWindowModel { get; set; }

        private RecognitionResult _recognitionResult;
        public RecognitionResult RecognitionResult {
            get { return _recognitionResult; }
            set {
                if (_recognitionResult != value) {
                    _recognitionResult = value;
                    OnPropertyChanged("RecognitionResult");
                }
            }
        }


        #endregion Properties
    }
}
