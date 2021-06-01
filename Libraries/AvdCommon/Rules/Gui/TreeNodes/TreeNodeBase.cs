using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Utils;

namespace AvdCommon.Rules.Gui.TreeNodes
{
    public abstract class TreeNodeBase : INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        public TreeNodeBase(TreeNodeBase parent, Window window)
        {
            _parent = parent;
            OwnerWindow = window;
        }

        #region Properties

        #region Children

        private ObservableCollectionAsync<TreeNodeBase> _children = new ObservableCollectionAsync<TreeNodeBase>();

        public ObservableCollectionAsync<TreeNodeBase> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        #endregion Children

        #region IsSelected

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        #endregion

        #region IsExpanded

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        #endregion

        #region Parent

        private readonly TreeNodeBase _parent;

        public TreeNodeBase Parent
        {
            get { return _parent; }
        }

        #endregion

        public abstract string Name { get; }
        protected Window OwnerWindow { get; set; }

        public TreeNodeBase Root
        {
            get { return Parent == null ? this : Parent.Root; }
        }

        #endregion Properties

        #region Methods

        public abstract void AddRules(List<Rule> rules);

        #endregion Methods
    }
}