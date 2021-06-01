using System.Windows;
using System.Windows.Controls;

namespace eBalanceKit.Controls.XbrlVisualisation {
    internal class XbrlTextPanel : XbrlBasePanel {
        public XbrlTextPanel() {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
            base.ContentPanel.Children.Add(grid);
            ContentPanel = new StackPanel();
            grid.Children.Add(ContentPanel);
            Grid.SetRow(ContentPanel, 1);

            Grid = grid;
        }

        private Grid Grid { get; set; }
        public new StackPanel ContentPanel { get; private set; }

        public void Init(string xbrlElemId, string bindingPath = null, bool showTextHeader = false) {
            if (Model.Elements.ContainsKey(xbrlElemId)) {
                if (string.IsNullOrEmpty(Header)) {
                    Header = Model.Elements[xbrlElemId].Label;
                }

                if (showTextHeader) {
                    Model.UIElements.AddInfo(Grid, Model.Elements[xbrlElemId], height: -1, width: 0,
                                             forceVerticalOrientation: true, bindingPath: bindingPath);
                } else {
                    Model.UIElements.AddInfo(Grid, Model.Elements[xbrlElemId], height: -1, width: 0,
                                             forceVerticalOrientation: true, hideLabel: true, setTopMargin: false,
                                             bindingPath: bindingPath);
                }
            }

            Model.RegisterGotFocusEventHandler();
        }
    }
}