using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;

namespace DocumentGenerator
{
    public class PdfTraits {
        public string Author, Creator;

        /// <summary>
        /// Margins are in mm
        /// </summary>
        public float MarginLeft, MarginRight, MarginTop, MarginBottom;
        //Used for headlines
        public Font fontH1;
        //Used for subheadlines
        public Font fontH2;
        //Used for subsubheadlines
        public Font fontH3;
        //
        public Font fontH4;
        public Font xfont;
        public Rectangle PageSizeAndOrientation;
        public Boolean CreateTableOfContents;

        public PdfTraits(string author, string creator){
            this.Author = author;
            this.Creator = creator;

            this.MarginLeft = 25 * PageSize.A4.Rotate().Width / 210f;
            this.MarginRight = 25 * PageSize.A4.Rotate().Width / 210f;
            this.MarginTop = 18 * PageSize.A4.Rotate().Width / 210f;
            this.MarginBottom = 8 * PageSize.A4.Rotate().Width / 210f;
            this.PageSizeAndOrientation = PageSize.A4.Rotate();
            this.CreateTableOfContents = true;

            fontH1 = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
            fontH2 = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
            fontH3 = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);
            fontH4 = new Font(Font.FontFamily.HELVETICA, 9, Font.ITALIC);
            xfont = new Font(Font.FontFamily.HELVETICA, 11);
        }

        public float mm() {
            return PageSize.A4.Rotate().Width / 210f;
        }
    }

    /// <summary>
    /// This class simplifies the pdf generation, but at the moment it is tailored for the E-Bilanz Kit.
    /// Using the PdfTraits class it can be configured for other uses and it is designed to be subclassed if special needs arise.
    /// The general usage is to create an object of PdfGenerator, use AddHeadline (creates a new page) and AddSubHeadline to create numbered titles
    /// and then fill the pages by retrieving the Document and creating Paragraphes, Tables or whatever you like to use from itextsharp
    /// The last step is to call WriteFile which closes the document and writes the output to a file.
    /// The class is designed to automatically generate booksmarks whenever you call AddHeadline/AddSubHeadline and can later on create a table of contents from this data.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-06-06</since>
    public class PdfGenerator {
       
        protected MemoryStream _mstream;
        protected PdfWriter _writer;
        protected Headline _currentHeadline;
        // shows the deepest sub head line. In the ToC it will be the 3rd depth
        protected Headline _currentSubHeadline;
        protected Section _currentSubSection;
        protected Section _currentSubSubSection;
        protected Dictionary<string, bool> _dontShowInTOC = new Dictionary<string, bool>();
        private static Font italicFont = new Font(Font.FontFamily.HELVETICA, Font.DEFAULTSIZE, Font.ITALIC);

        #region properties
        
        public PdfTraits Traits { get; set; }
        public Document Document { get; set; }
        public Chapter CurrentChapter { get; protected set; }
        public Section CurrentSection { get { return _currentSubSection; } }
        public Section CurrentSubSection { get { return _currentSubSubSection; } }

        #region Headlines
        private List<Headline> Headlines { get { return _headlines; } }
        private List<Headline> _headlines = new List<Headline>();
        #endregion

        #endregion properties

        public PdfGenerator(PdfTraits traits) {
            this.Traits = traits;
            CurrentChapter = null;
            Init();
            _mstream = new MemoryStream();
            _writer = PdfWriter.GetInstance(Document, _mstream);
            _writer.ViewerPreferences = PdfWriter.PageModeUseOutlines;

            Document.Open();
        }

        public virtual void WriteFile(Stream outStream, Phrase headerPhrase, bool addPageHeader = true, bool addViewboxStyle = false, List<Tuple<string, string>> optimizationTexts = null,
                                                                                                            Tuple<string, string> userText = null, string from = "", int noOfHorizontalPages = 1, string pageLoc = "page", string partLoc="part") {
            //Add the last chapter if present
            if (CurrentChapter != null) Document.Add(CurrentChapter);
            Document.Close();

            PdfReader reader = new PdfReader(new MemoryStream(_mstream.GetBuffer()));
            PdfStamper stamper = new PdfStamper(reader, outStream);

            if (Traits.CreateTableOfContents) {
                //Retrieve bookmarks which essentially are the table of contents
                IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
                PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks);
                if (TOCReader != null)
                    AddTableOfContents(reader, stamper, TOCReader);
            }
            if (addViewboxStyle) AddViewboxStyle(reader, stamper, headerPhrase, optimizationTexts, userText, from, noOfHorizontalPages, pageLoc, partLoc);
            else if (addPageHeader) AddPageHeader(reader, stamper, headerPhrase);
            else stamper.Close();
        }

        //Writes File with user-defined Head-table and user-defined Footer-table! sideOnHeaderOrFooter: 1 -> on header | 2 -> on footer | default -> neither on footer nor on header
        public virtual void WriteFile(Stream outStream, Phrase headerPhrase, PdfPTable headerContent, PdfPTable footerContent, Font sideFont, string sideString = "Seite", int sideOnHeaderOrFooter = 0, bool currentAndCompleteSideNr = true, string completeSideStringSeperator = " / ", float sideCellPaddingTop = 0f, bool printHeaderOnlyOnFirstPage = false, bool printFooterOnlyOnLastPage = false) {
            //Add the last chapter if present
            if (CurrentChapter != null) Document.Add(CurrentChapter);
            Document.Close();

            PdfReader reader = new PdfReader(new MemoryStream(_mstream.GetBuffer()));
            PdfStamper stamper = new PdfStamper(reader, outStream);

            if (Traits.CreateTableOfContents) {
                //Retrieve bookmarks which essentially are the table of contents
                IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
                PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks);
                if (TOCReader != null)
                    AddTableOfContents(reader, stamper, TOCReader);
            }

            AddHeader(reader, stamper, headerContent, sideString, sideFont, sideOnHeaderOrFooter, currentAndCompleteSideNr, completeSideStringSeperator, sideCellPaddingTop, printHeaderOnlyOnFirstPage);
            AddFooter(reader, stamper, footerContent, sideString, sideFont, sideOnHeaderOrFooter, currentAndCompleteSideNr, completeSideStringSeperator, sideCellPaddingTop, printFooterOnlyOnLastPage);

            stamper.Close();
        }


        public virtual void WriteFile(string filename, Phrase headerPhrase){
            using (var stream = new FileStream(filename, FileMode.Create)) {
                WriteFile(stream, headerPhrase);
                stream.Close();
            }
        }

        /// <summary>
        /// for visualization of a pdfptable. Debug purposes.
        /// </summary>
        public static XElement PdfTableToElement(PdfPTable from) {
            return new XElement("root", PdfTableToElementRecursive(from)); 
        }

        /// <summary>
        /// for visualization of a pdfptable. Debug purposes.
        /// </summary>
        private static XElement PdfTableToElementRecursive(PdfPTable from) {
            if (from == null) {
                return null;
            }
            XElement ret = null;
            bool firstTime = true;
            int i = 1;
            for (var iEnum = from.Rows.GetEnumerator(); iEnum.MoveNext(); i++) {
                int j = 1;
                for (var jEnum = iEnum.Current.GetCells().GetEnumerator(); jEnum.MoveNext(); j++) {
                    if (firstTime) {
                        firstTime = false;
                        ret = new XElement("a" + i + "_" + j);
                    }
                    ret.Add(PdfTableToElementRecursive(((PdfPCell) jEnum.Current).Table) ??
                            new XElement(((PdfPCell) jEnum.Current).Phrase.Count == 0 ? "empty" : "value",
                                         ((PdfPCell) jEnum.Current).Phrase.Count == 0
                                             ? string.Empty
                                             : ((PdfPCell) jEnum.Current).Phrase[0].ToString()));
                }
            }
            return ret;
        }

        public virtual void WriteFile(string filename, string headerString) {
            using (var stream = new FileStream(filename, FileMode.Create)) {
                WriteFile(stream, new Phrase(headerString));
                stream.Close();
            }
        }

        protected virtual void Init(){
            Document = new Document(Traits.PageSizeAndOrientation);
            Document.AddAuthor(Traits.Author);
            Document.AddCreator(Traits.Creator);
            Document.AddCreationDate();
            Document.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);        
        }

        public void AddHeadline(string caption) {

            Document.NewPage();
            _currentHeadline = new Headline { Ordinal = Headlines.Count + 1, Caption = caption };
            Headlines.Add(_currentHeadline);
            //Add current chapter
            if(CurrentChapter != null) Document.Add(CurrentChapter);
            CurrentChapter = new Chapter(new Paragraph(_currentHeadline.DisplayString, Traits.fontH1), Headlines.Count);
        }

        /// <summary by="sev">If the level (sub or subsub) is unknown and you are working with IPresentationTreeEntry use AddNewSubHeadline instead.</summary>
        /// <summary by="mga">Don't use the AddNewSubHeadline: If there were no Sections there will be an exception, AddNewSubHeadline don't work deeper than 2 level.
        ///  Everything goes to the last sibling.</summary>
        public void AddSubHeadline(string caption, bool showInTOC) {
            _currentSubHeadline = _currentHeadline.AddChildren(caption);
            Paragraph p = new Paragraph(_currentSubHeadline.DisplayString, Traits.fontH2);
            p.SpacingBefore = 3;
            if (!showInTOC) _dontShowInTOC.Add(_currentSubHeadline.OrdinalNumber + ". " +_currentSubHeadline.DisplayString, false);
            _currentSubSection = CurrentChapter.AddSection(p);
            
        }

        /// <summary by="sev">If the level (sub or subsub) is unknown and you are working with IPresentationTreeEntry use AddNewSubHeadline instead.</summary>
        /// <summary by="mga">Don't use the AddNewSubHeadline: If there were no Sections there will be an exception, AddNewSubHeadline don't work deeper than 2 level.
        ///  Everything goes to the last sibling.</summary>
        public void AddSubHeadline(string caption) {
            AddSubHeadline(caption, true);
        }

        /// <summary by="sev">If the level (sub or subsub) is unknown and you are working with IPresentationTreeEntry use AddNewSubHeadline instead.</summary>
        /// <summary by="mga">Don't use the AddNewSubHeadline: If there were no Sections there will be an exception, AddNewSubHeadline don't work deeper than 2 level.
        ///  Everything goes to the last sibling.</summary>
        public void AddSubSubHeadline(string caption) {
            _currentSubHeadline = _currentSubHeadline.AddChildren(caption);
            Paragraph p = new Paragraph(_currentSubHeadline.DisplayString, Traits.fontH3);
            p.SpacingBefore = 3;
            _currentSubSubSection = CurrentSection.AddSection(p);
        }

        
        private bool _newSubSub = true;

        /// <summary>
        /// Adds a new paragraph in the needed level to the pdf.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="label">The text.</param>
        /// <param name="font1">The font of text if it is in first level</param>
        /// <param name="font2">The font of text if it is in second level</param>
        /// <param name="font3">The font of text if it is in third level</param>
        /// <author>Sebastian Vetter</author>
        public void AddNewSubHeadline(int level, string label, Font font1 = null, Font font2 = null, Font font3 = null)
        {
            
            Headline child = _currentSubHeadline.AddChildren(label);

            level = level*3;
            
            if (level <= 3) {

                Paragraph p = new Paragraph(child.DisplayString, font1 ?? Traits.fontH2);
                p.SpacingBefore = level;
                // We need a new section in the current chapter
                _currentSubSection = CurrentChapter.AddSection(p);
                _newSubSub = true;
            } else if (level <= 6) {

                Paragraph p = new Paragraph(child.DisplayString, font2 ?? Traits.fontH3);
                p.SpacingBefore = level;

                if (_newSubSub) {
                    // We need a new SubSubSection
                    _currentSubSubSection = _currentSubSection.AddSection(p);
                    _newSubSub = false;
                }
                else {
                    // The current SubSection needs a new entry
                    _currentSubSection.AddSection(p);
                }
            }
            else {
                Paragraph p = new Paragraph(child.DisplayString, font3 ?? Traits.fontH4);
                p.SpacingBefore = level;
                // New section in the current SubSubSection
                _currentSubSubSection.AddSection(p);
                _newSubSub = true;
            }
            
        }
        
        //pageNumberStrings looks similar to "4 FitH 72.4", it returns the number before the first space (which is the page number)
        //and adds 1 to remedy the page of the table of contents
        private string GetCorrectPageNumber(string pageNumberString) {
            return Convert.ToString(Convert.ToInt32(pageNumberString.Split(' ')[0]) + 1);
        }

        /// <summary>
        /// Creates a document for the table of contents which consists of 1 page and can later be copied into the original document.
        /// </summary>
        /// <param name="bookmarks">the bookmarks of the original document</param>
        /// <returns>A PdfReader containing the table of contents</returns>
        [Obsolete("This Function is obsolete; use GenerateTableOfCompleteContents instead")]
        protected PdfReader GenerateTableOfContents(IList<Dictionary<string, object>> bookmarks) {
            Document TOC = new Document(PageSize.A4.Rotate());
            TOC.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);
            MemoryStream mstream = new MemoryStream();
            PdfWriter writerTOC = PdfWriter.GetInstance(TOC, mstream);

            TOC.Open();
            TOC.Add(new Paragraph("Inhaltsverzeichnis", Traits.fontH1));

            PdfPTable resultTable = new PdfPTable(new float[] { 10.0f, 1f });
            resultTable.DefaultCell.Border = Rectangle.NO_BORDER;
            resultTable.SpacingBefore = 5f;
            resultTable.SpacingAfter = 5f;
            resultTable.WidthPercentage = 95f;

            if (bookmarks == null) {
                PdfTraits traits = this.Traits;
                traits.CreateTableOfContents = false;
                Traits = traits;
                return null;
            }
            foreach (var item in bookmarks) {
                    resultTable.AddCell(new Paragraph((string) item["Title"], Traits.fontH1));
                    resultTable.AddCell(GetCorrectPageNumber((string) item["Page"]));

                    if (!item.ContainsKey("Kids")) {
                        continue;
                    }
                    foreach (var subItem in (IList<Dictionary<string, object>>) item["Kids"]) {
                        if (_dontShowInTOC.ContainsKey((string) subItem["Title"])) continue;
                        resultTable.AddCell(new Paragraph("\t\t" + (string) subItem["Title"], Traits.fontH2));
                        resultTable.AddCell(GetCorrectPageNumber((string) subItem["Page"]));


                    }
            }
            TOC.Add(resultTable);

            TOC.Close();

            writerTOC.Close();

            return new PdfReader(mstream.GetBuffer());
        }


        /// <summary>
        /// Creates a document for the table of contents which includes all bookmarks and can later be copied into the original document.
        /// </summary>
        /// <param name="bookmarks">the bookmarks of the original document</param>
        /// <returns>A PdfReader containing the table of contents</returns>
        /// <author>Sebastian Vetter</author>
        protected PdfReader GenerateTableOfCompleteContents(IList<Dictionary<string, object>> bookmarks) {
            Document TOC = new Document(PageSize.A4.Rotate());
            TOC.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);
            MemoryStream mstream = new MemoryStream();
            PdfWriter writerTOC = PdfWriter.GetInstance(TOC, mstream);

            TOC.Open();
            TOC.Add(new Paragraph("Inhaltsverzeichnis", Traits.fontH1));

            PdfPTable resultTable = new PdfPTable(new float[] {10.0f, 1f});
            resultTable.DefaultCell.Border = Rectangle.NO_BORDER;
            resultTable.SpacingBefore = 5f;
            resultTable.SpacingAfter = 5f;
            resultTable.WidthPercentage = 95f;

            if (bookmarks == null) {
                PdfTraits traits = this.Traits;
                traits.CreateTableOfContents = false;
                Traits = traits;
                return null;
            }
            
            GetBookmarks(bookmarks, resultTable);

            TOC.Add(resultTable);

            TOC.Close();

            writerTOC.Close();

            return new PdfReader(mstream.GetBuffer());
        }

        /// <summary>
        /// Iterates through all bookmarks and stores it in the resultTable 
        /// </summary>
        /// <param name="bookmarks">The bookmarks</param>
        /// <param name="resultTable">The table where the bookmarks are stored.</param>
        /// <param name="subCounter">Counter for the level of the bookmark. Decides about the used font.</param>
        /// <author>Sebastian Vetter</author>
        protected void GetBookmarks(IList<Dictionary<string, object>> bookmarks, PdfPTable resultTable, int subCounter = 0) {
            string span = String.Empty;
            Font font;

            switch (subCounter) {
                case -1:
                    font = Traits.fontH1;
                    break;
                case 0:
                    font = Traits.fontH1;
                    break;
                case 1:
                    font = Traits.fontH2;
                    break;
                case 2:
                    font = Traits.fontH3;
                    break;
                default:
                    font = Traits.fontH4;
                    break;
            }

            for (int i = 0; i < subCounter; i++) {
                span += "\t\t";
            }

            foreach (var item in bookmarks)
            {

                resultTable.AddCell(new Paragraph(span + (string)item["Title"], font));
                resultTable.AddCell(GetCorrectPageNumber((string)item["Page"]));

                if (!item.ContainsKey("Kids"))
                {
                    continue;
                } else {
                    subCounter = subCounter + 1;
                    GetBookmarks((IList<Dictionary<string, object>>)item["Kids"], resultTable, subCounter);
                    subCounter--;
                }
            }
        }

        protected void AddTableOfContents(PdfReader reader, PdfStamper stamper, PdfReader TOCReader) {
            //Copy the table of contents into the first page
            for (int i = 0; i < TOCReader.NumberOfPages; i++) {
                stamper.InsertPage(0, Traits.PageSizeAndOrientation);
                //stamper.ReplacePage(TOCReader, i + 1, i + 1);
            }

            for (int i = 1; i <= TOCReader.NumberOfPages; i++) {
                stamper.ReplacePage(TOCReader, i, i);
            }

        }

        protected void AddPageHeader(PdfReader reader, PdfStamper stamper, Phrase headerPhrase) {
            for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                PdfPTable header = new PdfPTable(2);
                header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
                
                header.SetWidths(new float[] { 10, 1 });
                header.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                header.DefaultCell.VerticalAlignment = Element.ALIGN_BOTTOM;
                header.DefaultCell.BorderWidth = 0;
                header.DefaultCell.BorderWidthBottom = .25f;
                header.DefaultCell.PaddingBottom = 6;

                PdfPCell cell = new PdfPCell(headerPhrase);
                //PdfPCell cell = new PdfPCell(new Phrase(
                //    Config.Document.Name +
                //    " (" + Config.Document.Company.Name + ", Geschäftsjahr " + Config.Document.FinancialYear.FYear + ")", fontH1));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.BorderWidth = 0;
                cell.BorderWidthBottom = .25f;
                cell.PaddingBottom = 6;
                header.AddCell(cell);

                //Do not include page number for first page (table of contents)
                if (current == 1 && Traits.CreateTableOfContents)
                    header.AddCell(new Phrase(""));
                else
                    header.AddCell(new Phrase(String.Format("{0}/{1}", current, last), Traits.xfont));

                PdfContentByte cb = stamper.GetOverContent(current);
                header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), Traits.PageSizeAndOrientation.GetTop(12.5f * Traits.mm()), cb);
            }

            stamper.Close();
        }

        protected void AddViewboxStyle(PdfReader reader, PdfStamper stamper, Phrase headerPhrase, List<Tuple<string, string>> optimizationTexts, Tuple<string, string> userText, string from, int noOfHorizontalPages, string pageLoc, string partLoc) {
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            Font headLineTitle = new Font(bf, 15, Font.BOLD);
            Font headLineNormal = new Font(bf, 8, Font.NORMAL);
            Font headLineNormalGrey = new Font(bf, 8, Font.NORMAL, BaseColor.GRAY);

            var countPrefix = 0;
            var countSuffix = 0;

            for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                #region Header
                PdfPTable header = new PdfPTable(new float[]{ 0.45f, 0.55f });
                header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;

                var image = Image.GetInstance(Thread.GetDomain().BaseDirectory + "/Content/img/pdf/viewbox-logo.png");
                image.ScaleToFit(170f, 23f);
                PdfPCell logoCell = new PdfPCell(image, false);

                logoCell.Border = Rectangle.NO_BORDER;
                logoCell.Padding = 0;
                header.AddCell(logoCell);

                PdfPTable nested = new PdfPTable(1);
                int i = 0;
                foreach (var o in optimizationTexts) {
                    Chunk key = new Chunk(o.Item1 + ": ", headLineNormalGrey);
                    Chunk value = new Chunk(o.Item2, headLineNormal);
                    Phrase content = new Phrase();
                    content.Add(key);
                    content.Add(value);
                    PdfPCell optCell = new PdfPCell(content);
                    optCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    optCell.VerticalAlignment = Element.ALIGN_TOP;
                    optCell.Border = Rectangle.NO_BORDER;
                    optCell.Padding = 1;
                    nested.AddCell(optCell);
                    i++;
                }
                
                Chunk userKey = new Chunk(userText.Item1 + ": ", headLineNormalGrey);
                Chunk userValue = new Chunk(userText.Item2, headLineNormal);
                Phrase userContent = new Phrase();
                userContent.Add(userKey);
                userContent.Add(userValue);
                PdfPCell userCell = new PdfPCell(userContent);
                userCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                userCell.VerticalAlignment = Element.ALIGN_TOP;
                userCell.Border = Rectangle.NO_BORDER;
                userCell.Padding = 1;
                nested.AddCell(userCell);

                while (i < 4) {
                    PdfPCell optCell = new PdfPCell();
                    optCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    optCell.VerticalAlignment = Element.ALIGN_TOP;
                    optCell.Border = Rectangle.NO_BORDER;
                    optCell.Padding = 1;
                    optCell.FixedHeight = 8;
                    nested.AddCell(optCell);
                    i++;
                }

                nested.ExtendLastRow = true;

                PdfPCell nesthousing = new PdfPCell(nested);
                nesthousing.Padding = 0f;
                nesthousing.Border = Rectangle.NO_BORDER;
                header.AddCell(nesthousing);

                headerPhrase.Font = headLineTitle;
                PdfPCell titleCell = new PdfPCell(new Phrase(headerPhrase.Content, headLineTitle));
                titleCell.Colspan = 2;
                titleCell.Border = Rectangle.NO_BORDER;
                titleCell.Padding = 0;
                header.AddCell(titleCell);

                PdfContentByte cb = stamper.GetOverContent(current);
                header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), Traits.PageSizeAndOrientation.GetTop(20), cb);
                #endregion Header

                #region Footer
                PdfPTable footer = new PdfPTable(1);
                footer.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;

                Phrase footerText;
                if (noOfHorizontalPages == 1) footerText = new Phrase(pageLoc + " "  + current + " " + from + " " + last, headLineNormalGrey);
                //else footerText = new Phrase((countPrefix + 1) + "_" + (countSuffix + 1) + " " + from + " " + (last / noOfHorizontalPages), headLineNormalGrey);
                else footerText = new Phrase(partLoc + " " + (countSuffix + 1) + " / " + noOfHorizontalPages + ", " + pageLoc + " " + (countPrefix + 1) + " / " + last, headLineNormalGrey);

                PdfPCell pageCell = new PdfPCell(footerText);
                pageCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                pageCell.Border = Rectangle.NO_BORDER;
                pageCell.Padding = 0;
                footer.AddCell(pageCell);

                footer.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), 50, cb);
                #endregion Footer

                countSuffix++;

                if (noOfHorizontalPages > 1 && countSuffix % noOfHorizontalPages == 0) {
                    countSuffix = 0;
                    countPrefix++;
                }
            }

            stamper.Close();
        }


        protected void AddHeader(PdfReader reader, PdfStamper stamper, PdfPTable headerContent, string sideString, Font sideFont, int sideOnHeaderOrFooter, bool currentAndCompleteSideNr, string completeSideStringSeperator, float sideCellPaddingTop, bool printHeaderOnlyOnFirstPage) {
            if (printHeaderOnlyOnFirstPage) {
                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
                header.DefaultCell.Border = Rectangle.NO_BORDER;
                header.DefaultCell.Padding = 0f;

                header.AddCell(headerContent);

                header.ExtendLastRow = true;

                PdfContentByte cb = stamper.GetOverContent(1);
                header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft),
                                         Traits.PageSizeAndOrientation.GetTop(20), cb);
            }else {
                for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                    PdfPTable header = new PdfPTable(1);
                    header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
                    header.DefaultCell.Border = Rectangle.NO_BORDER;
                    header.DefaultCell.Padding = 0f;

                    if (sideOnHeaderOrFooter == 1) {
                        var side = sideString + " " + current;
                        if (currentAndCompleteSideNr) side += completeSideStringSeperator + last;
                        PdfPCell sideCell = new PdfPCell(new Phrase(side, sideFont));
                        sideCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        sideCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        sideCell.Border = PdfPCell.NO_BORDER;
                        sideCell.PaddingTop = sideCellPaddingTop;

                        header.AddCell(sideCell);
                    }
                    header.AddCell(headerContent);

                    header.ExtendLastRow = true;

                    PdfContentByte cb = stamper.GetOverContent(current);
                    header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft),
                                             Traits.PageSizeAndOrientation.GetTop(20), cb);
                }
            }
        }

        protected void AddFooter(PdfReader reader, PdfStamper stamper, PdfPTable footerContent, string sideString, Font sideFont, int sideOnHeaderOrFooter, bool currentAndCompleteSideNr, string completeSideStringSeperator, float sideCellPaddingTop, bool printFooterOnlyOnLastPage) {
            if (printFooterOnlyOnLastPage) {
                PdfPTable footer = new PdfPTable(1);
                footer.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
                footer.DefaultCell.Border = PdfPCell.NO_BORDER;
                footer.DefaultCell.Padding = 0f;

                footer.AddCell(footerContent);

                footer.ExtendLastRow = true;

                PdfContentByte cb = stamper.GetOverContent(reader.NumberOfPages);
                footer.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft),
                                         CalculatePdfPTableHeight(footer) + 20, cb);
            } else {
                for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                    PdfPTable footer = new PdfPTable(1);
                    footer.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
                    footer.DefaultCell.Border = PdfPCell.NO_BORDER;
                    footer.DefaultCell.Padding = 0f;

                    if (sideOnHeaderOrFooter == 2) {
                        var side = sideString + " " + current;
                        if (currentAndCompleteSideNr) side += completeSideStringSeperator + last;
                        PdfPCell sideCell = new PdfPCell(new Phrase(side, sideFont));
                        sideCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        sideCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        sideCell.Border = PdfPCell.NO_BORDER;
                        sideCell.PaddingTop = sideCellPaddingTop;

                        footer.AddCell(sideCell);
                        var emptyCell = new PdfPCell();
                        emptyCell.Border = PdfPCell.NO_BORDER;
                        emptyCell.FixedHeight = 10f;
                        footer.AddCell(emptyCell);
                    }
                    footer.AddCell(footerContent);

                    footer.ExtendLastRow = true;

                    PdfContentByte cb = stamper.GetOverContent(current);
                    footer.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft),
                                             CalculatePdfPTableHeight(footer) + 20, cb);
                }
            }
        }

        private static PdfPCell HeaderCell(string content, Font usedFont) {
            PdfPCell cell = new PdfPCell(new Phrase(content, usedFont));
            cell.BackgroundColor = new BaseColor(240, 240, 240);
            return cell;
        }


        public void AddTable(DataTable dataTable, bool writeHeader = true, int spacingBefore = 0, int spacingAfter = 0) {
            PdfPTable table = new PdfPTable(dataTable.Columns.Count){SpacingBefore = spacingBefore, SpacingAfter = spacingAfter, WidthPercentage = 100};

            if (writeHeader) {
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    //table.AddCell(new PdfPCell((new Paragraph(dataTable.Columns[i].ColumnName, new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)))));    
                    table.AddCell(HeaderCell(dataTable.Columns[i].ColumnName, italicFont));
                }
            }

            foreach (DataRow row in dataTable.Rows) {
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    table.AddCell(new PdfPCell((new Paragraph(row[i].ToString(), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)))));    
                }
            }

            CurrentChapter.Add(table);
            //Document. Add(table);
        }

        //Calculates the height of a PdfPTable. Be Careful: you have to SetTotalWidth to the PdfPTable because it is needed for WriteSelectedRows!
        public float CalculatePdfPTableHeight(PdfPTable table) {
            using (MemoryStream ms = new MemoryStream()) {
                Document doc = new Document(PageSize.TABLOID);
                PdfWriter w = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                table.WriteSelectedRows(0, table.Rows.Count, 0, 0, w.DirectContent);

                doc.Close();
                w.Close();

                return table.TotalHeight;
            }
        }

        public virtual void WriteFile(PdfReader firstPageReader, Stream otherPagesStream, Image watermark) {
            PdfPTable[] footers = Footers();
            if (CurrentChapter != null)
                Document.Add(CurrentChapter);
            Document.Close();
            PdfReader reader = new PdfReader(new MemoryStream(_mstream.GetBuffer()));
            PdfStamper stamper = new PdfStamper(reader, otherPagesStream);

            if (Traits.CreateTableOfContents) {
                //Retrieve bookmarks which essentially are the table of contents
                IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
                PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks/*, false*/);
                if (TOCReader != null)
                    AddTableOfContents(reader, stamper, TOCReader);
            }

            PdfPTable pageNumberTable = new PdfPTable(1);
            PdfPCell pageNumberCell = new PdfPCell();
            pageNumberCell.Border = 0;

            // insert page number
            for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                PdfContentByte cb = stamper.GetUnderContent(current);
                pageNumberTable = new PdfPTable(1);
                pageNumberCell.Phrase = new Phrase(current + " / " + reader.NumberOfPages, new Font(Font.FontFamily.HELVETICA, 8));
                pageNumberTable.SetTotalWidth(new float[] { 50 });
                pageNumberTable.AddCell(pageNumberCell);
                pageNumberTable.WriteSelectedRows(0, -1, (Traits.PageSizeAndOrientation.Width - pageNumberTable.TotalWidth) / 2, 40, cb);
            }

            // insert first page
            stamper.InsertPage(0, Traits.PageSizeAndOrientation);
            stamper.ReplacePage(firstPageReader, 1, 1);

            // insert watermark and footer
            for (int current = 1, last = reader.NumberOfPages; current <= last; current++) {
                //PdfContentByte cb = stamper.GetOverContent(current);
                PdfContentByte cb = stamper.GetUnderContent(current);
                watermark.Alignment = Image.UNDERLYING;
                if (current == 1) {
                    watermark.ScalePercent(33);
                    //watermark.SetAbsolutePosition(595 - watermark.ScaledWidth, 842 - watermark.ScaledHeight);
                    watermark.SetAbsolutePosition(Traits.PageSizeAndOrientation.Width - watermark.ScaledWidth, Traits.PageSizeAndOrientation.Height - watermark.ScaledHeight);
                    cb.AddImage(watermark);
                    watermark.ScalePercent(20);
                    footers[0].WriteSelectedRows(0, -1, 45, 40, cb);
                } else {
                    watermark.SetAbsolutePosition(Traits.PageSizeAndOrientation.Width - watermark.ScaledWidth, Traits.PageSizeAndOrientation.Height - watermark.ScaledHeight);
                    cb.AddImage(watermark);
                    footers[1].WriteSelectedRows(0, -1, (Traits.PageSizeAndOrientation.Width - footers[1].TotalWidth) / 2, 30, cb);
                }
            }
            stamper.Close();
            reader.Close();
        }

        #region Footers 
        private PdfPTable[] Footers() {
            PdfPTable[] footers = new PdfPTable[2];
            footers[0] = CreateFirstPageFooter();
            footers[1] = CreateOtherPagesFooter();
            return footers;
        }

        private PdfPTable CreateFirstPageFooter() {
            string[] firstPageFooter = new string[2];
            firstPageFooter[0] =
                "Geschäftsführer: Emanuel Böminghaus - AG Charlottenburg - HRB 89998 - USt-ID: DE230867342 - Steuernummer: 27/404/03642";
            firstPageFooter[1] =
                "Bankdaten: Raiffeisenbank Eschweiler - Kto: 280 105 7015 - BLZ: 393 622 54 - Swift/BIC: GENODED1RSC - IBAN: DE54 3936 2254 2801 0570 15";
            PdfPTable table = new PdfPTable(1);
            table.LockedWidth = true;
            table.SetTotalWidth(new float[] {Traits.PageSizeAndOrientation.Width});
            PdfPCell pdfCell = new PdfPCell();
            pdfCell.Border = Rectangle.NO_BORDER;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Phrase = new Phrase(firstPageFooter[0],
                                        new Font(Font.FontFamily.HELVETICA, 7, Font.NORMAL, BaseColor.GRAY));
            table.AddCell(pdfCell);
            pdfCell.Phrase = new Phrase(firstPageFooter[1],
                                        new Font(Font.FontFamily.HELVETICA, 7, Font.NORMAL, BaseColor.GRAY));
            table.AddCell(pdfCell);
            return table;
        }

        private PdfPTable CreateOtherPagesFooter() {
            string[] avenData = new string[6] {
                "AvenDATA GmbH", "Salzufer 8 - 10587 Berlin", "Telefon: +49.30.700.157.500",
                "Telefax: +49.30.700.157.599", "info@avendata.de", "www.avendata.de"
            };
            PdfPTable table = new PdfPTable(3);
            table.LockedWidth = true;
            table.SetTotalWidth(new float[] {70, 350, 70});
            PdfPCell pdfCell = new PdfPCell();
            pdfCell.Border = Rectangle.NO_BORDER;
            pdfCell.PaddingLeft = 0;
            pdfCell.PaddingRight = 0;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Phrase = new Phrase(avenData[0], new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD));
            table.AddCell(pdfCell);
            string middleString = " - " + avenData[1] + " - " + avenData[2] + " - " + avenData[3] + " - " + avenData[4] +
                                  " - ";
            pdfCell.Phrase = new Phrase(middleString,
                                        new Font(Font.FontFamily.HELVETICA, 7, Font.NORMAL, BaseColor.GRAY));
            table.AddCell(pdfCell);
            pdfCell.Phrase = new Phrase(avenData[5], new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD));
            table.AddCell(pdfCell);
            return table;
        }

        #endregion Footers
    }

    public class Headline {

        public Headline() {
            Children = new List<Headline>();
            Level = 1;
        }
        private Headline Parent { get; set; }
        public string Caption { get; set; }
        public List<Headline> Children { get; set; }
        public int Level { get; set; }
        public int Ordinal { get; set; }

        public string OrdinalNumber {
            get {
                string result = this.Ordinal.ToString();
                if (this.Parent != null) result = this.Parent.OrdinalNumber + "." + result;
                return result;
            }
        }

        public string DisplayString {
            get {
                string result = this.Ordinal.ToString();
                if (this.Parent != null) result = this.Parent.OrdinalNumber + "." + result;
                //return result + " " + Caption;
                return Caption;
            }
        }

        public Headline AddChildren(string caption) {
            Headline headline = new Headline { Caption = caption, Level = this.Level + 1, Parent = this, Ordinal = this.Children.Count + 1 };
            this.Children.Add(headline);
            return headline;
        }
    }
}
