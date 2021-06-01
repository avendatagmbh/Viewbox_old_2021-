/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ViewBuilder.Windows;

namespace ViewBuilder
{

    public enum SignalLightStates
    {
        Red,
        Yellow,
        Green
    }

    /// <summary>
    /// Interaktionslogik für SignalLight.xaml
    /// </summary>
    public partial class SignalLight : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalLight"/> class.
        /// </summary>
        public SignalLight()
        {
            InitializeComponent();
        }

        public Delegate ExtMethod;

        private static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                "State",
                typeof(SignalLightStates),
                typeof(SignalLight),
                new PropertyMetadata(SignalLightStates.Red));

        private static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(
                "PopupContent",
                typeof(string),
                typeof(SignalLight),
                new PropertyMetadata(""));
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        public SignalLightStates State
        {
            get { return (SignalLightStates)this.GetValue(FilterProperty); }
            set
            {
                this.SetValue(FilterProperty, value);
            }
        }


        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        public string PopupContent
        {
            get { return (string)this.GetValue(PopupContentProperty); }
            set
            {
                this.SetValue(PopupContentProperty, value);
            }
        }

        private PopupControl popupControl;
        private static Popup popup;
        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (String.IsNullOrEmpty(PopupContent))
                return;
            if (popupControl == null)
            {
                popupControl = new PopupControl("Warnings", PopupContent, ExtMethod);
                popup = new Popup
                {
                    StaysOpen = false,
                    Placement = PlacementMode.Bottom,
                    PlacementTarget = (Grid)sender,
                    Child = popupControl,
                    PopupAnimation = PopupAnimation.Slide,
                    AllowsTransparency = true,
                    IsOpen = true,
                };
                popup.Closed += popup_Closed;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            popupControl = null;
        }

    }
}
