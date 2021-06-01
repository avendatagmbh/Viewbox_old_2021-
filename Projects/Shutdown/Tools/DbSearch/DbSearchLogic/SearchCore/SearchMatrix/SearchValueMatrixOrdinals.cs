namespace DbSearchLogic.SearchCore.SearchMatrix {
    public class SearchValueMatrixOrdinals {
        public int FirstOrdinal { get; set; }
        public int SecondOrdinal { get; set; }

        public SearchValueMatrixOrdinals(int first, int second) {
            FirstOrdinal = first;
            SecondOrdinal = second;
        }
    }
}