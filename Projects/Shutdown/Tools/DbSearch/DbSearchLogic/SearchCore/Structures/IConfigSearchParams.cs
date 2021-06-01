using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Structures {
    public interface IConfigSearchParams {
        bool UseCaseIgnore { get; set; }
        bool UseInStringSearch { get; set; }
        bool UseStringSearch { get; set; }
        bool UseSearchRoundedValues { get; set; }
        int NumericPrecision { get; set; }
        StringComparison StringComparison { get; }
    }
}
