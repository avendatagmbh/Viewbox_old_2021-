using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using DbAnalyser.Helpers;
using System.Threading.Tasks;
using DbAnalyser.Models;
using DbAnalyser.MySqlDBCommands;
using DbAnalyser.Processing;
using DbAnalyser.Processing.NonSAP.Helpers;
using DbAnalyser.Processing.NonSAP.Methods;
using DbAnalyser.Processing.SAP;

namespace DbAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow
    {
        protected ParallelOptions parallelOptions = new ParallelOptions();

        static int stage;
        string actualtable = "";
        static int processedTableId = 1;

        // Data waiting to be inserted to the 'analyse_table_info' table
        List<AnalyseTableInfo> analyseTableInfo = new List<AnalyseTableInfo>();
        // Data waiting to be inseted to the 'tables' table
        List<ViewboxTablesInfo> viewboxTablesInfo = new List<ViewboxTablesInfo>();
        ConcurrentBag<ListboxElement> TheColumnList = new ConcurrentBag<ListboxElement>();
        // Data waiting to be inseted to the 'columns' table 
        ConcurrentBag<ViewboxColumnsInfo> viewboxColumnsInfo = new ConcurrentBag<ViewboxColumnsInfo>();
        // Data waiting to be inseted to the 'analyse_column_info' table
        ConcurrentBag<AnalysedColumnInfo> analyseCololumnInfo = new ConcurrentBag<AnalysedColumnInfo>();

        readonly List<ResultBoxView> sourceTables = new List<ResultBoxView>();
        readonly List<ResultBoxView> selectedTables = new List<ResultBoxView>();
        static volatile int _threadCount;

        public MainWindow()
        {
            InitializeComponent();
            labelstage.Visibility = Visibility.Hidden;
            listBoxTables.Visibility = Visibility.Hidden;
            listBoxtype.Visibility = Visibility.Hidden;
            listBoxChoosenTable.Visibility = Visibility.Hidden;
            buttonAdd.Visibility = Visibility.Hidden;
            buttonremove.Visibility = Visibility.Hidden;
            processPB.Minimum = 0;
            processPB.Maximum = 0;
            _threadCount = 0;
            MetaDataPb.Visibility = Visibility.Hidden;
            parallelOptions.MaxDegreeOfParallelism = 3;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            switch (stage)
            {
                case (0):
                    MetaDataPb.Visibility = Visibility.Visible;
                    buttonNext.IsEnabled = false;
                    finalDbName.Content = "Final name: " + DbConnection.FinalDatabase;
                    var bw = new BackgroundWorker();
                    bw.DoWork += MetadataReadDoWork;
                    bw.RunWorkerCompleted += MetaDataReadRunWorkerCompleted;
                    bw.RunWorkerAsync();
                    break;
                case (1):
                    buttonAdd.IsEnabled = false;
                    buttonremove.IsEnabled = false;
                    labelstage.IsEnabled = false;
                    listBoxTables.IsEnabled = false;
                    buttonBack.IsEnabled = false;
                    listBoxtype.Visibility = Visibility.Visible;
                    labelColumnsTables.Visibility = Visibility.Visible;
                    isSAPDb.Visibility = Visibility.Hidden;
                    ProcessTables();
                    success = true;
                    break;
                case (2):
                    InsertManulUpdates();
                    buttonNext.IsEnabled = false;
                    break;
            }
            if (success)
            {
                stage++;
                ChangeDesign();
            }
        }

        void MetadataReadDoWork(object sender, DoWorkEventArgs e)
        {
            QueryResult result = DbSelectCommnands.ReadingTables();
            e.Result = result;
        }

        void MetaDataReadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool success = false;
            var result = e.Result as QueryResult;

            if (result != null && result.technicalresult == SqlQueryResult.Successful)
            {
                listBoxTables.Visibility = Visibility.Visible;
                labelstage.Visibility = Visibility.Visible;
                int itemnumber = 0;
                foreach (var item in result.tableresult)
                {
                    listBoxTables.Items.Add(new ResultBoxView(item, result.rownumber[item], result.columnnumber[itemnumber] * (float)result.rownumber[item]));
                    sourceTables.Add(new ResultBoxView(item, result.rownumber[item], result.columnnumber[itemnumber] * (float)result.rownumber[item]));
                    itemnumber++;
                }
                success = true;
                Title = "DbAnalyser (Connected)";
                buttonconfigure.Visibility = Visibility.Hidden;
                buttonBack.Visibility = Visibility.Visible;
                filterTablesBtn.Visibility = Visibility.Visible;
                filterTablesTb.Visibility = Visibility.Visible;
                filterSelTablesBtn.Visibility = Visibility.Visible;
                filterSelTablesTb.Visibility = Visibility.Visible;
                isSAPDb.Visibility = Visibility.Visible;
                skipUserUpdate.Visibility = Visibility.Visible;
                StrictMode.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Database schema read was unsuccessful!");
            }
            if (success)
            {
                stage++;
                ChangeDesign();
            }
            MetaDataPb.Visibility = Visibility.Hidden;
        }

        /*
         * Override the data created by the DbAnalyser
         */
        private void InsertManulUpdates()
        {
            processPB.Maximum = TheColumnList.Count;
            if (skipUserUpdate.IsChecked == false)
            {
                var bw = new BackgroundWorker();
                bw.DoWork += bw_InsertManulUpdate_DoWork;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += bw_InsertManualUpdate_ReportProgress;
                bw.RunWorkerCompleted += bw_InserManualUpdate_RunWorkerCompleted;
                bw.RunWorkerAsync();
            }
            else
            {
                stage++;
                ChangeDesign();
            }
        }

        void bw_InserManualUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            stage++;
            ChangeDesign();
        }

        void bw_InsertManualUpdate_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            processPB.Value = e.ProgressPercentage;
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
        }

        void bw_InsertManulUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = sender as BackgroundWorker;
            int i = 1;
            foreach (var item in TheColumnList)
            {
                var aColInfoItem = analyseCololumnInfo.SingleOrDefault(c => c.name == item.columnname && c.tableName == item.tablename);
                var vColInfoItem = viewboxColumnsInfo.SingleOrDefault(c => c.colName == item.columnname && c.tableName == item.tablename);

                if (aColInfoItem != null)
                {
                    aColInfoItem.type = item.value;
                    aColInfoItem.length = item.length;
                    if (vColInfoItem != null)
                    {
                        vColInfoItem.maxLength = item.length;
                        vColInfoItem.dataTypeName = item.value;

                        switch (item.value)
                        {
                            case "DATE":
                                aColInfoItem.typeId = ProcessDataTypes.Date;
                                vColInfoItem.dataType = ProcessDataTypes.Date;
                                break;
                            case "DATETIME":
                                aColInfoItem.typeId = ProcessDataTypes.DateTime;
                                vColInfoItem.dataType = ProcessDataTypes.DateTime;
                                break;
                            case "TIME":
                                aColInfoItem.typeId = ProcessDataTypes.Time;
                                vColInfoItem.dataType = ProcessDataTypes.Time;
                                break;
                            case "VARCHAR":
                                aColInfoItem.typeId = ProcessDataTypes.String;
                                vColInfoItem.dataType = ProcessDataTypes.String;
                                break;
                            case "TEXT":
                                aColInfoItem.typeId = ProcessDataTypes.String;
                                vColInfoItem.dataType = ProcessDataTypes.String;
                                break;
                            case "INT":
                                aColInfoItem.typeId = ProcessDataTypes.Integer;
                                vColInfoItem.dataType = ProcessDataTypes.Integer;
                                break;
                            case "DECIMAL":
                                aColInfoItem.typeId = ProcessDataTypes.Decimal;
                                vColInfoItem.dataType = ProcessDataTypes.Decimal;
                                break;
                            case "BOOL":
                                aColInfoItem.typeId = ProcessDataTypes.Bool;
                                vColInfoItem.dataType = ProcessDataTypes.Bool;
                                break;
                        }
                    }
                }
                i++;
                if (bw != null) bw.ReportProgress(i);
            }
        }

        /**
         * This method starts the analysation process for every table in the selected database
         */
        private void ProcessTables()
        {
            /**
             * To initializethe analysation tools on a new thread before starting the multi thread analysation process! 
             */
            processPB.Visibility = Visibility.Visible;
            processPB.Value = processPB.Minimum;
            colsToProcess.Visibility = Visibility.Visible;

            buttonNext.IsEnabled = false;
            if (isSAPDb.IsChecked == true)
            {
                processPB.Maximum = 10;
                processPB.Maximum += listBoxChoosenTable.Items.Count;
                var bw = new BackgroundWorker();
                currentlyWorking.Content = "Loading dd03l table";
                bw.DoWork += bw_SAP_Init_DoWork;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += bw_SAP_Init_ProgressChanged;
                bw.RunWorkerCompleted += bw_SAP_Init_RunWorkerCompleted;
                bw.RunWorkerAsync();
            }
            else
            {
                processPB.Maximum = 5 * listBoxChoosenTable.Items.Count + 5;


                processPB.Value += 1;
                processPB.Value += 1;
                processPB.Value += 1;
                processPB.Value += 1;
                processPB.Value += 1;

                int cntr = 1;
                foreach (ResultBoxView item in listBoxChoosenTable.Items)
                {
                    string tablename = item.colName;

                    if (isSAPDb.IsChecked == false)
                    {
                        var tableInfo = new TableInfo { id = cntr, name = tablename };

                        var bw = new BackgroundWorker();
                        bw.DoWork += bw_NonSAP_Init_DoWork;
                        bw.RunWorkerCompleted += bw_NonSap_Init_RunWorkerCompleted;
                        bw.WorkerReportsProgress = true;
                        bw.ProgressChanged += bw_NonSap_Init_ProgressChanged;
                        bw.RunWorkerAsync(tableInfo);
                    }
                    cntr++;
                }
            }
        }

        void bw_NonSap_Init_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPB.Value += e.ProgressPercentage;
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
        }

        void bw_NonSap_Init_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var columnsInfo = e.Result as List<ColData>;
            // Processing every column on different thread
            currentlyWorking.Content = "Analysing tables (non-SAP).";
            ReadColumnsTypes(columnsInfo);
        }

        void bw_NonSAP_Init_DoWork(object sender, DoWorkEventArgs e)
        {
            var tableInfo = e.Argument as TableInfo;
            var bw = sender as BackgroundWorker;

            if (tableInfo != null)
            {
                List<ColData> columnsInfo = DbSelectCommnands.ReadingRows(tableInfo.name, tableInfo.id);
                if (bw != null)
                {
                    bw.ReportProgress(3);

                    long rowCount = DbSelectCommnands.ReadingRowCount(tableInfo.name);
                    bw.ReportProgress(1);

                    foreach (var colInfo in columnsInfo)
                    {
                        colInfo.RowCount = rowCount;
                    }

                    // insert data to analyseTableInfo
                    var tableInfoItem = new AnalyseTableInfo
                    {
                        tableId = columnsInfo[0].TableId,
                        analysationState = Transfer.AnalyseDatabaseInfo.AnalysationState.Successful,
                        comment = "",
                        count = rowCount,
                        description = "",
                        duration = 0,
                        name = tableInfo.name,
                        timeStamp = DateTime.Now,
                        type = 0
                    };
                    analyseTableInfo.Add(tableInfoItem);

                    // insert data to viewboxTableInfo
                    var vTableInfoItem = new ViewboxTablesInfo
                    {
                        category = 0,
                        databaseName = DbConnection.SourceDatabase,
                        tableName = tableInfo.name,
                        type = 1,
                        rowCount = rowCount,
                        visible = 1,
                        archived = 0,
                        objectType = 0,
                        defaultSheme = 0,
                        transactionNumber = 10000 + tableInfo.id,
                        tableId = tableInfo.id,
                        userDefined = 0,
                        ordinal = tableInfo.id - 1
                    };
                    viewboxTablesInfo.Add(vTableInfoItem);
                    bw.ReportProgress(1);
                }

                //setting result
                e.Result = columnsInfo;
            }
        }

        private void ReadColumnsTypes(List<ColData> colsInfo)
        {
            processPB.Maximum += colsInfo.Count;

            foreach (var colInfo in colsInfo)
            {
                var bw = new BackgroundWorker();
                if (StrictMode.IsChecked == true)
                {
                    bw.DoWork += bw_nonSAP_col_analyse_DoWork;
                }
                else
                {
                    bw.DoWork += bw_nonSAP_col_analyse_fast_DoWork;
                }
                bw.RunWorkerCompleted += bw_nonSAP_col_analyse_RunWorkerCompleted;
                bw.RunWorkerAsync(colInfo);
            }
        }

        void bw_nonSAP_col_analyse_fast_DoWork(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads))
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("addsa")
            {
                _threadCount++;
            }
            var colData = (e.Argument as ColData);
            if (colData != null)
            {
                List<string> values = colData.ColumnValues;
                string columntype;
                int length;
                if (colData.RowCount != 0)
                {
                    length = ProcessingModul.GetLongestTextInCol(colData.TableName, colData.ColumnName);
                    if (length >= 30 && length < 256)
                    {
                        columntype = "VARCHAR";
                    }
                    else if (length >= 30 && length > 255)
                    {
                        columntype = "TEXT";
                        length = 0;
                    }
                    else
                    {
                        columntype = ProcessingModul.ProcessColumnFast(ref values, colData.TableName, colData.ColumnName, colData.RowCount);
                    }
                }
                else
                {
                    columntype = "TEXT";
                    length = 0;
                    Thread.Sleep(rnd.Next(10, 100));
                }

                var readeditem = new ListboxElement
                {
                    tablename = colData.TableName,
                    columnname = colData.ColumnName,
                    columnid = colData.ColumnId,
                    length = length
                };

                if (length <= 255 && length > 0 && columntype == "TEXT")
                {
                    columntype = "VARCHAR";
                    readeditem.value = columntype;
                }
                else if (columntype == "INT" && length >= 10)
                {
                    columntype = "BIGINT";
                    readeditem.value = columntype;
                }
                else if (length > 65535)
                {
                    columntype = "BLOB";
                    readeditem.value = columntype;
                }
                else
                {
                    readeditem.value = columntype;
                }
                TheColumnList.Add(readeditem);

                var aColInfoItem = new AnalysedColumnInfo();
                var vColInfoItem = new ViewboxColumnsInfo();

                aColInfoItem.tableId = colData.TableId;
                aColInfoItem.tableName = colData.TableName;
                aColInfoItem.colId = colData.ColumnId;
                aColInfoItem.description = "";
                aColInfoItem.length = length;
                aColInfoItem.name = colData.ColumnName;
                aColInfoItem.type = columntype; // TODO
                switch (columntype)
                {
                    case "DATE":
                        DateAnalyser dfilter = DateAnalyser.DateFilter;
                        aColInfoItem.dateFormat = dfilter.GetDateFormat(ref values);
                        if (aColInfoItem.dateFormat == String.Empty)
                        {
                            aColInfoItem.typeId = ProcessDataTypes.String;
                            vColInfoItem.dataType = ProcessDataTypes.String;
                            aColInfoItem.type = "TEXT";
                        }
                        else
                        {
                            aColInfoItem.typeId = ProcessDataTypes.Date;
                            vColInfoItem.dataType = ProcessDataTypes.Date;
                        }
                        break;
                    case "DATETIME":
                        DateTimeAnalyser dtfilter = DateTimeAnalyser.DateTimeFilter;
                        aColInfoItem.dateFormat = dtfilter.GetDateFormat(ref values);
                        if (aColInfoItem.dateFormat == String.Empty)
                        {
                            aColInfoItem.typeId = ProcessDataTypes.String;
                            vColInfoItem.dataType = ProcessDataTypes.String;
                            aColInfoItem.type = "TEXT";
                        }
                        else
                        {
                            aColInfoItem.typeId = ProcessDataTypes.DateTime;
                            vColInfoItem.dataType = ProcessDataTypes.DateTime;
                        }
                        break;
                    case "TIME":
                        aColInfoItem.typeId = ProcessDataTypes.Time;
                        vColInfoItem.dataType = ProcessDataTypes.Time;
                        break;
                    case "VARCHAR":
                        aColInfoItem.typeId = ProcessDataTypes.String;
                        vColInfoItem.dataType = ProcessDataTypes.String;
                        break;
                    case "TEXT":
                        aColInfoItem.typeId = ProcessDataTypes.String;
                        vColInfoItem.dataType = ProcessDataTypes.String;
                        break;
                    case "INT":
                        aColInfoItem.typeId = ProcessDataTypes.Integer;
                        vColInfoItem.dataType = ProcessDataTypes.Integer;
                        break;
                    case "DECIMAL":
                        aColInfoItem.typeId = ProcessDataTypes.Decimal;
                        vColInfoItem.dataType = ProcessDataTypes.Decimal;
                        string longestText = ProcessingModul.GetLongestTextInColumn(colData.TableName, colData.ColumnName);
                        aColInfoItem.decimals = DecimalAnalyser.getFractionPartSize(longestText);
                        vColInfoItem.decimals = aColInfoItem.decimals;
                        aColInfoItem.decimalFormat = ProcessingModul.GetDecimalCaster(colData.TableName, colData.ColumnName, longestText);
                        break;
                    case "BOOL":
                        aColInfoItem.typeId = ProcessDataTypes.Bool;
                        vColInfoItem.dataType = ProcessDataTypes.Bool;
                        break;
                    case "BLOB":
                        aColInfoItem.typeId = ProcessDataTypes.Blob;
                        vColInfoItem.dataType = ProcessDataTypes.Blob;
                        break;
                }

                analyseCololumnInfo.Add(aColInfoItem);

                // insert data to viewboxColumnsInfo    
                vColInfoItem.isVisible = 1;
                vColInfoItem.tableId = colData.TableId;
                vColInfoItem.tableName = colData.TableName;
                vColInfoItem.dataTypeName = columntype;
                vColInfoItem.originalName = "";
                vColInfoItem.optimaliztaionType = 0;
                vColInfoItem.isEmpty = (length == 0) ? 1 : 0;
                vColInfoItem.maxLength = length;
                vColInfoItem.paramOperators = 0;
                vColInfoItem.flag = 0;
                vColInfoItem.colName = colData.ColumnName;
                vColInfoItem.userDefined = 0;
                vColInfoItem.ordinal = colData.ColumnId;
                viewboxColumnsInfo.Add(vColInfoItem);
            }

            // disposing the column values, because it contains a treshold number of string which uses up a lot of memory!!
            if (colData != null) colData.Dispose();
        }

        /**
         * BackgroundWorker for non-SAP based database analysation
         */
        void bw_nonSAP_col_analyse_DoWork(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads))
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("lock")
            {
                _threadCount++;
            }
            var colData = (e.Argument as ColData);
            if (colData != null)
            {
                List<string> values = colData.ColumnValues;
                string columntype;
                int length;
                if (colData.RowCount != 0)
                {
                    length = ProcessingModul.GetLongestTextInCol(colData.TableName, colData.ColumnName);
                    if (length >= 30 && length < 256)
                    {
                        columntype = "VARCHAR";
                    }
                    else if (length >= 30 && length > 255)
                    {
                        columntype = "TEXT";
                        length = 0;
                    }
                    else
                    {
                        columntype = ProcessingModul.ProcessColumn(ref values, colData.TableName, colData.ColumnName, colData.RowCount);
                    }
                }
                else
                {
                    columntype = "TEXT";
                    length = 0;
                    Thread.Sleep(rnd.Next(10, 100));
                }

                var readeditem = new ListboxElement
                {
                    tablename = colData.TableName,
                    columnname = colData.ColumnName,
                    columnid = colData.ColumnId,
                    length = length
                };

                if (length <= 255 && length > 0 && columntype == "TEXT")
                {
                    columntype = "VARCHAR";
                    readeditem.value = columntype;
                }
                else if (columntype == "INT" && length >= 10)
                {
                    columntype = "BIGINT";
                    readeditem.value = columntype;
                }
                else if (length > 65535)
                {
                    columntype = "BLOB";
                    readeditem.value = columntype;
                }
                else
                {
                    readeditem.value = columntype;
                }
                TheColumnList.Add(readeditem);

                var aColInfoItem = new AnalysedColumnInfo();
                var vColInfoItem = new ViewboxColumnsInfo();

                aColInfoItem.tableId = colData.TableId;
                aColInfoItem.tableName = colData.TableName;
                aColInfoItem.colId = colData.ColumnId;
                aColInfoItem.description = "";
                aColInfoItem.length = length;
                aColInfoItem.name = colData.ColumnName;
                aColInfoItem.type = columntype; // TODO
                switch (columntype)
                {
                    case "DATE":
                        DateAnalyser dfilter = DateAnalyser.DateFilter;
                        aColInfoItem.dateFormat = dfilter.GetDateFormat(ref values);
                        if (aColInfoItem.dateFormat == String.Empty)
                        {
                            aColInfoItem.typeId = ProcessDataTypes.String;
                            vColInfoItem.dataType = ProcessDataTypes.String;
                            aColInfoItem.type = "TEXT";
                        }
                        else
                        {
                            aColInfoItem.typeId = ProcessDataTypes.Date;
                            vColInfoItem.dataType = ProcessDataTypes.Date;
                        }
                        break;
                    case "DATETIME":
                        DateTimeAnalyser dtfilter = DateTimeAnalyser.DateTimeFilter;
                        aColInfoItem.dateFormat = dtfilter.GetDateFormat(ref values);
                        if (aColInfoItem.dateFormat == String.Empty)
                        {
                            aColInfoItem.typeId = ProcessDataTypes.String;
                            vColInfoItem.dataType = ProcessDataTypes.String;
                            aColInfoItem.type = "TEXT";
                        }
                        else
                        {
                            aColInfoItem.typeId = ProcessDataTypes.DateTime;
                            vColInfoItem.dataType = ProcessDataTypes.DateTime;
                        }
                        break;
                    case "TIME":
                        aColInfoItem.typeId = ProcessDataTypes.Time;
                        vColInfoItem.dataType = ProcessDataTypes.Time;
                        break;
                    case "VARCHAR":
                        aColInfoItem.typeId = ProcessDataTypes.String;
                        vColInfoItem.dataType = ProcessDataTypes.String;
                        break;
                    case "TEXT":
                        aColInfoItem.typeId = ProcessDataTypes.String;
                        vColInfoItem.dataType = ProcessDataTypes.String;
                        break;
                    case "INT":
                        aColInfoItem.typeId = ProcessDataTypes.Integer;
                        vColInfoItem.dataType = ProcessDataTypes.Integer;
                        break;
                    case "DECIMAL":
                        aColInfoItem.typeId = ProcessDataTypes.Decimal;
                        vColInfoItem.dataType = ProcessDataTypes.Decimal;
                        string longestText = ProcessingModul.GetLongestTextInColumn(colData.TableName, colData.ColumnName);
                        aColInfoItem.decimals = DecimalAnalyser.getFractionPartSize(longestText);
                        vColInfoItem.decimals = aColInfoItem.decimals;
                        aColInfoItem.decimalFormat = ProcessingModul.GetDecimalCaster(colData.TableName, colData.ColumnName, longestText);
                        break;
                    case "BOOL":
                        aColInfoItem.typeId = ProcessDataTypes.Bool;
                        vColInfoItem.dataType = ProcessDataTypes.Bool;
                        break;
                    case "BLOB":
                        aColInfoItem.typeId = ProcessDataTypes.Blob;
                        vColInfoItem.dataType = ProcessDataTypes.Blob;
                        break;
                }

                analyseCololumnInfo.Add(aColInfoItem);

                // insert data to viewboxColumnsInfo    
                vColInfoItem.isVisible = 1;
                vColInfoItem.tableId = colData.TableId;
                vColInfoItem.tableName = colData.TableName;
                vColInfoItem.dataTypeName = columntype;
                vColInfoItem.originalName = "";
                vColInfoItem.optimaliztaionType = 0;
                vColInfoItem.isEmpty = (length == 0) ? 1 : 0;
                vColInfoItem.maxLength = length;
                vColInfoItem.paramOperators = 0;
                vColInfoItem.flag = 0;
                vColInfoItem.colName = colData.ColumnName;
                vColInfoItem.userDefined = 0;
                vColInfoItem.ordinal = colData.ColumnId;
                viewboxColumnsInfo.Add(vColInfoItem);
            }

            // disposing the column values, because it contains a treshold number of string which uses up a lot of memory!!
            if (colData != null) colData.Dispose();
        }

        void bw_nonSAP_col_analyse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // CancelAsync was called.
            }
            else
            {
                processPB.Value += 1;
                lock ("thiaswell")
                {
                    _threadCount--;
                    if (_threadCount < 0)
                    {
                        _threadCount = 0;
                    }
                }
                colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
                if (processPB.Value % 100 == 0)
                {
                    AddReadedName();
                }
                if (processPB.Maximum == processPB.Value)
                {
                    buttonNext.IsEnabled = true;
                    AddReadedName();
                }
            }
        }

        /**
         * Initialize SAP dd03l tables content 
         */
        void bw_SAP_Init_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bw.ReportProgress(1);
            ProcessingModul.dbAnalyser = new SAPDbAnalyser(selectedTables.Select(t => t.colName).ToList());
            bw.ReportProgress(1);
        }

        /**
         * Initialize SAP dd03l tables content progress changed method
         */
        void bw_SAP_Init_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPB.Value += 5;
        }

        /**
         * Processing tables after Init finished 
         */
        void bw_SAP_Init_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // CancelAsync was called.
            }
            else
            {
                int cntr = 1;
                currentlyWorking.Content = "Analysing tables (SAP).";
                foreach (ResultBoxView item in listBoxChoosenTable.Items)
                {
                    string tablename = item.colName;
                    actualtable = tablename;
                    ReadedColumnsNameAndType(tablename, cntr, item.length);
                    cntr++;
                }
            }
        }

        /**
         * This method start a BackgroundWorkers which require the selected tables name 
         */
        private void ReadedColumnsNameAndType(string actualTableName, int tableId, long rowCount)
        {
            processedTableId = tableId;
            var tableInfo = new TableInfo { id = processedTableId, name = actualTableName, RowCount = rowCount };

            var bw = new BackgroundWorker();
            if (isSAPDb.IsChecked == true)
            {
                bw.DoWork += bw_DoWork_SAP;
                bw.RunWorkerCompleted += bw_SAP_worker_RunWorkerCompleted;
                bw.RunWorkerAsync(tableInfo);
            }
        }

        /**
         * Displaying the results after analysation finished
         */
        void bw_SAP_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AddReadedName();

            if (e.Error != null)
            {
                // MessageBox.Show(e.Error.Message);
                Logging.Logger.LogError(e.Error.Message);
            }
            // Finally, handle the case where the operation  
            // succeeded.
            processPB.Value += 1;
            lock ("asd")
            {
                _threadCount--;
                if (_threadCount < 0)
                {
                    _threadCount = 0;
                }
            }
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";

            if (processPB.Value == processPB.Maximum)
            {
                buttonNext.IsEnabled = true;
            }

        }

        /**
         * BackgroundWorker for SAP based database analysation
         */
        void bw_DoWork_SAP(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads))
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("addsa")
            {
                _threadCount++;
            }
            var tableInfo = e.Argument as TableInfo;
            try
            {
                if (tableInfo != null)
                {
                    List<string> actualColumnNames = DbSelectCommnands.ReadingColumnsName(tableInfo.name).tableresult; // return column names of the selected SAP table
                    // QueryResult dbData = DbSelectCommnands.ReadingTables(); // returns the selected tables metainformation
                    var sapReturnValues = new List<SAPReturnValue>();
                    if (tableInfo.RowCount > 0)
                    {
                        sapReturnValues = ProcessingModul.ProcessSAPTable(actualColumnNames, tableInfo.name); // returns the processed SAP table data
                    }
                    else
                    {
                        sapReturnValues.AddRange(actualColumnNames.Select(colName => new SAPReturnValue(colName, "TEXT", 0, 0)));
                    }

                    int ordinalCntr = 0;

                    foreach (var result in sapReturnValues)
                    {
                        string columntype = result.Type;
                        int length = result.Length;
                        var readeditem = new ListboxElement
                        {
                            tablename = tableInfo.name,
                            columnname = result.Name,
                            length = length
                        };

                        if (length < 65535 && length > 0 && columntype == "TEXT")
                        {
                            columntype = "VARCHAR";
                            readeditem.value = columntype;
                        }
                        else if (columntype == "INT" && length >= 10)
                        {
                            columntype = "BIGINT";
                            readeditem.value = columntype;
                        }
                        else
                        {
                            readeditem.value = columntype;
                        }


                        TheColumnList.Add(readeditem);

                        // insert data to analyseColumnInfo
                        var aColInfoItem = new AnalysedColumnInfo();
                        var vColInfoItem = new ViewboxColumnsInfo();

                        aColInfoItem.tableId = tableInfo.id;
                        aColInfoItem.tableName = tableInfo.name;
                        aColInfoItem.description = "";
                        aColInfoItem.length = length;
                        aColInfoItem.colId = ordinalCntr;
                        aColInfoItem.name = result.Name;
                        aColInfoItem.type = columntype;
                        switch (columntype)
                        {
                            case "DATE":
                                aColInfoItem.typeId = ProcessDataTypes.Date;
                                vColInfoItem.dataType = ProcessDataTypes.Date;
                                break;
                            case "DATETIME":
                                aColInfoItem.typeId = ProcessDataTypes.DateTime;
                                vColInfoItem.dataType = ProcessDataTypes.DateTime;
                                break;
                            case "TIME":
                                aColInfoItem.typeId = ProcessDataTypes.Time;
                                vColInfoItem.dataType = ProcessDataTypes.Time;
                                break;
                            case "VARCHAR":
                                aColInfoItem.typeId = ProcessDataTypes.String;
                                vColInfoItem.dataType = ProcessDataTypes.String;
                                break;
                            case "TEXT":
                                aColInfoItem.typeId = ProcessDataTypes.String;
                                vColInfoItem.dataType = ProcessDataTypes.String;
                                break;
                            case "INT":
                                aColInfoItem.typeId = ProcessDataTypes.Integer;
                                vColInfoItem.dataType = ProcessDataTypes.Integer;
                                break;
                            case "DECIMAL":
                                aColInfoItem.typeId = ProcessDataTypes.Decimal;
                                vColInfoItem.dataType = ProcessDataTypes.Decimal;
                                aColInfoItem.decimals = result.Decimals;
                                vColInfoItem.decimals = result.Decimals;
                                break;
                            case "BOOL":
                                aColInfoItem.typeId = ProcessDataTypes.Bool;
                                vColInfoItem.dataType = ProcessDataTypes.Bool;
                                break;
                            case "BLOB":
                                aColInfoItem.typeId = ProcessDataTypes.Blob;
                                vColInfoItem.dataType = ProcessDataTypes.Blob;
                                break;
                        }
                        analyseCololumnInfo.Add(aColInfoItem);

                        // insert data to viewboxColumnsInfo
                        vColInfoItem.isVisible = 1;
                        vColInfoItem.tableId = tableInfo.id;
                        vColInfoItem.tableName = tableInfo.name;
                        vColInfoItem.dataTypeName = columntype;
                        vColInfoItem.originalName = "";
                        vColInfoItem.optimaliztaionType = 0;
                        vColInfoItem.isEmpty = (length == 0) ? 1 : 0;
                        vColInfoItem.maxLength = length;
                        vColInfoItem.paramOperators = 0;
                        vColInfoItem.flag = 0;
                        vColInfoItem.colName = result.Name;
                        vColInfoItem.userDefined = 0;
                        vColInfoItem.ordinal = ordinalCntr;
                        viewboxColumnsInfo.Add(vColInfoItem);
                        ordinalCntr++;
                    }
                }

                Parallel.ForEach(analyseCololumnInfo.Where(c => c.typeId == ProcessDataTypes.Decimal && c.tableId == tableInfo.id), parallelOptions, col =>
                {
                    string longestText = ProcessingModul.GetLongestTextInColumn(col.tableName, col.name);
                    col.decimalFormat = ProcessingModul.GetDecimalCaster(col.tableName, col.name, longestText);
                });

                // insert data to analyseTableInfo
                if (tableInfo != null)
                {
                    var tableInfoItem = new AnalyseTableInfo
                    {
                        tableId = tableInfo.id,
                        analysationState = Transfer.AnalyseDatabaseInfo.AnalysationState.Successful,
                        comment = "",
                        count = tableInfo.RowCount,
                        description = "",
                        duration = 0,
                        name = tableInfo.name,
                        timeStamp = DateTime.Now,
                        type = 0
                    };
                    analyseTableInfo.Add(tableInfoItem);
                }

                // insert data to viewboxTableInfo
                if (tableInfo != null)
                {
                    var vTableInfoItem = new ViewboxTablesInfo
                    {
                        category = 0,
                        databaseName = DbConnection.SourceDatabase,
                        tableName = tableInfo.name,
                        type = 1,
                        rowCount = tableInfo.RowCount,
                        visible = 1,
                        archived = 0,
                        objectType = 0,
                        defaultSheme = 0,
                        transactionNumber = 10000 + tableInfo.id,
                        tableId = tableInfo.id,
                        userDefined = 0,
                        ordinal = tableInfo.id - 1
                    };
                    viewboxTablesInfo.Add(vTableInfoItem);
                }
            }
            catch (Exception)
            {
                
            }
        }

        /**
         * Append the selected tables column information to the 2 listboxes
         */
        private void AddReadedName()
        {
            if (stage > 1)
            {
                var resultBoxView = listBoxChoosenTable.SelectedItem as ResultBoxView;
                if (resultBoxView != null)
                {
                    string tablename = resultBoxView.colName;
                    actualtable = tablename;
                }
                var tempColumnList = new List<ListboxElement>(TheColumnList);
                var list = tempColumnList
                    .Where(c => c.tablename == actualtable)
                    .OrderBy(c => c.tablename)
                    .ThenBy(c => c.columnid);
                listBoxtype.ItemsSource = list.ToList();
            }
        }


        private void ChangeDesign()
        {
            switch (stage)
            {
                case (1):
                    listBoxChoosenTable.Visibility = Visibility.Visible;
                    buttonAdd.Visibility = Visibility.Visible;
                    buttonremove.Visibility = Visibility.Visible;
                    blackListBtn.Visibility = Visibility.Visible;
                    whiteListBtn.Visibility = Visibility.Visible;
                    buttonNext.IsEnabled = false;
                    labelproccessstage.Content = "Step 1: Choosing tables";
                    break;
                case (2):
                    listBoxChoosenTable.SelectionMode = SelectionMode.Single;
                    listBoxChoosenTable.SelectedIndex = 0;
                    blackListBtn.Visibility = Visibility.Hidden;
                    whiteListBtn.Visibility = Visibility.Hidden;
                    string tablename = listBoxChoosenTable.SelectedItem.ToString();
                    actualtable = tablename;
                    labelproccessstage.Content = "Step 2: Processing tables";
                    AddReadedName();
                    break;
                case (3):
                    labelproccessstage.Content = "Step 3: Copy to the final database";
                    buttonNext.Visibility = Visibility.Hidden;
                    skipUserUpdate.Visibility = Visibility.Hidden;
                    buttonImport.Visibility = Visibility.Visible;
                    buttonImport.IsEnabled = true;
                    break;
                case (4):
                    labelproccessstage.Content = "Step 3: Copy to the final database";
                    buttonNext.Visibility = Visibility.Hidden;
                    buttonImport.Visibility = Visibility.Visible;
                    buttonImport.IsEnabled = true;
                    break;
            }
        }

        private void buttonconfigure_Click(object sender, RoutedEventArgs e)
        {
            if (stage == 0)
            {
                var connectionWindow = new ConnectionWindow { Top = Top };
                connectionWindow.Show();
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var changedList = new List<ResultBoxView>();
            if (listBoxTables.SelectedItems.Count > 0)
            {
                foreach (ResultBoxView item in listBoxTables.SelectedItems)
                {
                    listBoxChoosenTable.Items.Add(item);
                    selectedTables.Add(item);
                    changedList.Add(item);
                }
                foreach (var item in changedList)
                {
                    listBoxTables.Items.Remove(item);
                    sourceTables.Remove(item);
                }
            }
            else
            {
                foreach (ResultBoxView item in listBoxTables.Items)
                {
                    listBoxChoosenTable.Items.Add(item);
                    selectedTables.Add(item);
                    changedList.Add(item);
                }
                foreach (var item in changedList)
                {
                    listBoxTables.Items.Remove(item);
                    sourceTables.Remove(item);
                }
            }
            buttonNext.IsEnabled = true;
            if (isSAPDb.IsChecked == true)
            {
                processPB.Maximum = listBoxChoosenTable.Items.Count + 2;
            }
            labelstage.Content = "Selected tables (" + DbConnection.SourceDatabase + "): \n" + (listBoxTables.Items.Count + listBoxChoosenTable.Items.Count) + " / " + listBoxChoosenTable.Items.Count;
        }

        private void buttonremove_Click(object sender, RoutedEventArgs e)
        {
            var changedList = new List<ResultBoxView>();
            if (listBoxChoosenTable.SelectedItems.Count > 0)
            {
                foreach (ResultBoxView item in listBoxChoosenTable.SelectedItems)
                {
                    listBoxTables.Items.Add(item);
                    sourceTables.Add(item);
                    changedList.Add(item);
                }
                foreach (var item in changedList)
                {
                    listBoxChoosenTable.Items.Remove(item);
                    selectedTables.Remove(item);
                }
            }
            else
            {
                foreach (ResultBoxView item in listBoxChoosenTable.Items)
                {
                    listBoxTables.Items.Add(item);
                    sourceTables.Add(item);
                    changedList.Add(item);
                }
                foreach (var item in changedList)
                {
                    listBoxChoosenTable.Items.Remove(item);
                    selectedTables.Remove(item);
                }
            }
            if (listBoxChoosenTable.Items.Count == 0)
            {
                buttonNext.IsEnabled = false;
            }
            labelstage.Content = "Selected tables (" + DbConnection.SourceDatabase + "): \n" + (listBoxTables.Items.Count + listBoxChoosenTable.Items.Count) + " / " + listBoxChoosenTable.Items.Count;
        }

        private void listBoxChoosenTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (stage > 1)
            {
                var resultBoxView = listBoxChoosenTable.SelectedItem as ResultBoxView;
                if (resultBoxView != null)
                {
                    string tablename = resultBoxView.colName;
                    actualtable = tablename;
                }
                AddReadedName();
            }
        }

        /**
         * This method creates the required template databases: *_analyse, *_final, *_final_system
         * After that the metainformation will be inserted to the *_analyse table
         * The final database will be inserted in the *_final table
         */
        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            processPB.Value = processPB.Minimum;
            _threadCount = 0;
            buttonImport.IsEnabled = false;

            processPB.Maximum = (2 * listBoxChoosenTable.Items.Count) + 14;

            currentlyWorking.Content = "Generating final_system schema and tables.";

            var bw = new BackgroundWorker();
            bw.DoWork += bw_Do_import_Work;
            bw.RunWorkerCompleted += bw_RunWorker_import_Completed;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += bw_import_ProgressChanged;
            var threadData = new MyBool { isSap = (isSAPDb.IsChecked == true) };
            bw.RunWorkerAsync(threadData);
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            DbConnection.IsPaused = true;
            buttonPause.IsEnabled = false;
            buttonPause.Visibility = Visibility.Hidden;
            buttonContinue.IsEnabled = true;
            buttonContinue.Visibility = Visibility.Visible;
            currentlyWorking.Content = "Transferring paused.";
        }

        private void buttonContinue_Click(object sender, RoutedEventArgs e)
        {
            DbConnection.IsPaused = false;
            buttonPause.IsEnabled = true;
            buttonPause.Visibility = Visibility.Visible;
            buttonContinue.IsEnabled = false;
            buttonContinue.Visibility = Visibility.Hidden;
            currentlyWorking.Content = "Transferring from source schema to final schema.";
        }
        /**
         * Creating the required meta databases on a new thread 
         */
        void bw_Do_import_Work(object sender, DoWorkEventArgs e)
        {
            var bw = sender as BackgroundWorker;
            var threadData = e.Argument as MyBool;

            // Create *_analystic database & tables
            DbCreateCommands.CreateAnalyticsDatabase();
            if (bw != null)
            {
                bw.ReportProgress(1);
                DbCreateCommands.CreatePatternCache();
                bw.ReportProgress(1);
                DbCreateCommands.CreateDatabaseInfo();
                bw.ReportProgress(1);
                DbCreateCommands.CreateTableInfo();
                bw.ReportProgress(1);
                DbCreateCommands.CreateColumnInfo();
                bw.ReportProgress(1);

                // Create *_final database & tables
                DbCreateCommands.CreateFinalDatabase();
                bw.ReportProgress(1);

                // Create *_final_system database & tables
                DbCreateCommands.CreateFinalSystemDatabase();
                bw.ReportProgress(1);
                DbCreateCommands.CreateFinalSystemTables();
                bw.ReportProgress(1);

                // Insert data into *_analyse tables
                DbInsertCommands.InsertToAnalyseTableInfo(ref analyseTableInfo);
                bw.ReportProgress(1);
                DbInsertCommands.InsertToAnalyseColumnInfo(ref analyseCololumnInfo);
                bw.ReportProgress(1);

                // Store used regexes to pattern_cache table if we used patterns (Non-SAP database)
                if (threadData != null && threadData.isSap == false)
                {
                    if (DateAnalyser.getUsedRegexes().Count > 0 ||
                        DateTimeAnalyser.getUsedRegexes().Count > 0 ||
                        TimeAnalyser.GetUsedRegexes().Count > 0)
                    {
                        DbInsertCommands.InsertToPatternCache();
                    }
                }

                bw.ReportProgress(1);
                // Insert data into *_final_system tables
                DbInsertCommands.InsertToFinalSystemTable(ref viewboxTablesInfo);
                bw.ReportProgress(1);
                DbInsertCommands.InsertToFinalSystemCol(ref viewboxColumnsInfo);
                bw.ReportProgress(1);
                DbInsertCommands.InsertToFinalSystemDefValues();
                bw.ReportProgress(1);
            }
        }

        /*
         * Worker reports progess
         */
        private void bw_import_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.processPB.Value++;
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
        }

        /**
         * Copying the old tables from the source database to the newly created *_final database simultaneously
         * using BackgroundWorkers
        */
        void bw_RunWorker_import_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonPause.IsEnabled = true;
            buttonPause.Visibility = Visibility.Visible;
            buttonImport.Visibility = Visibility.Hidden;
            currentlyWorking.Content = "Transferring from source schema to final schema.";
            _threadCount = 0;
            foreach (var tableInfo in analyseTableInfo)
            {
                List<AnalysedColumnInfo> colInfo = analyseCololumnInfo.Where(c => c.tableName == tableInfo.name).ToList();
                var tableItem = new NonSAPColumnItem { rowCount = tableInfo.count, colInfos = colInfo };

                var bw = new BackgroundWorker();
                bw.DoWork += bw_Create_Final_Tables_DoWork;
                bw.RunWorkerCompleted += bw_Create_Final_Tables_RunWorkerCompleted;
                bw.RunWorkerAsync(tableItem);
            }
        }

        void bw_Create_Final_Tables_DoWork(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads) && DbConnection.IsPaused)
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("addsa")
            {
                _threadCount++;
            }
            var colInfo = e.Argument as NonSAPColumnItem;
            var tableInfo = analyseTableInfo.Where(t => colInfo != null && t.name == colInfo.colInfos[0].tableName).ToList();
            DbCreateCommands.CreateFinalTables(ref tableInfo, ref analyseCololumnInfo);
        }

        void bw_Create_Final_Tables_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //MessageBox.Show(e.Error.Message);
                Logging.Logger.LogError(e.Error.Message);
            }
            lock ("dsadsa")
            {
                _threadCount--;
                if (_threadCount < 0)
                {
                    _threadCount = 0;
                }
            }
            processPB.Value += 1;
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
            if (listBoxChoosenTable.Items.Count == (processPB.Maximum - processPB.Value))
            {
                foreach (var tableInfo in analyseTableInfo)
                {
                    if (tableInfo.count > 0)
                    {
                        List<AnalysedColumnInfo> colInfo = analyseCololumnInfo.Where(c => c.tableName == tableInfo.name).ToList();
                        var tableItem = new NonSAPColumnItem { rowCount = tableInfo.count, colInfos = colInfo };

                        var m_importWorker = new BackgroundWorker();
                        m_importWorker.DoWork += m_importWorker_DoWork;
                        m_importWorker.RunWorkerCompleted += m_importWorker_RunWorkerCompleted;
                        m_importWorker.WorkerSupportsCancellation = true;
                        m_importWorker.RunWorkerAsync(tableItem);
                    }
                    else
                    {
                        processPB.Value += 1;
                        colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
                    }
                }
            }

        }

        void m_importWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var colInfo = e.Argument as NonSAPColumnItem;
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads) && DbConnection.IsPaused)
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("addsa")
            {
                _threadCount++;
            }
            try
            {
                if (colInfo != null)
                {
                    long from = DbSelectCommnands.ReadingCurrentRowCount(colInfo.colInfos[0].tableName);
                    if (colInfo.rowCount > @from)
                    {
                        while (@from <= colInfo.rowCount)
                        {
                            DbInsertCommands.InsertTableIntoFinal(colInfo.colInfos[0].tableName, colInfo.colInfos, colInfo.rowCount, @from);
                            @from += DbConnection.InsertStepSize;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (colInfo != null)
                    Logging.Logger.LogError(String.Format("Transfer failed for : {0} table.", colInfo.colInfos[0].tableName));
                lock ("dsadsa")
                {
                    _threadCount--;
                    if (_threadCount < 0)
                    {
                        _threadCount = 0;
                    }
                    if (_threadCount < 0)
                    {
                        _threadCount = 0;
                    }
                }
                processPB.Value += 1;
                colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
                if (processPB.Maximum == processPB.Value)
                {
                    buttonImport.Visibility = Visibility.Hidden;
                    buttonFinished.Visibility = Visibility.Visible;
                    buttonFinished.IsEnabled = true;
                }
            }

        }

        void m_importWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock ("dsadsa")
            {
                _threadCount--;
                if (_threadCount < 0)
                {
                    _threadCount = 0;
                }
                if (_threadCount < 0)
                {
                    _threadCount = 0;
                }
            }
            processPB.Value += 1;
            colsToProcess.Content = (processPB.Value / processPB.Maximum * 100).ToString("F2") + " %";
            if (processPB.Maximum == processPB.Value)
            {
                buttonImport.Visibility = Visibility.Hidden;
                buttonFinished.Visibility = Visibility.Visible;
                buttonFinished.IsEnabled = true;
            }
        }

        /**
         * Clicking the Finished button closes the application
         */
        private void buttonFinished_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            labelstage.Visibility = Visibility.Hidden;
            listBoxTables.Visibility = Visibility.Hidden;
            listBoxtype.Visibility = Visibility.Hidden;
            listBoxChoosenTable.Visibility = Visibility.Hidden;
            buttonAdd.Visibility = Visibility.Hidden;
            buttonremove.Visibility = Visibility.Hidden;
            filterTablesBtn.Visibility = Visibility.Hidden;
            filterTablesTb.Visibility = Visibility.Hidden;
            filterSelTablesBtn.Visibility = Visibility.Hidden;
            filterSelTablesTb.Visibility = Visibility.Hidden;

            buttonBack.Visibility = Visibility.Hidden;
            buttonconfigure.Visibility = Visibility.Visible;
            buttonNext.IsEnabled = true;

            listBoxTables.Items.Clear();
            listBoxChoosenTable.Items.Clear();

            stage--;
        }

        private void genSystemBtn_Click(object sender, RoutedEventArgs e)
        {
            restoreProgress.Maximum = 10;
            var sysTableWorker = new BackgroundWorker();
            sysTableWorker.DoWork += sysTableWorker_DoWork;
            sysTableWorker.RunWorkerCompleted += sysTableWorker_RunWorkerCompleted;
            sysTableWorker.RunWorkerAsync();
        }

        void sysTableWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            restoreProgress.Value = 10;
            restoreProgress.Maximum += viewboxTablesInfo.Count;
            restoreProgressPercentage.Content = (restoreProgress.Value / restoreProgress.Maximum * 100).ToString("F2") + "%";
            foreach (var item in viewboxTablesInfo)
            {
                var bw = new BackgroundWorker();
                bw.DoWork += bw_genSys_DoWork;
                bw.WorkerReportsProgress = true;
                bw.RunWorkerCompleted += bw_genSys_RunWorkerCompleted;
                bw.RunWorkerAsync(item);
            }
        }

        void sysTableWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            viewboxTablesInfo.Clear();
            DbCreateCommands.CreateFinalSystemDatabase();
            DbCreateCommands.CreateFinalSystemTables();

            viewboxTablesInfo = DbSelectCommnands.ReadTableInfo();
            DbInsertCommands.InsertToFinalSystemTable_Generation(ref viewboxTablesInfo);
        }

        void bw_genSys_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock ("dsadsa")
            {
                _threadCount--;
                if (_threadCount < 0)
                {
                    _threadCount = 0;
                }
            }
            restoreProgress.Value += 1;
            restoreProgressPercentage.Content = (restoreProgress.Value / restoreProgress.Maximum * 100).ToString("F2") + "%";
            if (restoreProgress.Value == restoreProgress.Maximum)
            {
                restoreProgress.Maximum += 2;
                DbInsertCommands.InsertToFinalSystemCol_Generation(ref viewboxColumnsInfo);
                restoreProgress.Value += 1;
                DbInsertCommands.InsertToFinalSystemDefValues();
                restoreProgress.Value += 1;
                restoreProgressPercentage.Content = (restoreProgress.Value / restoreProgress.Maximum * 100).ToString("F2") + "%";
            }
        }

        void bw_genSys_DoWork(object sender, DoWorkEventArgs e)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 500));
            while (!(_threadCount < DbConnection.AllowedThreads))
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock ("addsa")
            {
                _threadCount++;
            }
            var item = e.Argument as ViewboxTablesInfo;
            ConcurrentBag<ViewboxColumnsInfo> tableCols = DbSelectCommnands.ReadColInfo(item);
            foreach (var colItem in tableCols)
            {
                viewboxColumnsInfo.Add(colItem);
            }
        }

        private void fixRowBtn_Click(object sender, RoutedEventArgs e)
        {
            /*List<ViewboxTablesInfo> tablesInfo = DbSelectCommnands.ReadTableInfo();
            ConcurrentBag<ViewboxColumnsInfo> colInfo = DbSelectCommnands.ReadColInfo(tablesInfo);
            DbAlterCommands.changeTimeStampToDateTime(colInfo);*/
        }

        private void filterBtn_Click(object sender, RoutedEventArgs e)
        {
            string filterWord = filterTablesTb.Text;

            if (filterWord.Length > 0)
            {
                var filtered = ListFiltering.FilterByWord(sourceTables, filterWord);
                listBoxTables.Items.Clear();
                foreach (var item in filtered)
                {
                    listBoxTables.Items.Add(item);
                }
            }
            else
            {
                listBoxTables.Items.Clear();
                foreach (var item in sourceTables)
                {
                    listBoxTables.Items.Add(item);
                }
            }
        }

        private void filterSelBtn_Click(object sender, RoutedEventArgs e)
        {
            string filterWord = filterSelTablesTb.Text;

            if (filterWord.Length > 0)
            {
                var filtered = ListFiltering.FilterByWord(selectedTables, filterWord);
                listBoxChoosenTable.Items.Clear();
                foreach (var item in filtered)
                {
                    listBoxChoosenTable.Items.Add(item);
                }
            }
            else
            {
                listBoxChoosenTable.Items.Clear();
                foreach (var item in selectedTables)
                {
                    listBoxChoosenTable.Items.Add(item);
                }
            }
        }

        private void blackListBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> blackList = CsvListFiltering.ReadList();

            foreach (ResultBoxView item in listBoxTables.Items)
            {
                if (blackList.Contains(item.colName))
                {
                    var toRemove = sourceTables.Single(t => t.colName == item.colName);
                    sourceTables.Remove(toRemove);
                }
            }
            listBoxTables.Items.Clear();
            foreach (var item in sourceTables)
            {
                listBoxTables.Items.Add(item);
            }

            labelstage.Content = "Selected tables (" + DbConnection.SourceDatabase + "): \n" + (listBoxTables.Items.Count + listBoxChoosenTable.Items.Count) + " / " + listBoxChoosenTable.Items.Count;
        }

        private void whiteListBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> whiteList = CsvListFiltering.ReadList();

            var changedList = new List<ResultBoxView>();
            foreach (ResultBoxView item in listBoxTables.Items)
            {
                if (whiteList.Contains(item.colName))
                {
                    listBoxChoosenTable.Items.Add(item);
                    selectedTables.Add(item);
                    changedList.Add(item);
                }
            }
            foreach (var item in changedList)
            {
                listBoxTables.Items.Remove(item);
                sourceTables.Remove(item);
            }

            buttonNext.IsEnabled = true;
            if (isSAPDb.IsChecked == true)
            {
                processPB.Maximum = listBoxChoosenTable.Items.Count + 2;
            }

            labelstage.Content = "Selected tables (" + DbConnection.SourceDatabase + "): \n" + (listBoxTables.Items.Count + listBoxChoosenTable.Items.Count) + " / " + listBoxChoosenTable.Items.Count;
        }
    }
}
