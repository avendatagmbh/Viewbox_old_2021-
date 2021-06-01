// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-05-26
// copyright 2011 AvenDATA
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.MappingTemplate {
    public  static class MappingTemplatePresentationTrees {
        public static IEnumerable<IPresentationTree> Create(MappingTemplateHead template, IEnumerable<MappingHeaderGui> mappingAssignments) {

            Debug.Assert(mappingAssignments != null, "mappingAssignments != null");

            XbrlElementValueBase.InitMode = true;

            var ptrees = PresentationTree.CreatePresentationTrees(template.Taxonomy, null, null);

            var valueTreeNode = new ValueTreeNode(null, null, template.Taxonomy);
            foreach (var ptree in ptrees.Values) {
                var values = new Dictionary<string, IValueTreeEntry>();               
                
                // create new monetary value objects (needed to set IsEnabled and IsComputed states)
                foreach (var ptreeNode in ptree.Nodes.OfType<IPresentationTreeNode>()) {
                    switch (ptreeNode.Element.ValueType) {
                        case XbrlElementValueTypes.Monetary: {
                            var value = new XbrlElementValue_Monetary(ptreeNode.Element, valueTreeNode, null);
                            valueTreeNode.Values[value.Element.Id] = value;
                            ptreeNode.Value = value;

                            var info = template.GetElementInfo(value.Element.Id);
                            if (info != null) {
                                info.PresentationTreeNode = ptreeNode;
                                value.SetFlags(info.AutoComputeEnabled, info.SupressWarningMessages,
                                               info.SendAccountBalances);
                            }

                            values[ptreeNode.Element.Id] = value;
                        }
                            break;
                    }
                }

                // add summation targets
                foreach (var elem in template.Taxonomy.Elements.Values)
                    try {
                        AddSummationTargets(elem, values);
                    } catch (Exception ex) {
                        Debug.WriteLine(ex.Message);
                        throw;
                    }

                // init states
                XbrlElementValueBase.InitStates(values.Values);
            }

            XbrlElementValueBase.InitMode = false;

            // add assignments
            var mappingAssignmentsList = mappingAssignments.ToList();
            foreach (var ptree in ptrees.Values) {
                foreach (var assignmentHead in mappingAssignmentsList) {
                    foreach (var assignmentLine in assignmentHead.TemplateLines) {
                        if (assignmentLine.ElementId == null) continue;
                        var node = ptree.GetNode(assignmentLine.ElementId) as IPresentationTreeNode;
                        if (node == null) continue;
                        node.AddChildren(assignmentLine);
                        assignmentLine.PresentationTreeNode = node;
                    }
                }
            }

            return ptrees.Values;
        }

        private static void AddSummationTargets(IElement root, IDictionary<string, IValueTreeEntry> values) {
            if (!values.ContainsKey(root.Id)) return;
            var srcValue = values[root.Id];
            foreach (var summationTarget in root.SummationTargets) {
                if (!values.ContainsKey(summationTarget.Element.Id)) continue;
                var destValue = values[summationTarget.Element.Id];
                srcValue.AddSummationTarget(destValue, (decimal) summationTarget.Weight);
                destValue.AddSummationSource(srcValue);
            }

            //foreach (Taxonomy.Element child in root.Children) {
            //    AddSummationTargets(child, values);
            //}
        }
    }
}