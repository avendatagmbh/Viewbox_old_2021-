using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;
using DbSearchLogic.SearchCore.KeySearch;

namespace DbSearchLogic.SearchCore.Keys {
    public class DisplayPrimaryKey : IDisplayPrimaryKey {

        private Func<IDisplayDbKey, IKey> _initAction;

        public DisplayPrimaryKey(Key primaryKey, Func<IDisplayDbKey, IKey> initAction) {
            this.Id = primaryKey.Id;
            this.TableName = primaryKey.TableName;
            this.Label = primaryKey.Label;
            this.Columns = new List<IColumn>();
            this._initAction = initAction;
        }

        public void Initialize() {
            IKey key = _initAction(this);
            this.Columns.AddRange(key.Columns);
            this.TableName = key.TableName;
            this.DisplayString = key.DisplayString;
        }

        public bool IsInitialized { get { return Columns.Any(); } }

        public string TableName { get; internal set; }

        public List<IColumn> Columns { get; internal set; }

        public int Id { get; internal set; }

        public string Label { get; internal set; }

        public string DisplayString { get; internal set; }
    }
}
