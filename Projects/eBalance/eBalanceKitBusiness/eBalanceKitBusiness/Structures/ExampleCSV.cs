// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using eBalanceKitBusiness.Import;

namespace eBalanceKitBusiness.Structures {
    public class ExampleCSV : Utils.NotifyPropertyChangedBase {
        public ExampleCSV() {
        }

        #region CsvExample
        private DataTable _csvExample;

        public DataTable CsvExample {
            get {
                return _csvExample ?? (_csvExample = CreateTableFromOutputStream(CsvSource, "csvExample"));
            }
            set {
                if (_csvExample != value) {
                    _csvExample = value;
                    OnPropertyChanged("CsvExample");
                }
            }
        }
        #endregion CsvExample

        private DataTable CreateTableFromOutputStream(string outputStreamText, string tableName) {

            //Process output and return
            string[] split = outputStreamText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length >= 2) {
                int iteration = 0;
                DataTable table = null;

                foreach (string values in split) {
                    if (iteration == 0) {
                        string[] columnNames = SplitString(values);

                        table = new DataTable(tableName);

                        List<DataColumn> columnList = new List<DataColumn>();

                        foreach (string columnName in columnNames) {
                            columnList.Add(new DataColumn(columnName));
                        }

                        table.Columns.AddRange(columnList.ToArray());

                    }
                    else {
                        string[] fields = SplitString(values);

                        if (table != null) {
                            table.Rows.Add(fields);
                        }
                    }

                    iteration++;
                }
                return table;
            }

            return null;

        }

        public BalanceListImportConfig ImportConfig {
            get {
                return new BalanceListImportConfig() {
                    BalanceListName = eBalanceKitResources.Localisation.ResourcesBalanceList.DefaultBalanceListName,
                    Comment = false,
                    Encoding = System.Text.Encoding.UTF8,
                    Seperator = ";",
                    FirstLineIsHeadline = true,
                    TaxonomyColumnExists = false,
                    TextDelimiter = "'",
                    Index = false,
                    ImportType = BalanceListImportType.SignedBalance,
                    ColumnDict =
                        new Dictionary<string, string>() {
                            {"AccountNumberCol", CsvExample.Columns[0].ColumnName},
                            {"AccountNameCol", CsvExample.Columns[1].ColumnName},
                            {"BalanceCol", CsvExample.Columns[2].ColumnName}
                        }
                };
            }
        }

        private string[] SplitString(string inputString) {
            return inputString.Split(';');
        }


        public string CsvFilePath { get; private set; }

        public string CsvSource {
            get {
                var path = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\TestData\\";
                
                try {
                    if (!System.IO.Directory.Exists(path)) {
                        System.IO.Directory.CreateDirectory(path);
                    }
                } catch (Exception ex) {
                    eBalanceKitBase.Structures.ExceptionLogging.LogException(ex);
                    path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) +
                           "\\AvenDATA\\eBalanceKit\\";

                    if (!System.IO.Directory.Exists(path)) {
                        System.IO.Directory.CreateDirectory(path);
                    }
                }

                CsvFilePath = path + "balance_list.csv";


                if (!System.IO.File.Exists(CsvFilePath)) {
                    System.IO.File.WriteAllText(CsvFilePath, CsvContent,Encoding.UTF8);
                }


                if (System.IO.File.Exists(CsvFilePath)) {
                    return System.IO.File.ReadAllText(CsvFilePath, Encoding.UTF8);
                }


                return CsvContent;
            }
        }
        private string CsvContent {
            get {
                return @"Kontonummer;Kontobezeichnung;Saldo
424;KRAFTFAHRZEUGE;51153
351315;BETRIEBSVORRICHTUNG;21
1531;WOHNGRUNDSTÜCKE;6464
313;FABRIKGRUNDSTÜCKE;63531
5;GESCHÄFTSGRUNDSTÜCKE;0
155;UNBEBAUTE GRUNDSTÜCKE;513
105;BETRIEBSAUSSTATTUNG;8646
15341351;GESCHÄFTSAUSSTATTUNG;-64
31;KASSE;-61351
54;FORDERUNGEN;5
688;KONTO;65
0031;STATISTISCHES KONTO FÜR XY;-129859
13;RÜCKSTELLUNGEN GEWERBESTEUER;-634
867;PACKMITTEL;-9910
15;MINERALWASSER;-4546
44;PATENTE;6486
1687;HILFS- UND BETRIEBSSTOFFE;46846
16;STROM;-354546
31648;UNBEKANNT;-564646
16387;STEUERHINTERZIEHUNG;5665433
23456;BEISPIELKONTO;-4723607";
            }
        }
    }
}