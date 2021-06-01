using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearch.Manager {
    public static class ColorManager<T> {
        #region Properties
        private static List<Color> _colorPool = new List<Color>() {
                                                              Colors.Red,
                                                              Colors.BlueViolet,
                                                              Colors.Teal,
                                                              Colors.DarkSalmon,
                                                              Colors.Blue,
                                                              Colors.Yellow
                                                          };

        private static Dictionary<T, Color> _tableToColor  = new Dictionary<T, Color>();
        private static Random _rand = new Random();
        #endregion Properties

        #region Methods
        public static Color GetColor(T info) {
            if(!_tableToColor.ContainsKey(info)) _tableToColor[info] = AcquireNewColor();
            return _tableToColor[info];
        }
        public static SolidColorBrush GetBrush(T info, float opacity = 1.0f) {
            if (!_tableToColor.ContainsKey(info)) _tableToColor[info] = AcquireNewColor();
            SolidColorBrush temp = new SolidColorBrush(_tableToColor[info]);
            temp.Opacity = opacity;
            temp.Freeze();
            return temp;
        }

        private static Color AcquireNewColor() {
            HashSet<int> usedIndices = new HashSet<int>();

            foreach(var color in _tableToColor.Values) {
                usedIndices.Add(_colorPool.IndexOf(color));
            }

            for(int i = 0; i < _colorPool.Count; ++i) {
                if(!usedIndices.Contains(i)) return _colorPool[i];
            }

            //Add random color
            _colorPool.Add(new Color(){R = (byte)_rand.Next(0,255), G = (byte)_rand.Next(0,255), B = (byte)_rand.Next(0,255), A = 255});
            
            return _colorPool[_colorPool.Count - 1];
        }

        public static void ClearColors() {
            _tableToColor.Clear();
        }
        #endregion Methods
    }
}
