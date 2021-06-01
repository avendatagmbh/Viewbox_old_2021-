using System.Linq;
using System.Collections.ObjectModel;

namespace ViewValidator.Structures {
    public class MapModel {
        ObservableCollection<SimpleMap> assignMap;

        int count;
        public int Count  {get{return count;}}

        public MapModel() {
            assignMap = new ObservableCollection<SimpleMap>();
        }

        public void Add(SimpleMap item){
            assignMap.Add(item);
            count = assignMap.Count;
        }

        public SimpleMap simpleMapAtPos( int pos) { 
                if (pos > assignMap.Count) {
                    return null;
                }
                return assignMap.ElementAt(pos); 
        } 


        
    }
}
