using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace DbComparison.Business {
    static class ComparisonResultPdf {
        private static Font font = new Font(Font.FontFamily.HELVETICA);
        private static Font italic = new Font(Font.FontFamily.HELVETICA,Font.DEFAULTSIZE,Font.ITALIC);


        private static PdfPCell StandardCell(string content, Font usedFont) {
            PdfPCell cell = new PdfPCell(new Phrase(content, usedFont));
            cell.Border = PdfPCell.NO_BORDER;
            return cell;
        }

        private static PdfPCell BorderCell(string content, Font usedFont) {
            PdfPCell cell = new PdfPCell(new Phrase(content, usedFont));
            return cell;
        }

        private static PdfPCell BorderCell(string content) {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            return cell;
        }

        private static PdfPCell HeaderCell(string content, Font usedFont) {
            PdfPCell cell = new PdfPCell(new Phrase(content, usedFont));
            cell.BackgroundColor = new BaseColor(240, 240, 240);
            //cell.Border = PdfPCell.NO_BORDER;
            return cell;
        }


        private static PdfPCell StandardCell(string content) {
            return StandardCell(content, font);
        }

        private static PdfPTable StandardTable(int numCols) {
            float[] widths = new float[numCols];
            for (int i = 0; i < numCols; ++i)
                widths[i] = 1.0f / numCols;
            return StandardTable(widths);
        }
        private static PdfPTable StandardTable(float[] widths) {
            PdfPTable table = new PdfPTable(widths);
            table.DefaultCell.Border = PdfPCell.NO_BORDER;
            table.WidthPercentage = 100;
            return table;
        }

        private static void AddParagraph(string caption, PdfGenerator.PdfGenerator pdf) {
            Paragraph p = new Paragraph(caption, pdf.Traits.fontH1);
            p.SpacingBefore = 3;
            pdf.CurrentChapter.Add(p);
            minorNr = 1;
        }

        private static int majorNr=1, minorNr=1;

        private static string GetNumber(){
            return "" + majorNr + "." + minorNr++;
        }

        private static string GetMajorNumber() {
            return ""+ ++majorNr;
        }

        static string[] dbName = new string[2];
        public static void WritePdf(ComparisonResult result, string filename) {
            string db1 = result.DbName[0];
            string db2 = result.DbName[1];
            if (db1 == db2) {
                db1 = result.Hostname1 + "." + db1;
                db2 = result.Hostname2 + "." + db2;
            }
            dbName[0] = db1;
            dbName[1] = db2;

            PdfGenerator.PdfGenerator pdf = new PdfGenerator.PdfGenerator(new PdfGenerator.PdfTraits("", "") { CreateTableOfContents = true });
            
            #region Übersicht
            pdf.AddHeadline("Übersicht");

            int colDatatypeMismatchCount = 0;
            foreach (var pair in result.ColumnMismatchSingle)
                foreach (var mismatch in pair.Value)
                    if (mismatch.Mismatch == ComparisonResult.ColumnInfoMismatch.MismatchType.TypeMismatch) colDatatypeMismatchCount++;

            PdfPTable generalInfoBoth = StandardTable(new float[] { 0.8f, 0.2f });
            generalInfoBoth.AddCell(StandardCell("1.1 Gesamtanzahl der Spalten Typ Mismatches"));
            generalInfoBoth.AddCell(StandardCell("" + colDatatypeMismatchCount));
            pdf.CurrentChapter.Add(generalInfoBoth);

            for (int i = 0; i < 2; ++i) {
                AddParagraph(GetMajorNumber() + " Datenbank " + dbName[i], pdf);
                //pdf.AddSubHeadline("Datenbank " + dbName[i]);
                PdfPTable generalInfo = StandardTable(new float[] { 0.8f, 0.2f });
                generalInfo.AddCell(StandardCell(GetNumber() + " Gesamtanzahl der Tabellen"));
                generalInfo.AddCell(StandardCell("" + result.TableCount[i]));

                generalInfo.AddCell(StandardCell(GetNumber() + " Gesamtanzahl der Spalten"));
                generalInfo.AddCell(StandardCell("" + result.ColumnCount[i]));

                generalInfo.AddCell(StandardCell(GetNumber() + " Fehlende Tabellen"));
                generalInfo.AddCell(StandardCell("" + result.MissingTables[i].Count));

                generalInfo.AddCell(StandardCell(GetNumber() + " Leere Tabellen (die in " + dbName[1 - i] + " nicht leer sind)"));
                generalInfo.AddCell(StandardCell("" + result.EmptyTablesInOne[i].Count));

                generalInfo.AddCell(StandardCell(GetNumber() + " Leere Tabellen"));
                generalInfo.AddCell(StandardCell("" + result.EmptyTablesCount[i]));

                generalInfo.AddCell(StandardCell(GetNumber() + " Gesamtanzahl der fehlenden Spalten"));
                int colErrorCount = 0;
                foreach (var pair in result.ColumnMismatchSingle)
                    foreach(var mismatch in pair.Value)
                        if(mismatch.Db == i && mismatch.Mismatch == ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist) colErrorCount++;
                generalInfo.AddCell(StandardCell("" + colErrorCount));

                pdf.CurrentChapter.Add(generalInfo);
            }
            #endregion Übersicht

            #region MissingTables
            GenerateTableForTableInfoList("Tabellen die in einer DB fehlen",result.MissingTables, pdf);
            #endregion

            #region EmptyTables
            GenerateTableForTableInfoList("Tabellen die in nur einer DB leer sind", result.EmptyTablesInOne, pdf);
            #endregion

            //#region DifferentColumns
            //pdf.AddHeadline("Spalten Mismatch");
            //int id = 1;
            //for (int i = 0; i < 2; ++i) {
            //    pdf.AddSubHeadline("in Datenbank " + dbName[i]);
            //    foreach (var pair in result.ColumnMismatch[i]) {
            //        pdf.AddSubSubHeadline(pair.Key);
            //        PdfPTable table = StandardTable(new float[] { 0.1f, 0.6f, 0.3f });
                    
            //        foreach (var columnInfo in pair.Value) {
            //            table.AddCell(StandardCell("" + id++));
            //            table.AddCell(StandardCell("\t\t\t" + columnInfo.Name));
            //            if(columnInfo.Mismatch == ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist)
            //                table.AddCell(StandardCell("fehlt"));
            //            else
            //                table.AddCell(StandardCell("Datentyp anders: " + columnInfo.DataTypeOwn + "/" + columnInfo.DataTypeOther));
            //        }
            //        pdf.CurrentSection.Add(table);
            //    }

                
            //}
            //#endregion
            #region DifferentColumns
            pdf.AddHeadline("Spalten Mismatch");
            int id = 1;
            foreach (var pair in result.ColumnMismatchSingle) {
                pdf.AddSubHeadline(pair.Key, false);
                PdfPTable table = StandardTable(new float[] { 0.1f, 0.55f, 0.35f });
                table.SpacingBefore = 5;
                table.HeaderRows = 1;
                table.AddCell(HeaderCell("Id", italic)); table.AddCell(HeaderCell("Spaltenname", italic)); table.AddCell(HeaderCell("Typ", italic));


                foreach (var columnInfo in pair.Value) {
                    table.AddCell(BorderCell("" + id++));
                    table.AddCell(BorderCell("\t\t\t" + columnInfo.Name));
                    if (columnInfo.Mismatch == ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist)
                        table.AddCell(BorderCell("fehlt in " + dbName[columnInfo.Db]));
                    else
                        table.AddCell(BorderCell("Datentyp anders: " + columnInfo.DataTypeOwn + "/" + columnInfo.DataTypeOther));
                }
                pdf.CurrentChapter.Add(table);
            }
            #endregion

            #region Columns of Missing tables
            pdf.AddHeadline("Spalteninformationen zu den fehlenden Tabellen");
            id = 1;
            foreach (var pair in result.MissingTableToColumns) {
                if (!result.MissingTablesDict.ContainsKey(pair.Key)) continue;
                pdf.AddSubHeadline(pair.Key, false);
                PdfPTable table = StandardTable(new float[] { 0.1f, 0.55f, 0.35f });
                table.SpacingBefore = 10;
                table.HeaderRows = 1;
                table.AddCell(HeaderCell("Id", italic)); table.AddCell(HeaderCell("Spaltenname", italic)); table.AddCell(HeaderCell("Datentyp", italic));

                foreach (var columnInfo in pair.Value) {
                    table.AddCell(BorderCell("" + id++));
                    table.AddCell(BorderCell("\t\t\t" + columnInfo.Name));
                    table.AddCell(BorderCell(columnInfo.DataType));
                }
                pdf.CurrentChapter.Add(table);
            }
            #endregion

            pdf.WriteFile(filename, "Datenbank-Vergleichs-Ergebnisse");
        }

        private static void GenerateTableForTableInfoList(string header, List<ComparisonResult.TableInfoMismatch>[] tableInfos, PdfGenerator.PdfGenerator pdf) {
            int id = 1;
            pdf.AddHeadline(header);
            
            for (int i = 0; i < 2; ++i) {
                pdf.AddSubHeadline("in " + dbName[i]);
                PdfPTable table = StandardTable(new float[] { 0.1f, 0.55f, 0.35f });
                table.SpacingBefore = 10;
                table.AddCell(HeaderCell("Nr", italic));
                table.AddCell(HeaderCell("Tabellenname", italic));
                table.AddCell(HeaderCell("Einträge in " + dbName[1 - i], italic));
                table.HeaderRows = 1;

                foreach (var tableInfo in tableInfos[i]) {
                    table.AddCell(BorderCell("" + id++));
                    table.AddCell(BorderCell(tableInfo.Name));
                    table.AddCell(BorderCell("" + tableInfo.CountOther));
                }
                pdf.CurrentChapter.Add(table);
            }
        }
    }
}
