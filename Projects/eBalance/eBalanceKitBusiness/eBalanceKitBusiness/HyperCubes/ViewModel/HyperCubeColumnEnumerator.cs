using System;
using System.Collections;
using System.Collections.Generic;
using eBalanceKitBusiness.HyperCubes.Interfaces;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModel;

namespace eBalanceKitBusiness.HyperCubes.ViewModel {
    internal class HyperCubeColumnEnumerator : IEnumerator<IHyperCubeColumn> {
        public HyperCubeColumnEnumerator(IEnumerable<IHyperCubeColumn> roots) {
            _roots = roots;
            PopulateEnumData();
        }

        private readonly IEnumerable<IHyperCubeColumn> _roots;
        private readonly Queue<IHyperCubeColumn> _enumData = new Queue<IHyperCubeColumn>();
        private IHyperCubeColumn _currentItem;
        
        private void PopulateEnumData() {
            _currentItem = null;
            _enumData.Clear();
            foreach (var root in _roots) PopulateEnumData(root);        
        }

        private void PopulateEnumData(IHyperCubeColumn root) {
            _enumData.Enqueue(root);
            foreach (var child in root.Children) PopulateEnumData(child);
        }
        
        public IHyperCubeColumn Current {
            get {
                if (_currentItem == null) throw new InvalidOperationException("Use MoveNext before calling Current");
                return _currentItem;
            }
        }

        object IEnumerator.Current {
            get {
                if (_currentItem == null) throw new InvalidOperationException("Use MoveNext before calling Current");
                return _currentItem;
            }
        }

        public bool MoveNext() {
            if (_enumData.Count > 0) {
                _currentItem = _enumData.Dequeue();
                return true;
            }
            return false;
        }

        void IEnumerator.Reset() { PopulateEnumData(); }
        
        public void Dispose() { }
    }
}