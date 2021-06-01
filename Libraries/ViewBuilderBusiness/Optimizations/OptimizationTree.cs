using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ViewBuilderBusiness.Optimizations
{
    public class OptimizationTree : ICollection<OptimizationTree.Node>
    {
        private readonly IList<Node> _nodeList = new List<Node>();
        private readonly string _rootNodeValue;

        public OptimizationTree(string rootNodeValue)
        {
            _rootNodeValue = rootNodeValue;
            _nodeList.Add(new Node
                              {
                                  Id = 1,
                                  ParentId = 0,
                                  Level = 1,
                                  Value = _rootNodeValue
                              });
        }

        #region ICollection<Node> Members

        public int Count
        {
            get { return _nodeList.Count; }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodeList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Node item)
        {
            _nodeList.Add(item);
        }

        public void Clear()
        {
            _nodeList.Clear();
        }

        public bool Contains(Node item)
        {
            return _nodeList.Contains(item);
        }

        public void CopyTo(Node[] array, int arrayIndex)
        {
            _nodeList.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return _nodeList.IsReadOnly; }
        }

        public bool Remove(Node item)
        {
            return _nodeList.Remove(item);
        }

        #endregion

        public void Add(string parentNodeValue, string childNodeValue, int parentLevel, string fullParentNodeValue)
        {
            var parentNodes = _nodeList.Where(n => n.Value == parentNodeValue && n.Level == parentLevel).ToList();
            Node parentNode = null;
            if (!String.IsNullOrEmpty(fullParentNodeValue))
                foreach (var node in parentNodes)
                {
                    var extraparent = _nodeList.FirstOrDefault(w => w.Id == node.ParentId);
                    if (extraparent != null && extraparent.Value == fullParentNodeValue)
                    {
                        parentNode = node;
                        break;
                    }
                }
            if (parentNode == null)
                parentNode = parentNodes.FirstOrDefault();
            if (parentNode == null)
            {
                throw new ArgumentException(string.Format(
                    "Cannot find parent node with this name: {0}", parentNodeValue));
            }
            var isChildNodeExist = _nodeList.Any(n => n.Value == childNodeValue && n.ParentId == parentNode.Id);
            if (!isChildNodeExist)
            {
                _nodeList.Add(new Node
                                  {
                                      Id = _nodeList.Count + 1,
                                      ParentId = parentNode.Id,
                                      Value = childNodeValue,
                                      Level = parentNode.Level + 1
                                  });
            }
        }

        #region Nested type: Node

        public class Node : IEquatable<Node>
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Value { get; set; }
            public int Level { get; set; }

            #region IEquatable<Node> Members

            public bool Equals(Node other)
            {
                return
                    Id == other.Id &&
                    ParentId == other.ParentId &&
                    Value == other.Value &&
                    Level == other.Level;
            }

            #endregion

            public override bool Equals(object obj)
            {
                return obj is Node && (obj as Node).Equals(this);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode() + ParentId.GetHashCode() + Value.GetHashCode() + Level.GetHashCode();
            }
        }

        #endregion
    }
}