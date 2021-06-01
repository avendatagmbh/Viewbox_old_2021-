using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyDocExtraction.DataAccess
{
    public abstract class DataAccessBase<TEntity>
    {
        #region Fields
        private EasyArchiveRepository _repository;
        #endregion

        #region Constructor
        protected DataAccessBase() 
        {
            _repository = new EasyArchiveRepository();
        }
        #endregion

        #region Properties
        protected EasyArchiveRepository Repository
        {
            get { return _repository; }
            set { _repository = value; }
        }
        #endregion

        #region Methods
        public TEntity Find(params object[] keyValues)
        {
            var findMethod = typeof(EasyArchiveRepository).GetMethod("Find");
            return (TEntity)findMethod.Invoke(Repository, keyValues);
        }
        public bool Exists(params object[] keyValues) 
        {
            return Find(keyValues) != null;
        }
        public bool Add(params TEntity[] entities){
            try
            {
                // TODO carry on here
                var entityProperty = typeof(EasyArchiveRepository).GetProperty(this.GetType().Name);
                var addMethod = Type.GetType(this.GetType().FullName).GetMethod("Add");
                var propertyValue= entityProperty.GetValue(_repository, null);
                addMethod.Invoke(propertyValue, entities.OfType<object>().ToArray());
                return true;
            }
            catch (Exception ex) 
            {
                Logger.WriteError("Error while trying to Add " + typeof(TEntity).Name + " to the repository.", ex);
                return false;
            }
        }
        public int Save() 
        {
            try
            {
                return Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error while trying to SaveChanges for type " + typeof(TEntity).Name + " in the repository.", ex);
            }
            return -1;
        }
        public int Save(params TEntity[] entities)
        {
            if (Add(entities)) 
            {
                return Save();
            }
            return -1;
        }
        #endregion

    }
}
