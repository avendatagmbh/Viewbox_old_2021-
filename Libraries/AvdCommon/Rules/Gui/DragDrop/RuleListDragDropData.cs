using System.Collections.ObjectModel;
using System.ComponentModel;
using AvdCommon.Rules.Gui.TreeNodes;

namespace AvdCommon.Rules.Gui.DragDrop
{
    public class RuleListDragDropData : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public RuleListDragDropData()
        {
            Rules = new ObservableCollection<Rule>();
            Clear();
        }

        public void Clear()
        {
            Rules.Clear();
            _dragging = false;
            DragSource = null;
            DeleteRuleOnFinished = true;
            SourceRuleSet = null;
        }

        #region Properties

        private bool _dragging;
        public ObservableCollection<Rule> Rules { get; set; }

        public bool Dragging
        {
            get { return _dragging; }
            set
            {
                if (_dragging != value)
                {
                    _dragging = value;
                    OnPropertyChanged("Dragging");
                }
            }
        }

        public RuleTreeNode DragSource { get; set; }
        public bool DeleteRuleOnFinished { get; set; }
        public RuleSet SourceRuleSet { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}