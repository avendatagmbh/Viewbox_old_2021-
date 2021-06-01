using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DbAnalyser
{
    class ThreadWorker
    {
        // This event handler is where the time-consuming work is done. 
        /*public void ColumnReadingThread(string tablename)
        {
            // BackgroundWorker worker = sender as BackgroundWorker;
            
            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
            }
            else
            {
                QuerryResult result = DbCommand.ReadingColumnsName(tablename);
                if (result.technicalresult == SqlQuerryResult.Successful)
                {
                    DbCommand.columnnameresult.Clear();
                    foreach (var item in result.tableresult)
                    {
                        DbCommand.columnnameresult.Add(item);
                    }
                }
            }
        }*/


        public void ColumnReadingThread(string tablename)
        {
            QuerryResult result = DbCommand.ReadingColumnsName(tablename);
            if (result.technicalresult == SqlQuerryResult.Successful)
            {
                DbCommand.columnnameresult.Clear();
                foreach (var item in result.tableresult)
                {
                    DbCommand.columnnameresult.Add(item);   
                }
            }
        }

        public void ColumnTypeReadingThread(string tablename, string columname)
        {
            QuerryResult result = DbCommand.ReadingColumnsType(tablename, columname);
            if (result.technicalresult == SqlQuerryResult.Successful)
            {
                DbCommand.columtyperesult.Clear();
                string columntype = ProcessingModul.ProcessColumn(ref result.tableresult);
                DbCommand.columtyperesult.Add(columntype);
            }
        }
    }
}
