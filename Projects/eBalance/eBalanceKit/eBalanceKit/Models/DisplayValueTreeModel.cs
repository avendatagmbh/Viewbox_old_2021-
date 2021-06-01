// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-01-04
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Utils;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKit.Structures;
using System.ComponentModel;
using Taxonomy;
using System.Windows;
using System.Windows.Controls;
using AvdWpfControls;

namespace eBalanceKit.Models {
    
    public class DisplayValueTreeModel : NotifyPropertyChangedBase {

        protected override void OnPropertyChanged(string propertyName) {
            if (propertyName == "SelectionChanged") {
                
            }
            base.OnPropertyChanged(propertyName);
        }

        public DisplayValueTreeModel(ITaxonomy taxonomy, ValueTreeWrapper valueTreeWrapper) {
            IsInitalized = false;

            UIElements = new TaxonomyUIElements();
            
            _valueTreeWrapper = valueTreeWrapper;
            _valueTreeWrapper.PropertyChanged += ValueTreeWrapperPropertyChanged;

            if (taxonomy != null) {
                Elements = taxonomy.Elements;
            }
            Children = new List<DisplayValueTreeModel>();
        }

        void ValueTreeWrapperPropertyChanged(object sender, PropertyChangedEventArgs e) { OnPropertyChanged("ValueTreeRoot"); }

        #region eventHandler

        private void TextElement_GotFocus(object sender, RoutedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((TextBox) sender).Tag; }
        void DatePickerElement_GotFocus(object sender, RoutedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((DatePicker) sender).Tag; }
        void CheckBoxElement_GotFocus(object sender, RoutedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((ThreeStateCheckBox) sender).Tag; }
        void ComboBoxElement_GotFocus(object sender, RoutedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((ComboBox) sender).Tag; }
        void ContentControlElement_GotFocus(object sender, RoutedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((ContentControl) sender).Tag; }
        void DatePickerElement_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) { GlobalResources.Info.SelectedElement = (IElement) ((DatePicker) sender).Tag; }

        #endregion eventHandler

        #region properties

        #region ValueTreeWrapper
        public ValueTreeWrapper ValueTreeWrapper {
            get { return _valueTreeWrapper; }
            set { _valueTreeWrapper = value; }
        }
        private ValueTreeWrapper _valueTreeWrapper;
        #endregion
        
        public TaxonomyViewModel TaxonomyViewmodel { get; set; }
        public bool IsInitalized { get; set; }
        public ValueTreeNode ValueTreeRoot { get { return _valueTreeWrapper.ValueTreeRoot; } }
        public TaxonomyUIElements UIElements { get; set; }
        public Dictionary<string, IElement> Elements { get; private set; }

        #region UpdateChildrenContainsValidationErrorElement
        public void UpdateChildrenContainsValidationErrorElement() {
            OnPropertyChanged("ContainsValidationErrorElement");
            UIElements.ContainsValidationElementChanged();
            foreach (var child in Children)
                child.UpdateChildrenContainsValidationErrorElement();
        }
        #endregion UpdateChildrenContainsValidationErrorElement

        #region UpdateChildrenContainsValidationWarningElement
        public void UpdateChildrenContainsValidationWarningElement() {
            OnPropertyChanged("ContainsValidationWarningElement");
            UIElements.ContainsValidationElementChanged();
            foreach (var child in Children)
                child.UpdateChildrenContainsValidationWarningElement();
        }
        #endregion UpdateChildrenContainsValidationWarningElement

        #region ContainsValidationErrorElement
        public bool ContainsValidationErrorElement {
            get {
                foreach (var child in Children) {
                    if (child.ContainsValidationErrorElement) return true;
                }
                return UIElements.ContainsValidationErrorElement;
            }
        }
        #endregion

        #region ContainsValidationWarningElement
        public bool ContainsValidationWarningElement {
            get {
                // hide the validation warnings if the UserOptions tell to not to show it
                //if (eBalanceKitBusiness.Options.GlobalUserOptions.UserOptions.HideAllWarnings) {
                //    return false;
                //}

                foreach (var child in Children) {
                    if (child.ContainsValidationWarningElement) return true;
                }
                return UIElements.ContainsValidationWarningElement;
            }
        }
        #endregion

        private List<DisplayValueTreeModel> Children { get; set; }

        public void AddChild(DisplayValueTreeModel child) {
            Children.Add(child);
        }


        #endregion properties

        #region methods

        /// <summary>
        /// Registers the got focus event handler.
        /// </summary>
        public void RegisterGotFocusEventHandler() {
            foreach (var uiGroup in UIElements.UIElements.Values) {
                switch (uiGroup.UIElement.DependencyObjectType.Name) {
                    case "NumericTextbox":
                    case "TextBox":
                    case "SearchableTextBox":
                        uiGroup.UIElement.GotFocus += TextElement_GotFocus;
                        break;

                    case "DatePicker":
                    case "SearchableDatePicker":
                        uiGroup.UIElement.GotFocus += DatePickerElement_GotFocus;
                        uiGroup.UIElement.GotKeyboardFocus += DatePickerElement_GotKeyboardFocus;
                        break;

                    case "ThreeStateCheckBox":
                        uiGroup.UIElement.GotFocus += CheckBoxElement_GotFocus;
                        break;

                    case "AvdComboBox":
                    case "ComboBox":
                        uiGroup.UIElement.GotFocus += ComboBoxElement_GotFocus;
                        break;

                    case "ContentControl":
                        uiGroup.UIElement.GotFocus += ContentControlElement_GotFocus;
                        break;

                    default:
                        break;

                }

            }
        }
               
        #endregion methods
    }
}
