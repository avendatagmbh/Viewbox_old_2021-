// --------------------------------------------------------------------------------
// author: Lajos Szoke
// since: 2011-12-12
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Threading;

namespace eBalanceKit.Models
{
    /// <summary>
    /// Class for locking, and unlocking treeviews, and listboxes ItemsSource Property - Better performance
    /// </summary>
    public class SourceLocker
    {
        #region Properties
        /// <summary>
        /// Dictionary for storing the locked ItemsSources
        /// </summary>
        private static readonly Dictionary<object, IEnumerable> LockedSources = new Dictionary<object, IEnumerable>();
        #endregion Properties

        #region Methods

        #region LockSource
        /// <summary>
        /// It stores the treeview or listbox ItemsSource property to LockedSources and it sets the property to null
        /// </summary>
        /// <param name="source"></param>
        public static void LockSource(object source)
        {
            if (LockedSources.ContainsKey(source)) return;
            if (source is TreeView)
            {
                var itemSource = (source as TreeView).ItemsSource;
                LockedSources.Add(source, itemSource);
                (source as TreeView).ItemsSource = null;
            }
            else if (source is ListBox)
            {
                var itemSource = (source as ListBox).ItemsSource;
                LockedSources.Add(source, itemSource);
                (source as ListBox).ItemsSource = null;
            }
        }
        #endregion LockSource

        #region UnLockSource
        /// <summary>
        /// It retrieves the ItemsSource property´s value from LockedSources
        /// </summary>
        /// <param name="source"></param>
        public static void UnLockSource(object source)
        {
            if (!LockedSources.ContainsKey(source)) return;
            if (source is TreeView)
            {
                (source as TreeView).ItemsSource = LockedSources[source];
                LockedSources.Remove(source);
            }
            else if (source is ListBox)
            {
                (source as ListBox).ItemsSource = LockedSources[source];
                LockedSources.Remove(source);
            }
            Thread.Sleep(15);
        }
        #endregion UnLockSource

        #endregion Methods
    }
}
