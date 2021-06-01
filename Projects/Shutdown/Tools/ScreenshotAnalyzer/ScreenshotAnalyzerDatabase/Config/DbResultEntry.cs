// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;

namespace ScreenshotAnalyzerDatabase.Config {
    [DbTable("result_entries", ForceInnoDb = true)]
    class DbResultEntry : DatabaseObjectBase<int>, IDbResultEntry {
        public DbResultEntry() {
            
        }
        public DbResultEntry(DbResult result, DbScreenshot screenshot, DbOcrRectangle ocrRect, string textResult) {
            Result = result;
            Screenshot = screenshot;
            OcrRectangle = ocrRect;
            RecognizedText = textResult;
            EditedValue = RecognizedText;
        }

        #region Properties
        [DbColumn("result_id",IsInverseMapping = true)]
        internal DbResult Result { get; set; }

        [DbColumn("scr_id")]
        public int ScreenshotId { get;  internal set; }
        internal DbScreenshot Screenshot { get; set; }

        [DbColumn("rect_id")]
        public int OcrRectangleId { get; internal set; }
        internal DbOcrRectangle OcrRectangle { get; set; }

        #region EditedValue
        [DbColumn("edited_value", Length = 4096)]
        public string EditedValue {
            get { return _editedValue; }
            set {
                _editedValue = value;
                Save();
            }
        }
        private string _editedValue;
        #endregion EditedValue



        #region RecognizedText
        [DbColumn("recognized_text", Length = 4096)]
        public string RecognizedText {
            get { return _recognizedText; }
            set {
                _recognizedText = value;
            }
        }
        private string _recognizedText;
        #endregion RecognizedText

        #endregion Properties


        private void Save() {
            if (Screenshot == null) return;
            
            if(EditedValue != RecognizedText)
                using (IDatabase conn = Screenshot.ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                    if(Screenshot != null)
                        ScreenshotId = Screenshot.Id;
                    if(OcrRectangle != null)
                        OcrRectangleId = OcrRectangle.Id;
                    conn.DbMapping.Save(this);
                }
            else if(Id != 0) 
                using (IDatabase conn = Screenshot.ScreenshotGroup.Table.Profile.GetOpenConnection())
                    conn.DbMapping.Delete(this);
        }
    }
}
