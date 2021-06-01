// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace ScreenshotAnalyzer.Models.ListView {

    interface IListViewModel {

        /// <summary>
        /// ObservableCollection of all available items.
        /// </summary>
        object Items { get; }

        /// <summary>
        /// Panel, which contains the detail data for selected items.
        /// </summary>
        Panel DataPanel { get; set; }
                        
        /// <summary>
        /// Selected list entry.
        /// </summary>
        object SelectedItem { get; set; }
        
        /// <summary>
        /// Owner window.
        /// </summary>
        Window Owner { get; set; }

        /// <summary>
        /// Initializes the user interface elements in the DataPanel panel.
        /// </summary>
        /// <param name="dataPanel"></param>
        void InitUiElements(Panel dataPanel, Panel listPanel);

        /// <summary>
        /// Adds a new item to the Items collection.
        /// </summary>
        bool AddItem();
        
        /// <summary>
        /// Deletes the selected item from the Items collection.
        /// </summary>
        void DeleteSelectedItem();

        /// <summary>
        /// Deletes the specified item from the Items collection.
        /// </summary>
        void DeleteItem(object item);

        string HeaderString { get; }
    }
}