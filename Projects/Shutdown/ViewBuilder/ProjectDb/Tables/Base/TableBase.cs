using System.ComponentModel;
using DbAccess;

namespace ProjectDb.Tables.Base
{
    /// <summary>
    ///   Base class for all table mapping classes.
    /// </summary>
    public class TableBase : INotifyPropertyChanged
    {
        #region properties

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value> The id. </value>
        [DbColumn("id", AutoIncrement = true, AllowDbNull = false), DbPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        ///   Gets or sets the interface to the assigned system database.
        /// </summary>
        /// <value> The sys db. </value>
        public ProjectDb ProjectDb { get; set; }

        #endregion properties

        #region events

        /// <summary>
        ///   Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region methods

        /// <summary>
        ///   Saves or updates the assigned dataset.
        /// </summary>
        public void SaveOrUpdate()
        {
            using (IDatabase conn = ProjectDb.ConnectionManager.GetConnection())
            {
                SaveOrUpdate(conn);
            }
        }

        /// <summary>
        ///   Saves or updates the assigned dataset.
        /// </summary>
        /// <param name="conn"> Open database connection. </param>
        public virtual void SaveOrUpdate(IDatabase conn)
        {
            conn.DbMapping.Save(this);
        }

        /// <summary>
        ///   Deletes the specified mapping object.
        /// </summary>
        public void Delete()
        {
            if (!ProjectDb.ConnectionManager.IsDisposed)
            {
                using (IDatabase conn = ProjectDb.ConnectionManager.GetConnection())
                {
                    Delete(conn);
                }
            }
        }

        /// <summary>
        ///   Deletes the specified mapping object.
        /// </summary>
        /// <param name="conn"> Open database connection. </param>
        public virtual void Delete(IDatabase conn)
        {
            conn.DbMapping.Delete(this);
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}