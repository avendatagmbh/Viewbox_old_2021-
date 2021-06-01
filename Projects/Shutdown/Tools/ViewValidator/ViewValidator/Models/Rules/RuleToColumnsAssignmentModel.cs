// -----------------------------------------------------------
// Created by Benjamin Held - 30.08.2011 10:27:43
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.ObjectModel;
using System.Windows;
using AvdCommon.Rules.Gui.DragDrop;
using AvdCommon.Rules.Gui.TreeNodes;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Rules {
    public class RuleToColumnsAssignmentModel {
        #region Constructor
        public RuleToColumnsAssignmentModel(int which, Window ownerWindow, RuleListDragDropData dragDropData) {
            this.ColumnNodes = new ObservableCollection<TreeNodeBase>();
            Which = which;
            OwnerWindow = ownerWindow;
            DragDropData = dragDropData;
        }
        #endregion

        

        #region Properties
        public RuleListDragDropData DragDropData { get; set; }
        public ObservableCollection<TreeNodeBase> ColumnNodes { get; set; }
        private int Which { get; set; }

        #region TableMapping
        private TableMapping _tableMapping;
        public TableMapping TableMapping {
            get { return _tableMapping; }
            set {
                if (_tableMapping != value) {
                    _tableMapping = value;
                    FillColumnNodes();
                    _tableMapping.ColumnMappings.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ColumnMappings_CollectionChanged);
                }
            }
        }

        private void FillColumnNodes() {
            ColumnNodes.Clear();
            foreach (var columnMapping in _tableMapping.ColumnMappings) {
                ColumnNodes.Add(new ColumnTreeNode(columnMapping.GetColumn(Which), OwnerWindow));
            }
        }

        void ColumnMappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            FillColumnNodes();
        }

        private Window OwnerWindow { get; set; }
        #endregion

        #endregion
    }
}
