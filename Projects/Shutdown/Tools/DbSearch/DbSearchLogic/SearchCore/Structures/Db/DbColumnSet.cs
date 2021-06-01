using System;
using System.Collections.Generic;

namespace DbSearchLogic.SearchCore.Structures.Db {

    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>19.02.2010</since>
    public class DbColumnSet : IComparable {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbKey"/> class.
        /// </summary>
        private DbColumnSet() {
            this.Id = -1;
            this.TableName = null;
            this.Columns = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbKey"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public DbColumnSet(string tableName) {
            this.Id = -1;
            this.TableName = tableName;
            this.Columns = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbKey"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="tableName">Name of the table.</param>
        public DbColumnSet(int id, string tableName) {
            this.Id = id;
            this.TableName = tableName;
            this.Columns = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbKey"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fields">The fields.</param>
        public DbColumnSet(string tableName, List<string> fields) {
            this.Id = -1;
            this.TableName = tableName;
            this.Columns = fields;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Columns { get; protected set; }

        /// <summary>
        /// Adds a new field.
        /// </summary>
        /// <param name="field">The field.</param>
        public void AddField(string field) {
            this.Columns.Add(field);
        }

        /// <summary>
        /// Determines whether the instance is enclosed by the specified field list.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>
        /// 	<c>true</c> if the instance is enclosed by the specified field list; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainedIn(List<string> fields) {
            foreach (string sField in this.Columns) {
                if (!fields.Contains(sField)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public DbColumnSet Clone() {
            DbColumnSet oClone = new DbColumnSet();
            
            oClone.TableName = this.TableName;
            
            foreach (string sField in this.Columns) {
                oClone.Columns.Add(sField);
            }

            return oClone;
        }

        #region IComparable Member

        /// <summary>
        /// Vergleicht die aktuelle Instanz mit einem anderen Objekt desselben Typs.
        /// </summary>
        /// <param name="obj">Ein Objekt, das mit dieser Instanz verglichen werden soll.</param>
        /// <returns>
        /// Eine 32-Bit-Ganzzahl mit Vorzeichen, die die relative Reihenfolge der verglichenen Objekte angibt. Der Rückgabewert hat folgende Bedeutung:
        /// Wert
        /// Bedeutung
        /// Kleiner als 0
        /// Diese Instanz ist kleiner als <paramref name="obj"/>.
        /// 0
        /// Diese Instanz ist gleich <paramref name="obj"/>.
        /// Größer als 0
        /// Diese Instanz ist größer als <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="obj"/> hat nicht denselben Typ wie diese Instanz.
        /// </exception>
        int IComparable.CompareTo(object obj) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
