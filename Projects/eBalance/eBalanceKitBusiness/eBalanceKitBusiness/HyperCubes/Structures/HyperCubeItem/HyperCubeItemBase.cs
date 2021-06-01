// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem {
    internal class HyperCubeItemBase : NotifyPropertyChangedBase, IHyperCubeItem {

        protected HyperCubeItemBase(HyperCubeItemCollection items, DbEntityHyperCubeItem dbEntity, IElement primaryDimensionValue) {
            DbEntity = dbEntity;
            PrimaryDimensionValue = primaryDimensionValue;
            Items = items;
        }

        protected HyperCubeItemCollection Items { get; private set; }
        protected override void OnPropertyChanged(string propertyName) {
            if (propertyName == "IsLocked" || propertyName == "IsComputed")
                OnPropertyChanged("IsEditable");

            base.OnPropertyChanged(propertyName);
        }

        private DbEntityHyperCubeItem DbEntity { get; set; }

        #region Value
        public virtual object Value {
            get {
                if (!Items.Cube.Document.ReportRights.ReadAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentReadRights);

                return DbEntity.Value;
            }
            set {
                if (!Items.Cube.Document.ReportRights.WriteAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentWriteRights);

                string newValue = (value == null || string.IsNullOrEmpty(value.ToString()) ? null : value.ToString());
                string oldValue = DbEntity.Value;
                if (oldValue == newValue) return;
                DbEntity.Value = newValue;
                using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.DbMapping.Save(DbEntity);
                    } catch (System.Exception ex) {
                        throw new Exception(ex.Message, ex);
                    }
                }

                LogManager.Instance.UpdateHyperCubeValue(Items.Cube, DbEntity.DimensionId, oldValue, newValue);

                OnPropertyChanged("Value");
            }
        }
        #endregion Value

        public IElement PrimaryDimensionValue { get; internal set; }

        #region Context
        private IScenarioContext _context;

        public IScenarioContext Context {
            get { return _context; }
            internal set {
                if (_context == value) return;
                _context = value;
            }
        }
        #endregion Context

        #region HasValue
        public virtual bool HasValue { get { return Value != null; } }
        #endregion

        #region IsEditable
        public bool IsEditable { get { return !IsLocked && !IsComputed; } }
        #endregion

        #region IsComputed
        public virtual bool IsComputed { get { return false; } }
        #endregion

        #region IsLocked
        public virtual bool IsLocked {
            get { return false; }
// ReSharper disable ValueParameterNotUsed
            protected set {
// ReSharper restore ValueParameterNotUsed
                /* nothing to do */
            }
        }
        #endregion

    }
}