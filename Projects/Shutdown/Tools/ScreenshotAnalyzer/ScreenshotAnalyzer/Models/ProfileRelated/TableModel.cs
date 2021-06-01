using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzer.Models.ProfileRelated {
    public class TableModel {
        public TableModel(Table table) {
            Table = table;
        }

        public Table Table { get; set; }
    }
}
