using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Utils;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Import
{
    /// <summary>
    /// Interaction logic for DlgImportOrSampleFileSelectionCompany.xaml
    /// </summary>
    public partial class DlgImportOrSampleFileSelectionCompany : Window
    {
        public DlgImportOrSampleFileSelectionCompany() {
            InitializeComponent();
        }

        private void BtnImportCompanyCsvFilesClick(object sender, RoutedEventArgs e) {
            new DlgImportCompanyDetails { Owner = GlobalResources.MainWindow }.ShowDialog();
            DialogResult = true;
        }

        private void BtnCreateCsvFilesClick(object sender, RoutedEventArgs e) {
            string fileName = "Beispielfirma.csv";
            SaveFileDialog dlgSaveFile = new SaveFileDialog {
                FileName = fileName,
                DefaultExt = ".csv",
                Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv"
            };
            DialogResult result = dlgSaveFile.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK && result != System.Windows.Forms.DialogResult.Yes) return;
            fileName = dlgSaveFile.FileName;
            Exception ex;
            if (!FileHelper.IsFileBeingUsed(fileName, out ex)) {
                WriteExample(fileName);
                System.Windows.MessageBox.Show(this, "Beispiel-Datei wurde erfolgreich exportiert.", "",
                                               MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                System.Windows.MessageBox.Show(this, ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK,
                                               MessageBoxImage.Error);
            }
            //DialogResult = true;
        }

        private void WriteExample(string fileName) {
            using (CsvWriter writer = new CsvWriter(fileName, Encoding.Default)) {
                //Dictionary<int, List<string>> something = new Dictionary<int, List<string>>
                //                                          {{2, new List<string> {"1", "2"}},{3, new List<string> {"3", "4"}}};
                Dictionary<int, List<KeyValuePair<string, string>>> paragraphs =
                    new Dictionary<int, List<KeyValuePair<string, string>>>();

                paragraphs[0] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Name des Unternehmens", "genInfo.company.id.name"),
                    new KeyValuePair<string, string>("frühere Unternehmensnamen",
                                                     "genInfo.company.id.name.formerName"),
                    new KeyValuePair<string, string>("letztes Änderungsdatum",
                                                     "genInfo.company.id.name.dateOfLastChange"),
                    new KeyValuePair<string, string>("Rechtsform", "genInfo.company.id.legalStatus"),
                    new KeyValuePair<string, string>("frühere Rechtsformen",
                                                     "genInfo.company.id.legalStatus.formerStatus"),
                    new KeyValuePair<string, string>("letztes Änderungsdatum",
                                                     "genInfo.company.id.legalStatus.dateOfLastChange"),
                    new KeyValuePair<string, string>("Straße", "genInfo.company.id.location.street"),
                    new KeyValuePair<string, string>("Hausnummer", "genInfo.company.id.location.houseNo"),
                    new KeyValuePair<string, string>("Postleitzahl", "genInfo.company.id.location.zipCode"),
                    new KeyValuePair<string, string>("Ort", "genInfo.company.id.location.city"),
                };
                paragraphs[1] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("4stellige Bundesfinanzamtsnummer",
                                                     "genInfo.company.id.idNo.type.companyId.BF4"),
                    new KeyValuePair<string, string>("13stellige Steuernummer",
                                                     "genInfo.company.id.idNo.type.companyId.ST13"),
                    new KeyValuePair<string, string>("steuerliche IdNr.",
                                                     "genInfo.company.id.idNo.type.companyId.STID"),
                    new KeyValuePair<string, string>("Handelsregisternummer",
                                                     "genInfo.company.id.idNo.type.companyId.HRN"),
                    new KeyValuePair<string, string>("USt-IdNr.", "genInfo.company.id.idNo.type.companyId.UID"),
                    new KeyValuePair<string, string>("steuerliche W-IdNr.",
                                                     "genInfo.company.id.idNo.type.companyId.STWID"),
                    new KeyValuePair<string, string>("Bundesbank Konzernnummer",
                                                     "genInfo.company.id.idNo.type.companyId.BKN"),
                    new KeyValuePair<string, string>("Bundesbank Kreditnehmernummer",
                                                     "genInfo.company.id.idNo.type.companyId.BUN"),
                    new KeyValuePair<string, string>("individuelle Kennnummer",
                                                     "genInfo.company.id.idNo.type.companyId.IN"),
                    new KeyValuePair<string, string>("Kennnummer Empfänger",
                                                     "genInfo.company.id.idNo.type.companyId.EN"),
                    new KeyValuePair<string, string>("Kennnummer Sender",
                                                     "genInfo.company.id.idNo.type.companyId.SN"),
                    new KeyValuePair<string, string>("andere Kennnummer", "genInfo.company.id.idNo.type.companyId.S")
                };
                paragraphs[2] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Name des Gesellschafters",
                                                     "genInfo.company.id.shareholder.name"),
                    new KeyValuePair<string, string>("lfd. Nummer aus Feststellungserklärung",
                                                     "genInfo.company.id.shareholder.currentnumber"),
                    new KeyValuePair<string, string>(
                        "Zeichnernummer (soweit vorhanden) aus Feststellungserklärung (Vordruck FB)",
                        "genInfo.company.id.shareholder.signerId"),
                    new KeyValuePair<string, string>("Steuernummer der Gesellschafters",
                                                     "genInfo.company.id.shareholder.taxnumber"),
                    new KeyValuePair<string, string>("steuerliche IDNr.", "genInfo.company.id.shareholder.taxid"),
                    new KeyValuePair<string, string>("steuerliche W-IdNr.", "genInfo.company.id.shareholder.WID"),
                    new KeyValuePair<string, string>("Gewinnverteilungsschlüssel Gesellschafter",
                                                     "genInfo.company.id.shareholder.ProfitDivideKey"),
                    new KeyValuePair<string, string>("unterjähriges Änderungsdatum",
                                                     "genInfo.company.id.shareholder.ProfitDivideKey.dateOfunderyearChange"),
                    new KeyValuePair<string, string>("früherer Verteilungsschlüssel",
                                                     "genInfo.company.id.shareholder.ProfitDivideKey.formerkey"),
                    new KeyValuePair<string, string>("Rechtsform des Gesellschafters",
                                                     "genInfo.company.id.shareholder.legalStatus"),
                    new KeyValuePair<string, string>("Zähler",
                                                     "genInfo.company.id.shareholder.ShareDivideKey.numerator"),
                    new KeyValuePair<string, string>("Nenner",
                                                     "genInfo.company.id.shareholder.ShareDivideKey.denominator"),
                    new KeyValuePair<string, string>("Sonderbilanz benötigt?",
                                                     "genInfo.company.id.shareholder.SpecialBalanceRequired"),
                    new KeyValuePair<string, string>("Ergänzungsbilanz benötigt?",
                                                     "genInfo.company.id.shareholder.extensionRequired")
                };
                paragraphs[3] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Registerart", "genInfo.company.id.Incorporation.Type"),
                    new KeyValuePair<string, string>("Präfix", "genInfo.company.id.Incorporation.prefix"),
                    new KeyValuePair<string, string>("Abteilung", "genInfo.company.id.Incorporation.section"),
                    new KeyValuePair<string, string>("Nummer", "genInfo.company.id.Incorporation.number"),
                    new KeyValuePair<string, string>("Suffix", "genInfo.company.id.Incorporation.suffix"),
                    new KeyValuePair<string, string>("Amtsgericht", "genInfo.company.id.Incorporation.court"),
                    new KeyValuePair<string, string>("Datum der ersten Eintragung",
                                                     "genInfo.company.id.Incorporation.dateOfFirstRegistration")
                };
                paragraphs[4] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Börsenplatz", "genInfo.company.id.stockExch.city"),
                    new KeyValuePair<string, string>("Ticker Symbol", "genInfo.company.id.stockExch.ticker"),
                    new KeyValuePair<string, string>("Marktsegment der Notierung",
                                                     "genInfo.company.id.stockExch.market"),
                    new KeyValuePair<string, string>("Wertpapierart", "genInfo.company.id.stockExch.typeOfSecurity"),
                    new KeyValuePair<string, string>("Wertpapierkennnummer",
                                                     "genInfo.company.id.stockExch.securityCode"),
                    new KeyValuePair<string, string>("Wertpapiercode (ISIN)",
                                                     "genInfo.company.id.stockExch.securityCode.entry"),
                    new KeyValuePair<string, string>("Art des Wertpapiercodes",
                                                     "genInfo.company.id.stockExch.securityCode.type")
                };
                paragraphs[5] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Kontaktperson", "genInfo.company.id.contactAddress.person"),
                    new KeyValuePair<string, string>("Name", "genInfo.company.id.contactAddress.person.name"),
                    new KeyValuePair<string, string>("Abteilung", "genInfo.company.id.contactAddress.person.dept"),
                    new KeyValuePair<string, string>("Funktion", "genInfo.company.id.contactAddress.person.function"),
                    new KeyValuePair<string, string>("Telefonnummer",
                                                     "genInfo.company.id.contactAddress.person.phone"),
                    new KeyValuePair<string, string>("Faxnummer", "genInfo.company.id.contactAddress.person.fax"),
                    new KeyValuePair<string, string>("e-mail Adresse",
                                                     "genInfo.company.id.contactAddress.person.eMail")
                };
                paragraphs[6] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Jahr der letzten Betriebsprüfung",
                                                     "genInfo.company.id.lastTaxAudit"),
                    new KeyValuePair<string, string>("Größenklasse", "genInfo.company.id.sizeClass"),
                    new KeyValuePair<string, string>("Geschäftstätigkeit", "genInfo.company.id.business"),
                    new KeyValuePair<string, string>("Unternehmensstatus", "genInfo.company.id.CompanyStatus"),
                    new KeyValuePair<string, string>("Gründungsdatum", "genInfo.company.id.FoundationDate"),
                    new KeyValuePair<string, string>("Körperschaft-/einkommensteuerliche Organschaft", "genInfo.company.id.taxGroupKstEst"),
                    new KeyValuePair<string, string>("Internetadresse", "genInfo.company.id.internet"),
                    new KeyValuePair<string, string>("Website Beschreibung",
                                                     "genInfo.company.id.internet.description"),
                    new KeyValuePair<string, string>("Website URL", "genInfo.company.id.internet.url"),
                    new KeyValuePair<string, string>("Auskunftsquelle", "genInfo.company.id.comingfrom"),
                    new KeyValuePair<string, string>("URL Firmenlogo", "genInfo.company.id.companyLogo"),
                    new KeyValuePair<string, string>("nutzerspezifische Unternehmensinformationen",
                                                     "genInfo.company.userSpecific"),
                    new KeyValuePair<string, string>("Art des Wirtschaftszweigschlüssels",
                                                     "genInfo.company.id.industry.keyType"),
                    new KeyValuePair<string, string>("Wirtschaftszweigschlüssel-Nr.",
                                                     "genInfo.company.id.industry.keyEntry"),
                    new KeyValuePair<string, string>("Wirtschaftszweig Klartext", "genInfo.company.id.industry.name")
                };
                paragraphs[7] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Name des Mutterunternehmens", "genInfo.company.id.parent.name"),
                    new KeyValuePair<string, string>("frühere Unternehmensnamen", "genInfo.company.id.parent.name.formerName"),
                    new KeyValuePair<string, string>("letztes Änderungsdatum", "genInfo.company.id.parent.name.dateOfLastChange"),
                    new KeyValuePair<string, string>("Rechtsform", "genInfo.company.id.parent.legalStatus"),
                    new KeyValuePair<string, string>("frühere Rechtsformen", "genInfo.company.id.parent.legalStatus.formerStatus"),
                    new KeyValuePair<string, string>("letztes Änderungsdatum", "genInfo.company.id.parent.legalStatus.dateOfLastChange"),
                    // DEVNOTE: hack.
                    // the next 6 row is not the same id. There is no parent in the rows. For example
                    // new KeyValuePair<string, string>("4stillige Bundesfinanzamtsnummer", "genInfo.company.id.idNo.type.companyId.BF4"),
                    // is the correct not 
                    // new KeyValuePair<string, string>("4stillige Bundesfinanzamtsnummer", "genInfo.company.id.parent.idNo.type.companyId.BF4").
                    // the hack is needed because in ListViewCompanyModel.cs can't differe the ID's under Mutterunternehmens,
                    // and the ID's not under Mutterunternehmens. 
                    new KeyValuePair<string, string>("4stellige Bundesfinanzamtsnummer", "genInfo.company.id.parent.idNo.type.companyId.BF4"),
                    new KeyValuePair<string, string>("13stellige Steuernummer", "genInfo.company.id.parent.idNo.type.companyId.ST13"),
                    new KeyValuePair<string, string>("steuerliche IdNr.", "genInfo.company.id.parent.idNo.type.companyId.STID"),
                    new KeyValuePair<string, string>("Handelsregisternummer", "genInfo.company.id.parent.idNo.type.companyId.HRN"),
                    new KeyValuePair<string, string>("USt-IdNr.", "genInfo.company.id.parent.idNo.type.companyId.UID"),
                    new KeyValuePair<string, string>("steuerliche W-IdNr.", "genInfo.company.id.parent.idNo.type.companyId.STWID"),
                    new KeyValuePair<string, string>("Firmensitz", "genInfo.company.id.parent.location"),
                    new KeyValuePair<string, string>("Straße", "genInfo.company.id.parent.location.street"),
                    new KeyValuePair<string, string>("Hausnummer", "genInfo.company.id.parent.location.houseNo"),
                    new KeyValuePair<string, string>("Postleitzahl", "genInfo.company.id.parent.location.zipCode"),
                    new KeyValuePair<string, string>("Ort", "genInfo.company.id.parent.location.city"),
                    new KeyValuePair<string, string>("Land", "genInfo.company.id.parent.location.country"),
                    new KeyValuePair<string, string>("Iso Code Land", "genInfo.company.id.parent.location.country.isoCode"),
                    // the next 6 line is hacked as well. See above.
                    new KeyValuePair<string, string>("Bundesbank Konzernnummer", "genInfo.company.id.parent.idNo.type.companyId.BKN"),
                    new KeyValuePair<string, string>("Bundesbank Kreditnehmernummer", "genInfo.company.id.parent.idNo.type.companyId.BUN"),
                    new KeyValuePair<string, string>("individuelle Kennnummer", "genInfo.company.id.parent.idNo.type.companyId.IN"),
                    new KeyValuePair<string, string>("Kennnummer Empfänger", "genInfo.company.id.parent.idNo.type.companyId.EN"),
                    new KeyValuePair<string, string>("Kennnummer Sender", "genInfo.company.id.parent.idNo.type.companyId.SN"),
                    new KeyValuePair<string, string>("andere Kennnummer", "genInfo.company.id.parent.idNo.type.companyId.S")
                };
                paragraphs[8] = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("Geschäftsjahre", string.Empty),
                    new KeyValuePair<string, string>("Stichtag", string.Empty),
                    new KeyValuePair<string, string>("Beginn", string.Empty),
                    new KeyValuePair<string, string>("Ende", string.Empty)
                };
                writer.WriteCsvData("Name", "Taxonomie Id", "Wert", "Wert2");
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("ALLGEMEIN", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[0],
                                      new KeyValuePair<int, string>(0, "Beispielfirma"),
                                      new KeyValuePair<int, string>(2, DateToString(new DateTime(2012, 12, 3))));
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("KENNNUMMERN", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[1]);
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("GESELLSCHAFTER", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[2],
                                      new KeyValuePair<int, string>(0, "Gesellschafter 1"),
                                      new KeyValuePair<int, string>(0, "Gesellschafter 2"),
                                      new KeyValuePair<int, string>(12, "true"),
                                      new KeyValuePair<int, string>(12, "false"));
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("REGISTEREINTRAG", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[3]);
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("BÖRSENNOTIERUNG", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[4]);
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("KONTAKTPERSON", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[5],
                                      new KeyValuePair<int, string>(0, "Kontakt 1"),
                                      new KeyValuePair<int, string>(0, "Kontakt 2"),
                                      new KeyValuePair<int, string>(1, "Name der Kontaktperson 1"),
                                      new KeyValuePair<int, string>(1, "Name der Kontaktperson 2"));
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("SONSTIGE INFORMATIONEN", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[6]);
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("MUTTERUNTERNEHMEN", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[7],
                                      new KeyValuePair<int, string>(2, DateToString(new DateTime(2012, 12, 3))),
                                      new KeyValuePair<int, string>(5, DateToString(new DateTime(2012, 12, 4))));
                writer.WriteCsvData(string.Empty, string.Empty, string.Empty, string.Empty);
                writer.WriteCsvData("GESCHÄFTSJAHRE", string.Empty, string.Empty, string.Empty);
                WriteForEachWithValue(writer, paragraphs[8],
                                      new KeyValuePair<int, string>(0, Convert.ToString(2010)),
                                      new KeyValuePair<int, string>(0, Convert.ToString(2011)),
                                      new KeyValuePair<int, string>(1, DateToString(new DateTime(2010, 12, 31))),
                                      new KeyValuePair<int, string>(1, DateToString(new DateTime(2011, 12, 31))),
                                      new KeyValuePair<int, string>(2, DateToString(new DateTime(2010, 2, 1))),
                                      new KeyValuePair<int, string>(2, DateToString(new DateTime(2011, 1, 1))),
                                      new KeyValuePair<int, string>(3, DateToString(new DateTime(2010, 12, 31))),
                                      new KeyValuePair<int, string>(3, DateToString(new DateTime(2011, 12, 31))));
            }

            Process.Start(fileName);
        }

        private string DateToString(DateTime date) {
            return date.ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// write rows to the writer. 2 columns will be filled with the default data, and the rest will be optional.
        /// </summary>
        /// <param name="writer">the csv writer to ouput the rows</param>
        /// <param name="defaultData">list of key value pairs. Each row starts with the key, continues with the value</param>
        /// <param name="values">optional values. The key is the index of the row, the value will exchange a string.empty in the row. Multiple values is added with the same key.
        /// <example>
        /// <code>
        ///        WriteForEachWithValue(writer,
        ///                              new List&lt;KeyValuePair&lt;string, string&gt;&gt; {
        ///                                  new KeyValuePair&lt;string, string&gt;("a1", "a2"),
        ///                                  new KeyValuePair&lt;string, string&gt;("b1", "b2")
        ///                              },
        ///                              new KeyValuePair&lt;int, string&gt;(0, "a3"), new KeyValuePair&lt;int, string&gt;(0, "a4"),
        ///                              new KeyValuePair&lt;int, string&gt;(1, "b3"));
        /// </code>
        /// will create {"a1", "a2", "a3", "a4", string.Empty, string.Empty,...}
        ///             {"b1", "b2", "b3", string.Empty, string.Empty,...}
        /// </example>
        /// </param>
        private void WriteForEachWithValue(CsvWriter writer, IList<KeyValuePair<string, string>> defaultData, params KeyValuePair<int, string>[] values) {
            Debug.Assert(writer.FieldCount.HasValue);
            int columnCount = writer.FieldCount.Value;
            Dictionary<int, List<string>> multiMap = new Dictionary<int, List<string>>();
            foreach (KeyValuePair<int, string> value in values) {
                if (multiMap.ContainsKey(value.Key)) {
                    multiMap[value.Key].Add(value.Value);
                } else {
                    multiMap[value.Key] = new List<string> {value.Value};
                }
            }
            for (int i = 0; i < defaultData.Count; i++) {
                List<string> parameters = new List<string>(columnCount) {defaultData[i].Key, defaultData[i].Value};
                for (int j = 0; j < columnCount - 2; j++) {
                    List<string> value;
                    parameters.Add(multiMap.TryGetValue(i, out value) && value.Count > j ? value[j] : string.Empty);
                }
                writer.WriteCsvData(parameters.ToArray());
            }
        }
    }
}
