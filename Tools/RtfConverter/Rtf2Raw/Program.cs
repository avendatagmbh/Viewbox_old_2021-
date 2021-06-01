using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;
using DbAccess.Structures;
using Itenso.Rtf.Support;
using Itenso.Rtf.Parser;
using Itenso.Rtf.Converter.Text;

namespace Itenso.Solutions.Community.Rtf2Raw{
	class Program{
		static void Main(){
            using (var db = ConnectionManager.CreateConnection(new DbConfig("SQLServer") { ConnectionString = "Data Source=edna.av.local;Initial Catalog=temp_alpha_innotec_20120801;User ID=sa;Password=avendata" })) {
                //CONSTANTS
                //var columns = new List<string> { "artdesl1ex", "artdesl2ex", "artdesl3ex", "artmemo" };
                var columns = new List<string> { "adrmemo" };
                var table = "MLA001";
                //CONSTANTS

                db.Open();

                var sql = String.Format("SELECT * FROM {0};", db.Enquote(table));
                var reader = db.ExecuteReader(sql);

                var dbTable = new DataTable();
                dbTable.Load(reader);

                var textConvertSettings = new RtfTextConvertSettings();
                textConvertSettings.IsShowHiddenText = true;
                var textConverter = new RtfTextConverter(textConvertSettings);

                for (int i = 0; i < dbTable.Rows.Count; i++) {
                    var values = new List<string>();
                    var filter = db.Enquote("_row_no_") + "=" + (i + 1);

                    for (int j = 0; j < dbTable.Columns.Count; j++) {
                        if (columns.Contains(dbTable.Columns[j].ColumnName.ToLower())) {
                            var rtfText = dbTable.Rows[i][j].ToString();

                            var text = rtfText;

                            if (!String.IsNullOrEmpty(rtfText)) {
                                try {
                                    var structureBuilder = new RtfParserListenerStructureBuilder();
                                    var parser = new RtfParser();
                                    parser.AddParserListener(structureBuilder);
                                    parser.Parse(new RtfSource(rtfText));

                                    RtfInterpreterTool.Interpret(structureBuilder.StructureRoot, null, textConverter,
                                                                 null);

                                    text = textConverter.PlainText;
                                }catch(Exception e) {
                                    
                                }
                                values.Add(db.Enquote(dbTable.Columns[j].ColumnName) + "=" + db.GetSqlString(text));
                            }

                        }
                    }
                    if (values.Count > 0) {
                        db.ExecuteNonQuery(
                            String.Format("UPDATE {0} SET {1} WHERE {2};",
                                          db.Enquote(table),
                                          String.Join(",", values), filter));
                    }
                }
            }
		}
	}
}