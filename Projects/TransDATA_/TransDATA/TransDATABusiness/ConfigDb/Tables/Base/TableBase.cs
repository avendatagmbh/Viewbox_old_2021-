/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * hel                  2010-11-19      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DbAccess;
using DbAccess.Attributes;

namespace TransDATABusiness.ConfigDb.Tables.Base {

    /// <summary>
    /// Base class for all table mappings.
    /// </summary>
    public class TableBase : INotifyPropertyChanged {

        #region events

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /***************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        protected void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion eventTrigger

        /***************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>       
        [DbColumn("id", AutoIncrement = true, AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        
        #endregion properties

        /***************************************************************************************/

        #region methods

        /// <summary>
        /// Saves or updates the assigned dataset.
        /// </summary>
        public void SaveOrUpdate() {
            try {
                using (IDatabase conn = Global.AppConfig.ConfigDb.ConnectionManager.GetConnection()) {
                    SaveOrUpdate(conn);
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Saves or updates the assigned dataset.
        /// </summary>
        /// <param name="conn">Open database connection.</param>
        public virtual void SaveOrUpdate(IDatabase conn)
        {
            try {
                conn.BeginTransaction();
                conn.DbMapping.Save(this);
                conn.CommitTransaction();

            } catch (Exception ex) {
                conn.RollbackTransaction();
                throw ex;
            }
        }

        /// <summary>
        /// Deletes the specified mapping object.
        /// </summary>
        public void Delete() {
            if (!Global.AppConfig.ConfigDb.ConnectionManager.IsDisposed) {
                using (IDatabase conn = Global.AppConfig.ConfigDb.ConnectionManager.GetConnection()) {
                    Delete(conn);
                }
            }
        }

        /// <summary>
        /// Deletes the specified mapping object.
        /// </summary>
        /// <param name="conn">Open database connection.</param>
        public virtual void Delete(IDatabase conn) {
            conn.DbMapping.Delete(this);
        }

        #endregion methods
    }
}
