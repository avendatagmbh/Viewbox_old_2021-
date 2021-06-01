using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AvdCommon.DataGridHelper;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzerBusiness.Structures;
using ScreenshotAnalyzerBusiness.Structures.Config;
using ScreenshotAnalyzerBusiness.Structures.Results;
using Utils;

namespace ScreenshotAnalyzer.Models.Results {
    public class CorrectionAssistantModel : NotifyPropertyChangedBase{
        public CorrectionAssistantModel(Table table) {
            Table = table;
            RecognitionResult = table.RecognitionResult;
            foreach (var screenshot in RecognitionResult.ScreenshotGroup.Screenshots)
                _screenshotToBitmap[screenshot] = Image.FromFile(screenshot.Path);
            CreateRecognitionInfo();
            TextRecognitionResultModel = new TextRecognitionResultModel(table.RecognitionResult);
        }

        #region Properties

        public TextRecognitionResultModel TextRecognitionResultModel { get; set; }
        public Table Table { get; set; }
        public RecognitionResult RecognitionResult { get; set; }
        private readonly Dictionary<Screenshot,Image> _screenshotToBitmap = new Dictionary<Screenshot, Image>();

        #region IsAtEnd
        private bool _isAtEnd;
        public bool IsAtEnd {
            get { return _isAtEnd; }
            set {
                if (_isAtEnd != value) {
                    _isAtEnd = value;
                    OnPropertyChanged("IsAtEnd");
                    OnPropertyChanged("NextButtonText");
                }
            }
        }
        #endregion IsAtEnd

        #region IsNotAtBeginning
        private bool _isNotAtBeginning;
        public bool IsNotAtBeginning {
            get { return _isNotAtBeginning; }
            set {
                if (_isNotAtBeginning != value) {
                    _isNotAtBeginning = value;
                    OnPropertyChanged("IsNotAtBeginning");
                }
            }
        }
        #endregion IsNotAtBeginning

        public string NextButtonText { get { return IsAtEnd ? ResourcesGui.CorrectionAssistantModel_NextButtonText_Ready : ResourcesGui.CorrectionAssistantModel_NextButtonText_Next; } }

        public DataTable TextTable {
            get { return Table.RecognitionResult.TextTable; }
        }

        private RecognitionInfo _currentRecognitionInfo;
        public RecognitionInfo CurrentRecognitionInfo {
            get { return _currentRecognitionInfo; }
            set {
                if (_currentRecognitionInfo != value) {
                    _currentRecognitionInfo = value;
                    OnPropertyChanged("CurrentRecognitionInfo");
                }
            }
        }


        public string InfoString {
            get {
                return string.Format(ResourcesGui.CorrectionAssistantModel_InfoString,
                                     CurrentRecognitionInfo.ResultRowEntry.Screenshot.Filename,
                                     _currentRow + _currentColumn*TextTable.Rows.Count + 1,
                                     TextTable.Columns.Count*TextTable.Rows.Count);
            }
        }



        private int _currentColumn, _currentRow;
        #endregion Properties

        #region Methods
        #region ShowPrevious
        public void ShowPrevious() {
            _currentRow--;
            if (_currentRow < 0) {
                _currentRow = TextTable.Rows.Count - 1;
                _currentColumn--;
            }
            CreateRecognitionInfo();
        }
        #endregion ShowPrevious


        public void ShowNext() {
            _currentRow++;
            if (_currentRow >= TextTable.Rows.Count) {
                _currentRow = 0;
                _currentColumn++;
            }
            CreateRecognitionInfo();
        }

        private void CreateRecognitionInfo() {
            ResultRowEntry entry = (ResultRowEntry) TextTable.Rows[_currentRow].RowEntries[_currentColumn];
            CurrentRecognitionInfo = new RecognitionInfo(entry, _screenshotToBitmap[entry.Screenshot]);
            IsNotAtBeginning = !(_currentRow == 0 && _currentColumn == 0);
            IsAtEnd = _currentColumn == TextTable.Columns.Count - 1 && _currentRow == TextTable.Rows.Count - 1;
            OnPropertyChanged("InfoString");
        }
        #endregion
    }
}
