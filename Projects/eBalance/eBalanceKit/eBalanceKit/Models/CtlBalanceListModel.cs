/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-12-13      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace eBalanceKit.Models {

    /// <summary>
    /// Model class for all taxonomy views. This model combines all nessesary structures which are needed in the
    /// sevaral taxonomy detail views, e.g. balance sheets (Bilanzen) or income statements (GuV).
    /// </summary>
    class CtlBalanceListModel : INotifyPropertyChanged {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CtlBalanceListModel"/> class.
        /// </summary>
        public CtlBalanceListModel() {
            _balanceListWidth = new GridLength(500);
        }


        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region fields

        /// <summary>
        /// See property BalanceListWidth.
        /// </summary>
        private GridLength _balanceListWidth;

        /// <summary>
        /// See property BalanceListSelectedIndex.
        /// </summary>
        private int _balanceListSelectedIndex;

        /// <summary>
        /// See property Filter.
        /// </summary>
        private string _filter;

        private bool _exactSearch;

        #endregion fields

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the width of the balance list.
        /// </summary>
        /// <value>The width of the balance list.</value>
        public GridLength BalanceListWidth {
            get { return _balanceListWidth; }
            set {
                if (_balanceListWidth != value) {
                    _balanceListWidth = value;
                    OnPropertyChanged("BalanceListWidth");
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of the balance list selected.
        /// </summary>
        /// <value>The index of the balance list selected.</value>
        public int BalanceListSelectedIndex {
            get { return _balanceListSelectedIndex; }
            set {
                if (_balanceListSelectedIndex != value) {
                    _balanceListSelectedIndex = value;
                    OnPropertyChanged("BalanceListSelectedIndex");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        public string Filter {
            get { return _filter; }
            set {
                if (Filter != value) {
                    _filter = value;
                    OnPropertyChanged("Filter");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the an exact search should be applied.
        /// </summary>
        /// <value><c>true</c> if an exact search should be applied; otherwise, <c>false</c>.</value>
        public bool ExactSearch {
            get { return _exactSearch; }
            set {
                if (_exactSearch != value) {
                    _exactSearch = value;
                    OnPropertyChanged("ExactSearch");
                }
            }
        }

        #endregion properties
    }
}
