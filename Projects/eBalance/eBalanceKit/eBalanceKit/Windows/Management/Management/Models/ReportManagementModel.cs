// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Management.Models {
    public class ReportManagementModel {
        public ReportManagementModel(Window owner) {
            Owner = owner;

            if (!DocumentManager.Instance.AllowedDocuments.Any()) return;
            BuildTreeView();
        }

        private Window Owner { get; set; }

        private readonly ObservableCollectionAsync<ReportTreeNode> _reportTreeRoots =
            new ObservableCollectionAsync<ReportTreeNode>();

        public IEnumerable<ReportTreeNode> ReportTreeRoots { get { return _reportTreeRoots; } }

        public void DeleteReport(ReportTreeLeaf leaf) {
            if (
                MessageBox.Show(Owner, string.Format(ResourcesManamgement.RequestDeleteReport, leaf.Document.Name),
                                string.Empty,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question,
                                MessageBoxResult.No) == MessageBoxResult.Yes) {
                
                DocumentManager.Instance.DeleteDocument(leaf.Document);

                var financialYearNode = leaf.Parent;
                var companyNode = financialYearNode.Parent;
                var systemNode = companyNode.Parent;

                financialYearNode.Children.Remove(leaf);
                if (financialYearNode.Children.Count == 0) {
                    companyNode.Children.Remove(financialYearNode);
                    if (companyNode.Children.Count == 0) {
                        systemNode.Children.Remove(companyNode);
                        if (systemNode.Children.Count == 0) {
                            _reportTreeRoots.Remove(systemNode);
                        }
                    }
                }
            }

        }

        public void BuildTreeView() {
            _reportTreeRoots.Clear();
            
            ReportTreeNode currentSystemNode = null;
            ReportTreeNode currentCompanyNode = null;
            ReportTreeNode currentFinancialYearNode = null;

            eBalanceKitBusiness.Structures.DbMapping.System currentSystem = null;
            Company currentCompany = null;
            FinancialYear currentFYear = null;

            var reports = (from report in DocumentManager.Instance.AllowedDocuments
                           orderby report.System.Name, report.Company.Name, report.FinancialYear.FYear, report.Name
                           select report);

            foreach (var report in reports) {
                if (currentSystem != report.System) {
                    currentCompany = null;
                    currentFYear = null;
                    currentSystem = report.System;
                    currentSystemNode = new ReportTreeNode(currentSystem, ResourcesMain.System + ": " + currentSystem.Name);
                    _reportTreeRoots.Add(currentSystemNode);
                }

                if (currentCompany != report.Company) {
                    currentFYear = null;
                    currentCompany = report.Company;
                    currentCompanyNode = new ReportTreeNode(currentCompany, ResourcesMain.Company + ": " + currentCompany.Name);
                    Debug.Assert(currentSystemNode != null, "currentSystemNode != null");
                    currentSystemNode.Children.Add(currentCompanyNode);
                    currentCompanyNode.Parent = currentSystemNode;
                }

                if (currentFYear != report.FinancialYear) {
                    currentFYear = report.FinancialYear;
                    currentFinancialYearNode =
                        new ReportTreeNode(currentFYear, ResourcesMain.FinancialYear + ": " + currentFYear.FYear.ToString(CultureInfo.InvariantCulture));
                    Debug.Assert(currentCompanyNode != null, "currentCompanyNode != null");
                    currentCompanyNode.Children.Add(currentFinancialYearNode);
                    currentFinancialYearNode.Parent = currentCompanyNode;
                }

                Debug.Assert(currentFinancialYearNode != null, "currentFinancialYearNode != null");
                var leaf = new ReportTreeLeaf(report);
                currentFinancialYearNode.Children.Add(leaf);
                leaf.Parent = currentFinancialYearNode;
            }
        }
    }
}