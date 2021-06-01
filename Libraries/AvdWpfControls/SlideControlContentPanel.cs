using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class SlideControlContentPanel : Panel
    {
        public void Hide()
        {
            var slideControl = TryFindSlideControl(this);
            if (slideControl != null) slideControl.Hide();
        }

        private SlideControlBase TryFindSlideControl(DependencyObject child)
        {
            DependencyObject parentObject = GetParentObject(child);
            if (parentObject == null) return null;
            SlideControlBase parent = parentObject as SlideControlBase;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return TryFindSlideControl(parentObject);
            }
        }

        private DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;
                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }
            return VisualTreeHelper.GetParent(child);
        }
    }
}