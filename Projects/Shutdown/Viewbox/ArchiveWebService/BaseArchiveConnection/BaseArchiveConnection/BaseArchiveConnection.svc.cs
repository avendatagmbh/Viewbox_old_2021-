using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using SystemDb;
using ArchiveWebServiceInterface;
using DbAccess;
using DbAccess.Structures;
using Ionic.Zip;
using ViewboxDb;
using ViewboxDb.Filters;

namespace BaseArchiveConnection {
    public class BaseArchiveConnection : IArchiveWebService {
        public string GetPath(string id, string additional = null) {
            int tableId = 0;
            List<string> idColumnNames = new List<string>();
            string documentDirectory = String.Empty;
            string temporaryDirectory = String.Empty;
            int userId = 0;

            if (!String.IsNullOrEmpty(additional)) {
                var dict = Xml.ReadXmlString(additional);
                if (dict.ContainsKey("tableId")) tableId = Convert.ToInt32(dict["tableId"].First());
                if (dict.ContainsKey("idColumnName")) idColumnNames = dict["idColumnName"];
                if (dict.ContainsKey("documentDirectory")) documentDirectory = dict["documentDirectory"].First();
                if (dict.ContainsKey("temporaryDirectory")) temporaryDirectory = dict["temporaryDirectory"].First();
                if (dict.ContainsKey("userId")) userId = Convert.ToInt32(dict["userId"].First());
            }

            var dbConfig = new DbConfig(ConfigurationManager.AppSettings["DataProvider"]);
            dbConfig.ConnectionString = ConfigurationManager.ConnectionStrings["ViewboxDatabase"].ConnectionString;
            
            var files = new List<string>();

            using (IDatabase connection = ConnectionManager.CreateConnection(dbConfig)) {
                connection.Open();

                var sql = String.Empty;

                var table = SystemDb.SystemDb.GetArchiveById(connection, tableId);

                var filter = String.Empty;
                var ids = id.Split(new string[]{ "<DIV>" }, StringSplitOptions.None);
                for (int i = 0; i < ids.Length; i++) {
                    if (!String.IsNullOrEmpty(filter)) filter += " AND ";
                    filter += connection.Enquote(idColumnNames[i]) + "=" + ids[i];
                }

                if (table.Columns.Contains("url")) {
                    sql = "SELECT url FROM " + connection.Enquote(table.Database, table.TableName) + " WHERE " + filter + ";";
                } else if (table.Columns.Contains("path")) {
                    sql = "SELECT path FROM " + connection.Enquote(table.Database, table.TableName) + " WHERE " + filter + ";";
                } else {
                    return null;
                }

                var reader = connection.ExecuteReader(sql);
                var dbTable = new DataTable();
                dbTable.Load(reader);

                for(int i = 0; i < dbTable.Rows.Count; i++) files.Add(dbTable.Rows[i][0].ToString());
            }

            var newPath = temporaryDirectory + (temporaryDirectory.EndsWith("\\") ? "" : "\\") + userId + "\\" + BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", String.Empty) + ".zip";
            
            var fileName = Path.GetFileName(newPath);

            if (newPath.EndsWith(fileName)) newPath = newPath.Substring(0, newPath.Length - fileName.Length);

            Directory.CreateDirectory(newPath);

            newPath += fileName;

            using (var zipstream = new ZipOutputStream(newPath)) {
                zipstream.EnableZip64 = Zip64Option.AsNecessary;
                
                foreach (var file in files) {
                    var newFile = (documentDirectory.EndsWith("\\") && file.StartsWith("\\")
                                        ? file.Substring(1)
                                        : file);
                    var path = documentDirectory + (file.StartsWith("\\") ? "" : "\\") + newFile;
                    zipstream.PutNextEntry(Path.GetFileName(path));
                    
                    byte[] bytes;
                    using (var memStream = new MemoryStream()) {
                        using (var fileStream = new FileStream(path, FileMode.Open)) {
                            fileStream.CopyTo(memStream);
                            bytes = memStream.GetBuffer();
                        }
                    }

                    zipstream.Write(bytes, 0, bytes.Length);
                }   

                zipstream.Flush();
            }

            return newPath;
        }

        public List<Document> GetDocuments(IFilter filter, SortCollection sort, long from = 0, long to = 35, string additional = null) {
            //TODO: filter integrieren!

            string indexValue = null;
            string splitValue = null;
            string sortValue = null;
            int tableId = 0;
            List<string> idColumnNames = new List<string>();
            List<string> thumbnailColumnNames = new List<string>();
            List<string> descriptionColumnNames = new List<string>();
            List<string> typdescriptionColumnNames = new List<string>();
            List<string> typkuerzelColumnNames = new List<string>();
            string dateColumnName = null;

            if (!String.IsNullOrEmpty(additional)) {
                var dict = Xml.ReadXmlString(additional);
                if (dict.ContainsKey("indexValue")) indexValue = dict["indexValue"].First();
                if (dict.ContainsKey("splitValue")) splitValue = dict["splitValue"].First();
                if (dict.ContainsKey("sortValue")) sortValue = dict["sortValue"].First();
                if (dict.ContainsKey("tableId")) tableId = Convert.ToInt32(dict["tableId"].First());
                if (dict.ContainsKey("idColumnName")) idColumnNames = dict["idColumnName"];
                if (dict.ContainsKey("thumbnailColumnName")) thumbnailColumnNames = dict["thumbnailColumnName"];
                if (dict.ContainsKey("descriptionColumnName")) descriptionColumnNames = dict["descriptionColumnName"];
                if (dict.ContainsKey("typdescriptionColumnName")) typdescriptionColumnNames = dict["typdescriptionColumnName"];
                if (dict.ContainsKey("typkuerzelColumnName")) typkuerzelColumnNames = dict["typkuerzelColumnName"];
                if (dict.ContainsKey("dateColumnName")) dateColumnName = dict["dateColumnName"].First();
            }

            var dbConfig = new DbConfig(ConfigurationManager.AppSettings["DataProvider"]);
            dbConfig.ConnectionString = ConfigurationManager.ConnectionStrings["ViewboxDatabase"].ConnectionString;

            using (IDatabase connection = ConnectionManager.CreateConnection(dbConfig)) {
                connection.Open();

                var table = SystemDb.SystemDb.GetArchiveById(connection, tableId);

                var columns = new List<string>();
                columns.AddRange(idColumnNames);
                columns.AddRange(thumbnailColumnNames);
                columns.AddRange(descriptionColumnNames);
                columns.AddRange(typdescriptionColumnNames);
                columns.AddRange(typkuerzelColumnNames);
                columns.Add(dateColumnName);
                columns = columns.Distinct().ToList();

                string cols = String.Join(", ", from c in columns where !String.IsNullOrEmpty(c) select connection.Enquote(c));
                //if (cols.Length > 0)
                //    cols = "_row_no_, " + cols;
                //else
                //    cols = "_row_no_";

                var filterList = new List<string>();
                if (filter != null) {
                    //TODO: filter integrieren!
                }

                if (table.IndexTableColumn != null) filterList.Add(connection.Enquote(table.IndexTableColumn.Name) + "=" + connection.GetSqlString(indexValue));
                if (table.SplitTableColumn != null) filterList.Add(connection.Enquote(table.SplitTableColumn.Name) + "=" + connection.GetSqlString(splitValue));
                if (table.SortColumn != null) filterList.Add(connection.Enquote(table.SortColumn.Name) + "=" + connection.GetSqlString(sortValue));

                //String.Join(" AND ", filterList)

                

                long limit = to - from;

                //Order Areas auslesen
                var areas = new List<Tuple<long, long>>(from o in table.OrderAreas.GetRanges(indexValue, splitValue, sortValue) select o);
                long count = 0;
                long skip = from;
                var limits = new List<string>();
                foreach (var o in areas) {
                    if (limits.Count == 0) {
                        skip -= o.Item2 - o.Item1 + 1;
                        if (skip > 0) {
                            from = skip;
                            continue;
                        }
                    } else from = 0;

                    long size = Math.Min(limit - count, o.Item2 - o.Item1 + 1 - from);
                    limits.Add(String.Format("{0} BETWEEN {1} AND {2}", connection.Enquote("_row_no_"), o.Item1 + from, o.Item1 + size - 1 + from));
                    if ((count += size) == limit) break;
                }

                if (limits.Count == 0) limits.Add(String.Format("{0} BETWEEN {1} AND {2}", connection.Enquote("_row_no_"), from + 1, from + limit));
                //Order Areas auslesen

                string where = String.Format(" WHERE {0}", String.Join(" OR ", limits));

                string orderBy = String.Empty;
                //TODO: sort einbauen
                //if (sortColumnId > 0) {
                //    foreach (var col in table.Columns) {
                //        if (col.Name == "path" || col.Name == "thumbnail") continue;
                //        if (col.Id == sortColumnId) {
                //            orderBy += col.Name;
                //            orderBy += direction;
                //            break;
                //        }
                //    }
                //}
                if (orderBy.Length > 0)
                    orderBy = " ORDER BY " + orderBy;

                string sql = "SELECT " + cols + " FROM " + connection.Enquote(table.Database, table.TableName) + where + orderBy;
                var dataTable = connection.GetDataTable(sql);

                var documentList = new List<Document>();

                for (int i = 0; i < dataTable.Rows.Count; i++) {
                    var document = new Document();
                    if (idColumnNames.Count > 0) document.Id = String.Join("<DIV>", from id in idColumnNames select dataTable.Rows[i][id]);
                    if (thumbnailColumnNames.Count > 0) document.HasThumbnail = !String.IsNullOrEmpty(String.Join("", from thumbnail in thumbnailColumnNames select dataTable.Rows[i][thumbnail]));
                    if (descriptionColumnNames.Count > 0) document.Description = String.Join(" ", from description in descriptionColumnNames select dataTable.Rows[i][description]);
                    if (typdescriptionColumnNames.Count > 0) document.TypeDescription = String.Join(" ", from description in typdescriptionColumnNames select dataTable.Rows[i][description]);
                    if (typkuerzelColumnNames.Count > 0) document.TypeAbbreviation = String.Join(" ", from typ in typkuerzelColumnNames select dataTable.Rows[i][typ]);
                    if (!String.IsNullOrEmpty(dateColumnName)) {
                        DateTime date;
                        DateTime.TryParse(dataTable.Rows[i][dateColumnName].ToString(), out date);
                        document.Date = date;
                    }

                    documentList.Add(document);
                }

                return documentList;
            }
        }

        public long GetCount(IFilter filter, string additional = null) {
            //TODO: search integrieren!
            string indexValue = null;
            string splitValue = null;
            string sortValue = null;
            int tableId = 0;
            List<string> idColumnNames = new List<string>();

            if (!String.IsNullOrEmpty(additional)) {
                var dict = Xml.ReadXmlString(additional);
                if (dict.ContainsKey("indexValue")) indexValue = dict["indexValue"].First();
                if (dict.ContainsKey("splitValue")) splitValue = dict["splitValue"].First();
                if (dict.ContainsKey("sortValue")) sortValue = dict["sortValue"].First();
                if (dict.ContainsKey("tableId")) tableId = Convert.ToInt32(dict["tableId"].First());
                if (dict.ContainsKey("idColumnName")) idColumnNames = dict["idColumnName"]; 
            }

            var dbConfig = new DbConfig(ConfigurationManager.AppSettings["DataProvider"]);
            dbConfig.ConnectionString = ConfigurationManager.ConnectionStrings["ViewboxDatabase"].ConnectionString;

            using (IDatabase connection = ConnectionManager.CreateConnection(dbConfig)) {
                connection.Open();

                var table = SystemDb.SystemDb.GetArchiveById(connection, tableId);

                var filterList = new List<string>();
                if (filter != null) {
                    //TODO: filter integrieren!
                }

                if (table.IndexTableColumn != null) filterList.Add(connection.Enquote(table.IndexTableColumn.Name) + "=" + connection.GetSqlString(indexValue));
                if (table.SplitTableColumn != null) filterList.Add(connection.Enquote(table.SplitTableColumn.Name) + "=" + connection.GetSqlString(splitValue));
                if (table.SortColumn != null) filterList.Add(connection.Enquote(table.SortColumn.Name) + "=" + connection.GetSqlString(sortValue));

                if(idColumnNames.Count > 0) return connection.CountDistinctedTableWithFilter(table.Database, table.TableName, idColumnNames, String.Join(" AND ", filterList));
                
                return connection.CountTableWithFilter(table.Database, table.TableName, String.Join(" AND ", filterList));
            }
        }

        public Dictionary<string, string> GetTypes(string additional) {
            int tableId = 0;
            List<string> typdescriptionColumnNames = new List<string>();
            List<string> typkuerzelColumnNames = new List<string>();

            if (!String.IsNullOrEmpty(additional)) {
                var dict = Xml.ReadXmlString(additional);
                if (dict.ContainsKey("tableId")) tableId = Convert.ToInt32(dict["tableId"].First());
                if (dict.ContainsKey("typdescriptionColumnName")) typdescriptionColumnNames = dict["typdescriptionColumnName"];
                if (dict.ContainsKey("typkuerzelColumnName")) typkuerzelColumnNames = dict["typkuerzelColumnName"];
            }

            var dbConfig = new DbConfig(ConfigurationManager.AppSettings["DataProvider"]);
            dbConfig.ConnectionString = ConfigurationManager.ConnectionStrings["ViewboxDatabase"].ConnectionString;

            using (IDatabase connection = ConnectionManager.CreateConnection(dbConfig)) {
                connection.Open();

                var table = SystemDb.SystemDb.GetArchiveById(connection, tableId);

                var columns = new List<string>();
                columns.AddRange(typdescriptionColumnNames);
                columns.AddRange(typkuerzelColumnNames);
                columns = columns.Distinct().ToList();

                string cols = String.Join(", ", from c in columns where !String.IsNullOrEmpty(c) select connection.Enquote(c));

                string sql = "SELECT DISTINCT " + cols + " FROM " + connection.Enquote(table.Database, table.TableName);
                var dataTable = connection.GetDataTable(sql);

                var dict = new Dictionary<string, string>();

                for (int i = 0; i < dataTable.Rows.Count; i++) {
                    var typ = String.Join(" ", from t in typkuerzelColumnNames select dataTable.Rows[i][t]);
                    var description = typdescriptionColumnNames!= null && typdescriptionColumnNames.Count > 0 ? String.Join(" ", from d in typdescriptionColumnNames select dataTable.Rows[i][d]) : typ;

                    dict[typ] = description;
                }

                return dict;
            }
        }
    }
}
