using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb.Internal;
using AV.Log;
using SystemDb;
using System.IO;
using ViewboxDb;

namespace ViewboxMassTableArchivingTool
{
    /// <summary>
    /// Custom event args, containing archived tables' count.
    /// </summary>
    public class ArchivingStartedEventArgs : EventArgs
    {
        private int m_TableCount;
        /// <summary>
        /// The number of the archived tables.
        /// </summary>
        public int TableCount { get { return m_TableCount; } }

        /// <summary>
        /// Contructor...
        /// </summary>
        /// <param name="p_TableCount">The number of the archived tables.</param>
        public ArchivingStartedEventArgs(int p_TableCount)
        {
            this.m_TableCount = p_TableCount;
        }
    }

    /// <summary>
    /// Custom event args, containing archived table data.
    /// </summary>
    public class TableArchivingEventArgs : EventArgs
    {
        private string m_TableName;
        /// <summary>
        /// The name of the archived table.
        /// </summary>
        public string TableName { get { return m_TableName; } }

        public bool Success { get; set; }

        /// <summary>
        /// Contructor...
        /// </summary>
        /// <param name="p_TableName">The name of the archived table.</param>
        public TableArchivingEventArgs(string p_TableName)
        {
            this.m_TableName = p_TableName;
            this.Success = true;
        }
    }

    public static class ArchiveRestoreLogic
    {
        #region [ Events ]
        public delegate void ArchivingStartedHandler(object sender, ArchivingStartedEventArgs e, object progressCalculator);
        public static event ArchivingStartedHandler ArchivingStarted;

        public delegate void ArchivingFinishedHandler(object sender, EventArgs e, object progressCalculator);
        public static event ArchivingFinishedHandler ArchivingFinished;

        public delegate void TableArchivingStartedHandler(object sender, TableArchivingEventArgs e, object progressCalculator);
        public static event TableArchivingStartedHandler TableArchivingStarted;

        public delegate void TableArchivingFinishedHandler(object sender, TableArchivingEventArgs e, object progressCalculator);
        public static event TableArchivingFinishedHandler TableArchivingFinished;
        #endregion

        private static UTF8Encoding utf8 = new UTF8Encoding();
        
        internal static void ArchiveRestore(StreamReader p_FileStream, ArchiveType p_ArchiveType, Options p_Options,
            IEnumerable<ITableObject> p_Objs, MiniViewboxWrapper p_Vb)
        {
            if (p_FileStream != null)
            {
                try
                {
                    List<string> lines = new List<string>();

                    LogHelper.GetLogger().InfoFormat("Reading file stream");
                    Console.WriteLine("Reading file stream");
                    //using (System.IO.StreamReader fs = new System.IO.StreamReader(filepath, utf8))
                    //{
                    string lineFromStream;
                    while ((lineFromStream = p_FileStream.ReadLine()) != null)
                    {
                        lines.Add(lineFromStream);
                    }
                    //}

                    List<Exception> m_List = new List<Exception>();
                    ArchiveRestore(lines, p_ArchiveType, p_Options, p_Objs, p_Vb, null, m_List);
                }
                catch (Exception ex)
                {
                    LogHelper.GetLogger().Error(ex);
                    Console.WriteLine("Error: {0}", ex.Message);
                    throw;
                }
            }
            else
            {
                //LogHelper.GetLogger().WarnFormat("File NOT exists: {0}", filepath);
                //Console.WriteLine("File NOT exists: {0}", filepath);
                LogHelper.GetLogger().WarnFormat("File stream is null.");
                Console.WriteLine("File stream is null.");
            }
        }


        internal static void ArchiveRestore(List<string> p_TableNames, ArchiveType p_ArchiveType, Options p_Options,
            IEnumerable<ITableObject> p_Objs, MiniViewboxWrapper p_Vb, object p_ProgressCalculator,
            List<Exception> p_ExceptionList)
        {
            if (p_ExceptionList == null)
                p_ExceptionList = new List<Exception>();

            bool archiving = (p_ArchiveType == ArchiveType.Archive);

            if (p_TableNames == null)
            {
                #region [ Raising the start event with race-condition handling ]
                ArchivingStartedHandler eventCopyStartedNull = ArchivingStarted;

                if (eventCopyStartedNull != null)
                    eventCopyStartedNull(null, new ArchivingStartedEventArgs(0), p_ProgressCalculator);
                #endregion

                LogHelper.GetLogger().InfoFormat("Table name list is null!");
                Console.WriteLine("Table name list is null!");

                #region [Raising the finished event with race-condition handling ]
                ArchivingFinishedHandler eventCopyFinishedNull = ArchivingFinished;

                if (eventCopyFinishedNull != null)
                    eventCopyFinishedNull(null, new EventArgs(), p_ProgressCalculator);
                #endregion

                return;
            }

            try
            {
                LogHelper.GetLogger().InfoFormat("Prepare table objects");
                Console.WriteLine("Prepare table objects");

                List<SystemDb.ITableObject> tmpobj = new List<ITableObject>();
                if (p_Options.Invert)
                {
                    foreach (ITableObject obj in p_Objs)
                    {
                        if ((obj.Type == TableType.Table || (p_Options.ViewsEnabled && obj.Type == TableType.View)) &&
                                obj.IsArchived != archiving && !obj.IsUnderArchiving)
                        {
                            if (IsSystemOk(p_Options, obj))
                            {
                                if (!p_TableNames.Any(element => element.ToLower() == obj.TableName.ToLower()))
                                    tmpobj.Add(obj);
                            }
                        }
                    }
                }
                else
                {
                    foreach (ITableObject obj in p_Objs)
                    {
                        if ((obj.Type == TableType.Table || (p_Options.ViewsEnabled && obj.Type == TableType.View)) &&
                                obj.IsArchived != archiving && !obj.IsUnderArchiving)
                        {
                            if (IsSystemOk(p_Options, obj))
                            {
                                if (p_TableNames.Any(element => element.ToLower() == obj.TableName.ToLower()))
                                    tmpobj.Add(obj);
                            }
                        }
                    }
                }

                #region [ Raising the start event with race-condition handling ]
                ArchivingStartedHandler eventCopyStarted = ArchivingStarted;

                if (eventCopyStarted != null)
                    eventCopyStarted(null, new ArchivingStartedEventArgs(tmpobj.Count), p_ProgressCalculator);
                #endregion

                LogHelper.GetLogger().InfoFormat("Table objects to {1}: {0}", tmpobj.Count, p_ArchiveType);
                Console.WriteLine("Table objects to {1}: {0}", tmpobj.Count, p_ArchiveType);
                Console.WriteLine();

                int itemCount = tmpobj.Count;
                int itemIndex = 0;

                foreach (ITableObject obj in tmpobj)
                {
                    itemIndex++;
                    LogHelper.GetLogger().InfoFormat("{0}: {1}.{2}\t{3}/{4}", p_ArchiveType.ToString(), obj.Database,
                                                     obj.TableName, itemIndex, itemCount);
                    Console.WriteLine("{0}: {1}.{2}\t{3}/{4}", p_ArchiveType.ToString(), obj.Database, obj.TableName, itemIndex, itemCount);
                    try
                    {
                        #region [ Raising the start event with race-condition handling ]
                        TableArchivingStartedHandler eventCopyTableStarted = TableArchivingStarted;

                        if (eventCopyTableStarted != null)
                        {
                            TableArchivingEventArgs m_EventArgsStarted = new TableArchivingEventArgs(obj.TableName);

                            eventCopyTableStarted(null, m_EventArgsStarted, p_ProgressCalculator);
                        }
                        #endregion

                        p_Vb.DoArchive(obj, p_ArchiveType);

                        #region [ Raising the start event with race-condition handling ]
                        TableArchivingFinishedHandler eventCopyTableFinished = TableArchivingFinished;

                        if (eventCopyTableFinished != null)
                        {
                            TableArchivingEventArgs m_EventArgsFinished = new TableArchivingEventArgs(obj.TableName);

                            eventCopyTableFinished(null, m_EventArgsFinished, p_ProgressCalculator);
                        }
                        #endregion

                        LogHelper.GetLogger().InfoFormat("Done");
                        Console.WriteLine("Done");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.GetLogger().Error(ex);
                        Console.WriteLine("Error: {0}", ex.Message);
                        p_ExceptionList.Add(ex);
                    }

                    try
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo cki = Console.ReadKey(false);
                            if (cki.Key == ConsoleKey.Escape)
                            {
                                LogHelper.GetLogger().InfoFormat("Cancelled by user");
                                Console.WriteLine("Cancelled by user");

                                break;
                            }
                        }
                    }
                    catch { }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Error(ex);
                Console.WriteLine("Error: {0}", ex.Message);
                p_ExceptionList.Add(ex);

                throw;
            }
            finally
            {
                LogHelper.GetLogger().InfoFormat("Process done! No more step just close the program manually.");
                Console.WriteLine("Process done! No more step just close the program manually.");
            }

            #region [Raising the finished event with race-condition handling ]
            ArchivingFinishedHandler eventCopyFinished = ArchivingFinished;

            if (eventCopyFinished != null)
                eventCopyFinished(null, new EventArgs(), p_ProgressCalculator);
            #endregion
        }

        private static bool IsSystemOk(Options p_Options, ITableObject obj)
        {
            if (obj is View) {
                return ((View) obj).Database.ToLower() == p_Options.System.ToLower();
            }
            else if (obj is Table) {
                return ((Table)obj).Database.ToLower() == p_Options.System.ToLower();
            }
            return false;
        }
    }
}
