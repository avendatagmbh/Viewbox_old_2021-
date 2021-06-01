// --------------------------------------------------------------------------------
// author: Sebastian Vetter, Mirko Dibbert
// since: 2012-02-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfGenerator;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eBalanceKitBusiness.Export {

    /// <summary>
    /// This class contains all methods to export HyperCubes like "changesEquityStatement".
    /// </summary>
    public class HyperCubeExporter {

        public HyperCubeExporter(IExportConfig config) { Config = config; }

        #region CSV
        
        private StringBuilder _csvContent;
        private IExportConfig Config { get; set; }
        private string _filename;

        private readonly string[] _disallowedChars = {
            "\\", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<",
            ">", "|", ","
        };

        private void BuildCsvContent(IPresentationTreeNode root) {

            var cube = root.Document.GetHyperCube(root.Element.Id);
            if (cube == null) return;

            if (cube.Dimensions.AllDimensionItems.Count() == 2) {
                var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
                BuildHyperCubeTableContent(table);
    
            } else if (cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyperCube3DCube = cube.Get3DCube(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last(), cube.Dimensions.DimensionItems.First());
                BuildHyperCube3DCubeContent(hyperCube3DCube);

            } else {
                throw new Exception("Export for tables with more than 3 dimensions is not supported.");
            }

        }

        /// <summary>
        /// This method exports the specified hypercube table into a csv file.
        /// </summary>
        private void BuildHyperCubeTableContent(IHyperCubeTable table) {
            var columnHeaders = new List<string> { string.Empty }; // placeholder for row headlines
            columnHeaders.AddRange(table.AllColumns.Select(column => column.Header));
            _csvContent.Append(string.Join(";", columnHeaders));

            foreach (var row in table.AllRows) {
                _csvContent.AppendLine();
                _csvContent.Append(row.Header);
                foreach (var col in table.AllColumns) {
                    _csvContent.Append(";" + row[col].Item.Value);
                }
            }
        }

        /// <summary>
        /// This method exports the specified hypercube 3d-cube into a csv file.
        /// </summary>
        private void BuildHyperCube3DCubeContent(IHyperCube3DCube hyperCube3DCube) {
            var columnHeaders = new List<string> { string.Empty }; // placeholder for row headlines
            columnHeaders.AddRange(from col in hyperCube3DCube.Tables.First().AllColumns
                                   from dimCoordinate in hyperCube3DCube.ThirdDimension.Values
                                   select col.Header + " (" + dimCoordinate.Label + ")");
            _csvContent.Append(string.Join(";", columnHeaders));

            var rowCount = hyperCube3DCube.Tables.First().AllRows.Count();
            var colCount = hyperCube3DCube.Tables.First().AllColumns.Count();
            var rowDict = new Dictionary<IHyperCubeTable, IHyperCubeRow[]>();
            var colDict = new Dictionary<IHyperCubeTable, IHyperCubeColumn[]>();
            foreach (var table in hyperCube3DCube.Tables) {
                rowDict[table] = table.AllRows.ToArray();
                colDict[table] = table.AllColumns.ToArray();
            }

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++) {
                _csvContent.AppendLine();
                _csvContent.Append(rowDict.Values.First()[rowIndex].Header);
                for (int colIndex = 0; colIndex < colCount; colIndex++) {
                    foreach (var table in hyperCube3DCube.Tables) {
                        var row = rowDict[table][rowIndex];
                        var col = colDict[table][colIndex];
                        _csvContent.Append(";" + row[col].Item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate CSV file.
        /// </summary>
        /// <param name="node">The HyperCube node.</param>
        public void ExportCsv(IPresentationTreeNode node) {
            try {
                string label = node.Element.Label;
                _csvContent = new StringBuilder();

                _filename = Config.Document.Company.Name +
                            ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                            "" + label;

                foreach (var disallowedChar in _disallowedChars) {
                    _filename = _filename.Replace(disallowedChar, "_");
                }
                _filename = Config.FilePath + "\\" + _filename + ".csv";


                if (File.Exists(_filename) && IsFileLocked(new FileInfo(_filename))) {
                    _filename = _filename.Substring(0, _filename.Length - 4);
                    _filename += "_2.csv";
                }

                using (
                    var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                    Encoding.UTF8)) {
                    BuildCsvContent(node);

                    csvWrite.WriteLine(_csvContent);
                    csvWrite.Close();
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports für den Hypercube ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate CSV file.
        /// </summary>
        /// <param name="elementId">The ID to identify the HyperCube node in the Config.Document.GaapPresentationTree.</param>
        public void ExportCsv(string elementId) {
            try {
                string label = string.Empty;
                IPresentationTreeNode node = null;
                foreach (IPresentationTree ptree in Config.Document.GaapPresentationTrees.Values) {
                    if (ptree.HasNode(elementId)) {
                        node = ptree.GetNode(elementId) as IPresentationTreeNode;
                        label = node.Element.Label; //node.Value.Element.Name;
                        break;
                    }
                }
                if (node != null) {
                    _csvContent = new StringBuilder();

                    _filename = Config.Document.Company.Name +
                                ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + "_" +
                                "" + label;

                    foreach (var disallowedChar in _disallowedChars) {
                        _filename = _filename.Replace(disallowedChar, "_");
                    }
                    _filename = Config.FilePath + "\\" + _filename + ".csv";
                    if (File.Exists(_filename) && IsFileLocked(new FileInfo(_filename))) {
                        _filename = _filename.Substring(0, _filename.Length - 4); //.Reverse();
                        //_filename.Remove(0,4);
                        //_filename.Reverse();
                        _filename += "_2.csv";
                    }
                    using (
                        var csvWrite = new StreamWriter(File.Open(_filename, FileMode.Create),
                                                        Encoding.UTF8)) {
                        BuildCsvContent(node);

                        csvWrite.WriteLine(_csvContent);
                        csvWrite.Close();
                    }
                }
            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des CSV-Reports für den Hypercube ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }       

        #endregion CSV

        #region PDF

        private Font font = new Font(Font.FontFamily.HELVETICA, 8);
        private Font fontBold = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);
        private Font fontItalic = new Font(Font.FontFamily.HELVETICA, 8, Font.ITALIC);

        #region AddHeaderTable

        /// <summary>
        /// Creates an empty PdfPTable with the needed dimensions (considers column limitations) and the header row.
        /// </summary>
        /// <param name="cube">The HyperCube to calculate the dimensions.</param>
        /// <param name="table">The HyperCubeTable to access the collumns.</param>
        /// <returns>A Table with a header row and the right dimensions.</returns>
        private PdfPTable AddHeaderTableLimited(IHyperCube cube, IHyperCubeTable table) {
            /*
             * "colCounter" is the real number of columns with columns for dimensions
             * "dim" tells the dimensions for the pdf table (calculated with columnLimiter and dimension informations of the cube)
             * "foreach"-loop iterates through the dimensions
             * "i-loop" iterates through the current relevant columns to get the header
             */
            int colCounter = cube.Dimensions.AllDimensionItems.Count() > 2
                                 ? Convert.ToInt32(Math.BigMul(table.AllColumns.Count(),
                                                               skipFirstDimensionElement
                                                                   ? cube.Dimensions.AllDimensionItems.First().Values.
                                                                         Count() - 1
                                                                   : cube.Dimensions.AllDimensionItems.First().Values.
                                                                         Count()))
                                 : table.AllColumns.Count();

            if (columnStarter + columnLimiter > colCounter) {
                columnLimiter = colCounter % columnLimiter;
            } else {
                colCounter++;
            }
            int dim = 
            cube.Dimensions.AllDimensionItems.Count() > 2 ?
            columnLimiter * (cube.Dimensions.AllDimensionItems.First().Values.Count() - (skipFirstDimensionElement ? 1 : 0)) : columnLimiter;
            PdfPTable pdfTable = new PdfPTable(dim + 1);
            pdfTable.DefaultCell.Border = 0;
            pdfTable.WidthPercentage = 100;
            pdfTable.HeaderRows = 1;

            // keep empty for row headlines
            AddHeaderCell(pdfTable, "", Element.ALIGN_LEFT);
            for (int i = 0; i < columnLimiter; i++) {
                if (cube.Dimensions.AllDimensionItems.Count() > 2) {
                    // multi dimensional cubes
                    foreach (var item in cube.Dimensions.AllDimensionItems.First().Values) {
                        if (skipFirstDimensionElement &&
                            item == cube.Dimensions.AllDimensionItems.First().Values.First()) {
                            continue;
                        }
                        AddHeaderCell(pdfTable, table.AllColumns.ToList()[columnStarter + i].Header + " (" + item.Label + ")", Element.ALIGN_CENTER);
                    }
                } else {
                    AddHeaderCell(pdfTable, table.AllColumns.ToList()[columnStarter + i].Header, Element.ALIGN_CENTER);
                }
            
            }
            return pdfTable;
        }


        /// <summary>
        /// Creates an empty PdfPTable with the needed dimensions and the header row.
        /// </summary>
        /// <param name="cube">The HyperCube to calculate the dimensions.</param>
        /// <param name="table">The HyperCubeTable to access the collumns.</param>
        /// <returns>A Table with a header row and the right dimensions.</returns>
        private PdfPTable AddHeaderTable(IHyperCube cube, IHyperCubeTable table) {
            int colCounter = cube.Dimensions.AllDimensionItems.Count() > 2
                                 ? Convert.ToInt32(Math.BigMul(table.AllColumns.Count(),
                                                               skipFirstDimensionElement
                                                                   ? cube.Dimensions.AllDimensionItems.First().Values.
                                                                         Count() - 1
                                                                   : cube.Dimensions.AllDimensionItems.First().Values.
                                                                         Count()))
                                 : table.AllColumns.Count();
            colCounter++;
            PdfPTable pdfTable = new PdfPTable(colCounter);
            pdfTable.DefaultCell.Border = 0;
            pdfTable.WidthPercentage = 100;

            pdfTable.HeaderRows = 1;

            // keep empty for row headlines
            AddHeaderCell(pdfTable, "", Element.ALIGN_LEFT);

            foreach (var column in table.AllColumns) {
                if (cube.Dimensions.AllDimensionItems.Count() > 2) {
                    // multi dimensional cubes
                    foreach (var item in cube.Dimensions.AllDimensionItems.First().Values) {
                        if (skipFirstDimensionElement &&
                            item == cube.Dimensions.AllDimensionItems.First().Values.First()) {
                            continue;
                        }
                        AddHeaderCell(pdfTable, column.Header + " (" + item.Label + ")", Element.ALIGN_CENTER);
                    }
                } else {
                    AddHeaderCell(pdfTable, column.Header, Element.ALIGN_CENTER);
                }
            }

            return pdfTable;
        }
        #endregion AddHeaderTable

        #region AddHeaderCell
        private void AddHeaderCell(PdfPTable table, string value, int hAlignment) {
            PdfPCell cell = new PdfPCell(new Phrase(value, fontBold));
            cell.HorizontalAlignment = hAlignment;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.Border = table.DefaultCell.Border;
            table.AddCell(cell);
        }
        #endregion AddHeaderCell
        
        #region AddDefaultCell
        /// <summary>
        /// Adds a left alligned table cell.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="value"></param>
        private void AddDefaultCell(PdfPTable table, string value, BaseColor backgroundColor = null, bool italic = false) {
            PdfPCell cell = new PdfPCell(new Phrase(value, italic ? fontItalic : font));
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Border = table.DefaultCell.Border;
            if (backgroundColor != null) cell.BackgroundColor = backgroundColor;
            table.AddCell(cell);
        }
        #endregion AddDefaultCell
        
        private bool skipFirstDimensionElement = true;
        private int columnLimiter = 5;
        private int columnStarter = 0;
        

        /// <summary>
        /// Generates the content for the pdf file. The columns per page are limited.
        /// </summary>
        /// <param name="root">The HyperCube node.</param>
        /// <param name="pdfColumnLimiter">If >0 (0=default) it defines the number of columns per page. Otherwise it uses the standard (5) number of columns.</param>
        /// <returns>A pdf table you can add to your pdf file.</returns>
        public PdfPTable GeneratePdfContentLimited(IPresentationTreeNode root, int pdfColumnLimiter = 0) {
            /*
             * Explanation:
             * -------------
             * Example cube:
             *            1     2     3
             *       a   7,8   4,5   9,0
             *       b   6,z   u,i   q,w
             *       c   p,n   m,r   x,y
             *
             * rows: a,b,c
             * columns: 1,2,3
             *  
             * number of dimensions: 3
             * 
             * cube object looks linear like 7,4,9,8,5,0,6,u,q,z,i,w,p,m,x,n,r,y
             * the "skipper" is necessary to get 7 and 8 as a pair
             * "limit" stands for the limitation of dimensions that can be accessed
             * "skipFirstDimensionElement" is allways true because in the current configuration of "HyperCubes" the same value is stored twice (eg. 3 dimensions even if we need only 2)
             * "columnstarter" shows in which column we have to start
             * "columnLimiter" sets the limitation for how many value cols per page are allowed (carefull: it limitates the columns in the table.AllColumns so for our example: columnLimiter = 2 --> 4 value columns + 1 title column per Page 
             * the "k-loop" iterates through all dimensions
             * the "i-loop" gets all relevant columns with the values
             * the "foreach row"-loop itaerates through all rows
             * the "while-loop" limits the fun to the number of columns limited by columnLimiter
            */

            if (pdfColumnLimiter > 0) {
                columnLimiter = pdfColumnLimiter;
            }

            var cube = root.Document.GetHyperCube(root.Element.Id);
            
            PdfPTable pdfTable = null;

            if (cube != null) {
                var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
                int counter = 0;
                int skiper = Convert.ToInt32(Math.BigMul(table.AllRows.Count(), table.AllColumns.Count()));

                int limit = (cube.Dimensions.AllDimensionItems.Count() > 2) ? cube.Dimensions.AllDimensionItems.First().Values.Count() : 2;

                pdfTable = AddHeaderTableLimited(cube, table);

                while (columnStarter < table.AllColumns.Count()) {
                    
                    foreach (var row in table.AllRows) {

                        for (int i = columnStarter; i < columnStarter + columnLimiter; i++) {

                            if (i == columnStarter) {
                                AddDefaultCell(pdfTable, row.Header);
                            }

                            if (i >= table.AllColumns.Count()) {
                                break;
                            }

                            for (int k = 0; k < limit; k++) {

                                if (skipFirstDimensionElement && k == 1) {
                                    continue;
                                }

                                AddDefaultCell(pdfTable,
                                               "" +
                                               cube.Items.GetItemByID(
                                                   Convert.ToInt32(i + counter + Math.BigMul(skiper, k))).Value + "");
                            }
                        }

                        counter += table.AllColumns.Count();
                    }

                    counter = 0;
                    columnStarter += columnLimiter;
                }
            }
            return pdfTable;
        }


        /// <summary>
        /// Generates the content for the pdf file. All columns on one page.
        /// </summary>
        /// <param name="root">The HyperCube node.</param>
        /// <returns>A pdf table you can add to your pdf file.</returns>
        public PdfPTable GeneratePdfContent(IPresentationTreeNode root) {

            PdfPTable pdfTable = null;

            var cube = root.Document.GetHyperCube(root.Element.Id); 
            
            if (cube != null) {
                var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
                int counter = 0;
                int skiper = Convert.ToInt32(Math.BigMul(table.AllRows.Count(), table.AllColumns.Count()));

                int limit = (cube.Dimensions.AllDimensionItems.Count() > 2) ? cube.Dimensions.AllDimensionItems.First().Values.Count() : 2;

                pdfTable = AddHeaderTable(cube, table);
                

                foreach (var row in table.AllRows) {

                    for (int i = 0; i < (table.AllColumns.Count()); i++) {
                        if (i == 0) {
                            //_csvContent.AppendLine();
                            AddDefaultCell(pdfTable, row.Header);
                            //_csvContent.Append(row.Header);
                        }

                        for (int k = 0; k < limit; k++) {

                            if (skipFirstDimensionElement && k == 1) {
                                continue;
                            }
                            AddDefaultCell(pdfTable, "" + cube.Items.GetItemByID(Convert.ToInt32(i + counter + Math.BigMul(skiper, k))).Value);
                            //_csvContent.Append(";" + cube.Items.GetItemByID(Convert.ToInt32(i + counter + Math.BigMul(skiper, k))).Value);
                        }
                    }

                    counter += table.AllColumns.Count();
                }
            }

            return pdfTable;
        }


        private PdfGenerator.PdfGenerator pdf;

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file.
        /// </summary>
        /// <param name="elementId">The ID to identify the HyperCube node.</param>
        public void ExportPdf(string elementId) {

            IPresentationTreeNode node = null;
            foreach (IPresentationTree ptree in Config.Document.GaapPresentationTrees.Values) {
                if (ptree.HasNode(elementId)) {
                    node = ptree.GetNode(elementId) as IPresentationTreeNode;
                    ExportPdf(node);
                    break;
                }
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file with the default number of columns per page (5).
        /// </summary>
        /// <param name="elementId">The ID to identify the HyperCube node.</param>
        public void ExportPdfLimited(string elementId) {

            foreach (IPresentationTree ptree in Config.Document.GaapPresentationTrees.Values) {
                if (ptree.HasNode(elementId)) {
                    IPresentationTreeNode node = ptree.GetNode(elementId) as IPresentationTreeNode;
                    ExportPdfLimited(node);
                    break;
                }
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file with the specified columns per page.
        /// </summary>
        /// <param name="elementId">The ID to identify the HyperCube node.</param>
        /// <param name="pdfColumnLimiter">The number of columns per page. (Will be multiplied by the number of dimensions.)</param>
        public void ExportPdfLimited(string elementId, int pdfColumnLimiter) {

            foreach (IPresentationTree ptree in Config.Document.GaapPresentationTrees.Values) {
                if (ptree.HasNode(elementId)) {
                    IPresentationTreeNode node = ptree.GetNode(elementId) as IPresentationTreeNode;
                    ExportPdfLimited(node, pdfColumnLimiter);
                    break;
                }
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file with the specified columns per page.
        /// </summary>
        /// <param name="node">The Hypercube node.</param>
        /// <param name="pdfColumnLimiter">The number of columns per page. (Will be multiplied by the number of dimensions.)</param>
        public void ExportPdfLimited(IPresentationTreeNode node, int pdfColumnLimiter) {
            if (pdfColumnLimiter == 0) {
                ExportPdf(node);
            } else {
                columnLimiter = pdfColumnLimiter;
                ExportPdfLimited(node);
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file with the default number of columns per page (5).
        /// </summary>
        /// <param name="node">The HyperCube node.</param>
        public void ExportPdfLimited(IPresentationTreeNode node) {
            
            FileInfo fi = new FileInfo(Config.Filename);
            var tmpDrive = fi.Directory.Root.Name;
            // Directory without drive
            var tmpDirectory = fi.DirectoryName.Remove(0, 3);
            var tmpFileName = fi.Name;



            string[] disallowedChars = {
                ":", "\\", "+", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<", ">"
                , "|", ","
            };

            foreach (var disallowedChar in disallowedChars) {
                tmpFileName = tmpFileName.Replace(disallowedChar, "_");
            }
            string[] disallowedCharsDirectory = {
                ":", "+", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<",
                ">", "|", ","
            };

            foreach (var s in disallowedCharsDirectory) {
                tmpDirectory = tmpDirectory.Replace(s, "_");
            }


            var directory = new DirectoryInfo(tmpDrive + tmpDirectory);
            if (!directory.Exists) directory.Create();


            // Set default name if no file name is specified (avoid DirectoryNotFoundException) -- SeV
            if (tmpFileName == String.Empty)
                tmpFileName = Config.Document.Company.Name + "_" + Config.Document.FinancialYear.FYear + "_" +
                              Config.Document.Name + ".pdf";

            if (tmpFileName.LastIndexOf(".") == -1) tmpFileName = tmpFileName + ".pdf";
            if (tmpFileName.LastIndexOf(".") == tmpFileName.Length - 1) tmpFileName = tmpFileName + "pdf";
            var fileName = Config.Filename = tmpDrive + tmpDirectory + "\\" + tmpFileName;


            fileName = fileName.Substring(0, fileName.Length - 4); 
            fileName += "_" + node.Element.Label + ".pdf";

            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");

                pdf = new PdfGenerator.PdfGenerator(traits);

                // Generate Content
                pdf.AddHeadline(node.Element.Label);
                PdfPTable pt = GeneratePdfContentLimited(node);
                pdf.CurrentChapter.Add(pt);

                Phrase headerPhrase = new Phrase(
                    Config.Document.Name +
                    " (" + Config.Document.Company.Name + ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + ")",
                    pdf.Traits.fontH1);

                pdf.WriteFile(fileName, headerPhrase);
                System.Diagnostics.Process.Start(fileName);


            } catch (IOException ioEx) {
                if (ioEx.Message.Contains("fehlt ein erforderliches Recht")) {

                    throw new Exception(
                        "Bei der Erstellung des PDF-Reports für den HyperCube ist ein Fehler aufgetreten: " + Environment.NewLine +
                        "Keine Berechtigung.");
                }

                throw new Exception(
                    "Bei der Erstellung des PDF-Reports ist für den HyperCube ein Fehler aufgetreten: " + Environment.NewLine +
                    "Scheinbar ist die Datei bereits vorhanden und wird von einer anderen Anwendung verwendet.");

            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des PDF-Reports für den HyperCube ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }

        /// <summary>
        /// Exports a HyperCube node in a seperate PDF file.
        /// </summary>
        /// <param name="node">The HyperCube node.</param>
        public void ExportPdf(IPresentationTreeNode node) {
            
            FileInfo fi = new FileInfo(Config.Filename);
            var tmpDrive = fi.Directory.Root.Name;
            // Directory without drive
            var tmpDirectory = fi.DirectoryName.Remove(0, 3);
            var tmpFileName = fi.Name;



            string[] disallowedChars = {
                ":", "\\", "+", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<", ">"
                , "|", ","
            };

            foreach (var disallowedChar in disallowedChars) {
                tmpFileName = tmpFileName.Replace(disallowedChar, "_");
            }
            string[] disallowedCharsDirectory = {
                ":", "+", "#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<",
                ">", "|", ","
            };

            foreach (var s in disallowedCharsDirectory) {
                tmpDirectory = tmpDirectory.Replace(s, "_");
            }


            var directory = new DirectoryInfo(tmpDrive + tmpDirectory);
            if (!directory.Exists) directory.Create();


            // Set default name if no file name is specified (avoid DirectoryNotFoundException) -- SeV
            if (tmpFileName == String.Empty)
                tmpFileName = Config.Document.Company.Name + "_" + Config.Document.FinancialYear.FYear + "_" +
                              Config.Document.Name + ".pdf";

            if (tmpFileName.LastIndexOf(".") == -1) tmpFileName = tmpFileName + ".pdf";
            if (tmpFileName.LastIndexOf(".") == tmpFileName.Length - 1) tmpFileName = tmpFileName + "pdf";
            var fileName = Config.Filename = tmpDrive + tmpDirectory + "\\" + tmpFileName;


            fileName = fileName.Substring(0, fileName.Length - 4); 
            fileName += "_" + node.Element.Label + ".pdf";

            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");

                pdf = new PdfGenerator.PdfGenerator(traits);

                // Generate Content
                pdf.AddHeadline(node.Element.Label);
                pdf.CurrentChapter.Add(GeneratePdfContent(node));

                Phrase headerPhrase = new Phrase(
                    Config.Document.Name +
                    " (" + Config.Document.Company.Name + ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + ")",
                    pdf.Traits.fontH1);

                pdf.WriteFile(fileName, headerPhrase);
                System.Diagnostics.Process.Start(fileName);


            } catch (IOException ioEx) {
                if (ioEx.Message.Contains("fehlt ein erforderliches Recht")) {

                    throw new Exception(
                        "Bei der Erstellung des PDF-Reports für den HyperCube ist ein Fehler aufgetreten: " + Environment.NewLine +
                        "Keine Berechtigung.");
                }

                throw new Exception(
                    "Bei der Erstellung des PDF-Reports ist für den HyperCube ein Fehler aufgetreten: " + Environment.NewLine +
                    "Scheinbar ist die Datei bereits vorhanden und wird von einer anderen Anwendung verwendet.");

            } catch (Exception ex) {
                throw new Exception(
                    "Bei der Erstellung des PDF-Reports für den HyperCube ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message,
                    ex);
            }
        }
        #endregion PDF

        #region helper
        protected virtual bool IsFileLocked(FileInfo file) {
            FileStream stream = null;

            try {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        #endregion helper
    }

}

