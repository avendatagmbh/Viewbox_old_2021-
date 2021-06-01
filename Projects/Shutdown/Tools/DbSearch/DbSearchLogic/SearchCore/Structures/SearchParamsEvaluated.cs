using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Structures {
    internal class SearchParamsEvaluated : IConfigSearchParams {
        internal SearchParamsEvaluated() {
            StringComparison = StringComparison.CurrentCulture;
        }

        #region UseCaseIgnore
        private bool _useCaseIgnore;

        public bool UseCaseIgnore {
            get { return _useCaseIgnore; }
            set {
                if (_useCaseIgnore != value) {
                    _useCaseIgnore = value;
                    StringComparison = UseCaseIgnore
                                            ? StringComparison.CurrentCultureIgnoreCase
                                            : StringComparison.CurrentCulture;
                }
            }
        }
        #endregion UseCaseIgnore

        public bool UseInStringSearch { get; set; }
        public bool UseStringSearch { get; set; }
        public bool UseSearchRoundedValues { get; set; }
        public int NumericPrecision { get; set; }
        public StringComparison StringComparison { get; set; }
    }
}
