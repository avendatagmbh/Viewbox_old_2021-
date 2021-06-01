using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Export
{
    public class BalanceListExport
    {
        private StringBuilder _csvContent;

        private readonly string[] _disallowedChars = {
            "\\", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<",
            ">", "|", ","
        };
        private string _filename;
        private IExportConfig Config;
        #region Export
        public void Export(IExportConfig config)
        {
            Config = config;

            foreach (var disallowedChar in _disallowedChars)
            {
                if (!disallowedChar.Equals("\\"))
                    Config.FilePath = Config.FilePath.Replace(disallowedChar, "");
            }

            var directory = new DirectoryInfo(Config.FilePath);
            if (!directory.Exists) directory.Create();

            try
            {
                ExportBalanceLists();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion Export

        #region ExportBalanceLists

        private void ExportBalanceLists()
        {
            if (Config.BalanceListsByFile)
            {
                foreach (var balanceList in Config.Document.BalanceListsImported)
                    ExportBalanceList(balanceList);
            }
            else
                ExportBalanceList();
        }
        #endregion

        #region ExportBalanceList
        private void ExportBalanceList(IBalanceList toExport = null)
        {
            try
            {
                _csvContent = new StringBuilder();

                _filename = Config.Document.Company.Name + "_"
                            + ResourcesExport.ExportCSVFinancialYear + "_"
                            + Config.Document.FinancialYear.FYear + "_"
                            + ResourcesExport.ExportBalanceListFileName
                            + (toExport == null ? "" :  "_" + toExport.Name) ; 

                foreach (var disallowedChar in _disallowedChars)
                {
                    _filename = _filename.Replace(disallowedChar, "_");
                }

                _filename = Config.FilePath + "\\" + _filename + ".csv";

                // Create the header part
                bool needBalanceListColumn = toExport == null && Config.Document.BalanceListsImported.Count > 1;
                _csvContent.Append(ResourcesExport.ExportCSVHeaderNumber);
                _csvContent.Append(";");
                _csvContent.Append(ResourcesExport.ExportCSVHeaderName);
                _csvContent.Append(";");
                if (Config.ShowAccountsInGroup)
                {
                    _csvContent.Append(ResourcesExport.ExportCSVHeaderGroup);
                    _csvContent.Append(";");
                }
                _csvContent.Append(ResourcesExport.ExportCSVHeaderAmount);
                _csvContent.Append(";");
                _csvContent.Append(ResourcesExport.ExportCSVHeaderTaxonomyID);
                _csvContent.Append(";");
                if (Config.ShowOriginalAccountForSplitted)
                {
                    _csvContent.Append(ResourcesExport.ExportCSVHeaderSplitted);
                    _csvContent.Append(";");
                }
                if (needBalanceListColumn)
                {
                    _csvContent.Append(ResourcesExport.ExportCSVHeaderBalanceList);
                    _csvContent.Append(";");
                }

                _csvContent.AppendLine();

                foreach (var balanceList in Config.Document.BalanceListsImported.Where(w => toExport == null || w.Equals(toExport)))
                {
                    var sortedList = balanceList.Items.ToList();
                    if (sortedList.Count > 0)
                        foreach (var entry in sortedList.OrderBy(el => el.Number))
                        {
                            if (entry != null)
                            {
                                _csvContent.Append(entry.Number);
                                _csvContent.Append(";");
                                _csvContent.Append(entry.Name);
                                _csvContent.Append(";");
                                if (Config.ShowAccountsInGroup)
                                {
                                    _csvContent.Append(";");
                                }
                                _csvContent.Append(entry.Amount);
                                _csvContent.Append(";");

                                // Taxonomy ID
                                var parent = entry.Parents.FirstOrDefault();
                                if (parent != null && parent.Element != null)
                                    _csvContent.Append(parent.Element.Id);
                                _csvContent.Append(";");

                                // If the entry is SplittedAccount, we could export the original accounts number and name
                                if (Config.ShowOriginalAccountForSplitted)
                                {
                                    var splittedAccount = entry as SplittedAccount;
                                    if (splittedAccount != null)
                                    {
                                        if ((splittedAccount).SplitAccountGroup != null && ((SplitAccountGroup) (splittedAccount).SplitAccountGroup).Account != null)
                                        {
                                            var splitGroup = ((SplitAccountGroup) (splittedAccount).SplitAccountGroup).Account;
                                            _csvContent.Append(splitGroup.Number);
                                            _csvContent.Append(" - ");
                                            _csvContent.Append(splitGroup.Name);
                                        }
                                    }
                                    _csvContent.Append(";");
                                }
                                if (needBalanceListColumn)
                                {
                                    _csvContent.Append(balanceList.Name);
                                    _csvContent.Append(";");
                                }
                                _csvContent.AppendLine();

                                // If the entry is AccountGroup, we could export the groupped accounts datas
                                if (!Config.ShowAccountsInGroup || !(entry is AccountGroup)) continue;
                                foreach (var item in ((AccountGroup) entry).Items)
                                {
                                    _csvContent.Append(";");
                                    _csvContent.Append(";");
                                    _csvContent.Append(item.Number);
                                    _csvContent.Append(" - ");
                                    _csvContent.Append(item.Name);
                                    _csvContent.Append(";");
                                    _csvContent.Append(item.Amount);
                                    _csvContent.Append(";");
                                    var itemParent = item.Parents.FirstOrDefault();
                                    if (itemParent != null && itemParent.Element != null)
                                        _csvContent.Append(itemParent.Element.Id);
                                    _csvContent.Append(";");
                                    _csvContent.Append(";");
                                    _csvContent.AppendLine();
                                    if (needBalanceListColumn)
                                    {
                                        _csvContent.Append(balanceList.Name);
                                        _csvContent.Append(";");
                                    }
                                }
                            }
                        }
                }

                using (var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create), Encoding.UTF8))
                {
                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    ResourcesExport.ExportCSVError + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion ExportBalanceList

    }
}
