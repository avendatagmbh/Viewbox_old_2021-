using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;
using DbSearchLogic.SearchCore.ForeignKeySearch;

namespace DbSearchLogic.SearchCore.Keys {
    public class DisplayForeignKey : IDisplayForeignKey {

        private Func<IDisplayDbKey, IKey> _initAction;

        public DisplayForeignKey(ForeignKey foreignKey, Func<IDisplayDbKey, IKey> initAction, bool isChildOfPrimaryKey, IDisplayKey parentKey = null) {
            this.Id = foreignKey.Id;
            this._initAction = initAction;
            this.ForeignKeyColumns = new List<IColumn>();
            this.PrimaryKeyColumns = new List<IColumn>();
            if (isChildOfPrimaryKey && parentKey != null) {
                parentKey.KeySearchManagerInstance.InitForeignKey(foreignKey, null);
                Initialize();
            }
            this.Label = isChildOfPrimaryKey ? foreignKey.DisplayStringForPrimaryKey : foreignKey.Label;
        }

        public void Initialize() {
            ForeignKey key = (ForeignKey)_initAction(this);
            this.ForeignKeyColumns.AddRange(key.ForeignKeyColumns);
            this.PrimaryKeyColumns.AddRange(key.PrimaryKeyColumns);
            this.ForeignKeyTableName = key.ForeignKeyTable;
            this.PrimaryKeyTableName = key.PrimaryKeyTable;
            this.DisplayString = key.DisplayString;
        }

        public bool IsInitialized { get { return PrimaryKeyColumns.Any(); } }

        public string ForeignKeyTableName { get; internal set; }

        public string PrimaryKeyTableName { get; internal set; }

        public List<IColumn> ForeignKeyColumns { get; internal set; }

        public List<IColumn> PrimaryKeyColumns { get; internal set; }

        public int Id { get; internal set; }

        public string Label { get; internal set; }

        public string DisplayString { get; internal set; }
    }
}
