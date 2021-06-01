using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.Keys {
    public interface IDisplayKey { 
        string Label { get; }
        IDisplayDbKey Key { get; }
        ObservableCollection<IDisplayKey> Children { get; }
        void LoadSubKeys();
        bool IsSelected { get; set; }
        bool IsVisible { get; set; }
        event KeyManager.SelectedChangedEventHandler OnIsSelectedChanged;
        IDisplayKey Parent { get; }
        KeySearchManager KeySearchManagerInstance { get; }
    }
}
