// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:30
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using DbAccess;
using Utils;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Logic;
using ViewValidatorLogic.Structures.Reader;
using ViewValidatorLogic.Structures.Results;
using System.Globalization;
using System.Threading;

namespace ViewValidatorLogic.Logic {
    public class ViewValidator {
        public ValidationResults Results { get; private set; }
        public ValidationSetup ValidationSetup { get; private set; }
        //private BackgroundWorker BgWorker { get; set; }
        private ProgressCalculator _progressCalculator;
        private bool _userCanceled = false;

        public ViewValidator(ValidationSetup setup, ProgressCalculator progressCalculator) {
            this.ValidationSetup = setup;
            this.Results = new ValidationResults(setup);
            this._progressCalculator = progressCalculator;
        }

        public string TestValidation() {
            string messages = "";
            //Test if there is a sort criterion for each table mapping
            foreach (var tableMapping in ValidationSetup.TableMappings) {
                if (tableMapping.Used && tableMapping.KeyEntryMappings.Count == 0)
                    messages += "Das Tabellenpaar " + tableMapping.TableValidation.Name + " - " + tableMapping.TableView.Name + " hat kein Sortierkriterium." + Environment.NewLine;
            }
            return messages;
        }

        public void StartValidation(SortMethod sortMethod) {
            _userCanceled = false;
            _currentConnections = null;

            string error = TestValidation();
            if (!string.IsNullOrEmpty(error)) throw new Exception(error);
            
            Stopwatch watch = new Stopwatch();
            watch.Start();

            int CountUsedTables = 0, UsedTables = 0;
            for (int i = 0; i < ValidationSetup.TableMappings.Count; ++i)
                if (ValidationSetup.TableMappings[i].Used) CountUsedTables++;

            //Using parallel for
            //try {
            //    System.Threading.Tasks.Parallel.For(0, ValidationSetup.TableMappings.Count, delegate(int i) {

            //        if (ValidationSetup.TableMappings[i].Used) {
            //            using (IDatabase connView = ConnectionManager.CreateConnection(ValidationSetup.DbConfigView),
            //                connValidation = ConnectionManager.CreateConnection(ValidationSetup.DbConfigValidation)) {
            //                connView.Open();
            //                connValidation.Open();

            //                ValidateTableMapping(connValidation, connView, ValidationSetup.TableMappings[i], Results[ValidationSetup.TableMappings[i]]);
            //                if (BgWorker != null) BgWorker.ReportProgress((int)(100.0f / CountUsedTables * UsedTables++));
            //            }
            //        }
            //    });
            //} catch (AggregateException ex) {
            //    string Message = "";
            //    foreach (var exception in ex.Flatten().InnerExceptions)
            //        Message += Environment.NewLine + exception.Message;
            //    throw new Exception(Message);
            //}

            //Using tasks (is slower)
            //try {
            //    List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            //    for (int i = 0; i < ValidationSetup.TableMappings.Count; ++i) {
            //        if (ValidationSetup.TableMappings[i].Used) {
            //            System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(
            //                (obj) => {
            //                    int j = (int)obj;
            //                    if (j >= ValidationSetup.TableMappings.Count) return;
            //                    using (IDatabase connView = ConnectionManager.CreateConnection(ValidationSetup.DbConfigView),
            //                        connValidation = ConnectionManager.CreateConnection(ValidationSetup.DbConfigValidation)) {
            //                        connView.Open();
            //                        connValidation.Open();

            //                        ValidateTableMapping(connValidation, connView, ValidationSetup.TableMappings[j], Results[ValidationSetup.TableMappings[i]]);
            //                        if (BgWorker != null) BgWorker.ReportProgress((int)(100.0f / CountUsedTables * UsedTables++));
            //                    }
            //                }
            //            , i);
            //            task.Start();
            //            tasks.Add(task);
                        
            //        }
            //    }

            //    foreach (var task in tasks) {
            //        task.Wait();
            //    }
            //} catch (AggregateException ex) {
            //    string Message = "";
            //    foreach (var exception in ex.Flatten().InnerExceptions)
            //        Message += Environment.NewLine + exception.Message;
            //    throw new Exception(Message);
            //}


            //Single thread variant
            _progressCalculator.SetWorkSteps(CountUsedTables, true);
            _progressCalculator.Description = "Baue die Datenbankverbindung zu MySQL auf";
            using (IDatabase connView = ConnectionManager.CreateConnection(ValidationSetup.DbConfigView)) {
                connView.Open();
                for (int i = 0; i < ValidationSetup.TableMappings.Count; ++i) {
                    _progressCalculator.Description = "Baue die Datenbankverbindung zu Access auf";
                    using (IDatabase connValidation = ConnectionManager.CreateConnection(ValidationSetup.TableMappings[i].TableValidation.DbConfig)) {
                        connValidation.Open();

                        if (ValidationSetup.TableMappings[i].Used) {
                            ValidateTableMapping(connValidation, connView, ValidationSetup.TableMappings[i],
                                                 Results[ValidationSetup.TableMappings[i]],
                                                 _progressCalculator[UsedTables], sortMethod);
                            _progressCalculator.StepDone();
                            UsedTables++;
                        }
                    }
                }
            }
            watch.Stop();
            Console.WriteLine("Zeit vergangen: " + watch.ElapsedMilliseconds / 1000.0f + " Sekunden");
            if (_progressCalculator != null) _progressCalculator.ReportProgress(100);
        }


        private string GetCountSQL(IDatabase conn, int which, TableMapping tableMapping) {
            return "SELECT COUNT(*) FROM " + conn.Enquote(tableMapping.Tables[which].Name)
                + " " + tableMapping.Tables[which].Filter.GetFilterSQL();
                
        }

        private IDatabase[] _currentConnections;

        private void ValidateTableMapping(IDatabase connValidation, IDatabase connView, TableMapping tableMapping, 
            TableValidationResult result, ProgressCalculator progressCalculator, SortMethod sortMethod) {

            _currentConnections = new IDatabase[2] { connValidation, connView };

            progressCalculator.Description = "Lese die Tabellenstruktur aus und zähle Zeilen";
            tableMapping.TableValidation.ReadTableStructure(connValidation, tableMapping.ColumnMappings, 0, tableMapping.KeyEntryMappings);
            tableMapping.TableView.ReadTableStructure(connView, tableMapping.ColumnMappings, 1, tableMapping.KeyEntryMappings);
            if (tableMapping.ColumnMappings.Count == 0) return;
            //string sql = GetSQL(conn[0], 0, tableMapping);
            
            //Get table count for progressCalculator
            long rowsValidation = Convert.ToInt64(_currentConnections[0].ExecuteScalar(GetCountSQL(_currentConnections[0], 0, tableMapping)));
            long rowsView = Convert.ToInt64(_currentConnections[1].ExecuteScalar(GetCountSQL(_currentConnections[1], 1, tableMapping)));
            progressCalculator.SetWorkSteps(rowsValidation + rowsView, false);

            
            using (IDbReader readerValidation = ReaderFactory.AcquireReader(sortMethod, _currentConnections[0], 0, tableMapping)) {
                
                using (IDbReader readerView = ReaderFactory.AcquireReader(sortMethod, _currentConnections[1], 1, tableMapping)) {
                    ValidateRows(readerValidation, readerView, tableMapping, result, progressCalculator);
                }

            }
        }

        private void ValidateRows(IDbReader readerValidation, IDbReader readerView, TableMapping tableMapping,
            TableValidationResult result, ProgressCalculator progressCalculator)
        {
            //CultureInfo m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            //Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("hu-HU");

            IDbReader[] reader = new IDbReader[2] { readerValidation, readerView };
            List<ColumnInfoHelper>[] columnInfos = new List<ColumnInfoHelper>[2];

            Row[] row = new Row[2];
            for (int i = 0; i < 2; ++i)
            {
                columnInfos[i] = tableMapping.GetColumnInfosFromMapping(i);

                row[i] = new Row(tableMapping.Tables[i], columnInfos[i], tableMapping.Tables[i].KeyEntries, tableMapping.ColumnMappings, i);
            }

            Stopwatch watch = new Stopwatch();
            progressCalculator.Description = "Verprobungsdatenbank wird sortiert";
            watch.Start();
            readerValidation.DoPrecomputations(row[0]);
            Console.WriteLine("Time needed to acquire Validation reader: " + watch.ElapsedMilliseconds / 1000.0f + " seconds");
            watch.Restart();
            progressCalculator.Description = "Viewdatenbank wird sortiert";
            readerView.DoPrecomputations(row[1]);
            Console.WriteLine("Time needed to acquire View reader: " + watch.ElapsedMilliseconds / 1000.0f + " seconds");

            bool[] readerAtEnd = new bool[2] { false, false };
            bool[] read = new bool[2] { true, true };

            progressCalculator.Description = "Vergleiche Zeilen";

            
            ReadRows(reader, row, readerAtEnd, read, progressCalculator);

            //Both tables are empty, nothing to do
            if (readerAtEnd[0] && readerAtEnd[1]) return;

            int currentRowCount = 0;
            //Only if both tables contain data
            if (!readerAtEnd[0] && !readerAtEnd[1])
            {
                //Main loop comparing rows
                do
                {
                    if (_userCanceled) return;

                    CompareRows(result, row, read);

                    currentRowCount++;

                    ReadRows(reader, row, readerAtEnd, read, progressCalculator);

                }
                while (!readerAtEnd[0] && !readerAtEnd[1]);
            }

            PostProcessMissingRows(
                result.ResultPerTable[0].MissingRows,
                result.ResultPerTable[1].MissingRows,
                result);

            PostProcessMissingRows(
                result.ResultPerTable[1].MissingRows,
                result.ResultPerTable[0].MissingRows,
                result);

            //Read the rest of the datasets and add as missing rows
            for (int i = 0; i < 2; ++i)
            {
                if (_userCanceled) return;
                if (!readerAtEnd[i])
                {
                    do
                    {
                        row[i].ReadRow(reader[i]);
                        result.AddRowMissing(1 - i, row[i]);
                        progressCalculator.StepDone();
                    } while (reader[i].Read());
                }
            }

            if (readerValidation is RowWiseReader)
            {
                (readerValidation as RowWiseReader).DeleteTempTables();
                (readerView as RowWiseReader).DeleteTempTables();
            }

            //Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
        }

        private static void PostProcessMissingRows(
            List<Row> p_MissingRows1,
            List<Row> p_MissingRows2,
            TableValidationResult p_Result)
        {
            int i = 0;

            while (i < p_MissingRows1.Count)
            {
                int j = 0;

                while (j < p_MissingRows2.Count && p_MissingRows2[j].CompareKeyEntries(p_MissingRows1[i]) != 0)
                    j++;

                if (j < p_MissingRows2.Count)
                {
                    p_Result.AddRowsEqual();

                    p_MissingRows1.RemoveAt(i);
                    p_MissingRows2.RemoveAt(j);                    

                    p_Result.ResultPerTable[0].MissingRowsCount--;
                    p_Result.ResultPerTable[1].MissingRowsCount--;
                }
                else
                    i++;
            }
        }

        private static int CompareRows(TableValidationResult result, Row[] row, bool[] read) {
            int lastComparisonResult = row[0].CompareKeyEntries(row[1]);
            switch (lastComparisonResult) {
                case -1:
                    result.AddRowMissing(1, row[0]);
                    read[0] = true;
                    read[1] = false;
                    break;
                case 0:
                    if (!row[0].IsEqualTo(row[1]))
                        result.AddRowDifference(row[0], row[1]);
                    else result.AddRowsEqual();
                    read[0] = true;
                    read[1] = true;
                    break;
                case 1:
                    result.AddRowMissing(0, row[1]);
                    read[0] = false;
                    read[1] = true;
                    break;
            }
            return lastComparisonResult;
        }

        private static void ReadRows(IDbReader[] reader, Row[] row, bool[] readerAtEnd, bool[] read, ProgressCalculator progressCalculator) {
            for (int i = 0; i < 2; ++i) {
                if (read[i] && !readerAtEnd[i]) {
                    if (reader[i].Read()) {
                        row[i].ReadRow(reader[i]);
                        progressCalculator.StepDone();
                    } else readerAtEnd[i] = true;
                }
            }
        }

        public void AbortOperation() {
            _userCanceled = true;
            try {
                if (_currentConnections != null) {
                    for (int i = 0; i < 2; ++i)
                        if (_currentConnections[i] != null && _currentConnections[i].IsOpen)
                            _currentConnections[i].CancelCommand();
                }
            } catch (Exception) { }
        }
    }
}
