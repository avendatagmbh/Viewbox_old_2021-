using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using DbSearchLogic.SearchCore.Structures;

namespace DbSearch.Models.Search {
    public class SearchParamsModel {
        public SearchParamsModel(Brush foreground, ConfigSearchParams searchParams) {
            Foreground = foreground;
            Params = searchParams;
            Foreground = Brushes.White;
        }

        public ConfigSearchParams Params { get; set; }
        public Brush Foreground { get; set; }
    }
}
