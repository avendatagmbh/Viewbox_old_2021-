using System.Windows;

namespace AvdWpfControls
{
    public class SlideControl : SlideControlBase
    {
        private object _selectedContent;

        static SlideControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SlideControl),
                                                     new FrameworkPropertyMetadata(typeof (SlideControl)));
        }

        public object SelectedContent
        {
            get { return _selectedContent ?? this; }
            set
            {
                _selectedContent = value;
                OnPropertyChanged("SelectedContent");
            }
        }
    }
}