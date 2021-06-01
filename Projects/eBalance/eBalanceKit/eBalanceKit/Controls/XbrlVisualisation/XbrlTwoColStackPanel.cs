using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace eBalanceKit.Controls.XbrlVisualisation {
    
    internal class XbrlTwoColStackPanel : XbrlBasePanel {

        public XbrlTwoColStackPanel() {
            this.ContentPanelLeft = new StackPanel();
            this.ContentPanelRight = new StackPanel();
            
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(20, System.Windows.GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            grid.Children.Add(this.ContentPanelLeft);
            grid.Children.Add(this.ContentPanelRight);
            Grid.SetColumn(this.ContentPanelRight, 2);

            base.ContentPanel.Children.Add(grid);
        }

        public Panel ContentPanelLeft { get; private set; }
        public Panel ContentPanelRight { get; private set; }
    }
}
