// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Structures.Presentation {
    internal class PresentationTreeListNode : PresentationTreeNode {

        public PresentationTreeListNode(PresentationTree ptree, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode ptreeNode)
            : base(ptree, ptreeNode) { }

        public void AddItem() { }

        public void DeleteItem() { }
    }
}