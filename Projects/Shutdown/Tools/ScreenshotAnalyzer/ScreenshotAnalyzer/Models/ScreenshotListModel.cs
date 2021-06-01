using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures;
using Utils;

namespace ScreenshotAnalyzer.Models {
    public class ScreenshotListModel : NotifyPropertyChangedBase{
        public ScreenshotListModel() {
            //Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\01_01.PNG"));
            //Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\01_01 - Kopie.PNG"));
            //for (int i = 10; i <= 15; ++i)
            //    Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\" + i + ".PNG"));
            //for (int i = 10; i <= 35; ++i)
            //    Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\" + i + ".PNG"));
            //Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\01_01_large.PNG"));
        }

        #region Properties

        private ScreenshotGroup _screenshots;
        public ScreenshotGroup Screenshots {
            get { return _screenshots; }
            set {
                if (_screenshots != value) {
                    _screenshots = value;
                    if (SelectedScreenshot == null && Screenshots.Screenshots.Count > 0)
                        SelectedScreenshot = Screenshots.Screenshots[0];
                    else SelectedScreenshot = null;
                    OnPropertyChanged("Screenshots");
                }
            }
        }

        private Screenshot _selectedScreenshot;
        public Screenshot SelectedScreenshot {
            get { return _selectedScreenshot; }
            set {
                if (_selectedScreenshot != value) {
                    _selectedScreenshot = value;
                    OnPropertyChanged("SelectedScreenshot");
                }
            }
        }


        #endregion

        public void AddImages(IEnumerable<string> paths) {
            if (Screenshots == null) return;
            foreach (var path in paths) {
                Screenshots.Add(path);
            }
            if (SelectedScreenshot == null && Screenshots.Screenshots.Count > 0) SelectedScreenshot = Screenshots.Screenshots[0];
        }

        public void DeleteImages(IList selectedItems) {
            if (Screenshots == null) return;
            List<Screenshot> screenshotsToDelete = new List<Screenshot>();
            foreach (Screenshot selectedItem in selectedItems) {
                screenshotsToDelete.Add(selectedItem);
            }

            foreach (var screenshot in screenshotsToDelete)
                Screenshots.RemoveScreenshot(screenshot);
        }
    }
}
