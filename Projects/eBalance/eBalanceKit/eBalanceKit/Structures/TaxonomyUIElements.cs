/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-01-07      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AvdWpfControls;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKit.Converters;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness;
using System.ComponentModel;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Structures {

    public class TaxonomyUIElements : INotifyPropertyChanged {

        private const int TOP_MARGIN = 15;
        private const int LEFT_MARGIN = 15;

        internal class UIGroup {
            public UIElement UIElement { get; set; }
            public UIElement Warning { get; set; }
            public UIElement Error { get; set; }
        }

        internal TaxonomyUIElements() {
            UIElements = new Dictionary<string, UIGroup>();
            ExpanderGroups = new Dictionary<int, List<Expander>>();
            ExpanderGroupIds = new Dictionary<Expander, int>();
            AssignedDetailTextboxes = new Dictionary<string, UIElement>();
        }

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the expander groups.
        /// </summary>
        /// <value>The expander groups.</value>
        private Dictionary<int, List<Expander>> ExpanderGroups { get; set; }

        /// <summary>
        /// Gets or sets the expander group ids.
        /// </summary>
        /// <value>The expander group ids.</value>
        private Dictionary<Expander, int> ExpanderGroupIds { get; set; }

        /// <summary>
        /// Gets or sets the UIElements.
        /// </summary>
        /// <value>The UIElements.</value>
        internal Dictionary<string, UIGroup> UIElements { get; set; }

        private Dictionary<string, UIElement> AssignedDetailTextboxes { get; set; }

        #endregion properties

        /*****************************************************************************************************/

        #region eventHandler

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        void tb_GotFocus(object sender, RoutedEventArgs e) {
            ((TextBox)sender).SelectAll();
        }

        void cb_Changed(object sender, RoutedEventArgs e) {
            ThreeStateCheckBox cb = sender as ThreeStateCheckBox;
            
            if (!string.IsNullOrEmpty(cb.Name)) {
                if (AssignedDetailTextboxes.ContainsKey(cb.Name)) {
                    if (cb.IsChecked.HasValue && cb.IsChecked.Value == true) {
                        AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Visible;
                    } else {
                        AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        void cb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;

            Taxonomy.IElement selectedElem = cb.SelectedValue as Taxonomy.IElement;
            if (selectedElem == null) {
                AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Collapsed;
            } else {
                if (selectedElem.Id.EndsWith(".S") || TaxonomyManager.ElementsWithExplanation.Contains(selectedElem.Name + "ERL")) {
                    (AssignedDetailTextboxes[cb.Name] as TextBox).Text = string.Empty;
                    AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Visible;
                } else {
                    AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Collapsed;
                }
            }

            var substitutionGroup = (selectedElem == null) ? (cb.SelectionBoxItem as Taxonomy.IElement).SubstitutionGroup : selectedElem.SubstitutionGroup;

                // Try to find the MainWindow to tell it to check the Visibility of all NavigationTreeEntries
                var mainWindow = UIHelpers.TryFindParent<eBalanceKit.Windows.MainWindow>(cb.Parent);
                // The legalStatus was changed and not automatically set (then would mainWindow be null
                if (substitutionGroup.EndsWith("genInfo.company.id.legalStatus.legalStatus.head") && mainWindow != null) {
                    // Check which navigation entries and which positions in the presentation tree has to be shown
                    mainWindow.Model.CheckNavigationVisibility(selectedElem == null);
                }
                else if (substitutionGroup.EndsWith("genInfo.report.id.incomeStatementFormat.incomeStatementFormat.head") && mainWindow != null) {
                    // The incomeStatementFormat was changed (GKV / UKV) so update the visibility
                    mainWindow.Model.CheckNavigationVisibility();
                }
        }
        
        void cb_Unchecked(object sender, RoutedEventArgs e) {
            ThreeStateCheckBox cb = sender as ThreeStateCheckBox;
            AssignedDetailTextboxes[cb.Name].Visibility = Visibility.Visible;
        }

        public void  ContainsValidationElementChanged(){
            OnPropertyChanged("ContainsValidationWarningElement");
            OnPropertyChanged("ContainsValidationErrorElement");
        }
        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Adds the specified target.
        /// </summary>
        public void Add(
            Panel target,
            Taxonomy.IElement element, 
            string bindingPath, 
            int maxLength = 0, 
            int height = 0, 
            int width = 440,
            int maxWidth = 0, 
            string prefix = "", 
            int leftMargin = 0,
            bool isReadOnly = false,
            bool useExpander = false,
            int expanderGroup = 0,
            Brush bgColor = null,
            bool addExpanderTopMargin = false,
            bool forceVerticalOrientation = false,
            bool hideLabel = false,
            bool setTopMargin = true,
            bool setLeftMargin = false,
            List<Taxonomy.IElement> multipleChoiceValues = null,
            string writeAllowedRightBindingPath = "ReportRights.WriteRestAllowed") {

            if (maxWidth > 0) {
                // workaround to left align elements if maxWidth is defined
                Grid grid1 = new Grid();
                grid1.ColumnDefinitions.Add(new ColumnDefinition { MaxWidth = maxWidth });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
                target.Children.Add(grid1);
                target = grid1;                    
            }

            Grid groupPanel = new Grid();
            groupPanel.Margin = new Thickness(
                (setLeftMargin ? LEFT_MARGIN : 0),
                (setTopMargin ? TOP_MARGIN : 0),
                0, 0);

            StackPanel validationInfoPanel = new StackPanel();
            Taxonomy.Interfaces.PresentationTree.IPresentationTree ptree = 
                TaxonomyManager.GCD_Taxonomy.GetPresentationTree("http://www.xbrl.de/taxonomies/de-gcd/role/gcd");

            if (//(element.ValueType != Taxonomy.XbrlElementValue.XbrlElementValueTypes.Boolean) &&
            (element.ValueType != Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice)) {

                if (forceVerticalOrientation) {
                    groupPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
                    groupPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    groupPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

                    groupPanel.Children.Add(validationInfoPanel);
                    Grid.SetRow(validationInfoPanel, 2);
                } else {
                    groupPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
                    groupPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    groupPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
                    groupPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

                    groupPanel.Children.Add(validationInfoPanel);
                    Grid.SetColumnSpan(validationInfoPanel, 2);
                    Grid.SetRow(validationInfoPanel, 1);
                }
                
            }

            target.Children.Add(groupPanel);
            
            TextBlock label = null;

            if (!hideLabel && !useExpander &&
                (element.ValueType != Taxonomy.Enums.XbrlElementValueTypes.Boolean)) {
                
                label = new TextBlock {
                    TextWrapping = TextWrapping.Wrap,
                    Text = element.MandatoryLabel,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };

                if (!forceVerticalOrientation) {
                    label.Width = 230;
                } else if (maxWidth > 0) {
                    label.MaxWidth = maxWidth;
                }                              

                groupPanel.Children.Add(label);
            }

            UIElement uiElem = null;

            switch (element.ValueType) {
                case Taxonomy.Enums.XbrlElementValueTypes.String: {
                    SearchableTextBox tb = new SearchableTextBox { Background = bgColor ?? GlobalResources.TextboxBgBrush, Style = SearchableTextBox.TextBoxStyle };
                        // DEVNOTE: the following code is not working. Style is null yet. We have to overwrite the whole style.
                        // if we want to add something to the style after init, there will be an error : the object is sealed.
                        //Setter mySetter1 = new Setter {Property = Control.BorderBrushProperty, Value = new SolidColorBrush(Colors.PowderBlue)};
                        //Setter mySetter2 = new Setter {Property = Control.BorderThicknessProperty, Value = new Thickness(3d)};
                        //Trigger myTrigger = new Trigger {
                        //    Property = SearchableDatePicker.IsHighlightedProperty,
                        //    Value = true,
                        //    Setters = {mySetter1, mySetter2}
                        //};
                        //tb.Style.Triggers.Add(myTrigger);

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = true;
                                    tb.BringIntoView();
                                    tb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = false;
                                }
                            };
                        }

                        if (maxLength > 0)
                            tb.MaxLength = maxLength;

                        if (height > 0) {
                            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                            tb.Height = height;
                            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                            tb.AcceptsReturn = true;
                        } else if (height < 0) {
                            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                            tb.AcceptsReturn = true;
                        } else {
                            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        }

                        if (width > 0) {
                            tb.Width = width;
                            tb.HorizontalAlignment = HorizontalAlignment.Left;
                        }

                        if (maxWidth > 0) {
                            tb.MaxWidth = maxWidth;
                        }

                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            tb.ToolTip = element.Documentation;
                        }

                        tb.SetBinding(TextBox.TextProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                        tb.Margin = new Thickness(leftMargin, 0, 0, 0);    
                        tb.GotFocus += new RoutedEventHandler(tb_GotFocus);
                        tb.Tag = element;

                        //tb.SetBinding(TextBox.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        uiElem = tb;

                        if (!useExpander) {
                            if (forceVerticalOrientation) {
                                Grid.SetRow(uiElem, 1);
                            } else {
                                Grid.SetColumn(uiElem, 1);
                            }

                            groupPanel.Children.Add(uiElem);
                            
                        } else {
                            tb.Margin = new Thickness(25, 0, 0, 0);
                            Expander expander = new Expander {
                                Header = new TextBlock {
                                    Text = element.MandatoryLabel,
                                    TextWrapping = TextWrapping.Wrap,
                                    VerticalAlignment = VerticalAlignment.Top
                                }
                            };

                            if (addExpanderTopMargin) {
                                expander.Margin = new Thickness(0, TOP_MARGIN, 0, 0);
                            }

                            expander.Content = uiElem;

                            if (forceVerticalOrientation) {
                                Grid.SetRow(expander, 1);
                            } else {
                                Grid.SetColumn(expander, 1);
                            }
                            
                            groupPanel.Children.Add(expander);

                            if (!ExpanderGroups.ContainsKey(expanderGroup)) {
                                ExpanderGroups[expanderGroup] = new List<Expander>();
                            }
                            ExpanderGroups[expanderGroup].Add(expander);
                            ExpanderGroupIds[expander] = expanderGroup;
                            expander.Expanded += new RoutedEventHandler(expander_Expanded);
                        }
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Date: {
                    SearchableDatePicker dp = new SearchableDatePicker {
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Background = bgColor ?? GlobalResources.TextboxBgBrush,
                        Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name),
                        Style = SearchableDatePicker.DatePickerStyle
                    };
                        
                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            dp.ToolTip = element.Documentation;
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    dp.IsHighlighted = true;
                                    dp.BringIntoView();
                                    dp.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    dp.IsHighlighted = false;
                                }
                            };
                        }

                        dp.SetBinding(DatePicker.SelectedDateProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                        dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        dp.Width = 140;
                        dp.Tag = element;
                    
                        if (isReadOnly) dp.IsEnabled = false;

                        //dp.SetBinding(DatePicker.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        uiElem = dp;

                        if (forceVerticalOrientation) {
                            Grid.SetRow(uiElem, 1);
                        } else {
                            Grid.SetColumn(uiElem, 1);
                        }

                        groupPanel.Children.Add(uiElem);
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Int: {
                    SearchableTextBox tb = new SearchableTextBox {
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Background = bgColor ?? GlobalResources.TextboxBgBrush,
                        Style = SearchableTextBox.TextBoxStyle
                    };

                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            tb.ToolTip = element.Documentation;
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = true;
                                    tb.BringIntoView();
                                    tb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = false;
                                }
                            };
                        }

                        if (width > 0) {
                            tb.Width = width;
                            tb.HorizontalAlignment = HorizontalAlignment.Left;
                        }

                        tb.SetBinding(TextBox.TextProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                        tb.GotFocus += tb_GotFocus;
                        tb.Tag = element;

                        //tb.SetBinding(TextBox.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        uiElem = tb;

                        if (forceVerticalOrientation) {
                            Grid.SetRow(uiElem, 1);
                        } else {
                            Grid.SetColumn(uiElem, 1);
                        }
                        
                        groupPanel.Children.Add(uiElem);
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Monetary: {
                    NumericTextbox tb = new NumericTextbox {
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = bgColor ?? GlobalResources.TextboxBgBrush,
                        Style = NumericTextbox.NumericTextBoxStyle
                    };

                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            tb.ToolTip = element.Documentation;
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = true;
                                    tb.BringIntoView();
                                    tb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    tb.IsHighlighted = false;
                                }
                            };
                        }

                        if (isReadOnly) {
                            // computed node
                            tb.SetBinding(NumericTextbox.TextProperty, new Binding(bindingPath + ".MonetaryValue.Value") { Mode = BindingMode.OneWay, Converter = new StringToNullableMonetaryConverter() });
                        } else {
                            // value node
                            tb.SetBinding(NumericTextbox.TextProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus, Converter = new StringToNullableMonetaryConverter() });
                        }

                            
                        tb.GotFocus += tb_GotFocus;
                        tb.Tag = element;
                        tb.Margin = new Thickness(leftMargin, 0, 0, 0);
                        tb.KeyDown += tb_KeyDown;
                        tb.TextAlignment = TextAlignment.Right;
                        tb.IsReadOnly = isReadOnly;
                        tb.IsTabStop = !isReadOnly;

                        if (maxLength > 0) {
                            tb.MaxLength = maxLength;
                        }

                        if (width > 0) {
                            tb.Width = width;
                            tb.HorizontalAlignment = HorizontalAlignment.Left;
                        }

                        StackPanel sp = new StackPanel() { 
                            Orientation = Orientation.Horizontal,
                            VerticalAlignment = VerticalAlignment.Center,
                        };

                        if (prefix.Length > 0) {

                            Grid grid = new Grid();
                            tb.Margin = new Thickness();
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(leftMargin) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            int marginCorrector = (prefix.Equals("-") ? 2 : 0);

                            label = new TextBlock() {
                                TextWrapping = TextWrapping.Wrap,
                                Text = prefix,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                Padding = new Thickness(),
                                Margin = new Thickness(0, 0, marginCorrector, 0)
                            };

                            grid.Children.Add(label);
                            grid.Children.Add(tb);

                            Grid.SetColumn(tb, 1);

                            sp.Children.Add(grid);
                        } else {
                            sp.Children.Add(tb);
                        }

                        label = new TextBlock() {
                            Text = " €",
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness()
                        };

                        sp.Children.Add(label);

                        //sp.SetBinding(StackPanel.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });


                        if (forceVerticalOrientation) {
                            Grid.SetRow(sp, 1);
                        } else {
                            Grid.SetColumn(sp, 1);
                        } 

                        groupPanel.Children.Add(sp);

                        uiElem = tb;
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Numeric: {
                    NumericTextbox tb = new NumericTextbox {
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Background = bgColor ?? GlobalResources.TextboxBgBrush,
                        Style = NumericTextbox.NumericTextBoxStyle
                    };

                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            tb.ToolTip = element.Documentation;
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    //tb.BorderBrush = GlobalResources.SelectedBorderBrush;
                                    //tb.BorderThickness = GlobalResources.SelectedElementThickness;
                                    tb.IsHighlighted = true;
                                    tb.BringIntoView();
                                    tb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    //tb.BorderBrush = GlobalResources.DefaultTextBoxBorderBrush;
                                    //tb.BorderThickness = GlobalResources.DefaultTextBoxThickness;
                                    tb.IsHighlighted = false;
                                }
                            };
                        }

                        if (width > 0) {
                            tb.Width = width;
                            tb.HorizontalAlignment = HorizontalAlignment.Left;
                        }

                        tb.SetBinding(TextBox.TextProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                        tb.GotFocus += new RoutedEventHandler(tb_GotFocus);
                        tb.Tag = element;

                        //tb.SetBinding(TextBox.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        uiElem = tb;

                        if (forceVerticalOrientation) {
                            Grid.SetRow(uiElem, 1);
                        } else {
                            Grid.SetColumn(uiElem, 1);
                        }

                        groupPanel.Children.Add(uiElem);
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Boolean: {
                    ThreeStateCheckBox cb = new ThreeStateCheckBox {
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Margin = new Thickness(0), 
                        Style = ThreeStateCheckBox.ThreeStateCheckBoxStyle
                    };
                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            cb.ToolTip = element.Documentation;
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    //cb.BorderBrush = GlobalResources.SelectedBorderBrush;
                                    //cb.BorderThickness = GlobalResources.SelectedElementThickness;
                                    cb.IsHighlighted = true;
                                    cb.BringIntoView();
                                    cb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    cb.IsHighlighted = false;
                                    //cb.BorderBrush = GlobalResources.DefaultBooleanBorderBrush;
                                    //cb.BorderThickness = GlobalResources.DefaultBooleanThickness;
                                }
                            };
                        }

                        cb.SetBinding(ThreeStateCheckBox.IsCheckedProperty, new Binding(bindingPath + ".Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                        cb.Content = element.MandatoryLabel;
                        cb.Toggled += new RoutedEventHandler(cb_Changed);

                        cb.Tag = element;

                        //cb.SetBinding(ThreeStateCheckBox.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        uiElem = cb;

                        groupPanel.Children.Add(uiElem);
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.Tuple: {
                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.SingleChoice: {

                        StackPanel subGroupPanel = new StackPanel();
                        AvdComboBox cb = new AvdComboBox();
                        cb.Name = "cbo_" + element.Id.Replace(".", "_").Replace("-", "_");
                        cb.SetBinding(ComboBox.ItemsSourceProperty, new Binding(bindingPath + ".Elements"));
                        cb.SetBinding(ComboBox.SelectedItemProperty, new Binding(bindingPath + ".SelectedValue") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                        cb.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(cb_SelectionChanged));
                        cb.SetValue(ComboBox.VerticalAlignmentProperty, VerticalAlignment.Top);
                        cb.SetValue(ComboBox.DisplayMemberPathProperty, "ComboBoxLabel");
                        cb.SetBinding(ComboBox.IsEnabledProperty, new Binding(bindingPath + "." + writeAllowedRightBindingPath));
                        cb.SelectValueMessage = "Kein Eintrag ausgewählt...";
                        cb.Tag = element;
                        cb.Style = AvdComboBox.AvdComboBoxStyle;
                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            cb.SetValue(ComboBox.ToolTipProperty, element.Documentation);
                        }

                        if (width > 0) cb.Width = width;
                        if (maxWidth > 0) cb.MaxWidth = maxWidth;
                    
                        uiElem = cb;

                        TextBox detailTB = new TextBox();
                        detailTB.Margin = new Thickness(0, 5, 0, 0);
                        detailTB.Visibility = Visibility.Collapsed;
                        detailTB.SetBinding(TextBox.TextProperty, new Binding(bindingPath + ".ValueOther") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                        AssignedDetailTextboxes["cbo_" + element.Id.Replace(".", "_").Replace("-", "_")] = detailTB;

                        Grid grid = new Grid();

                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());

                        if (maxWidth > 0) {
                            grid.ColumnDefinitions[0].MaxWidth = maxWidth;
                            groupPanel.MaxWidth = maxWidth;
                        }
                    
                        Button btnReset = new Button { 
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Left, 
                            Width = 18, Height = 18, Margin = new Thickness(3, 0, 0, 0), 
                            Style = (Style)target.FindResource("ImageButtonStyle") };
                        btnReset.SetBinding(Button.IsEnabledProperty, new Binding(bindingPath + "." + writeAllowedRightBindingPath));
                        btnReset.Content = new Image {
                            Source = ImageReset,
                            Stretch = Stretch.Fill
                        };

                        btnReset.Tag = "cbo_" + element.Id.Replace(".", "_").Replace("-", "_");
                        btnReset.Click += new RoutedEventHandler(btnReset_Click);
                        
                        grid.Children.Add(cb);
                        
                        grid.Children.Add(btnReset);
                        Grid.SetColumn(btnReset, 1);

                        grid.Children.Add(detailTB);
                        Grid.SetRow(detailTB, 1);

                        subGroupPanel.Children.Add(grid);

                        if (forceVerticalOrientation) {
                            Grid.SetRow(subGroupPanel, 1);
                        } else {
                            Grid.SetColumn(subGroupPanel, 1);
                        }

                        if (element.Id.StartsWith("de-gcd_genInfo")) {
                            ptree.GetNode(element.Id).ScrollIntoViewRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    //cb.BorderBrush = GlobalResources.SelectedBorderBrush;
                                    //cb.BorderThickness = GlobalResources.SelectedElementThickness;
                                    cb.IsHighlighted = true;
                                    cb.BringIntoView();
                                    cb.Focus();
                                    GlobalResources.Info.SelectedElement = element;
                                }
                            };
                            ptree.GetNode(element.Id).SearchLeaveFocusRequested += path => {
                                if (path != null && IsCorrectPath(path)) {
                                    cb.IsHighlighted = false;
                                    //cb.BorderBrush = GlobalResources.DefaultComboBoxBorderBrush;
                                    //cb.BorderThickness = GlobalResources.DefaultComboBoxThickness;
                                }
                            };
                        }

                        //subGroupPanel.SetBinding(StackPanel.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                        groupPanel.Children.Add(subGroupPanel);
                        //////////////////////////////////////////////////////////////////////////////////////////////////

                    }
                    break;

                case Taxonomy.Enums.XbrlElementValueTypes.MultipleChoice: {
                    StackPanel sp = new StackPanel { VerticalAlignment = System.Windows.VerticalAlignment.Top, Margin = new Thickness(0,12,0,0) };

                        if (!string.IsNullOrEmpty(element.Documentation)) {
                            sp.ToolTip = element.Documentation;
                        }

                        if (multipleChoiceValues != null) {
                            foreach (var elem in multipleChoiceValues) {
                                ThreeStateCheckBox cb = new ThreeStateCheckBox();
                                cb.Margin = new Thickness(0, 8, 0, 0);
                                cb.Tag = elem;
                                cb.Content = elem.MandatoryLabel;
                                cb.Toggled += new RoutedEventHandler(cb_Changed);

                                cb.SetBinding(ThreeStateCheckBox.IsCheckedProperty, new Binding(bindingPath + ".IsChecked[" + elem.Id + "].BoolValue") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                                cb.SetBinding(ThreeStateCheckBox.IsEnabledProperty, new Binding(bindingPath + "." + writeAllowedRightBindingPath));

                                if (elem.Id.EndsWith(".S")) {
                                    cb.Name = "chk_" + element.Id.Replace(".", "_").Replace("-", "_");
                                }


                                cb.SetBinding(ThreeStateCheckBox.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.TaxonomyPart.ReportParts[" + elem.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

                                
                                ////////////////////////////////////////////////////////////////////////////////
                                // workaround to bind warning/error to choice value elements.
                                //string bindingPath1 = bindingPath.Replace("genInfo.report.id.reportElement", elem.Name);

                                TextBlock warning1;
                                TextBlock error1;
                                StackPanel spWarning = CreateStackPanelWarningReportElement(bindingPath, out warning1, elem.Id);
                                StackPanel spError = CreateStackPanelErrorReportElement(bindingPath, out error1, elem.Id);
                                //spWarning.IsVisibleChanged += new DependencyPropertyChangedEventHandler(ValidationElement_IsVisibleChanged);
                                //spError.IsVisibleChanged += new DependencyPropertyChangedEventHandler(ValidationElement_IsVisibleChanged);
                                warning1.TargetUpdated += new EventHandler<DataTransferEventArgs>(ValidationMessage_TargetUpdated);
                                error1.TargetUpdated += new EventHandler<DataTransferEventArgs>(ValidationMessage_TargetUpdated);
                                ////////////////////////////////////////////////////////////////////////////////

                                UIGroup uiGroup = new UIGroup { UIElement = cb, Warning = warning1, Error = error1 };
                                UIElements.Add(elem.Id, uiGroup);

                                sp.Children.Add(cb);
                                
                                sp.Children.Add(spWarning);                                
                                sp.Children.Add(spError);
                            }
                        }
                    
                        TextBox detailTB = new TextBox();
                        detailTB.Margin = new Thickness(0, 8, 0, 0);
                        detailTB.Visibility = Visibility.Collapsed;
                        detailTB.SetBinding(TextBox.TextProperty, new Binding(bindingPath + ".ValueOther") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

                        AssignedDetailTextboxes["chk_" + element.Id.Replace(".", "_").Replace("-", "_")] = detailTB;

                        sp.Children.Add(detailTB);

                        if (forceVerticalOrientation) {
                            Grid.SetRow(sp, 1);
                        } else {
                            Grid.SetColumn(sp, 1);
                        }

                        groupPanel.Children.Add(sp);
                    }
                    break;

                default:
                    break;
            }
            
            //groupPanel.SetBinding(Grid.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });

            if (uiElem != null) {

                if (uiElem is FrameworkElement) {
                    if (!isReadOnly) {
                        (uiElem as FrameworkElement).SetBinding(FrameworkElement.IsEnabledProperty, new Binding(bindingPath + "." + writeAllowedRightBindingPath));
                    }
                    uiElem.GotFocus += (sender, args) => GlobalResources.Info.SelectedElement = element;
                }


                if (!useExpander) {
                    // add validation warning panel
                    StackPanel spWarning = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                    spWarning.Children.Add(new Image { Width = 12, Source = ImageWarning, Margin = new Thickness(0, 0, 2, 0) });
                    TextBlock warning = new TextBlock();
                    warning.Foreground = Brushes.DarkOrange;
                    warning.SetBinding(TextBlock.TextProperty, new Binding(bindingPath + ".ValidationWarningMessage") { NotifyOnTargetUpdated = true });
                    warning.TargetUpdated += new EventHandler<DataTransferEventArgs>(ValidationMessage_TargetUpdated);

                    spWarning.Children.Add(warning);
                    spWarning.SetBinding(
                        StackPanel.VisibilityProperty,
                        new Binding(bindingPath + ".ValidationWarning") {
                            Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true }
                        });
                    validationInfoPanel.Children.Add(spWarning);

                    // add validation error panel                
                    StackPanel spError = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                    spError.Children.Add(new Image { Width = 12, Source = ImageError, Margin = new Thickness(0, 0, 2, 0) });
                    TextBlock error = new TextBlock();
                    error.Foreground = Brushes.Red;
                    error.SetBinding(TextBlock.TextProperty, new Binding(bindingPath + ".ValidationErrorMessage") { NotifyOnTargetUpdated = true });
                    error.TargetUpdated += new EventHandler<DataTransferEventArgs>(ValidationMessage_TargetUpdated);

                    spError.Children.Add(error);
                    spError.SetBinding(
                        StackPanel.VisibilityProperty,
                        new Binding(bindingPath + ".ValidationError") {
                            Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true }
                        });
                    validationInfoPanel.Children.Add(spError);

                    //validationInfoPanel.SetBinding(StackPanel.VisibilityProperty, new Binding("ValueTreeRoot.ValueTree.Document.GcdPresentationTreeNodes[" + element.Id + "].IsVisible") { Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true } });


                    UIGroup uiGroup = new UIGroup { UIElement = uiElem, Warning = warning, Error = error };
                    this.UIElements.Add(element.Id, uiGroup);

                }
            }
        }

        private bool IsCorrectPath(IList<ISearchableNode> path) { return true; }

        void ValidationMessage_TargetUpdated(object sender, DataTransferEventArgs e) {
            OnPropertyChanged("ContainsValidationWarningElement");
            OnPropertyChanged("ContainsValidationErrorElement");
        }

        private static StackPanel CreateValidationMessageStackPanelReportElement(string bindingPath, bool isError, out TextBlock error1, string elemName) {
            StackPanel spError = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            spError.Children.Add(new Image { Width = 12, Source = isError ? ImageError : ImageWarning, Margin = new Thickness(0, 0, 2, 0) });

            error1 = new TextBlock();

            error1.Foreground = Brushes.Red;
            spError.Visibility = System.Windows.Visibility.Collapsed;

            spError.SetBinding(
                StackPanel.VisibilityProperty,
                new Binding(bindingPath + ".IsChecked[" + elemName + "]" + (isError ? ".ValidationError" : ".ValidationWarning")) {
                    Converter = new BoolToVisibilityConverter { HiddenState = Visibility.Collapsed, VisibleValue = true }                    
                });

            error1.SetBinding(TextBlock.TextProperty, new Binding(bindingPath + ".IsChecked[" + elemName + "]" + (isError ? ".ValidationErrorMessage" : ".ValidationWarningMessage")) { NotifyOnTargetUpdated = true });

            //error1.TargetUpdated += new EventHandler<DataTransferEventArgs>(ValidationMessage_TargetUpdated);
            if (!isError) {
                Style s = new Style();
                //s.Triggers.Add(new DataTrigger() { Binding = new Binding(bindingPath + ".IsChecked[" + elemName + "]" + ".Value.HideAllWarnings"), Value = "true", Setters = { new Setter(StackPanel.OpacityProperty, Double.Parse("0")) } });
                s.Triggers.Add(new DataTrigger() { Binding = new Binding("ValueTreeRoot.Values[" + elemName + "].HideAllWarnings") {NotifyOnTargetUpdated = true}, Value = "true", Setters = { new Setter(StackPanel.OpacityProperty, Double.Parse("0")) } });
                //spError.Style.Triggers.Add(new DataTrigger() { Binding = new Binding(bindingPath + ".IsChecked[" + elemName + "]" + ".Value.HideWarning"), Value = "true", Setters = { new Setter(StackPanel.OpacityProperty, 0) } });
                //ValueTreeRoot.Values[elemName].Value.HideAllWarnings
                spError.Style = s;
            }
            spError.Children.Add(error1);

            return spError;
        }


        private static StackPanel CreateStackPanelErrorReportElement(string bindingPath, out TextBlock textBlock, string elemName) { 
            return CreateValidationMessageStackPanelReportElement(bindingPath, true, out textBlock, elemName); 
        }
        private static StackPanel CreateStackPanelWarningReportElement(string bindingPath, out TextBlock textBlock, string elemName) {
            return CreateValidationMessageStackPanelReportElement(bindingPath, false, out textBlock, elemName); 
        }

        void btnReset_Click(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            string name = btn.Tag.ToString();
            
            DependencyObject grid = VisualTreeHelper.GetParent(btn);
            ComboBox cb = VisualTreeHelper.GetChild(grid, 0) as ComboBox;
            cb.SelectedIndex = -1;
        }
        
        void expander_Expanded(object sender, RoutedEventArgs e) {
            Expander expander = sender as Expander;
            if (expander != null) {
                int expanderGroup = ExpanderGroupIds[expander];

                if (expanderGroup != 0) {
                    foreach (Expander expander1 in ExpanderGroups[expanderGroup]) {
                        if (expander1 != expander) {
                            expander1.IsExpanded = false;
                        }
                    }
                }
            }
        }

        void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            //throw new NotImplementedException();
        }


        public void AddMonetaryInput(Panel target, Taxonomy.IElement element, string prefix, string bindingPath = null, int leftMargin = 0, int level = 0, int width = 200, int maxWidth = 0) {
            Add(
                target,
                element,
                (bindingPath ?? "ValueTreeRoot.Values[" + element.Id + "]"),
                maxLength: 20,
                leftMargin: leftMargin,
                width: width,
                maxWidth: maxWidth,
                bgColor: GlobalResources.ComputationSourceBgBrush,
                prefix: prefix);
        }

        public void AddMonetaryInfo(Panel target, Taxonomy.IElement element, string prefix, string bindingPath = null, int leftMargin = 0, bool isReadOnly = false, int width = 200) {
            Add(
                target,
                element,
                (bindingPath ?? "ValueTreeRoot.Values[" + element.Id + "]"),
                maxLength: 20,
                width: width,
                bgColor: GlobalResources.InfoBgBrush,
                prefix: prefix,
                leftMargin: leftMargin,
                isReadOnly: isReadOnly);
        }

        public void AddMonetaryComputed(Panel target, Taxonomy.IElement element, string prefix, string bindingPath = null, int leftMargin = 0, bool addTopMargin = false, int width = 200) {
            Add(
                target,
                element,
                (bindingPath ?? "ValueTreeRoot.Values[" + element.Id + "]"),
                maxLength: 20,
                leftMargin: leftMargin,
                width: width,
                isReadOnly: true,
                bgColor: Brushes.LightGray,
                prefix: prefix);
        }

        public void AddSeparator(Panel target) {
            target.Children.Add(new Separator { Margin = new Thickness(0, TOP_MARGIN, 0, 0) });
        }

        public void AddDoubleSeparator(Panel target, int width = 0) {
            if (width > 0) {
                target.Children.Add(new Separator { Margin = new Thickness(0, TOP_MARGIN, 0, 0), Width = width, HorizontalAlignment = HorizontalAlignment.Left });
                target.Children.Add(new Separator { Width = width, HorizontalAlignment = HorizontalAlignment.Left });
            } else {
                target.Children.Add(new Separator { Margin = new Thickness(0, TOP_MARGIN, 0, 0) });
                target.Children.Add(new Separator());
            }
        }


        public void AddInfo(
            Panel target,
            Taxonomy.IElement element,
            string bindingPath = null,
            int maxLength = 0,
            int width = 440,
            int height = 0,
            int maxWidth = 0,
            bool useExpander = false,
            int expanderGroup = 0,
            bool forceVerticalOrientation = false,
            bool isReadOnly = false,
            bool hideLabel = false,
            bool setTopMargin = true,
            bool setLeftMargin = false,
            List<Taxonomy.IElement> multipleChoiceValues = null) {

            Add(
                target,
                element,
                (bindingPath == null ? "ValueTreeRoot.Values[" + element.Id + "]" : bindingPath),
                maxLength: maxLength,
                width: width,
                height: height,
                maxWidth: maxWidth,
                useExpander: useExpander,
                expanderGroup: expanderGroup,
                bgColor: GlobalResources.InfoBgBrush,
                forceVerticalOrientation: forceVerticalOrientation,
                isReadOnly: isReadOnly,
                hideLabel: hideLabel,
                setTopMargin: setTopMargin,
                setLeftMargin: setLeftMargin,
                multipleChoiceValues: multipleChoiceValues);
        }

        public void AddCompanyInfo(
            Panel target,
            Taxonomy.IElement element,
            string bindingPath = null,
            int maxLength = 0,
            int width = 440,
            int height = 0,
            int maxWidth = 0,
            bool useExpander = false,
            int expanderGroup = 0,
            bool forceVerticalOrientation = false,
            bool isReadOnly = false,
            bool hideLabel = false,
            bool setTopMargin = true,
            bool setLeftMargin = false,
            List<Taxonomy.IElement> multipleChoiceValues = null) {

            Add(
                target,
                element,
                (bindingPath == null ? "ValueTreeRoot.Values[" + element.Id + "]" : bindingPath),
                maxLength: maxLength,
                width: width,
                height: height,
                maxWidth: maxWidth,
                useExpander: useExpander,
                expanderGroup: expanderGroup,
                bgColor: GlobalResources.InfoBgBrush,
                forceVerticalOrientation: forceVerticalOrientation,
                isReadOnly: isReadOnly,
                hideLabel: hideLabel,
                setTopMargin: setTopMargin,
                setLeftMargin: setLeftMargin,
                multipleChoiceValues: multipleChoiceValues, 
                writeAllowedRightBindingPath: "IsCompanyEditAllowed");
        }

        /// <summary>
        /// Adds a new expander panel.
        /// </summary>
        /// <param name="target">The target panel.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Panel AddExpanderPanel(Panel target, IValueTreeEntry value, int expanderGroup = 0) {
            return AddExpanderPanel(target, value.Element.MandatoryLabel, expanderGroup);
        }

        /// <summary>
        /// Adds a new expander panel.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="header">The header.</param>
        /// <param name="expanderGroup">The expander group.</param>
        /// <returns></returns>
        public Panel AddExpanderPanel(Panel target, string header, int expanderGroup = 0) {
            Expander expander = new Expander { Header = header };
            Panel panel = new StackPanel();
            panel.Margin = new Thickness(25, 0, 0, 8);
            expander.Content = panel;
            target.Children.Add(expander);

            if (!ExpanderGroups.ContainsKey(expanderGroup)) {
                ExpanderGroups[expanderGroup] = new List<Expander>();
            }
            ExpanderGroups[expanderGroup].Add(expander);
            ExpanderGroupIds[expander] = expanderGroup;
            expander.Expanded += new RoutedEventHandler(expander_Expanded);

            return panel;
        }

        public Panel AddExpanderMonetaryInfoPanel(Panel target, Taxonomy.IElement elem, string prefix, string bindingPath = null, int leftMargin = 0, int expanderGroup = 0, bool addTopMargin = false, bool isReadOnly = false, int width = 200) {

            StackPanel expanderHeader = new StackPanel();
            AddMonetaryInfo(expanderHeader, elem, prefix, bindingPath, leftMargin: leftMargin, isReadOnly: isReadOnly, width: width);

            Expander expander = new Expander { Header = expanderHeader };
            Panel panel = new StackPanel();
            panel.Margin = new Thickness(50, 0, 0, 8);

            if (addTopMargin) {
                expander.Margin = new Thickness(0, TOP_MARGIN, 0, 0);
            }

            expander.Content = panel;
            target.Children.Add(expander);

            if (!ExpanderGroups.ContainsKey(expanderGroup)) {
                ExpanderGroups[expanderGroup] = new List<Expander>();
            }
            ExpanderGroups[expanderGroup].Add(expander);
            ExpanderGroupIds[expander] = expanderGroup;
            expander.Expanded += new RoutedEventHandler(expander_Expanded);

            return panel;
        }

        public Panel AddExpanderMonetaryComputedPanel(Panel target, Taxonomy.IElement elem, string prefix, string bindingPath = null, int leftMargin = 0, int expanderGroup = 0, bool addTopMargin = false, int width = 200) {

            StackPanel expanderHeader = new StackPanel();
            AddMonetaryComputed(expanderHeader, elem, prefix, bindingPath, leftMargin: leftMargin, width: width);

            Expander expander = new Expander { Header = expanderHeader };
            Panel panel = new StackPanel();

            panel.Margin = new Thickness(50, 0, 0, 8);

            if (addTopMargin) {
                expander.Margin = new Thickness(0, TOP_MARGIN, 0, 0);
            }

            expander.Content = panel;
            target.Children.Add(expander);

            if (!ExpanderGroups.ContainsKey(expanderGroup)) {
                ExpanderGroups[expanderGroup] = new List<Expander>();
            }
            ExpanderGroups[expanderGroup].Add(expander);
            ExpanderGroupIds[expander] = expanderGroup;
            expander.Expanded += new RoutedEventHandler(expander_Expanded);

            return panel;
        }

        static BitmapImage ImageReset = new BitmapImage(new Uri("pack://application:,,,/eBalanceKitResources;component/Resources/delete.png"));
        static BitmapImage ImageWarning = new BitmapImage(new Uri("pack://application:,,,/eBalanceKitResources;component/Resources/ValidationWarn.png"));
        static BitmapImage ImageError = new BitmapImage(new Uri("pack://application:,,,/eBalanceKitResources;component/Resources/ValidationError.png"));

        public bool ContainsValidationWarningElement {
            get {
                foreach (UIGroup uiGroup in UIElements.Values) {

                    if ((uiGroup.Warning as TextBlock).Visibility != Visibility.Visible) {
                        return false;
                    }

                    if (!(string.IsNullOrEmpty((uiGroup.Warning as TextBlock).Text))) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ContainsValidationErrorElement {
            get {
                foreach (UIGroup uiGroup in UIElements.Values) {
                    if (!(string.IsNullOrEmpty((uiGroup.Error as TextBlock).Text))) return true;
                }
                return false;
            }
        }

        #endregion methods

    }
}