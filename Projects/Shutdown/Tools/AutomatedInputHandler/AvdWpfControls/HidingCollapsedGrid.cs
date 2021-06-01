// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls
{
    public class HidingCollapsedGrid : Grid
    {
        public static readonly DependencyProperty ColumnWidthProperty =
  DependencyProperty.Register("ColumnWidth", typeof(string), typeof(HidingCollapsedGrid), new UIPropertyMetadata("0"));

        public string ColumnWidth {
            get { return (string) GetValue(ColumnWidthProperty); }
            set {
                SetValue(ColumnWidthProperty, value);
                ColumnDefinitions[1].Width = value == "*"
                                             ? new GridLength(1d, GridUnitType.Star)
                                             : new GridLength(double.Parse(value));
            }
        }

    }
}
