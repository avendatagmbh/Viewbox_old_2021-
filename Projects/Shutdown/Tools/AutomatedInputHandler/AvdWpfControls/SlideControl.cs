// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace AvdWpfControls {
    public class SlideControl : SlideControlBase {
        private object _selectedContent;

        static SlideControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SlideControl),
                                                     new FrameworkPropertyMetadata(typeof (SlideControl)));
        }

        public object SelectedContent {
            get { return _selectedContent ?? this; }
            set {
                _selectedContent = value;
                OnPropertyChanged("SelectedContent");
            }
        }
    }
}