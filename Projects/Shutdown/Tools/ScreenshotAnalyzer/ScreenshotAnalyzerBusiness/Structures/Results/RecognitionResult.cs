// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Resources;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures.Results {
    public class RecognitionResult : NotifyPropertyChangedBase {
        public RecognitionResult(ScreenshotGroup screenshotGroup) {
            ScreenshotGroup = screenshotGroup;
            //DbResult = DatabaseObjectFactory.LoadResult(screenshotGroup.DbScreenshotGroup);
            //if (DbResult == null) DbResult = DatabaseObjectFactory.CreateResult(screenshotGroup.DbScreenshotGroup);
            //else {
            //    if (DbResult.ResultColumns.Count > 0) {
            //        CreateDataTable(DbResult);
            //    }
            //}
        }

        #region Properties
        public IDbResult DbResult { get; private set; }
        public ScreenshotGroup ScreenshotGroup { get; set; }

        private DataTable _textTable;
        public DataTable TextTable {
            get { return _textTable; }
            set {
                if (_textTable != value) {
                    _textTable = value;
                    OnPropertyChanged("TextTable");
                }
            }
        }


        private Dictionary<OcrRectangle, IDbResultEntry> _rectToResultEntry = new Dictionary<OcrRectangle, IDbResultEntry>();
        #endregion Properties

        #region Methods
        public void ExtractText(Screenshot screenshot) {
            CreateDataTable(screenshot);
            var row = CreateRow(screenshot);
            TextTable.AddRow(row);
        }

        public void ExtractText() {
            if(ScreenshotGroup.Screenshots.Count == 0)
                throw new InvalidOperationException(ResourcesBusiness.RecognitionResult_ExtractText_ErrorNoScreenshotsLoaded);

            int rectCount = ScreenshotGroup.Screenshots[0].Rectangles.Count;
            foreach (var screenshot in ScreenshotGroup.Screenshots) {
                if(screenshot.Rectangles.Count != rectCount)
                    throw new InvalidOperationException(ResourcesBusiness.RecognitionResult_ExtractText_ErrorDifferentRectanglesCount);
            }

            CreateDataTable(ScreenshotGroup.Screenshots[0]);

            foreach (var screenshot in ScreenshotGroup.Screenshots) {
                var row = CreateRow(screenshot);
                TextTable.AddRow(row);
            }
        }

        IDbResult LoadResult(IDbScreenshotGroup dbScreenshotGroup) {
            IDbResult dbResult = DatabaseObjectFactory.LoadResult(dbScreenshotGroup);
            return dbResult;
        }

        private void CreateDataTable(Screenshot screenshot) {
            //Load former results from database
            DbResult = LoadResult(ScreenshotGroup.DbScreenshotGroup);
            if (DbResult == null) DbResult = DatabaseObjectFactory.CreateResult(ScreenshotGroup.DbScreenshotGroup);
            //Load resultrowentries and set correct OcrRectangle
            _rectToResultEntry.Clear();
            foreach (var entry in DbResult.ResultEntries) {
                Tuple<OcrRectangle, IDbOcrRectangle> rectPair = OcrRectangleRegistry.GetRectangle(ScreenshotGroup.FromId(entry.ScreenshotId), entry.OcrRectangleId);
                DatabaseObjectFactory.SetDbOcrRectangle(entry, rectPair.Item2);
                _rectToResultEntry[rectPair.Item1] = entry;
            }

            TextTable = new DataTable();
            for (int i = 0; i < screenshot.Rectangles.Count; ++i) {
                ResultColumn resultColumn = null;
                //Try to find saved result column
                foreach (var dbResultColumn in DbResult.ResultColumns) {
                    Tuple<OcrRectangle, IDbOcrRectangle> tuple =
                        OcrRectangleRegistry.GetRectangle(ScreenshotGroup.Screenshots[0], dbResultColumn.RectangleId);
                    if (tuple == null) continue;
                    if (i < ScreenshotGroup.Screenshots[0].Rectangles.Count && ScreenshotGroup.Screenshots[0].Rectangles[i] == tuple.Item1) {
                        resultColumn = new ResultColumn(dbResultColumn);
                        break;
                    }
                }

                //If none was found, create a new one
                if (resultColumn == null) {
                    var usedScreenshot = i < ScreenshotGroup.Screenshots[0].Rectangles.Count ? ScreenshotGroup.Screenshots[0] : screenshot;
                    resultColumn = new ResultColumn("Spalte " + (i + 1), DbResult, ScreenshotGroup.DbScreenshotGroup,
                                                    OcrRectangleRegistry.GetRectangle(usedScreenshot,
                                                                                      usedScreenshot.Rectangles[i]));
                }
                TextTable.AddColumn(resultColumn);
            }
        }

        //private void CreateDataTable(int rectCount) {
        //    if (TextTable == null) {
        //        TextTable = new DataTable();
        //        TextTable.CreateColumnFunc = name => null;
        //        //TextTable.CreateColumnFunc = name => new ResultColumn(name, DbResult, ScreenshotGroup.DbScreenshotGroup);
        //        for (int i = 1; i <= rectCount; ++i)
        //            TextTable.AddColumn(new ResultColumn("Spalte " + i, DbResult, ScreenshotGroup.DbScreenshotGroup, 
        //                OcrRectangleRegistry.GetRectangle(ScreenshotGroup.Screenshots[0], ScreenshotGroup.Screenshots[0].Rectangles[i-1]))
        //                );
        //            //TextTable.AddColumn("Rechteck " + i);
        //    } else {
        //        TextTable.Rows.Clear();
        //    }
        //}

        private IDataRow CreateRow(Screenshot screenshot) {
            IDataRow row = TextTable.CreateRow();

            //row[0] = new DataRowEntry(new FileInfo(screenshot.Path).Name);
            //List<string> textResults = Images.ExtractText(screenshot.Path, screenshot.Rectangles);
            List<string> textResults = Images.ExtractTextExperimental(screenshot.Path, screenshot.Rectangles);
            for (int i = 0; i < screenshot.Rectangles.Count; ++i) {
                IDbResultEntry resultEntry;
                if (!_rectToResultEntry.TryGetValue(screenshot.Rectangles[i], out resultEntry) || resultEntry.RecognizedText != textResults[i]) {
                    resultEntry = DatabaseObjectFactory.CreateResultRowEntry(DbResult, screenshot.DbScreenshot,
                                                                             OcrRectangleRegistry.GetRectangle(
                                                                                 screenshot, screenshot.Rectangles[i]),
                                                                             textResults[i]);

                }
                row[i] = new ResultRowEntry(resultEntry,screenshot, screenshot.Rectangles[i]);
                //row[i] = new ResultRowEntry(screenshot, screenshot.Rectangles[i], textResults[i]);
            }

            return row;
        }

        #endregion Methods
    }
}
