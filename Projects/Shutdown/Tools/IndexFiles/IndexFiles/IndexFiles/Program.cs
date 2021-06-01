using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AV.Log;
using DbAccess;
using DbAccess.DbSpecific.MySQL;
using DbAccess.Structures;
using System.IO;
using log4net;
using System.Data;

namespace IndexFiles
{
    class Program
    {
        private static ILog log = LogHelper.GetLogger();

        static void Main(string[] args)
        {
            const string sep = "----------------------------------------------------------------";
            const string paramReadSql = " select distinct c.id, p.database_name, p.table_name, p.column_name "
                                            + " from parameter p "
                                            + "  join tables t on t.database = p.database_name and t.name = p.table_name "
                                            + "  join columns c on c.table_id = t.id and c.name = p.column_name ";
            const string checkSql = "select table_name, table_rows, data_length "
                + " from information_Schema.tables "
                + " where table_schema = '{0}' and (table_name like 'index%' or table_name like 'value%')";



            IDatabase fromConn = null;
            IDatabase toConn = null;
            bool canStart = true;
            try
            {

                string[] configLines = System.IO.File.ReadAllLines(@"Config.txt");
                string[] tableLines = System.IO.File.ReadAllLines(@"Tables.txt");
                
                Console.WriteLine(sep);
                Console.WriteLine(sep);
                Console.WriteLine("                        Index table merge");
                Console.WriteLine(sep);
                Console.WriteLine(sep);
                Console.WriteLine();
                Console.WriteLine();
                string[] fromData = configLines[0].Split(';');
                string[] toData = configLines[1].Split(';');

                string fromServer = fromData[0];
                string fromDb = fromData[1];
                string fromFolder = fromData[2];

                

                string toServer = toData[0];
                string toDb = toData[1];
                string toFolder = toData[2];

                Console.WriteLine("FromServer: " + fromServer);
                Console.WriteLine("FromDb: " + fromDb);
                
                Console.WriteLine("Connecting to from db...");
                try
                {
                    fromConn = new Database(new DbConfig("MySQL"){Hostname = fromServer, Username = "root", Password="avendata", DbName = fromDb});
                    fromConn.Open();
                    Console.WriteLine("Connection established.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                
                Console.WriteLine("FromFolder: " + fromFolder);
                if (!Directory.Exists(fromFolder))
                    Console.WriteLine("Directory don't exists");
                Console.WriteLine();

                Console.WriteLine("ToServer: " + toServer);
                Console.WriteLine("ToDb: " + toDb);
                
                Console.WriteLine("Connecting to to db...");
                try
                {
                    toConn = new Database(new DbConfig("MySQL") { Hostname = toServer, Username = "root", Password = "avendata", DbName = toDb });
                    toConn.Open();
                    Console.WriteLine("Connection established.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                } 
                
                Console.WriteLine("ToFolder: " + toFolder);
                if (!Directory.Exists(fromFolder))
                    Console.WriteLine("Directory don't exists");

                Console.WriteLine();
                Console.WriteLine(sep);
                Console.WriteLine();
                Console.WriteLine("Can we start?");
                Console.ReadLine();

                


                var fromParams = new List<Tuple<int, string, string, string>>();
                var fromCheck = new List<Tuple<string, long, long>>(); 
                var toParams = new List<Tuple<int, string, string, string>>();
                var toCheck = new List<Tuple<string, long, long>>();

                Console.WriteLine("Load parameters from fromDb");

                using (IDataReader reader = fromConn.ExecuteReader(paramReadSql))
                {
                    while (reader.Read())
                    {
                        fromParams.Add(new Tuple<int, string, string, string>(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                    }
                }

                Console.WriteLine("Load table infos from fromDb-index");
                using (IDataReader reader = fromConn.ExecuteReader(String.Format(checkSql, fromDb + "_index")))
                {
                    while (reader.Read())
                    {
                        fromCheck.Add(new Tuple<string, long, long>(reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2)));
                    }
                }

                Console.WriteLine("Load parameters from toDb");
                using (IDataReader reader = toConn.ExecuteReader(paramReadSql))
                {
                    while (reader.Read())
                    {
                        toParams.Add(new Tuple<int, string, string, string>(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                    }
                }

                Console.WriteLine("Load table infos from toDb-index");
                using (IDataReader reader = toConn.ExecuteReader(String.Format(checkSql, toDb + "_index")))
                {
                    while (reader.Read())
                    {
                        toCheck.Add(new Tuple<string, long, long>(reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2)));
                    }
                }

                Console.WriteLine("Start processing");
                foreach(var tab in tableLines)
                {
                    Console.WriteLine("Processing " + tab + "...");
                    var tabSplit = tab.ToLower().Split(';');

                    var fromList = fromParams.Where(w => w.Item2.ToLower() == tabSplit[0] && w.Item3.ToLower() == tabSplit[1]);
                    
                    Console.WriteLine();
                    foreach(var fromPa in fromList)
                    {
                        Console.Write("\t" + fromPa.Item4 + "\t");
                        var toPa = toParams.FirstOrDefault(w => w.Item2.ToLower() == tabSplit[0] && w.Item3.ToLower() == tabSplit[1] && w.Item4.ToLower() == fromPa.Item4.ToLower());
                        if (toPa == null)
                        {
                            Console.WriteLine(" failed.");
                        }
                        else
                        {
                            var toCheckIndexItem = toCheck.FirstOrDefault(w => w.Item1.ToLower() == "index_" + toPa.Item1);
                            var toCheckValueItem = toCheck.FirstOrDefault(w => w.Item1.ToLower() == "index_" + toPa.Item1);
                            
                            var fromCheckIndexItem = fromCheck.FirstOrDefault(w => w.Item1.ToLower() == "index_" + fromPa.Item1);
                            var fromCheckValueItem = fromCheck.FirstOrDefault(w => w.Item1.ToLower() == "index_" + fromPa.Item1);

                            if (fromCheckIndexItem == null || fromCheckValueItem == null)
                            {
                                Console.WriteLine(" failed - from table not found.");
                                continue;
                            }

                            Console.Write(fromPa.Item1 + " -> " + toPa.Item1);

                            if (toCheckIndexItem != null && toCheckValueItem != null)
                            {
                                if (fromCheckIndexItem.Item2 == toCheckIndexItem.Item2 &&
                                    fromCheckIndexItem.Item3 == toCheckIndexItem.Item3 &&
                                    fromCheckValueItem.Item2 == toCheckValueItem.Item2 &&
                                    fromCheckValueItem.Item3 == toCheckValueItem.Item3)
                                {
                                    Console.WriteLine(" same tables - copy is not needed");
                                    continue;
                                }
                            }
                            
                            

                            string fromIndexFileName = fromFolder + "index_" + fromPa.Item1;
                            string toIndexFileName = toFolder + "index_" + toPa.Item1;
                            string fromValueFileName = fromFolder + "value_" + fromPa.Item1;
                            string toValueFileName = toFolder + "value_" + toPa.Item1;

                            File.Copy(fromIndexFileName + ".frm", toIndexFileName + ".frm");
                            File.Copy(fromIndexFileName + ".myi", toIndexFileName + ".myi");
                            File.Copy(fromIndexFileName + ".myd", toIndexFileName + ".myd");

                            File.Copy(fromValueFileName + ".frm", toValueFileName + ".frm");
                            File.Copy(fromValueFileName + ".myi", toValueFileName + ".myi");
                            File.Copy(fromValueFileName + ".myd", toValueFileName + ".myd");

                            Console.WriteLine(" copy ready");
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine(tab + "finished.");
                }
            }
            finally
            {
                if (fromConn != null)
                {
                    fromConn.Dispose();
                }
                if (toConn != null)
                {
                    toConn.Dispose();
                }
            }

            

            Console.WriteLine("Press any key to continue!");
            Console.ReadLine();
        }
    }
}
