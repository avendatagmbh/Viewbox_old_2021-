using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;
using System.IO;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBase.Structures;

namespace eBalanceKitBusiness.Export.Models
{
    /// <summary>
    /// Company export model for exporting the company to csv file
    /// </summary>
    public class ExportCompanyModel : NotifyPropertyChangedBase
    {
        #region Properties

        public string FileName { get; set; }
        private Company _selectedCompany;
        public Company SelectedCompany { get { return _selectedCompany; } set { _selectedCompany = value; OnPropertyChanged("SelectedCompany"); } }
        public bool OnlyWithValue { get; set; }
        public List<Company> Companies { get; set; }
        private int _maxListCount;
        
        #endregion Properties

        #region Methods

        public ExportCompanyModel()
        {
            SelectedCompany = CreateExampleCompany();
            FileName = "CompanyExport.csv";
            OnlyWithValue = true;
            Companies = new List<Company> {SelectedCompany};
            Companies.AddRange(CompanyManager.Instance.AllowedCompanies);
        }

        #region Export
        /// <summary>
        /// Export the company´s valuetree
        /// </summary>
        public void Export()
        {
            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                
                using (var writer = new CsvWriter(FileName, Encoding.UTF8))
                {
                    GetMaxListCount();

                    // Export header
                    var header = new string[_maxListCount + 3];
                    header[0] = "Name";
                    header[1] = "Taxonomy Id";
                    header[2] = "Value";
                    for (int i = 1; i < _maxListCount; i++)
                        header[i + 2] = "Value" + (i + 1);
                    header[_maxListCount + 2] = "";
                    writer.WriteCsvData(header);

                    // Export single values
                    foreach (var data in SelectedCompany.ValueTree.Root.Values
                        .Where(w => w.Value.DbValue is ValuesGCD_Company && !(w.Value.Element.IsSelectionListEntry) &&!(w.Value is XbrlElementValue_List) && !(w.Value is XbrlElementValue_Tuple) && (!OnlyWithValue || w.Value.Value != null) && w.Value.Element.Name.StartsWith("genInfo.company")))
                    {
                        writer.WriteCsvData(GetDataFromElement(data.Value));
                    }

                    // Export list values
                    var datas = new Dictionary<string, List<string>>();
                    foreach (var masterData in SelectedCompany.ValueTree.Root.Values.Where(w => w.Value is XbrlElementValue_List && w.Value.Element.Name.StartsWith("genInfo.company")))
                    {
                        var nodeValue = masterData.Value as XbrlElementValue_List;                        
                        if (nodeValue == null) continue;
                        foreach (var nodeItem in nodeValue.Items)
                        {
                            foreach (var data in nodeItem.Values
                                .Where(w => w.Value.DbValue is ValuesGCD_Company))
                            {
                                if (!datas.ContainsKey(data.Key))
                                {
                                    datas.Add(data.Key, new List<string>());
                                    datas[data.Key].Add(data.Value.Element.Label);
                                    datas[data.Key].Add(data.Value.Element.Name);
                                }   
                                datas[data.Key].Add(GetValue(data.Value));
                            }
                        }   
                    }
                    // export tuple values
                    foreach (var masterData in SelectedCompany.ValueTree.Root.Values.Where(w => w.Value is XbrlElementValue_Tuple && !(w.Value is XbrlElementValue_List) && w.Value.Element.Name.StartsWith("genInfo.company")))
                    {
                        var nodeValue = masterData.Value as XbrlElementValue_Tuple;
                        if (nodeValue == null) continue;
                        foreach (var nodeItem in nodeValue.Items)
                        {
                            foreach (var data in nodeItem.Values
                                .Where(w => w.Value.DbValue is ValuesGCD_Company))
                            {
                                if (!datas.ContainsKey(data.Key))
                                {
                                    datas.Add(data.Key, new List<string>());
                                    datas[data.Key].Add(data.Value.Element.Label);
                                    datas[data.Key].Add(data.Value.Element.Name);
                                }
                                datas[data.Key].Add(GetValue(data.Value));
                            }
                        }
                    }

                    foreach (var value in datas.Values)
                    {
                        if (OnlyWithValue && value.Count(dataValue => !String.IsNullOrEmpty(dataValue)) < 3)
                            continue;
                        for (int i = value.Count; i < _maxListCount + 3; i++)
                            value.Add("");
                        writer.WriteCsvData(value.ToArray());
                    }

                    // TODO: add/extend financial years section.
                    var taxonomyData = new string[_maxListCount + 3];
                    taxonomyData[0] = "Taxonomie Version";
                    taxonomyData[1] = SelectedCompany.TaxonomyInfo.Version;
                    for (int i = 0; i < taxonomyData.Length; i++)
                        taxonomyData[i] = taxonomyData[i] ?? "";
                    writer.WriteCsvData(taxonomyData);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(ResourcesExport.ExportCSVError + exception.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show(ResourcesExport.ExportSuccess, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);

        }
        #endregion Export

        #region GetDataFromElement
        private string[] GetDataFromElement(IValueTreeEntry element)
        {
            var result = new string[_maxListCount + 3];
            result[0] = element.Element.Label;
            result[1] = element.Element.Name;
            result[2] = GetValue(element);
            for (int i = 0; i < result.Length; i++)
                result[i] = result[i] ?? "";
            return result;
        }
        #endregion GetDataFromElement

        #region GetValue
        private string GetValue(IValueTreeEntry element)
        {
            string result = "";
            var xbrlElementValueDate = element as XbrlElementValue_Date;
            if (xbrlElementValueDate != null)
            {
                result = xbrlElementValueDate.DateValue.HasValue ? xbrlElementValueDate.DateValue.Value.ToString(LocalisationUtils.GermanCulture) : "";
            }
            else
                result = element.Value == null ? "" : element.Value.ToString();
            return result;
        }
        #endregion GetValue

        private void GetMaxListCount()
        {
            _maxListCount = 0;

            foreach (var masterData in SelectedCompany.ValueTree.Root.Values.Where(w => w.Value is XbrlElementValue_List))
            {
                var nodeValue = masterData.Value as XbrlElementValue_List;
                if (nodeValue == null) continue;
                _maxListCount = _maxListCount < nodeValue.Items.Count ? nodeValue.Items.Count : _maxListCount;
            }

            foreach (var masterData in SelectedCompany.ValueTree.Root.Values.Where(w => w.Value is XbrlElementValue_Tuple && !(w.Value is XbrlElementValue_List)))
            {
                var nodeValue = masterData.Value as XbrlElementValue_Tuple;
                if (nodeValue == null) continue;
                _maxListCount = _maxListCount < nodeValue.Items.Count ? nodeValue.Items.Count : _maxListCount;
            }

            _maxListCount = Math.Max(_maxListCount, 1);
        }

        #region CreateExampleCompany
        /// <summary>
        /// Creates example company. It uses the ResourcesExampleCompanyData resx file.
        /// </summary>
        /// <returns></returns>
        private Company CreateExampleCompany()
        {
            const string prefix = "de-gcd_";
            Company company = new Company { TaxonomyInfo = TaxonomyManager.GetTaxonomyInfo(TaxonomyType.GCD, "2012-06-01") };
            company.SetFinancialYearIntervall(2009, 2030);
            company.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
            company.Name = ">>> " + ResourcesExampleCompanyData.genInfo_company_id_name + " <<<";
            var resourceSet = ResourcesExampleCompanyData.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                var resourceKey = entry.Key as string;
                var resource = entry.Value as string;
                if (resourceKey == null || resource == null) continue;

                var isValueOther = resourceKey.EndsWith("__2__");
                if (isValueOther)
                    resourceKey = resourceKey.Replace("__2__", "");
                resourceKey = resourceKey.Replace("_", ".");
                resourceKey = prefix + resourceKey;
                if (!company.ValueTree.Root.Values.ContainsKey(resourceKey)) {Console.WriteLine(resourceKey);continue;}
                
                var value = company.ValueTree.Root.Values[resourceKey].DbValue as ValuesGCD_Company;
                if (value == null) continue;

                if (isValueOther)
                    value.ValueOther = resource;
                else
                    value.Value = resource;
            }

            var shareholderRoot = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.shareholder"] as XbrlElementValue_List;
            if (shareholderRoot != null)
            {
                var shareholder = shareholderRoot.AddValue();
                shareholderRoot.SelectedItem = shareholder;
                shareholder.Values[prefix + "genInfo.company.id.shareholder.name"].DbValue.Value =
                    ResourcesExampleCompanyData.genInfo_company_id_shareholder_name;
                shareholder.Values[prefix + "genInfo.company.id.shareholder.name"].DbValue.ValueOther =
                    ResourcesExampleCompanyData.genInfo_company_id_shareholder_name__2__;
            }

            var contactRoot = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.contactAddress"] as XbrlElementValue_List;
            if (contactRoot != null)
            {
                var contact = contactRoot.AddValue();

                contactRoot.SelectedItem = contact;
                contact.Values[prefix + "genInfo.company.id.contactAddress.person"].DbValue.Value =
                    ResourcesExampleCompanyData.genInfo_company_id_contactAddress_person;
                contact.Values[prefix + "genInfo.company.id.contactAddress.person"].DbValue.ValueOther =
                    ResourcesExampleCompanyData.genInfo_company_id_contactAddress_person__2__;
                contact.Values[prefix + "genInfo.company.id.contactAddress.person.name"].DbValue.Value =
                    ResourcesExampleCompanyData.genInfo_company_id_contactAddress_person_name;
                contact.Values[prefix + "genInfo.company.id.contactAddress.person.name"].DbValue.ValueOther =
                    ResourcesExampleCompanyData.genInfo_company_id_contactAddress_person_name__2__;
            }
            return company;
        }
        #endregion CreateExampleCompany
        
        #endregion Methods
    }
}
