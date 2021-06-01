using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using SystemDb;
using EO.Pdf;
using EO.Pdf.Acm;
using Utils;
using Resx = ViewBuilderBusiness.Resources.Resource;

namespace ViewBuilderBusiness.Reports
{

    #region Licence

    /// <summary>
    ///   Licence for EO.Pdf stuff.
    /// </summary>
    public static class Licence
    {
        /// <summary>
        ///   Adds the licence.
        /// </summary>
        public static void AddLicence()
        {
            try
            {
                Runtime.AddLicense(
                    "m/Gd3PbaGeWol+jyH+R2mbXA3LNoqbTC3qFZ7ekDHuio5cGz36FZpsKetZ9Z" +
                    "l6TNHuig5eUFIPGetbX+2s6D6sXCAOyauu4EFa6bz+YB9cB2tMDAHuig5eUF" +
                    "IPGetZGb566l4Of2GfKetZGbdePt9BDtrNzCnrWfWZekzRfonNzyBBDInbW4" +
                    "yt6ycK+2xOS3dabw+g7kp+rp2g+9RoGkscufdePt9BDtrNzpz+eupeDn9hny" +
                    "ntzCnrWfWZekzQzrpeb7z7iJWZekscufWZfA8g/jWev9ARC8W7zTv/vjn5mk" +
                    "BxDxrODz/+ihbaW0s8uud4SOscufWbOz8hfrqO7CnrWfWZekzRo=");
            }
            catch (Exception ex)
            {
                Debug.Assert(true, ex.Message);
            }
        }
    }

    #endregion Licence

    public class PDFReportGenerator
    {
        private const int _progressFactor = 1;
        private readonly PdfDocument _document;
        private readonly string _fileName;
        private readonly string _projectManager;
        private readonly string _projectNumber;
        private readonly ISystemDb _systemDb;
        private readonly TOCHelper _toc;
        private PdfPage _frontPage;
        private ProgressCalculator _progress;

        public PDFReportGenerator(string fileName, ISystemDb systemDb)
        {
            Licence.AddLicence();
            _fileName = fileName;
            _systemDb = systemDb;
            _toc = new TOCHelper();
            _document = new PdfDocument();
            TOCHelper.Document = _document;
        }

        public PDFReportGenerator(string fileName, SystemDb.SystemDb systemDb, string projectManager,
                                  string projectNumber)
            : this(fileName, systemDb)
        {
            _projectManager = projectManager ?? "";
            _projectNumber = projectNumber ?? "";
        }

        /// <summary>
        ///   creates and add pages, if page param is null, creates a new page othewise uses the provided page. Renders header and footer for all pages.
        /// </summary>
        /// <param name="page"> </param>
        /// <param name="contents"> </param>
        /// <returns> </returns>
        private PdfPage CreateAndAddPage(PdfPage page, params AcmContent[] contents)
        {
            AcmPageLayout layout = null;
            PdfPage currentpage = null;
            if (page == null)
            {
                currentpage = new PdfPage();
                layout = new AcmPageLayout(new AcmPadding(1f, 1.3f, 0f, 1f));
                _document.Pages.Add(currentpage);
            }
            else
            {
                layout = new AcmPageLayout(new AcmPadding(1f, 1.7f, 0f, 1f));
                currentpage = page;
            }
            var bodyRender = new AcmRender(currentpage, 0, layout);
            if (page == null)
            {
                bodyRender.BeforeRenderPage += HeaderContent;
                bodyRender.AfterRenderPage += FooterContent;
            }
            bodyRender.Render(contents);
            return currentpage;
        }

        private void FooterContent(object sender, AcmPageEventArgs e)
        {
            var pageNumber = new AcmBlock(new AcmText(Resx.Page + " " + (e.Page.Index + 1))).SetProperty(
                "Style.FontSize", 7f).SetProperty("Style.HorizontalAlign", AcmHorizontalAlign.Center);
            var bottomLine = new AcmBlock(
                GetLinedTexts(
                    Resx.Geschäftsführer +
                    ": Emanuel Böminghaus - AG Charlottenburg - HRB 89998 - USt-ID: DE230867342 - " + Resx.Steuernummer +
                    ": 27/404/03642",
                    Resx.Bankdaten +
                    ": Raiffeisenbank Eschweiler - Kto: 280 105 7015 - BLZ: 393 622 54 - Swift/BIC: GENODED1RSC - IBAN: DE54 3936 2254 2801 0570 15"));
            bottomLine.Style.ForegroundColor = Color.Gray;
            bottomLine.Style.FontSize = 7f;
            bottomLine.Style.HorizontalAlign = AcmHorizontalAlign.Center;
            var bottomLineRender = new AcmRender(e.Page, 10.5f, new AcmPageLayout(new AcmPadding(0f, 0f, 0f, 0)));
            bottomLineRender.Render(pageNumber, bottomLine);
        }

        private void HeaderContent(object sender, AcmPageEventArgs e)
        {
            if (_frontPage == e.Page) return;
            AcmRender imageRender = new AcmRender(e.Page, 0, new AcmPageLayout(new AcmPadding(4.8f, 0f, 0f, 0f)));
            imageRender.Render(new AcmContent(new AcmImage(Resx.logo) {ScaleX = 0.3f, ScaleY = 0.3f}));
        }

        public void CreateAndSaveDocument(ProgressCalculator progress)
        {
            _progress = progress;
            var totalSteps = ((_systemDb.Issues.Count + _systemDb.Views.Count)/_progressFactor) + 3;
            _progress.SetWorkSteps(totalSteps, false);
            CreateFrontPage(null, FooterContent);
            _progress.StepDone(); //step 1
            PrepareTOCPage();
            CreateOverviewPage();
            _progress.StepDone(); //step 2
            CreateViewsPages();
            CreatReportPages();
            RenderTOCPage();
            _progress.SetStep(totalSteps); //step done
            _document.Save(_fileName);
        }

        private void PrepareTOCPage()
        {
            // sets the pages number required to fit the full TOC
            _toc.PageCount = (int) Math.Ceiling((_systemDb.Issues.Count + _systemDb.Views.Count)/(float) 39);
            // creates pages
            for (int i = 0; i < _toc.PageCount; i++)
            {
                var text = "";
                if (i == 0) text = Resx.Inhaltsverzeichnis;
                CreateAndAddPage(null, new AcmBlock(new AcmBold(new AcmText(text))));
            }
        }

        private void RenderTOCPage()
        {
            // for each prepared TOC pages rendres the TOC part that fits a page
            for (int i = 0; i < _toc.PageCount; i++)
            {
                var bodyRender = new AcmRender(_document.Pages[i + 1], 0,
                                               new AcmPageLayout(new AcmPadding(1.1f, 1.5f, 0f, 1f)));
                bodyRender.Render(_toc.GetTOC(i));
            }
        }

        public void CreateViewsPages()
        {
            // creates title
            var page = CreateAndAddPage(null,
                                        _toc.GetTitle("2.", Resx.Views, _document.Pages.Count));
            int i = 0;
            foreach (var view in _systemDb.Views)
            {
                i++;
                // creates a table the view details
                var detailsTable = new EOPdfTableHelper(new float[] {2, 2});
                detailsTable.AddRow(new[] {Resx.Viewbezeichnung + ":", view.TableName});
                if (!string.IsNullOrEmpty(view.Descriptions[_systemDb.DefaultLanguage]))
                    detailsTable.AddRow(new[] {Resx.Beschreibung + ":", view.Descriptions[_systemDb.DefaultLanguage]});
                detailsTable.AddRow(new[] {Resx.ErstelltDurch + ":", ""});
                // creates the view table
                var viewTable = new EOPdfTableHelper(new[] {"#", Resx.Feldname, Resx.Beschreibung},
                                                     new[] {0.6f, 3.5f, 2.3f},
                                                     new object[]
                                                         {
                                                             AcmHorizontalAlign.Center, AcmHorizontalAlign.Left,
                                                             AcmHorizontalAlign.Left
                                                         }) {ShowBorder = true};
                int j = 0;
                foreach (var column in view.Columns)
                {
                    j++;
                    viewTable.AddRow(new[] {j.ToString(), column.Name, column.Descriptions[_systemDb.DefaultLanguage]});
                }
                // renders all
                CreateAndAddPage(page,
                                 _toc.GetTitle("2." + i + ".", view.Descriptions[_systemDb.DefaultLanguage],
                                               _document.Pages.Count),
                                 detailsTable.GetTable()
                                     .SetProperty("Style.Padding.Left", 0.5f)
                                     .SetProperty("Style.Padding.Bottom", 0f)
                                     .SetProperty("Style.FontSize", 9f),
                                 viewTable.GetTable());
                // forces a new page to be created in the next iteration
                page = null;
                if (i%_progressFactor == 0)
                {
                    _progress.StepDone();
                }
            }
        }

        public void CreatReportPages()
        {
            var page = CreateAndAddPage(null,
                                        _toc.GetTitle("3.", Resx.Sachverhalte, _document.Pages.Count));
            int i = 0;
            foreach (var issue in _systemDb.Issues)
            {
                i++;
                var detailsTable = new EOPdfTableHelper(new float[] {2, 2});
                detailsTable.AddRow(new[] {Resx.Viewbezeichnung + ":", issue.TableName});
                if (!string.IsNullOrEmpty(issue.Descriptions[_systemDb.DefaultLanguage]))
                    detailsTable.AddRow(new[] {Resx.Beschreibung + ":", issue.Descriptions[_systemDb.DefaultLanguage]});
                detailsTable.AddRow(new[] {Resx.ErstelltDurch + ":", ""});
                var reportTable = new EOPdfTableHelper(new[] {"#", Resx.Feldname, Resx.Beschreibung},
                                                       new[] {0.6f, 3.5f, 2.3f},
                                                       new object[]
                                                           {
                                                               AcmHorizontalAlign.Center, AcmHorizontalAlign.Left,
                                                               AcmHorizontalAlign.Left
                                                           }) {ShowBorder = true};
                int j = 0;
                foreach (var column in issue.Columns)
                {
                    j++;
                    reportTable.AddRow(new[] {j.ToString(), column.Name, column.Descriptions[_systemDb.DefaultLanguage]});
                }
                // displays the parameters table
                var parameterTable =
                    new EOPdfTableHelper(new[] {"#", Resx.Parametername, Resx.Parameterbeschreibung, Resx.Datentyp},
                                         new[] {0.6f, 2.5f, 2.5f, 1f},
                                         new object[]
                                             {
                                                 AcmHorizontalAlign.Center, AcmHorizontalAlign.Left,
                                                 AcmHorizontalAlign.Left, AcmHorizontalAlign.Left
                                             }) {ShowBorder = true};
                j = 0;
                foreach (var parameter in issue.Parameters)
                {
                    j++;
                    parameterTable.AddRow(new[]
                                              {
                                                  j.ToString(), parameter.Name,
                                                  parameter.Descriptions[_systemDb.DefaultLanguage],
                                                  parameter.DataType.ToString()
                                              });
                }
                CreateAndAddPage(page,
                                 _toc.GetTitle("3." + i + ".", issue.Descriptions[_systemDb.DefaultLanguage],
                                               _document.Pages.Count),
                                 detailsTable.GetTable()
                                     .SetProperty("Style.Padding.Left", 0.5f)
                                     .SetProperty("Style.Padding.Bottom", 0f)
                                     .SetProperty("Style.FontSize", 9f),
                                 reportTable.GetTable(),
                                 parameterTable.GetTable());
                // forces a new page to be created in the next iteration
                page = null;
                if (i%_progressFactor == 0)
                {
                    _progress.StepDone();
                }
            }
        }

        private void CreateOverviewPage()
        {
            // 
            // Creates reports summary table
            //
            var reportTableHelper = new EOPdfTableHelper(new[] {"#", Resx.Name}, new[] {0.6f, 6f},
                                                         new object[]
                                                             {AcmHorizontalAlign.Center, AcmHorizontalAlign.Left})
                                        {ShowBorder = true};
            int i = 0;
            foreach (var issue in _systemDb.Issues)
            {
                // description column
                //   gets the description in the appropriate language
                var description = issue.Descriptions[_systemDb.DefaultLanguage];
                i++;
                reportTableHelper.AddRow(new[] {i.ToString(), description});
            }
            //
            // Creates views summary tables
            //
            var viewTableHelper = new EOPdfTableHelper(new[] {"#", Resx.Name}, new[] {0.6f, 6f},
                                                       new object[] {AcmHorizontalAlign.Center, AcmHorizontalAlign.Left})
                                      {ShowBorder = true};
            i = 0;
            foreach (var view in _systemDb.Views)
            {
                // description column
                //   gets the description in the appropriate language
                var description = view.Descriptions[_systemDb.DefaultLanguage];
                i++;
                viewTableHelper.AddRow(new[] {i.ToString(), description});
            }
            var page = CreateAndAddPage(null,
                                        _toc.GetTitle("1.", Resx.Vorwort, _document.Pages.Count),
                                        new AcmLineBreak(),
                                        new AcmText(Resx.VorwortText),
                                        reportTableHelper.GetTable(),
                                        new AcmBlock(new AcmBold(new AcmText(Resx.Tabelle + ": " + Resx.ViewSummary))).
                                            SetProperty("Style.Padding.Left", 1.5f),
                                        new AcmLineBreak(),
                                        new AcmText(Resx.IssueText),
                                        viewTableHelper.GetTable(),
                                        new AcmBlock(new AcmBold(new AcmText(Resx.Tabelle + ": " + Resx.IssueSummary))).
                                            SetProperty("Style.Padding.Left", 1.5f));
        }

        private void CreateFrontPage(AcmPageEventHandler renderBefore, AcmPageEventHandler renderAfter)
        {
            _frontPage = new PdfPage();
            _document.Pages.Insert(0, _frontPage);
            AcmRender titleRender = new AcmRender(_frontPage, 0, new AcmPageLayout(new AcmPadding(1f, 1.8f, 0f, 0f)));
            titleRender.BeforeRenderPage += (object sender, AcmPageEventArgs e) =>
                                                {
                                                    if (e.Page.Equals(_frontPage))
                                                    {
                                                        AcmRender imageRender = new AcmRender(e.Page, 0,
                                                                                              new AcmPageLayout(
                                                                                                  new AcmPadding(3.0f,
                                                                                                                 0f, 0f,
                                                                                                                 0f)));
                                                        imageRender.Render(
                                                            new AcmContent(new AcmImage(Resx.logo)
                                                                               {ScaleX = 0.45f, ScaleY = 0.45f}));
                                                    }
                                                };
            //if (renderAfter != null) titleRender.AfterRenderPage += renderAfter;
            var title = new AcmBlock(
                new AcmBold(new AcmText("AvenDATA GmbH ").SetProperty("Style.FontSize", 10f)),
                new AcmText("Salzufer 8 - 10587 Berlin").SetProperty("Style.FontSize", 8f));
            titleRender.Render(title);
            AcmRender detailsRender = new AcmRender(_frontPage, 0, new AcmPageLayout(new AcmPadding(6.5f, 1.8f, 0f, 0f)));
            var details = new AcmBlock(GetLinedTexts("AvenDATA GmbH",
                                                     "Salzufer 8 - 10587 Berlin").SetProperty("Style.Margin.Bottom",
                                                                                              0.1f),
                                       GetLinedTexts("Telefon: +49-(0)30/7 00 15 75 00",
                                                     "Telefax: +49-(0)30/7 00 15 75 99").SetProperty(
                                                         "Style.Margin.Bottom", 0.1f),
                                       GetLinedTexts("info@avendata.de",
                                                     "www.avendata.de").SetProperty("Style.Margin.Bottom", 0.1f),
                                       new AcmLineBreak(),
                                       GetLinedTexts(Resx.Bearbeiter + ": " + _projectManager,
                                                     Resx.Versionsnummer + ": 1.0",
                                                     Resx.Projektnummer + ": " + _projectNumber,
                                                     Resx.Anlage + " " + Resx.Viewübersicht));
            details.Style.FontSize = 8f;
            detailsRender.Render(details);
        }

        /// <summary>
        ///   Helper method to create text with line break
        /// </summary>
        /// <param name="texts"> </param>
        /// <returns> </returns>
        private AcmContent GetLinedTexts(params string[] texts)
        {
            List<AcmContent> contents = new List<AcmContent>();
            for (int i = 0; i < texts.Length; i++)
            {
                contents.Add(new AcmText(texts[i]));
                if (texts.Length - 1 > i) contents.Add(new AcmLineBreak());
            }
            return new AcmBlock(contents.ToArray());
        }
    }
}