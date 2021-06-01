using System.Collections.Generic;
using System.Linq;

namespace ViewValidator.Structures {
    public class SimpleMap {

        // vorerst genügt es die Listen als "parallel" zu betrachten, da nur ein Element einem Element zugeordnet wird
        // zukünftig wird eine andere Strategie, sobald einem Element mehrere andere Elemente zugeordnet werden können

        List<string> _viewCols { get; set; }
        List<string> _testCols { get; set; }

        public SimpleMap(){
            _viewCols = new List<string>();
            _testCols = new List<string>();
        }


        public bool viewColsIsSet() {
            if (_viewCols.Count > 0)
                return true;
            else
                return false;
        }

        public bool testColsIsSet() {
            if (_testCols.Count > 0)
                return true;
            else
                return false;
        }

        public void addViewCol(string newElem) {
            _viewCols.Add(newElem);
        }

        public void addTestCol(string newElem) {
            _testCols.Add(newElem);
        }

        public string[] getMapping() { 
            return (new string[2] {_viewCols.ElementAt(0), _testCols.ElementAt(0)});
        }
    }
}
