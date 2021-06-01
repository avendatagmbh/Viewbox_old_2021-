using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Job.PdfExport.Layouts;

namespace Viewbox.Test.PdfExport
{
    /// <summary>
    ///   Entering point to test the result of the generated PDFs.
    /// </summary>
    [TestClass]
    public class PdfLayoutTest
    {
        [TestMethod]
        public void GenerateActebisFacturaPdfTest()
        {
            var layout = new FacturaPdfLayout();
            const string fileName = "ActebisFacturaPdfTest.pdf";
            layout.GeneratePdf(new FileStream(fileName, FileMode.Create));
            Process.Start(fileName);
        }

        [TestMethod]
        public void GenerateOrderPdfTest()
        {
            var layout = new OrderPdfLayout
                             {
                                 Betalingsbetingelser = "Betalingsbetingelser",
                                 Fakturaadresse1 = "IMENTO NORGE AS",
                                 Fakturaadresse2 = "",
                                 Fakturaadresse3 = "CARLBERGVEIEN 6",
                                 Fakturaadresse4 = "1526",
                                 Fakturaadresse5 = "MOSS",
                                 Fakturaadresse6 = "Norway",
                                 FolgeseddelPaPapier = "FolgeseddelPaPapier",
                                 Kundenavn = "Kundenavn",
                                 KundensRef = "Ref",
                                 Kundenummer = "3423434234",
                                 EstLeveringsdato = "01.03.2011",
                                 OnsketLeveringsdato = "25.02.2011",
                                 SumEksMva = "18,630",
                                 EstimertFraktkostnad = "64,35",
                                 MinimumOrdregebyr = "0,00",
                                 Leveringsadresse1 = "Forsvarets Virksomhetsomårde",
                                 Leveringsadresse2 = "Bjørn Kordahl",
                                 Leveringsadresse3 = "Bygg 66",
                                 Leveringsadresse4 = "0015",
                                 Leveringsadresse5 = "OSLO",
                                 Leveringsadresse6 = "Norway",
                                 Leveringsbetingelser = "rrrrrrtret",
                                 Leveringsinfo = "dfdsfdsf",
                                 Leveringsmetode = "dsfdsfs",
                                 Mva = "25%",
                                 Ordredato = "2.12.2011",
                                 PapirFakturagebyr = "3",
                                 Salgsperson = "ewrwerwer",
                                 SalgspersonEmail = "person@also.com",
                                 SalgspersonTlfNr = "234234",
                                 SumInklMva = "234324"
                             };

            layout.ProductItems.Add(
                new OrderPdfLayout.ProductItem
                    {
                        Antall = "18",
                        Enhet = "PC",
                        Pris = "1,035",
                        Nr = "10",
                        Produkt = "KTH-PL313/8G",
                        Produktbeskrivelse = "8GB 1333MHZ REG ECC MODULE (HP)",
                        Total = "18,630"
                    });
            layout.ProductItems.Add(
                new OrderPdfLayout.ProductItem
                    {
                        Antall = "18",
                        Enhet = "PC",
                        Pris = "1,035",
                        Nr = "10",
                        Produkt = "KTH-PL313/8G",
                        Produktbeskrivelse = "8GB 1333MHZ REG ECC MODULE (HP)",
                        Total = "18,630"
                    });
            const string fileName = "OrderPdfTest.pdf";
            layout.GeneratePdf(new FileStream(fileName, FileMode.Create));
            Process.Start(fileName);
        }

        [TestMethod]
        public void GenerateDeliveryNotePdfTest()
        {
            var layout = new DeliveryNotePdfLayout
                             {
                                 Vbeln = "00002435435",
                                 Fakturaadresse1 = "NETSHOP AS",
                                 //"ATEA AS", //"NETSHOP AS",
                                 Fakturaadresse2 = string.Empty,
                                 Fakturaadresse3 = "KLINESTADMOEN 1",
                                 Fakturaadresse4 = "3241",
                                 Fakturaadresse5 = "SANDEFJORD",
                                 Kundenummer = "3423434234",
                                 Leveringsadresse1 = "NETSHOP AS",
                                 Leveringsadresse2 = string.Empty,
                                 Leveringsadresse3 = "KLINESTADMOEN 1",
                                 Leveringsadresse4 = "3241",
                                 Leveringsadresse5 = "SANDEFJORD",
                                 Leveringsbetingelser = "rrrrrrtret",
                                 Salgsperson = "ewrwerwer",
                                 Antall = "5",
                                 BestiltAv = "sdfdsf",
                                 Dato = "2.03.2011",
                                 DelNr = "232323",
                                 DeresRef = "Ref",
                                 Leveringsmate = "dsfsdfsdfs",
                                 Ordrenummer = "223423423",
                                 PakkeIds = "34353544",
                                 PakketAv = "eertret",
                                 Speditor = "dsfdfsdf",
                                 TotalVekt = "5"
                             };
            for (int i = 0; i < 10; i++)
            {
                layout.ProductItems.Add(
                    new DeliveryNotePdfLayout.ProductItem
                        {
                            Nr = "10",
                            Varenummer = "Some produkt ... ",
                            Bestilt = "3",
                            Levert = "3",
                            Tidligere = "0",
                            Rest = "0",
                            PakkeNummer = "235435345345"
                        });
            }
            const string fileName = "DeliveryNotePdfTest.pdf";
            layout.GeneratePdf(new FileStream(fileName, FileMode.Create));
            Process.Start(fileName);
        }
    }
}