// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvdCommon.DataGridHelper.Interfaces;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures.Results {
    public class ResultRowEntry : NotifyPropertyChangedBase, IDataRowEntry {
        public ResultRowEntry(IDbResultEntry dbResultEntry, Screenshot screenshot, OcrRectangle rectangle) {
            DbResultEntry = dbResultEntry;
            Screenshot = screenshot;
            Rectangle = rectangle;
        }
        //public ResultRowEntry(Screenshot screenshot, OcrRectangle rectangle, string recognizedText) {
        //    Screenshot = screenshot;
        //    Rectangle = rectangle;
        //    RecognizedText = recognizedText;
        //    EditedText = RecognizedText;
        //}

        #region Properties
        public string RecognizedText {
            get { return DbResultEntry.RecognizedText; }
            set { DbResultEntry.RecognizedText = value; }
        }
        public OcrRectangle Rectangle { get; set; }
        public Screenshot Screenshot { get; set; }
        public IDbResultEntry DbResultEntry { get; set; }

        public string DisplayString {
            get { return RecognizedText; }
        }

        public string EditedText {
            get { return DbResultEntry.EditedValue; }
            set {
                if (EditedText != value) {
                    DbResultEntry.EditedValue = value;
                    OnPropertyChanged("EditedText");
                }
            }
        }
        #endregion
    }
}
