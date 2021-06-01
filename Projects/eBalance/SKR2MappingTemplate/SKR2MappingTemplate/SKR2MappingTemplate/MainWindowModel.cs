//#define LOG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using System.Windows;

namespace SKR2MappingTemplate
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {

        }

        public void InitModel()
        {
            TFileName = "";
            MFileName = "";
            OOutputDirectory = "";
            TTaxonomyName = "A";
            TTaxonomyID = "C";
            TTaxonomyPrefix = "D";
            MTaxonomyName = "C";
            MAccountName = "H";
            MAccountNumber = "F";
            MAccountNumber2 = "G";
        }

        #region Properties

        private Dictionary<string, Dictionary<String, String>> TaxonomyData;
        private Dictionary<string, List<MappingData>> MappingData;

        private int _maxMastervalue;
        public int MaxMasterValue { get { return _maxMastervalue; } set { _maxMastervalue = value; InvokePropertyChanged("MaxMasterValue"); } }

        private int _maxSheetvalue;
        public int MaxSheetValue { get { return _maxSheetvalue; } set { _maxSheetvalue = value; InvokePropertyChanged("MaxSheetValue"); } }

        private int _maxDetailvalue;
        public int MaxDetailValue { get { return _maxDetailvalue; } set { _maxDetailvalue = value; InvokePropertyChanged("MaxDetailValue"); } }


        private int _mastervalue;
        public int MasterValue { get { return _mastervalue; } set { _mastervalue = value; InvokePropertyChanged("MasterValue"); } }

        private int _sheetvalue;
        public int SheetValue { get { return _sheetvalue; } set { _sheetvalue = value; InvokePropertyChanged("SheetValue"); } }

        private int _detailvalue;
        public int DetailValue { get { return _detailvalue; } set { _detailvalue = value; InvokePropertyChanged("DetailValue"); } }

        private string _tFileName;
        public string TFileName { get { return _tFileName; } set { _tFileName = value; InvokePropertyChanged("TFileName"); } }

        private string _tTaxonomyName;
        public string TTaxonomyName { get { return _tTaxonomyName; } set { _tTaxonomyName = value; InvokePropertyChanged("TTaxonomyName"); } }

        private string _tTaxonomyID;
        public string TTaxonomyID { get { return _tTaxonomyID; } set { _tTaxonomyID = value; InvokePropertyChanged("TTaxonomyID"); } }

        private string _tTaxonomyPrefix;
        public string TTaxonomyPrefix { get { return _tTaxonomyPrefix; } set { _tTaxonomyPrefix = value; InvokePropertyChanged("TTaxonomyPrefix"); } }

        private string _mFileName;
        public string MFileName { get { return _mFileName; } set { _mFileName = value; InvokePropertyChanged("MFileName"); } }

        private string _mTaxonomyName;
        public string MTaxonomyName { get { return _mTaxonomyName; } set { _mTaxonomyName = value; InvokePropertyChanged("MTaxonomyName"); } }

        private string _mAccountName;
        public string MAccountName { get { return _mAccountName; } set { _mAccountName = value; InvokePropertyChanged("MAccountName"); } }

        private string _mAccountNumber;
        public string MAccountNumber { get { return _mAccountNumber; } set { _mAccountNumber = value; InvokePropertyChanged("MAccountNumber"); } }
        
        private string _mAccountNumber2;
        public string MAccountNumber2 { get { return _mAccountNumber2; } set { _mAccountNumber2 = value; InvokePropertyChanged("MAccountNumber2"); } }

        private string _oOutputDirectory;
        public string OOutputDirectory { get { return _oOutputDirectory; } set { _oOutputDirectory = value; InvokePropertyChanged("OOutputDirectory"); } }

        #endregion Properties

        public void DoConvert()
        {
            MaxMasterValue = 3;
            MasterValue = 0;
            ReadTaxonomyData();
            MasterValue++;
            ReadMappingData();
            MasterValue++;
            CreateXML();
            MasterValue++;
            MessageBox.Show("Converting completed.");
        }

        private void ReadTaxonomyData()
        {
            if (!File.Exists(TFileName))
                return;

            TaxonomyData = new Dictionary<string, Dictionary<string, string>>();
            var xlApp = new Microsoft.Office.Interop.Excel.Application() { ScreenUpdating = false, AlertBeforeOverwriting = false };
            var workBook = xlApp.Workbooks.Open(TFileName);
            try
            {
                MaxSheetValue = workBook.Worksheets.Count;
                SheetValue = 0;
                foreach (Worksheet workSheet in workBook.Worksheets)
                {
                    var dict = new Dictionary<string, string>();
                    MaxDetailValue = workSheet.UsedRange.Rows.Count;
                    DetailValue = 0;
                    for (var rowIndex = 2; rowIndex < workSheet.UsedRange.Rows.Count; rowIndex++)
                    {
                        string taxonomyName =
                            workSheet.UsedRange.Range[TTaxonomyName + rowIndex, TTaxonomyName + rowIndex].Text;
                        if (!dict.ContainsKey(taxonomyName))
                            dict.Add(taxonomyName,
                                workSheet.UsedRange.Range[TTaxonomyPrefix + rowIndex, TTaxonomyPrefix + rowIndex].Text + "_" + workSheet.UsedRange.Range[TTaxonomyID + rowIndex, TTaxonomyID + rowIndex].Text);
                        else
                        {
                            // log 
#if LOG

                            // log 
                            File.AppendAllText("test2.log", workSheet.Name);
                            File.AppendAllText("test2.log", Environment.NewLine);
                            File.AppendAllText("test2.log", taxonomyName);
                            File.AppendAllText("test2.log", Environment.NewLine);
                            File.AppendAllText("test2.log", dict[taxonomyName]);
                            File.AppendAllText("test2.log", Environment.NewLine);
                            File.AppendAllText("test2.log", Environment.NewLine);
#endif
                        }
                        DetailValue++;
                    }
                    TaxonomyData.Add(workSheet.Name, dict);
                    SheetValue++;
                }
            }
            finally
            {
                workBook.Close();
            }
        }

        private void ReadMappingData()
        {
            if (!File.Exists(MFileName))
                return;
            MappingData = new Dictionary<string, List<MappingData>>();
            var xlApp = new Microsoft.Office.Interop.Excel.Application() { ScreenUpdating = false, AlertBeforeOverwriting = false };
            var workBook = xlApp.Workbooks.Open(MFileName);
            try
            {
                MaxSheetValue = workBook.Worksheets.Count;
                SheetValue = 0;
                foreach (Worksheet workSheet in workBook.Worksheets)
                {
                    var list = new List<MappingData>();
                    var lastAccountName = "";
                    var lastTaxonomyName = "";
                    MaxDetailValue = workSheet.UsedRange.Rows.Count;
                    DetailValue = 0;
                    for (var rowIndex = 2; rowIndex < workSheet.UsedRange.Rows.Count; rowIndex++)
                    {
                        string accountNumberFrom = workSheet.UsedRange.Range[MAccountNumber + rowIndex, MAccountNumber + rowIndex].Text;
                        string accountNumberTo = workSheet.UsedRange.Range[MAccountNumber2 + rowIndex, MAccountNumber2 + rowIndex].Text;
                        int accountNumberFromInt = 0;
                        int accountNumberToInt = 0;
                        if (Int32.TryParse(accountNumberFrom, out accountNumberFromInt) && Int32.TryParse(accountNumberTo, out accountNumberToInt))
                        {
                            string accountName =
                                workSheet.UsedRange.Range[MAccountName + rowIndex, MAccountName + rowIndex].Text;
                            if (String.IsNullOrEmpty(accountName))
                                accountName = lastAccountName;

                            string taxonomyName =
                                workSheet.UsedRange.Range[MTaxonomyName + rowIndex, MTaxonomyName + rowIndex].Text;
                            if (String.IsNullOrEmpty(taxonomyName))
                                taxonomyName = lastTaxonomyName;

                            for (int i = accountNumberFromInt; i <= accountNumberToInt; i++)
                                list.Add(new MappingData(i.ToString(), accountName, taxonomyName));

                            lastTaxonomyName = taxonomyName;
                            lastAccountName = accountName;
                            DetailValue++;
                        }
                    }
                    MappingData.Add(workSheet.Name, list);
                    SheetValue++;
                }
            }
            finally
            {
                workBook.Close();
            }
        }

        private void CreateXML()
        {
            var directory = new DirectoryInfo(OOutputDirectory);
            if (!directory.Exists) directory.Create();

            MaxSheetValue = TaxonomyData.Count;
            SheetValue = 0;
            foreach (var taxonomyData in TaxonomyData)
            {
                foreach (var mappingData in MappingData)
                {
                    var fileName = directory.FullName + taxonomyData.Key + "_" + mappingData.Key + ".xml";
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);
                    var w = new XmlTextWriter(stream, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 4, IndentChar = ' ' };
                    w.WriteStartDocument();
                    w.WriteStartElement("e_balance_kit_mapping_template");

                    w.WriteStartElement("name");
                    w.WriteString(mappingData.Key);
                    w.WriteEndElement();

                    w.WriteStartElement("comment");
                    w.WriteString("");
                    w.WriteEndElement();

                    w.WriteStartElement("account_structure");
                    w.WriteString("");
                    w.WriteEndElement();

                    w.WriteStartElement("taxonomy");
                    w.WriteString(taxonomyData.Key + "-2011-09-14");
                    w.WriteEndElement();

                    w.WriteStartElement("assignments");
                    MaxDetailValue = MappingData.Values.Count;
                    DetailValue = 0;
                    foreach (var data in mappingData.Value)
                    {
                        if (taxonomyData.Value.ContainsKey(data.TaxonomyName))
                        {
                            w.WriteStartElement("assignment");
                            w.WriteAttributeString("number", data.AccountNumber.Trim());
                            w.WriteAttributeString("name", data.AccountName.Trim());
                            w.WriteAttributeString("credit_element_id",
                                                   taxonomyData.Value[data.TaxonomyName].Trim());
                            w.WriteEndElement();
                        }
                        else
                        {
#if LOG
                            // log 
                            File.AppendAllText("test.log", data.AccountNumber);
                            File.AppendAllText("test.log", Environment.NewLine);
                            File.AppendAllText("test.log", data.AccountName);
                            File.AppendAllText("test.log", Environment.NewLine);
                            File.AppendAllText("test.log", data.TaxonomyName);
                            File.AppendAllText("test.log", Environment.NewLine);
                            File.AppendAllText("test.log", taxonomyData.Key);
                            File.AppendAllText("test.log", Environment.NewLine);
                            //foreach (var val in taxonomyData.Value) {
                            //    File.AppendAllText("test.log", val.Key);
                            //    File.AppendAllText("test.log", val.Value);
                            //}
                            File.AppendAllText("test.log", Environment.NewLine);
#endif
                        }
                        DetailValue++;
                    }
                    w.WriteEndElement();

                    w.WriteEndElement();
                    w.WriteEndDocument();


                    w.Close();
                    SheetValue++;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(string property)
        {
            try
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
            catch (Exception)
            {
            }

        }

    }
}
