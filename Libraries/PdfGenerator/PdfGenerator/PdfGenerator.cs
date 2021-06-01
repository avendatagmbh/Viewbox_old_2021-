using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PdfGenerator
{
	public class PdfGenerator
	{
		private static readonly Font italicFont = new Font(Font.FontFamily.HELVETICA, 12f, 2);

		private readonly List<Headline> _headlines = new List<Headline>();

		private Headline _currentHeadline;

		private Headline _currentSubHeadline;

		private Section _currentSubSection;

		private Section _currentSubSubSection;

		private readonly Dictionary<string, bool> _dontShowInTOC = new Dictionary<string, bool>();

		private readonly MemoryStream _mstream;

		private bool _newSubSub = true;

		private readonly PdfWriter _writer;

		private readonly CancellationToken token;

		private PdfTraits Traits { get; set; }

		public Document Document { get; private set; }

		private Chapter CurrentChapter { get; set; }

		private Section CurrentSection => _currentSubSection;

		public Section CurrentSubSection => _currentSubSubSection;

		private List<Headline> Headlines => _headlines;

		public PdfGenerator(PdfTraits traits, CancellationToken token)
		{
			Traits = traits;
			CurrentChapter = null;
			Init();
			_mstream = new MemoryStream();
			_writer = PdfWriter.GetInstance(Document, _mstream);
			_writer.ViewerPreferences = 128;
			this.token = token;
			Document.Open();
		}

		public virtual void WriteFile(Stream outStream, Phrase headerPhrase, bool addPageHeader = true, bool addViewboxStyle = false, List<Tuple<string, string>> optimizationTexts = null, Tuple<string, string> userText = null, string from = "", int noOfHorizontalPages = 1, string pageLoc = "page", string partLoc = "part", bool closing = true, int optimizationHidden = 0, bool bukrs = false, bool gjahr = false, List<Tuple<string, string, string>> tableOptions = null)
		{
			if (CurrentChapter != null)
			{
				Document.Add(CurrentChapter);
			}
			Document.Close();
			PdfReader reader = new PdfReader((Stream)new MemoryStream(_mstream.GetBuffer()));
			PdfStamper stamper = new PdfStamper(reader, outStream);
			if (!closing)
			{
				stamper.Writer.CloseStream = false;
			}
			if (Traits.CreateTableOfContents)
			{
				IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
				PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks);
				if (TOCReader != null)
				{
					AddTableOfContents(reader, stamper, TOCReader);
				}
			}
			if (addViewboxStyle)
			{
				AddViewboxStyle(reader, stamper, headerPhrase, optimizationTexts, userText, from, noOfHorizontalPages, pageLoc, partLoc, optimizationHidden, bukrs, gjahr, tableOptions);
			}
			else if (addPageHeader)
			{
				AddPageHeader(reader, stamper, headerPhrase);
			}
			else
			{
				AddFooter(reader, stamper, headerPhrase, optimizationTexts, userText, from, noOfHorizontalPages, pageLoc, partLoc, optimizationHidden);
			}
		}

		public virtual void WriteFile(Stream outStream, Phrase headerPhrase, PdfPTable headerContent, PdfPTable footerContent, Font sideFont, string sideString = "Seite", int sideOnHeaderOrFooter = 0, bool currentAndCompleteSideNr = true, string completeSideStringSeperator = " / ", float sideCellPaddingTop = 0f, bool printHeaderOnlyOnFirstPage = false, bool printFooterOnlyOnLastPage = false)
		{
			if (CurrentChapter != null)
			{
				Document.Add(CurrentChapter);
			}
			Document.Close();
			PdfReader reader = new PdfReader((Stream)new MemoryStream(_mstream.GetBuffer()));
			PdfStamper stamper = new PdfStamper(reader, outStream);
			if (Traits.CreateTableOfContents)
			{
				IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
				PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks);
				if (TOCReader != null)
				{
					AddTableOfContents(reader, stamper, TOCReader);
				}
			}
			AddHeader(reader, stamper, headerContent, sideString, sideFont, sideOnHeaderOrFooter, currentAndCompleteSideNr, completeSideStringSeperator, sideCellPaddingTop, printHeaderOnlyOnFirstPage);
			AddFooter(reader, stamper, footerContent, sideString, sideFont, sideOnHeaderOrFooter, currentAndCompleteSideNr, completeSideStringSeperator, sideCellPaddingTop, printFooterOnlyOnLastPage);
			stamper.Close();
		}

		public virtual void WriteFile(string filename, Phrase headerPhrase)
		{
			using FileStream stream = new FileStream(filename, FileMode.Create);
			WriteFile(stream, headerPhrase);
			stream.Close();
		}

		public static XElement PdfTableToElement(PdfPTable from)
		{
			return new XElement("root", PdfTableToElementRecursive(from));
		}

		private static XElement PdfTableToElementRecursive(PdfPTable from)
		{
			if (from == null)
			{
				return null;
			}
			XElement ret = null;
			bool firstTime = true;
			int i = 1;
			List<PdfPRow>.Enumerator iEnum = from.Rows.GetEnumerator();
			while (iEnum.MoveNext())
			{
				int j = 1;
				IEnumerator jEnum = iEnum.Current.GetCells().GetEnumerator();
				while (jEnum.MoveNext())
				{
					if (firstTime)
					{
						firstTime = false;
						ret = new XElement("a" + i + "_" + j);
					}
					ret.Add(PdfTableToElementRecursive(((PdfPCell)jEnum.Current).Table) ?? new XElement((((PdfPCell)jEnum.Current).Phrase.Count == 0) ? "empty" : "value", (((PdfPCell)jEnum.Current).Phrase.Count == 0) ? string.Empty : ((PdfPCell)jEnum.Current).Phrase[0].ToString()));
					j++;
				}
				i++;
			}
			return ret;
		}

		public void WriteFile(string filename, string headerString)
		{
			using FileStream stream = new FileStream(filename, FileMode.Create);
			WriteFile(stream, new Phrase(headerString));
			stream.Close();
		}

		protected virtual void Init()
		{
			Document = new Document(Traits.PageSizeAndOrientation);
			Document.AddAuthor(Traits.Author);
			Document.AddCreator(Traits.Creator);
			Document.AddCreationDate();
			Document.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);
		}

		public void AddHeadline(string caption)
		{
			Document.NewPage();
			_currentHeadline = new Headline
			{
				Ordinal = Headlines.Count + 1,
				Caption = caption
			};
			Headlines.Add(_currentHeadline);
			if (CurrentChapter != null)
			{
				Document.Add(CurrentChapter);
			}
			CurrentChapter = new Chapter(new Paragraph(_currentHeadline.DisplayString, Traits.fontH1), Headlines.Count);
		}

		private void AddSubHeadline(string caption, bool showInTOC)
		{
			_currentSubHeadline = _currentHeadline.AddChildren(caption);
			Paragraph p = new Paragraph(_currentSubHeadline.DisplayString, Traits.fontH2);
			p.SpacingBefore = 3f;
			if (!showInTOC)
			{
				_dontShowInTOC.Add(_currentSubHeadline.OrdinalNumber + ". " + _currentSubHeadline.DisplayString, value: false);
			}
			_currentSubSection = CurrentChapter.AddSection(p);
		}

		public void AddSubHeadline(string caption)
		{
			AddSubHeadline(caption, showInTOC: true);
		}

		public void AddSubSubHeadline(string caption)
		{
			_currentSubHeadline = _currentSubHeadline.AddChildren(caption);
			Paragraph p = new Paragraph(_currentSubHeadline.DisplayString, Traits.fontH3);
			p.SpacingBefore = 3f;
			_currentSubSubSection = CurrentSection.AddSection(p);
		}

		public void AddNewSubHeadline(int level, string label, Font font1 = null, Font font2 = null, Font font3 = null)
		{
			Headline child = _currentSubHeadline.AddChildren(label);
			level *= 3;
			if (level <= 3)
			{
				Paragraph p3 = new Paragraph(child.DisplayString, font1 ?? Traits.fontH2);
				p3.SpacingBefore = level;
				_currentSubSection = CurrentChapter.AddSection(p3);
				_newSubSub = true;
			}
			else if (level <= 6)
			{
				Paragraph p2 = new Paragraph(child.DisplayString, font2 ?? Traits.fontH3);
				p2.SpacingBefore = level;
				if (_newSubSub)
				{
					_currentSubSubSection = _currentSubSection.AddSection(p2);
					_newSubSub = false;
				}
				else
				{
					_currentSubSection.AddSection(p2);
				}
			}
			else
			{
				Paragraph p = new Paragraph(child.DisplayString, font3 ?? Traits.fontH4);
				p.SpacingBefore = level;
				_currentSubSubSection.AddSection(p);
				_newSubSub = true;
			}
		}

		private string GetCorrectPageNumber(string pageNumberString)
		{
			return Convert.ToString(Convert.ToInt32(pageNumberString.Split(' ')[0]) + 1);
		}

		[Obsolete("This Function is obsolete; use GenerateTableOfCompleteContents instead")]
		protected PdfReader GenerateTableOfContents(IEnumerable<Dictionary<string, object>> bookmarks)
		{
			Document TOC = new Document(PageSize.A4.Rotate());
			TOC.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);
			MemoryStream mstream = new MemoryStream();
			PdfWriter writerTOC = PdfWriter.GetInstance(TOC, mstream);
			TOC.Open();
			TOC.Add(new Paragraph("Inhaltsverzeichnis", Traits.fontH1));
			PdfPTable resultTable = new PdfPTable(new float[2] { 10f, 1f });
			resultTable.DefaultCell.Border = 0;
			resultTable.SpacingBefore = 5f;
			resultTable.SpacingAfter = 5f;
			resultTable.WidthPercentage = 95f;
			if (bookmarks == null)
			{
				PdfTraits traits = Traits;
				traits.CreateTableOfContents = false;
				Traits = traits;
				return null;
			}
			foreach (Dictionary<string, object> item in bookmarks)
			{
				resultTable.AddCell(new Paragraph((string)item["Title"], Traits.fontH1));
				resultTable.AddCell(GetCorrectPageNumber((string)item["Page"]));
				if (!item.ContainsKey("Kids"))
				{
					continue;
				}
				foreach (Dictionary<string, object> subItem in (IList<Dictionary<string, object>>)item["Kids"])
				{
					if (!_dontShowInTOC.ContainsKey((string)subItem["Title"]))
					{
						resultTable.AddCell(new Paragraph("\t\t" + (string)subItem["Title"], Traits.fontH2));
						resultTable.AddCell(GetCorrectPageNumber((string)subItem["Page"]));
					}
				}
			}
			TOC.Add(resultTable);
			TOC.Close();
			writerTOC.Close();
			return new PdfReader(mstream.GetBuffer());
		}

		private PdfReader GenerateTableOfCompleteContents(IList<Dictionary<string, object>> bookmarks)
		{
			Document TOC = new Document(PageSize.A4.Rotate());
			TOC.SetMargins(Traits.MarginLeft, Traits.MarginRight, Traits.MarginTop, Traits.MarginBottom);
			MemoryStream mstream = new MemoryStream();
			PdfWriter writerTOC = PdfWriter.GetInstance(TOC, mstream);
			TOC.Open();
			TOC.Add(new Paragraph("Inhaltsverzeichnis", Traits.fontH1));
			PdfPTable resultTable = new PdfPTable(new float[2] { 10f, 1f });
			resultTable.DefaultCell.Border = 0;
			resultTable.SpacingBefore = 5f;
			resultTable.SpacingAfter = 5f;
			resultTable.WidthPercentage = 95f;
			if (bookmarks == null)
			{
				PdfTraits traits = Traits;
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

		private void GetBookmarks(IEnumerable<Dictionary<string, object>> bookmarks, PdfPTable resultTable, int subCounter = 0)
		{
			string span = string.Empty;
			Font font = subCounter switch
			{
				-1 => Traits.fontH1, 
				0 => Traits.fontH1, 
				1 => Traits.fontH2, 
				2 => Traits.fontH3, 
				_ => Traits.fontH4, 
			};
			for (int i = 0; i < subCounter; i++)
			{
				span += "\t\t";
			}
			foreach (Dictionary<string, object> item in bookmarks)
			{
				resultTable.AddCell(new Paragraph(span + (string)item["Title"], font));
				resultTable.AddCell(GetCorrectPageNumber((string)item["Page"]));
				if (item.ContainsKey("Kids"))
				{
					subCounter++;
					GetBookmarks((IList<Dictionary<string, object>>)item["Kids"], resultTable, subCounter);
					subCounter--;
				}
			}
		}

		private void AddTableOfContents(PdfReader reader, PdfStamper stamper, PdfReader TOCReader)
		{
			for (int j = 0; j < TOCReader.NumberOfPages; j++)
			{
				stamper.InsertPage(0, Traits.PageSizeAndOrientation);
			}
			for (int i = 1; i <= TOCReader.NumberOfPages; i++)
			{
				stamper.ReplacePage(TOCReader, i, i);
			}
		}

		private void AddPageHeader(PdfReader reader, PdfStamper stamper, Phrase headerPhrase)
		{
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				token.ThrowIfCancellationRequested();
				PdfPTable header = new PdfPTable(2);
				header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				header.SetWidths(new float[2] { 10f, 1f });
				header.DefaultCell.HorizontalAlignment = 2;
				header.DefaultCell.VerticalAlignment = 6;
				header.DefaultCell.BorderWidth = 0f;
				header.DefaultCell.BorderWidthBottom = 0.25f;
				header.DefaultCell.PaddingBottom = 6f;
				PdfPCell cell = new PdfPCell(headerPhrase);
				cell.HorizontalAlignment = 0;
				cell.BorderWidth = 0f;
				cell.BorderWidthBottom = 0.25f;
				cell.PaddingBottom = 6f;
				header.AddCell(cell);
				if (current == 1 && Traits.CreateTableOfContents)
				{
					header.AddCell(new Phrase(""));
				}
				else
				{
					header.AddCell(new Phrase($"{current}/{last}", Traits.xfont));
				}
				PdfContentByte cb = stamper.GetOverContent(current);
				header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), Traits.PageSizeAndOrientation.GetTop(12.5f * Traits.mm()), cb);
			}
			stamper.Close();
		}

		private void AddViewboxStyle(PdfReader reader, PdfStamper stamper, Phrase headerPhrase, List<Tuple<string, string>> optimizationTexts, Tuple<string, string> userText, string from, int noOfHorizontalPages, string pageLoc, string partLoc, int optimizationHidden = 0, bool bukrs = false, bool gjahr = false, List<Tuple<string, string, string>> tableOptions = null)
		{
			BaseFont bf = BaseFont.CreateFont(HttpRuntime.AppDomainAppPath + "Content\\fonts\\ARIALUNI.TTF", "Identity-H", embedded: true);
			Font headLineTitle = new Font(bf, 15f, 1);
			Font headLineNormal = new Font(bf, 8f, 0);
			Font headLineNormalGrey = new Font(bf, 8f, 0, BaseColor.GRAY);
			int countPrefix = 0;
			int countSuffix = 0;
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				token.ThrowIfCancellationRequested();
				PdfPTable header = new PdfPTable(new float[2] { 0.45f, 0.55f });
				header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				Image instance = Image.GetInstance(Thread.GetDomain().BaseDirectory + "/Content/img/pdf/viewbox-logo.png");
				instance.ScaleToFit(170f, 23f);
				PdfPCell logoCell = new PdfPCell(instance, fit: false);
				logoCell.Border = 0;
				logoCell.Padding = 0f;
				header.AddCell(logoCell);
				PdfPTable nested = new PdfPTable(1);
				int i = 0;
				foreach (Tuple<string, string> o in optimizationTexts)
				{
					if ((i == 2 && !bukrs) || (i == 3 && !gjahr))
					{
						i++;
						continue;
					}
					if (i < 2 || optimizationHidden == 0 || i > 3)
					{
						if (o.Item2 == string.Empty)
						{
							continue;
						}
						Chunk key = new Chunk(o.Item1.Trim() + ": ", headLineNormalGrey);
						Chunk value = new Chunk(o.Item2.Trim(), headLineNormal);
						PdfPCell optCell2 = new PdfPCell(new Phrase { key, value });
						optCell2.HorizontalAlignment = 2;
						optCell2.VerticalAlignment = 4;
						optCell2.Border = 0;
						optCell2.Padding = 1f;
						nested.AddCell(optCell2);
					}
					i++;
				}
				Chunk userKey = new Chunk(userText.Item1 + ": ", headLineNormalGrey);
				Chunk userValue = new Chunk(userText.Item2, headLineNormal);
				PdfPCell userCell = new PdfPCell(new Phrase { userKey, userValue });
				userCell.HorizontalAlignment = 2;
				userCell.VerticalAlignment = 4;
				userCell.Border = 0;
				userCell.Padding = 1f;
				nested.AddCell(userCell);
				for (; i < 4; i++)
				{
					PdfPCell optCell = new PdfPCell();
					optCell.HorizontalAlignment = 2;
					optCell.VerticalAlignment = 4;
					optCell.Border = 0;
					optCell.Padding = 1f;
					optCell.FixedHeight = 8f;
					nested.AddCell(optCell);
				}
				nested.ExtendLastRow = true;
				PdfPCell nesthousing = new PdfPCell(nested);
				nesthousing.Padding = 0f;
				nesthousing.Border = 0;
				header.AddCell(nesthousing);
				headerPhrase.Font = headLineTitle;
				PdfPCell titleCell = new PdfPCell(new Phrase(headerPhrase.Content, headLineTitle));
				titleCell.Colspan = 2;
				titleCell.Border = 0;
				titleCell.Padding = 0f;
				header.AddCell(titleCell);
				if (tableOptions != null)
				{
					foreach (Tuple<string, string, string> item in tableOptions)
					{
						PdfPCell valueCell = new PdfPCell(new Phrase(string.IsNullOrWhiteSpace(item.Item2) ? item.Item3 : (item.Item2 + " : " + item.Item3), headLineNormalGrey));
						valueCell.Colspan = 2;
						valueCell.Border = 0;
						valueCell.Padding = 1f;
						header.AddCell(valueCell);
					}
				}
				PdfContentByte cb = stamper.GetOverContent(current);
				header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), Traits.PageSizeAndOrientation.GetTop(20f), cb);
				PdfPTable footer = new PdfPTable(1);
				footer.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				Phrase footerText = ((noOfHorizontalPages != 1) ? new Phrase(partLoc + " " + (countSuffix + 1) + " / " + noOfHorizontalPages + ", " + pageLoc + " " + (countPrefix + 1) + " / " + last, headLineNormalGrey) : new Phrase(pageLoc + " " + current + " " + from + " " + last, headLineNormalGrey));
				PdfPCell pageCell = new PdfPCell(footerText);
				pageCell.HorizontalAlignment = 2;
				pageCell.Border = 0;
				pageCell.Padding = 0f;
				footer.AddCell(pageCell);
				footer.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), 50f, cb);
				countSuffix++;
				countPrefix++;
				if (noOfHorizontalPages > 1 && countSuffix % noOfHorizontalPages == 0)
				{
					countSuffix = 0;
				}
			}
			stamper.Close();
		}

		private void AddFooter(PdfReader reader, PdfStamper stamper, Phrase headerPhrase, List<Tuple<string, string>> optimizationTexts, Tuple<string, string> userText, string from, int noOfHorizontalPages, string pageLoc, string partLoc, int optimizationHidden = 0)
		{
			BaseFont bf = BaseFont.CreateFont(HttpRuntime.AppDomainAppPath + "Content\\fonts\\ARIALUNI.TTF", "Identity-H", embedded: true);
			new Font(bf, 15f, 1);
			new Font(bf, 8f, 0);
			Font headLineNormalGrey = new Font(bf, 8f, 0, BaseColor.GRAY);
			int countPrefix = 0;
			int countSuffix = 0;
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				token.ThrowIfCancellationRequested();
				PdfContentByte cb = stamper.GetOverContent(current);
				PdfPTable pdfPTable = new PdfPTable(1);
				pdfPTable.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				pdfPTable.AddCell(new PdfPCell(new Phrase($"Seite {countSuffix + 1}", headLineNormalGrey))
				{
					HorizontalAlignment = 2,
					Border = 0,
					Padding = 0f
				});
				pdfPTable.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), 50f, cb);
				countSuffix++;
				countPrefix++;
				if (noOfHorizontalPages > 1 && countSuffix % noOfHorizontalPages == 0)
				{
					countSuffix = 0;
				}
			}
			stamper.Close();
		}

		private void AddHeader(PdfReader reader, PdfStamper stamper, PdfPTable headerContent, string sideString, Font sideFont, int sideOnHeaderOrFooter, bool currentAndCompleteSideNr, string completeSideStringSeperator, float sideCellPaddingTop, bool printHeaderOnlyOnFirstPage)
		{
			if (printHeaderOnlyOnFirstPage)
			{
				PdfPTable pdfPTable = new PdfPTable(1);
				pdfPTable.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				pdfPTable.DefaultCell.Border = 0;
				pdfPTable.DefaultCell.Padding = 0f;
				pdfPTable.AddCell(headerContent);
				pdfPTable.ExtendLastRow = true;
				pdfPTable.WriteSelectedRows(canvas: stamper.GetOverContent(1), rowStart: 0, rowEnd: -1, xPos: Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), yPos: Traits.PageSizeAndOrientation.GetTop(20f));
				return;
			}
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				token.ThrowIfCancellationRequested();
				PdfPTable header = new PdfPTable(1);
				header.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				header.DefaultCell.Border = 0;
				header.DefaultCell.Padding = 0f;
				if (sideOnHeaderOrFooter == 1)
				{
					string side = sideString + " " + current;
					if (currentAndCompleteSideNr)
					{
						side = side + completeSideStringSeperator + last;
					}
					PdfPCell sideCell = new PdfPCell(new Phrase(side, sideFont));
					sideCell.HorizontalAlignment = 2;
					sideCell.VerticalAlignment = 5;
					sideCell.Border = 0;
					sideCell.PaddingTop = sideCellPaddingTop;
					header.AddCell(sideCell);
				}
				header.AddCell(headerContent);
				header.ExtendLastRow = true;
				PdfContentByte cb = stamper.GetOverContent(current);
				header.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), Traits.PageSizeAndOrientation.GetTop(20f), cb);
			}
		}

		private void AddFooter(PdfReader reader, PdfStamper stamper, PdfPTable footerContent, string sideString, Font sideFont, int sideOnHeaderOrFooter, bool currentAndCompleteSideNr, string completeSideStringSeperator, float sideCellPaddingTop, bool printFooterOnlyOnLastPage)
		{
			if (printFooterOnlyOnLastPage)
			{
				PdfPTable footer2 = new PdfPTable(1);
				footer2.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				footer2.DefaultCell.Border = 0;
				footer2.DefaultCell.Padding = 0f;
				footer2.AddCell(footerContent);
				footer2.ExtendLastRow = true;
				PdfContentByte cb2 = stamper.GetOverContent(reader.NumberOfPages);
				footer2.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), CalculatePdfPTableHeight(footer2) + 20f, cb2);
				return;
			}
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				token.ThrowIfCancellationRequested();
				PdfPTable footer = new PdfPTable(1);
				footer.TotalWidth = Document.PageSize.Width - Traits.MarginLeft - Traits.MarginRight;
				footer.DefaultCell.Border = 0;
				footer.DefaultCell.Padding = 0f;
				if (sideOnHeaderOrFooter == 2)
				{
					string side = sideString + " " + current;
					if (currentAndCompleteSideNr)
					{
						side = side + completeSideStringSeperator + last;
					}
					PdfPCell sideCell = new PdfPCell(new Phrase(side, sideFont));
					sideCell.HorizontalAlignment = 1;
					sideCell.VerticalAlignment = 5;
					sideCell.Border = 0;
					sideCell.PaddingTop = sideCellPaddingTop;
					footer.AddCell(sideCell);
					PdfPCell emptyCell = new PdfPCell();
					emptyCell.Border = 0;
					emptyCell.FixedHeight = 10f;
					footer.AddCell(emptyCell);
				}
				footer.AddCell(footerContent);
				footer.ExtendLastRow = true;
				PdfContentByte cb = stamper.GetOverContent(current);
				footer.WriteSelectedRows(0, -1, Traits.PageSizeAndOrientation.GetLeft(Traits.MarginLeft), CalculatePdfPTableHeight(footer) + 20f, cb);
			}
		}

		private static PdfPCell HeaderCell(string content, Font usedFont)
		{
			return new PdfPCell(new Phrase(content, usedFont))
			{
				BackgroundColor = new BaseColor(240, 240, 240)
			};
		}

		public void AddTable(DataTable dataTable, bool writeHeader = true, int spacingBefore = 0, int spacingAfter = 0)
		{
			PdfPTable table = new PdfPTable(dataTable.Columns.Count)
			{
				SpacingBefore = spacingBefore,
				SpacingAfter = spacingAfter,
				WidthPercentage = 100f
			};
			if (writeHeader)
			{
				for (int j = 0; j < dataTable.Columns.Count; j++)
				{
					table.AddCell(HeaderCell(dataTable.Columns[j].ColumnName, italicFont));
				}
			}
			foreach (DataRow row in dataTable.Rows)
			{
				for (int i = 0; i < dataTable.Columns.Count; i++)
				{
					table.AddCell(new PdfPCell(new Paragraph(row[i].ToString(), new Font(Font.FontFamily.HELVETICA, 10f, 0))));
				}
			}
			CurrentChapter.Add(table);
		}

		public float CalculatePdfPTableHeight(PdfPTable table)
		{
			using MemoryStream ms = new MemoryStream();
			Document document = new Document(PageSize.TABLOID);
			PdfWriter w = PdfWriter.GetInstance(document, ms);
			document.Open();
			table.WriteSelectedRows(0, table.Rows.Count, 0f, 0f, w.DirectContent);
			document.Close();
			w.Close();
			return table.TotalHeight;
		}

		public virtual void WriteFile(PdfReader firstPageReader, Stream otherPagesStream, Image watermark)
		{
			PdfPTable[] footers = Footers();
			if (CurrentChapter != null)
			{
				Document.Add(CurrentChapter);
			}
			Document.Close();
			PdfReader reader = new PdfReader((Stream)new MemoryStream(_mstream.GetBuffer()));
			PdfStamper stamper = new PdfStamper(reader, otherPagesStream);
			if (Traits.CreateTableOfContents)
			{
				IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(new PdfReader(_mstream.GetBuffer()));
				PdfReader TOCReader = GenerateTableOfCompleteContents(bookmarks);
				if (TOCReader != null)
				{
					AddTableOfContents(reader, stamper, TOCReader);
				}
			}
			PdfPTable pageNumberTable = new PdfPTable(1);
			PdfPCell pageNumberCell = new PdfPCell();
			pageNumberCell.Border = 0;
			int current2 = 1;
			for (int last2 = reader.NumberOfPages; current2 <= last2; current2++)
			{
				PdfContentByte cb = stamper.GetUnderContent(current2);
				pageNumberTable = new PdfPTable(1);
				pageNumberCell.Phrase = new Phrase(current2 + " / " + reader.NumberOfPages, new Font(Font.FontFamily.HELVETICA, 8f));
				pageNumberTable.SetTotalWidth(new float[1] { 50f });
				pageNumberTable.AddCell(pageNumberCell);
				pageNumberTable.WriteSelectedRows(0, -1, (Traits.PageSizeAndOrientation.Width - pageNumberTable.TotalWidth) / 2f, 40f, cb);
			}
			stamper.InsertPage(0, Traits.PageSizeAndOrientation);
			stamper.ReplacePage(firstPageReader, 1, 1);
			int current = 1;
			for (int last = reader.NumberOfPages; current <= last; current++)
			{
				PdfContentByte cb2 = stamper.GetUnderContent(current);
				watermark.Alignment = 8;
				if (current == 1)
				{
					watermark.ScalePercent(33f);
					watermark.SetAbsolutePosition(Traits.PageSizeAndOrientation.Width - watermark.ScaledWidth, Traits.PageSizeAndOrientation.Height - watermark.ScaledHeight);
					cb2.AddImage(watermark);
					watermark.ScalePercent(20f);
					footers[0].WriteSelectedRows(0, -1, 45f, 40f, cb2);
				}
				else
				{
					watermark.SetAbsolutePosition(Traits.PageSizeAndOrientation.Width - watermark.ScaledWidth, Traits.PageSizeAndOrientation.Height - watermark.ScaledHeight);
					cb2.AddImage(watermark);
					footers[1].WriteSelectedRows(0, -1, (Traits.PageSizeAndOrientation.Width - footers[1].TotalWidth) / 2f, 30f, cb2);
				}
			}
			stamper.Close();
			reader.Close();
		}

		private PdfPTable[] Footers()
		{
			return new PdfPTable[2]
			{
				CreateFirstPageFooter(),
				CreateOtherPagesFooter()
			};
		}

		private PdfPTable CreateFirstPageFooter()
		{
			string[] firstPageFooter = new string[2] { "Geschäftsführer: Emanuel Böminghaus - AG Charlottenburg - HRB 89998 - USt-ID: DE230867342 - Steuernummer: 27/404/03642", "Bankdaten: Raiffeisenbank Eschweiler - Kto: 280 105 7015 - BLZ: 393 622 54 - Swift/BIC: GENODED1RSC - IBAN: DE54 3936 2254 2801 0570 15" };
			PdfPTable table = new PdfPTable(1);
			table.LockedWidth = true;
			table.SetTotalWidth(new float[1] { Traits.PageSizeAndOrientation.Width });
			PdfPCell pdfCell = new PdfPCell();
			pdfCell.Border = 0;
			pdfCell.HorizontalAlignment = 1;
			pdfCell.Phrase = new Phrase(firstPageFooter[0], new Font(Font.FontFamily.HELVETICA, 7f, 0, BaseColor.GRAY));
			table.AddCell(pdfCell);
			pdfCell.Phrase = new Phrase(firstPageFooter[1], new Font(Font.FontFamily.HELVETICA, 7f, 0, BaseColor.GRAY));
			table.AddCell(pdfCell);
			return table;
		}

		private PdfPTable CreateOtherPagesFooter()
		{
			string[] avenData = new string[6] { "AvenDATA GmbH", "Salzufer 8 - 10587 Berlin", "Telefon: +49.30.700.157.500", "Telefax: +49.30.700.157.599", "info@avendata.de", "www.avendata.de" };
			PdfPTable table = new PdfPTable(3);
			table.LockedWidth = true;
			table.SetTotalWidth(new float[3] { 70f, 350f, 70f });
			PdfPCell pdfCell = new PdfPCell();
			pdfCell.Border = 0;
			pdfCell.PaddingLeft = 0f;
			pdfCell.PaddingRight = 0f;
			pdfCell.HorizontalAlignment = 1;
			pdfCell.Phrase = new Phrase(avenData[0], new Font(Font.FontFamily.HELVETICA, 7f, 1));
			table.AddCell(pdfCell);
			string middleString = " - " + avenData[1] + " - " + avenData[2] + " - " + avenData[3] + " - " + avenData[4] + " - ";
			pdfCell.Phrase = new Phrase(middleString, new Font(Font.FontFamily.HELVETICA, 7f, 0, BaseColor.GRAY));
			table.AddCell(pdfCell);
			pdfCell.Phrase = new Phrase(avenData[5], new Font(Font.FontFamily.HELVETICA, 7f, 1));
			table.AddCell(pdfCell);
			return table;
		}
	}
}
