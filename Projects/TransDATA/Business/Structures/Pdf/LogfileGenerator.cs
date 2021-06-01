using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Business.Interfaces;
using Config.Interfaces.DbStructure;
using Config.Structures;
using Logging;
using PdfGenerator;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SystemDrawingImage = System.Drawing.Image;

namespace Business.Structures.Pdf {
    public class LogfileGenerator {
        #region Constructor
        public LogfileGenerator(IDataTransferAgent transferAgent, string profileName) {
            _transferAgent = transferAgent;
            _profileName = profileName;
        }
        #endregion Constructor

        #region Properties
        private readonly IDataTransferAgent _transferAgent;
        private readonly Font _fontFirstPage = new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL);
        private readonly Font _fontBold = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
        private string _profileName;
        #endregion Properties

        #region Methods
        public FileInfo GenerateLogFile(LoggingDb log, List<ITransferEntity> Entities, bool writeCurrentStatus) {
            string tempFile = Path.GetTempPath() + Base.Localisation.ResourcesCommon.LogFile +
                              DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".pdf";

            var pdf = new PdfGenerator.PdfGenerator(new PdfTraits("TransDATA", "TransDATA"));

            var headerPhrase = new Phrase(Base.Localisation.ResourcesCommon.MailDocumentFinished, pdf.Traits.fontH1);
            int spacingBefore = 10, spacingAfter = 15;

            if (writeCurrentStatus) {
                // tables current status
                pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentCurrentStatus);
                pdf.AddTable(GetTablesStatusDataTable(), false, spacingBefore, spacingAfter);
            }

            // tables with ok status
            var table = GetTablesDataTable(_transferAgent.TransferProgress.EntityFinishedList, log, TransferStates.TransferedOk);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesOk + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with count difference status
            table = GetTablesDataTable(_transferAgent.TransferProgress.EntityFinishedList, log, TransferStates.TransferedCountDifference);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesCountDifference + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with error status
            table = GetTablesDataTable(_transferAgent.TransferProgress.EntityFinishedList, log, TransferStates.TransferedError);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesError + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with not transfered status
            table = GetTablesDataTable(Entities, log, TransferStates.NotTransfered);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesNotTransfered + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            var systemImage =
                    System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "Business.Resources.watermark.png");
            Image watermark = Image.GetInstance(systemImage);
            using (var outStream = new FileStream(tempFile, FileMode.Create)) {
                pdf.WriteFile(CreateFirstPage(), outStream, watermark);
            }
            
            return new FileInfo(tempFile);
            
        }

        public FileInfo GenerateLogFile(LoggingDb log, List<ITransferEntity> Entities) {
            string tempFile = Path.GetTempPath() + Base.Localisation.ResourcesCommon.LogFile +
                              DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".pdf";

            var pdf = new PdfGenerator.PdfGenerator(new PdfTraits("TransDATA", "TransDATA"));

            var headerPhrase = new Phrase(Base.Localisation.ResourcesCommon.MailDocumentFinished, pdf.Traits.fontH1);
            int spacingBefore = 10, spacingAfter = 15;

            // tables with ok status
            var table = GetTablesDataTable(Entities, log, TransferStates.TransferedOk);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesOk + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with count difference status
            table = GetTablesDataTable(Entities, log, TransferStates.TransferedCountDifference);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesCountDifference + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with error status
            table = GetTablesDataTable(Entities, log, TransferStates.TransferedError);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesError + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            // tables with not transfered status
            table = GetTablesDataTable(Entities, log, TransferStates.NotTransfered);
            pdf.AddHeadline(Base.Localisation.ResourcesCommon.MailDocumentTablesNotTransfered + " (" + table.Rows.Count + ")");
            pdf.AddTable(table, true, spacingBefore, spacingAfter);

            var systemImage =
                    System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "Business.Resources.watermark.png");
            Image watermark = Image.GetInstance(systemImage);
            using (var outStream = new FileStream(tempFile, FileMode.Create)) {
                pdf.WriteFile(CreateFirstPage(), outStream, watermark);
            }

            return new FileInfo(tempFile);
        }

        private PdfReader CreateFirstPage() {
            //string[] upTableRows = new string[4] { "Bearbeiter:" + firstPageDatas[1], "Versionsnummer: " + firstPageDatas[2], "Projektnummer: " + firstPageDatas[3], attachmentName[attachmentNumber] };
            //string[] downTableRows = new string[2] { attachmentName[attachmentNumber], firstPageDatas[0] };

            Document firstPageDoc = new Document(PageSize.A4.Rotate());
            MemoryStream mstream = new MemoryStream();
            PdfWriter firstPageWriter = PdfWriter.GetInstance(firstPageDoc, mstream);
            firstPageDoc.Open();
            //firstPageDoc.Add(watermark);
            firstPageDoc.Add(FirstPageUpTableCreator(new string[0]));
            firstPageDoc.Add(FirstPageCenterTableCreator());
            //firstPageDoc.Add(FirstPageDownTableCreator(downTableRows));
            firstPageDoc.Close();
            firstPageWriter.Close();
            return new PdfReader(mstream.GetBuffer());
        }

        PdfPCell NoBorderCell(string text, Font font, int colSpan=1) {
            PdfPCell pdfCell = new PdfPCell();
            pdfCell.Border = Rectangle.NO_BORDER;
            pdfCell.Phrase = new Phrase(text, font);
            pdfCell.Colspan = colSpan;
            return pdfCell;
        }

        private PdfPTable FirstPageCenterTableCreator() {
            PdfPTable result = new PdfPTable(2){HorizontalAlignment = Element.ALIGN_CENTER};
            Font descriptionFont = new Font(Font.FontFamily.HELVETICA, Font.DEFAULTSIZE, Font.BOLD);
            Font valueFont = new Font(Font.FontFamily.HELVETICA, Font.DEFAULTSIZE);
            result.AddCell(NoBorderCell("Transdata Transferlog", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD), 2));
            result.AddCell(NoBorderCell("Profil:", descriptionFont));
            result.AddCell(NoBorderCell(_profileName, valueFont));
            result.AddCell(NoBorderCell("Start:", descriptionFont));
            result.AddCell(
                NoBorderCell((_transferAgent == null ? string.Empty : _transferAgent.TransferStartTime.ToString()),
                             valueFont));
            result.AddCell(NoBorderCell("Ende:", descriptionFont));
            result.AddCell(NoBorderCell(DateTime.Now.ToString(), valueFont));

            result.AddCell(NoBorderCell("Eingabedaten:", descriptionFont));
            result.AddCell(
                NoBorderCell(
                    (_transferAgent == null ? string.Empty : _transferAgent.GetFirstInputAgent().GetDescription()),
                    valueFont));

            result.AddCell(NoBorderCell("Ausgabedaten:", descriptionFont));
            result.AddCell(
                NoBorderCell(
                    (_transferAgent == null ? string.Empty : _transferAgent.GetFirstOutputAgent().GetDescription()),
                    valueFont));
            return result;
        }

        private PdfPTable FirstPageUpTableCreator(string[] rows) {
            var avenData = new string[6] { "AvenDATA GmbH", "Salzufer 8 - 10587 Berlin", "Telefon: +49 30 700 157 500", "Telefax: +49 30 700 157 599", "info@avendata.de", "www.avendata.de" };
            PdfPTable firstPageTable = new PdfPTable(3);
            float[] widthsHeader = new float[] { 170, 272, 150 };
            firstPageTable.LockedWidth = true;
            firstPageTable.SetTotalWidth(widthsHeader);

            PdfPCell pdfCell = new PdfPCell();
            pdfCell.PaddingBottom = 0;
            pdfCell.PaddingTop = 100;
            pdfCell.Border = Rectangle.NO_BORDER;
            pdfCell.VerticalAlignment = Element.ALIGN_BOTTOM;

            pdfCell.Phrase = new Phrase(avenData[0], _fontBold);
            pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            firstPageTable.AddCell(pdfCell);

            pdfCell.Phrase = new Phrase(avenData[1], _fontFirstPage);
            pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
            firstPageTable.AddCell(pdfCell);

            pdfCell.Phrase = new Phrase(avenData[0], _fontFirstPage);
            firstPageTable.AddCell(pdfCell);

            firstPageTable = AddCellToFirstPageUpTable(firstPageTable, avenData[1], 5);
            firstPageTable = AddCellToFirstPageUpTable(firstPageTable, avenData[2], 0);
            firstPageTable = AddCellToFirstPageUpTable(firstPageTable, avenData[3], 5);
            firstPageTable = AddCellToFirstPageUpTable(firstPageTable, avenData[4], 0);
            firstPageTable = AddCellToFirstPageUpTable(firstPageTable, avenData[5], 10);

            for (int i = 0; i < rows.Count(); i++) {
                firstPageTable = AddCellToFirstPageUpTable(firstPageTable, rows[i], 0);
            }
            return firstPageTable;
        }

        private PdfPTable AddCellToFirstPageUpTable(PdfPTable firstPageTable, string row, int paddingBottom) {
            PdfPCell pdfCell = new PdfPCell();
            pdfCell.PaddingBottom = paddingBottom;
            pdfCell.Border = Rectangle.NO_BORDER;
            PdfPCell cellEmpty = new PdfPCell(new Phrase(string.Empty));
            cellEmpty.Border = Rectangle.NO_BORDER;

            firstPageTable.AddCell(cellEmpty);
            firstPageTable.AddCell(cellEmpty);
            pdfCell.Phrase = new Phrase(row, _fontFirstPage);
            firstPageTable.AddCell(pdfCell);
            return firstPageTable;
        }


        private DataTable GetTablesDataTable(List<ITransferEntity> entities, LoggingDb log, TransferStates state) {
            var table = new DataTable();

            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentCatalog, typeof(string));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentSchema, typeof(string));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentTable, typeof(string));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentCountSource, typeof(long));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentCountDestination, typeof(long));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentDuration, typeof(string));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentFilter, typeof(string));
            table.Columns.Add(Base.Localisation.ResourcesCommon.MailDocumentComment, typeof(string));

            var transferEntities = entities.ToList();
            foreach (var tab in transferEntities) {
                if (tab.TransferState.State != state)
                    continue;
                
                Logging.Interfaces.DbStructure.ITable item=null;
                try
                {
                    //TODO: Very inefficient
                    item = log.GetLogTables(tab.Id).LastOrDefault();
                }
                catch (Exception){}

                if (item != null)
                {
                    table.Rows.Add(new object[] { tab.Catalog, tab.Schema, tab.Name, item.Count, item.CountDest, TimespanToString(item.Duration), item.Filter, item.Error });
                }
            }

            return table;
        }
        private string TimespanToString(TimeSpan span) {
            if (span.TotalSeconds < 1)
                return "< 1 " + Base.Localisation.ResourcesCommon.Second;
            var sb = new StringBuilder();
            if (span.Days > 0) {
                sb.Append(span.Days + " ");
                sb.Append(span.Days > 1 ? Base.Localisation.ResourcesCommon.Days : Base.Localisation.ResourcesCommon.Day);
            }
            if (span.Hours > 0) {
                sb.Append(span.Hours + " ");
                sb.Append(span.Hours > 1 ? Base.Localisation.ResourcesCommon.Hours : Base.Localisation.ResourcesCommon.Hour);
            }
            if (span.Minutes > 0) {
                sb.Append(span.Minutes + " ");
                sb.Append(span.Minutes > 1 ? Base.Localisation.ResourcesCommon.Minutes : Base.Localisation.ResourcesCommon.Minute);
            }
            if (span.Seconds > 0) {
                sb.Append(span.Seconds + " ");
                sb.Append(span.Seconds > 1 ? Base.Localisation.ResourcesCommon.Seconds : Base.Localisation.ResourcesCommon.Second);
            }
            return sb.ToString();
        }

        private DataTable GetTablesStatusDataTable() {
            var table = new DataTable();
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("Action_Status", typeof(string));

            table.Rows.Add(new object[] { Base.Localisation.ResourcesCommon.MailDocumentCurrentAction, _transferAgent.TransferProgress.StepCaption });
            table.Rows.Add(new object[] { Base.Localisation.ResourcesCommon.MailDocumentTablesTotal, _transferAgent.TransferProgress.EntitiesTotal });
            table.Rows.Add(new object[] { Base.Localisation.ResourcesCommon.MailDocumentTablesProcessed, _transferAgent.TransferProgress.EntitiesFinished });
            table.Rows.Add(new object[] { Base.Localisation.ResourcesCommon.MailDocumentStartTime, _transferAgent.TransferStartTime.ToString("F") });
            table.Rows.Add(new object[]
                               {
                                   Base.Localisation.ResourcesCommon.MailDocumentTimeElapsed,
                                   TimespanToString(DateTime.Now - _transferAgent.TransferStartTime)
                               });

            var progressCounter = 1;
            foreach (var transferTableProgressWrapper in _transferAgent.TransferProgress.ActualProcessedTables) {
                if (transferTableProgressWrapper.Progress != null) {
                    table.Rows.Add(new object[]
                                       {
                                           string.Format(Base.Localisation.ResourcesCommon.MailDocumentJob, progressCounter),
                                           transferTableProgressWrapper.Progress.TableName + " (" +
                                           transferTableProgressWrapper.Progress.DataSetsProcessed + " / " +
                                           transferTableProgressWrapper.Progress.DatasetsTotal + ")"
                                       });
                    progressCounter++;
                }
            }
            return table;
        }
        #endregion Methods
    }
}
