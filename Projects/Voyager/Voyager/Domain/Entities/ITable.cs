using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace Domain.Entities {
   public interface ITable {

  /***************************************************************/
    #region methodes

       /// <summary>
       /// Creates a new table for the actual selected entitie implementation.
       /// </summary>
       /// <param name="db">The db.</param>
       void CreateTable(IDatabase db);

       /// <summary>
       /// Saves the specified dataset.
       /// </summary>
       /// <param name="db">The db.</param>
       void Save(IDatabase db);

       ///// <summary>
       ///// Loads the dataset with specified ID from specified database.
       ///// </summary>
       ///// <param name="db">The db.</param>
       ///// <param name="id">The id.</param>
       ///// <returns></returns>
       //T Load(IDatabase db, int id);

       ///// <summary>
       ///// Gets the list of specified object.
       ///// </summary>
       ///// <param name="db">The db.</param>
       ///// <returns></returns>
       //List<T> GetList(IDatabase db);

    #endregion methodes 

   }
}
