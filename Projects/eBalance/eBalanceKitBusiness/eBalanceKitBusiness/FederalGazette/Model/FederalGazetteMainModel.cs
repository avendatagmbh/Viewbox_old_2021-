// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2011-12-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.FederalGazette.Model {

    public class FederalGazetteModelElementInfo {
        public FederalGazetteModelElementInfo(string id, string caption) {
            Id = id;
            Caption = caption;
        }

        public string Id { get; set; }
        public string Caption { get; set; }

        public override string ToString() { return Caption; }
    }

    public class FederalGazetteMainModel:INotifyPropertyChanged {
        public FederalGazetteMainModel(Document document) {
            Document = document;
            ExportBalanceSheet = true;
            ExportNotes = true;
            ExportIncomeStatement = false;
            ExportNetProfit = false;
            ExportFixedAssets = false;
            // ExportAnnualStatement = false;
            ExportManagementReport = false;

            #region OrderTypes
            OrderTypes = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("1", "Neue Veröffentlichung einstellen"),
                new FederalGazetteModelElementInfo("2", "Berichtigung zu einer bereits erfolgten Veröffentlichung"),
                new FederalGazetteModelElementInfo("3", "Ergänzung zu einer bereits erfolgten Veröffentlichung"),
            };
            #endregion OrderTypes

            #region PublicationArea
            PublicationArea = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("22", "Rechnungslegung/Finanzberichte")
            };
            #endregion PublicationArea

            #region PublicationType
            PublicationType = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("13", "Sontiges"),
                new FederalGazetteModelElementInfo("86", "§§ 264 Abs. 3, 264b HGB"),
                new FederalGazetteModelElementInfo("121",
                                                   "Hinweis auf Jahresfinanzbericht, Halbjahresfinanzbericht, Quartalsfinanzbericht, Zwischenmitteilung"),
                new FederalGazetteModelElementInfo("135", "Jahresabschluss/Jahresfinanzbericht"),
                new FederalGazetteModelElementInfo("136", "Halbjahresfinanzbericht"),
                new FederalGazetteModelElementInfo("137", "Zwischenmitteilung"),
                new FederalGazetteModelElementInfo("138", "Quartalsfinanzbericht"),
                new FederalGazetteModelElementInfo("139", "Fehlerbekanntmachung")
            };
            #endregion PublicationType

            #region PublicationSubType
            PublicationSubType = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("3", "Jahresabschluss/Jahresfinanzbericht"),
                new FederalGazetteModelElementInfo("5", "Konzernabschluss"),
                new FederalGazetteModelElementInfo("10", "Hinweis auf Jahresfinanzbericht"),
                new FederalGazetteModelElementInfo("11", "Hinweis auf Halbjahresfinanzbericht"),
                new FederalGazetteModelElementInfo("12", "Hinweis auf Quartalsfinanzbericht"),
                new FederalGazetteModelElementInfo("13", "Hinweis auf Zwischenmitteilung der Geschäftsführung"),
                new FederalGazetteModelElementInfo("14", "Fehlerbekanntmachung für Jahresfinanzbericht"),
                new FederalGazetteModelElementInfo("15", "Fehlerbekanntmachung für Halbjahresfinanzbericht")
            };
            #endregion PublicationSubType

            #region PublicationLanguage
            PublicationLanguage = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("de", "Deutsch"),
                new FederalGazetteModelElementInfo("en", "English")
            };
            #endregion PublicationLanguage

            #region CompanyType
            CompanyType = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("1", "Kreditinstitut"),
                new FederalGazetteModelElementInfo("2", "Pensionsfond"),
                new FederalGazetteModelElementInfo("3", "Finanzierungsdienstleister"),
                new FederalGazetteModelElementInfo("4", "Versicherung"),
                new FederalGazetteModelElementInfo("5", "Rückversicherungsgesellschaft"),
                new FederalGazetteModelElementInfo("6", "keine der genannten Gesellschaftsarten")
            };
            #endregion CompanyType

            #region CompanySize
            CompanySize = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("small", "Klein"),
                new FederalGazetteModelElementInfo("midsize", "Mittel"),
                new FederalGazetteModelElementInfo("Big", "Groß")
            };
            #endregion CompanySize

            #region PublicationCategory
            PublicationCategory = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("14", "Jahresabschlüsse/Jahresfinanzberichte"),
                new FederalGazetteModelElementInfo("90", "weitere Finanzberichte")
            };
            #endregion PublicationCategory

            #region CompanyRegisterationType
            CompanyRegisterationType = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("german_registered",
                                                   "Firma/Institution mit Sitz in Deutschland, eingetragen in ein Register"),
                new FederalGazetteModelElementInfo("german_not_registered",
                                                   "Firma/Institution mit Sitz in Deutschland, nicht eingetragen in ein Register"),
                new FederalGazetteModelElementInfo("foreign", "Firma mit Sitz im Ausland")
            };
            #endregion CompanyRegisterationType

            #region States
            States = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("BW", "Baden-Württemberg"),
                new FederalGazetteModelElementInfo("BY", "Bayern"),
                new FederalGazetteModelElementInfo("BE", "Berlin"),
                new FederalGazetteModelElementInfo("BR", "Brandenburg"),
                new FederalGazetteModelElementInfo("HB", "Bremen"),
                new FederalGazetteModelElementInfo("HH", "Hamburg"),
                new FederalGazetteModelElementInfo("HE", "Hessen"),
                new FederalGazetteModelElementInfo("MV", "Mecklenburg-Vorpommern"),
                new FederalGazetteModelElementInfo("NI", "Niedersachsen"),
                new FederalGazetteModelElementInfo("NW", "Nordrhein-Westfalen"),
                new FederalGazetteModelElementInfo("RP", "Rheinland-Pfalz"),
                new FederalGazetteModelElementInfo("SL", "Saarland"),
                new FederalGazetteModelElementInfo("SN", "Sachsen"),
                new FederalGazetteModelElementInfo("ST", "Sachsen-Anhalt"),
                new FederalGazetteModelElementInfo("SH", "Schleswig-Holstein"),
                new FederalGazetteModelElementInfo("TH", "Thüringen")
            };
            #endregion States

            #region CompanyLegalForm
            CompanyLegalForm = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("AG", "Aktiengesellschaft"),
                new FederalGazetteModelElementInfo("eGen", "eingetragene Genossenschaft"),
                new FederalGazetteModelElementInfo("EK", "Einzelkaufmann"),
                new FederalGazetteModelElementInfo("Ekf", "Einzelkauffrau"),
                new FederalGazetteModelElementInfo("Ewl", "Europäische wirtschaftliche Interessenvereinigung"),
                new FederalGazetteModelElementInfo("GmbH", "Gesellschaft mit beschränkter Haftung"),
                new FederalGazetteModelElementInfo("KG", "Kommanditgesellschaft (inkl. GmbH & Co.KG)"),
                new FederalGazetteModelElementInfo("KGaA", "Kommanditgesellschaft auf Aktien"),
                new FederalGazetteModelElementInfo("OHG", "Offene Handelsgesellschaft (inkl. GmbH Co. OHG)"),
                new FederalGazetteModelElementInfo("Part", "Partnerschaft"),
                new FederalGazetteModelElementInfo("SE", "Europäische Aktiengesellschaft (SE)"),
                new FederalGazetteModelElementInfo("Ver", "eingetragener Verein"),
                new FederalGazetteModelElementInfo("VvaG", "Versicherungsverein auf Gegenseitigkeit"),
                new FederalGazetteModelElementInfo("ARGnR", "Rechtsform ausländischen Rechts GnR"),
                new FederalGazetteModelElementInfo("ARHRA", "Rechtsform ausländischen Rechts HRA"),
                new FederalGazetteModelElementInfo("ARHRB", "Rechtsform ausländischen Rechts HRB (z.B. Limited)"),
                new FederalGazetteModelElementInfo("ARPR", "Rechtsform ausländischen Rechts PR"),
                new FederalGazetteModelElementInfo("HRAJP", "HRA Juristische Person"),
                new FederalGazetteModelElementInfo("SG", "Seerechtliche Gesellschaft"),
            };
            #endregion CompanyLegalForm

            #region RegisteredCourt
            RegisteredCourt = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("R3101", "Aachen"),
                new FederalGazetteModelElementInfo("D3101", "Amberg"),
                new FederalGazetteModelElementInfo("D3201", "Ansbach"),
                new FederalGazetteModelElementInfo("R1901", "Arnsberg"),
                new FederalGazetteModelElementInfo("D4102", "Aschaffenburg"),
                new FederalGazetteModelElementInfo("D2102", "Augsburg"),
                new FederalGazetteModelElementInfo("P3101", "Aurich"),
                new FederalGazetteModelElementInfo("M1305", "Bad Hersfeld"),
                new FederalGazetteModelElementInfo("M1202", "Bad Homburg v.d.H."),
                new FederalGazetteModelElementInfo("T2101", "Bad Kreuznach"),
                new FederalGazetteModelElementInfo("R2108", "Bad Oeynhausen"),
                new FederalGazetteModelElementInfo("D4201", "Bamberg"),
                new FederalGazetteModelElementInfo("D4301", "Bayreuth"),
                new FederalGazetteModelElementInfo("F1103", "Berlin (Charlottenburg)"),
                new FederalGazetteModelElementInfo("R2101", "Bielefeld"),
                new FederalGazetteModelElementInfo("R2201", "Bochum"),
                new FederalGazetteModelElementInfo("R3201", "Bonn"),
                new FederalGazetteModelElementInfo("P1103", "Braunschweig"),
                new FederalGazetteModelElementInfo("H1101", "Bremen"),
                new FederalGazetteModelElementInfo("H1102", "Bremerhaven"),
                new FederalGazetteModelElementInfo("U1206", "Chemnitz"),
                new FederalGazetteModelElementInfo("D4401", "Coburg"),
                new FederalGazetteModelElementInfo("R2707", "Coesfeld"),
                new FederalGazetteModelElementInfo("G1103", "Cottbus"),
                new FederalGazetteModelElementInfo("M1103", "Darmstadt"),
                new FederalGazetteModelElementInfo("D2201", "Deggendorf"),
                new FederalGazetteModelElementInfo("R2402", "Dortmund"),
                new FederalGazetteModelElementInfo("U1104", "Dresden"),
                new FederalGazetteModelElementInfo("R1202", "Duisburg"),
                new FederalGazetteModelElementInfo("R3103", "Düren"),
                new FederalGazetteModelElementInfo("R1101", "Düsseldorf"),
                new FederalGazetteModelElementInfo("M1602", "Eschwege"),
                new FederalGazetteModelElementInfo("R2503", "Essen"),
                new FederalGazetteModelElementInfo("X1112", "Flensburg"),
                new FederalGazetteModelElementInfo("M1201", "Frankfurt am Main"),
                new FederalGazetteModelElementInfo("G1207", "Frankfurt/Oder"),
                new FederalGazetteModelElementInfo("B1204", "Freiburg"),
                new FederalGazetteModelElementInfo("M1405", "Friedberg"),
                new FederalGazetteModelElementInfo("M1603", "Fritzlar"),
                new FederalGazetteModelElementInfo("M1301", "Fulda"),
                new FederalGazetteModelElementInfo("D3304", "Fürth"),
                new FederalGazetteModelElementInfo("R2507", "Gelsenkirchen"),
                new FederalGazetteModelElementInfo("M1406", "Gießen"),
                new FederalGazetteModelElementInfo("P2204", "Göttingen"),
                new FederalGazetteModelElementInfo("R2103", "Gütersloh"),
                new FederalGazetteModelElementInfo("R2602", "Hagen"),
                new FederalGazetteModelElementInfo("K1101", "Hamburg"),
                new FederalGazetteModelElementInfo("R2404", "Hamm"),
                new FederalGazetteModelElementInfo("M1502", "Hanau"),
                new FederalGazetteModelElementInfo("P2305", "Hannover"),
                new FederalGazetteModelElementInfo("P2408", "Hildesheim"),
                new FederalGazetteModelElementInfo("D4501", "Hof"),
                new FederalGazetteModelElementInfo("V1102", "Homburg"),
                new FederalGazetteModelElementInfo("D5701", "Ingolstadt"),
                new FederalGazetteModelElementInfo("R2604", "Iserlohn"),
                new FederalGazetteModelElementInfo("Y1206", "Jena"),
                new FederalGazetteModelElementInfo("T3201", "Kaiserslautern"),
                new FederalGazetteModelElementInfo("M1607", "Kassel"),
                new FederalGazetteModelElementInfo("D2304", "Kempten (Allgäu)"),
                new FederalGazetteModelElementInfo("R3305", "Kerpen"),
                new FederalGazetteModelElementInfo("X1517", "Kiel"),
                new FederalGazetteModelElementInfo("R1304", "Kleve"),
                new FederalGazetteModelElementInfo("T2210", "Koblenz"),
                new FederalGazetteModelElementInfo("R3306", "Köln"),
                new FederalGazetteModelElementInfo("M1203", "Königstein"),
                new FederalGazetteModelElementInfo("M1608", "Korbach"),
                new FederalGazetteModelElementInfo("R1402", "Krefeld"),
                new FederalGazetteModelElementInfo("T3304", "Landau"),
                new FederalGazetteModelElementInfo("D2404", "Landshut"),
                new FederalGazetteModelElementInfo("R1105", "Langenfeld"),
                new FederalGazetteModelElementInfo("V1103", "Lebach"),
                new FederalGazetteModelElementInfo("U1308", "Leipzig"),
                new FederalGazetteModelElementInfo("R2307", "Lemgo"),
                new FederalGazetteModelElementInfo("R3311", "Leverkusen"),
                new FederalGazetteModelElementInfo("M1706", "Limburg"),
                new FederalGazetteModelElementInfo("X1721", "Lübeck"),
                new FederalGazetteModelElementInfo("T3104", "Ludwigshafen a.Rhein (Ludwigshafen)"),
                new FederalGazetteModelElementInfo("P2507", "Lüneburg"),
                new FederalGazetteModelElementInfo("T2304", "Mainz"),
                new FederalGazetteModelElementInfo("B1601", "Mannheim"),
                new FederalGazetteModelElementInfo("M1809", "Marburg"),
                new FederalGazetteModelElementInfo("D2505", "Memmingen"),
                new FederalGazetteModelElementInfo("V1104", "Merzig"),
                new FederalGazetteModelElementInfo("R1504", "Mönchengladbach"),
                new FederalGazetteModelElementInfo("T2214", "Montabaur"),
                new FederalGazetteModelElementInfo("D2601", "München"),
                new FederalGazetteModelElementInfo("R2713", "Münster"),
                new FederalGazetteModelElementInfo("N1105", "Neubrandenburg"),
                new FederalGazetteModelElementInfo("V1105", "Neunkirchen"),
                new FederalGazetteModelElementInfo("G1309", "Neuruppin"),
                new FederalGazetteModelElementInfo("R1102", "Neuss"),
                new FederalGazetteModelElementInfo("D3310", "Nürnberg"),
                new FederalGazetteModelElementInfo("M1114", "Offenbach am Main"),
                new FederalGazetteModelElementInfo("P3210", "Oldenburg (Oldenburg)"),
                new FederalGazetteModelElementInfo("P3313", "Osnabrück"),
                new FederalGazetteModelElementInfo("V1107", "Ottweiler"),
                new FederalGazetteModelElementInfo("R2809", "Paderborn"),
                new FederalGazetteModelElementInfo("D2803", "Passau"),
                new FederalGazetteModelElementInfo("X1321", "Pinneberg"),
                new FederalGazetteModelElementInfo("G1312", "Potsdam"),
                new FederalGazetteModelElementInfo("R2204", "Recklinghausen"),
                new FederalGazetteModelElementInfo("D3410", "Regensburg"),
                new FederalGazetteModelElementInfo("N1206", "Rostock"),
                new FederalGazetteModelElementInfo("V1109", "Saarbrücken"),
                new FederalGazetteModelElementInfo("V1110", "Saarlouis"),
                new FederalGazetteModelElementInfo("R3106", "Schleiden"),
                new FederalGazetteModelElementInfo("D4608", "Schweinfurt"),
                new FederalGazetteModelElementInfo("N1308", "Schwerin"),
                new FederalGazetteModelElementInfo("R3208", "Siegburg"),
                new FederalGazetteModelElementInfo("R2909", "Siegen"),
                new FederalGazetteModelElementInfo("V1111", "St. Ingbert (St Ingbert)"),
                new FederalGazetteModelElementInfo("V1112", "St. Wendel (St Wendel)"),
                new FederalGazetteModelElementInfo("P2106", "Stadthagen"),
                new FederalGazetteModelElementInfo("R2706", "Steinfurt"),
                new FederalGazetteModelElementInfo("W1215", "Stendal"),
                new FederalGazetteModelElementInfo("N1209", "Stralsund"),
                new FederalGazetteModelElementInfo("D3413", "Straubing"),
                new FederalGazetteModelElementInfo("B2609", "Stuttgart"),
                new FederalGazetteModelElementInfo("P2613", "Tostedt"),
                new FederalGazetteModelElementInfo("D2910", "Traunstein"),
                new FederalGazetteModelElementInfo("B2805", "Ulm"),
                new FederalGazetteModelElementInfo("V1115", "Völklingen"),
                new FederalGazetteModelElementInfo("P2716", "Walsrode"),
                new FederalGazetteModelElementInfo("D3508", "Weiden i. d. OPf."),
                new FederalGazetteModelElementInfo("M1710", "Wetzlar"),
                new FederalGazetteModelElementInfo("M1906", "Wiesbaden"),
                new FederalGazetteModelElementInfo("T2408", "Wittlich"),
                new FederalGazetteModelElementInfo("R1608", "Wuppertal"),
                new FederalGazetteModelElementInfo("D4708", "Würzburg"),
                new FederalGazetteModelElementInfo("T3403", "Zweibrücken"),

            };
            #endregion RegisteredCourt

            #region RegisterationType
            RegisterationType = new ObservableCollectionAsync<FederalGazetteModelElementInfo>() {
                new FederalGazetteModelElementInfo("HRA", "Personengesellschaften (HRA)"),
                new FederalGazetteModelElementInfo("HRB", "Kapitalgesellschaften (HRB)"),
                new FederalGazetteModelElementInfo("VR", "Vereinsregister (VR)"),
                new FederalGazetteModelElementInfo("GnR", "Genossenschaftsregister (GnR)"),
                new FederalGazetteModelElementInfo("PR", "Partnerschaftsregister (PR)"),

            };
            #endregion

            #region countries
            Countries = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("de", "Deutschland"),
                new FederalGazetteModelElementInfo("at", "Austria"),
                new FederalGazetteModelElementInfo("ch", "Switzerland"),
                new FederalGazetteModelElementInfo("nl", "Netherlands"),
                new FederalGazetteModelElementInfo("fr", "France")
            };
            #endregion countries

            #region Salutation
            Salutation = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("Frau", "Frau"),
                new FederalGazetteModelElementInfo("Herr", "Herr")
                
            };
            #endregion

            #region BalanceSheetStandard
            BalanceSheetStandard = new ObservableCollectionAsync<FederalGazetteModelElementInfo> {
                new FederalGazetteModelElementInfo("1", "HGB"),
                new FederalGazetteModelElementInfo("2", "IFRS"),
                new FederalGazetteModelElementInfo("3", "US-GAAP"),
                new FederalGazetteModelElementInfo("4", "Australia"),
                new FederalGazetteModelElementInfo("5", "Canada GAAP"),
                new FederalGazetteModelElementInfo("6", "Japan GAAP"),
                new FederalGazetteModelElementInfo("99", "Sontiges")
            };
            #endregion
        }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> Salutation { get; set; }
        public FederalGazetteModelElementInfo SelectedSalutation { get; set; } //1, 2, 3

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> OrderTypes { get; set; }
        public FederalGazetteModelElementInfo SelectedOrderType { get; set; } //1, 2, 3

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> PublicationArea { get; set; }
        public FederalGazetteModelElementInfo SelectedPublicatioanArea { get; set; } //22

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> PublicationType { get; set; }
        public FederalGazetteModelElementInfo SelectedPublicationType { get; set; } //135, 136, 137, 138, 139

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> PublicationSubType { get; set; }
        public FederalGazetteModelElementInfo SelectedPublicationSubType { get; set; } //3,5,10,11,12,13,14,15

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> PublicationLanguage { get; set; }
        public FederalGazetteModelElementInfo SelectedLanguage { get; set; } //de

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> CompanyType { get; set; }
        public FederalGazetteModelElementInfo SelectedCompanyType { get; set; } //1,2,3,4,5,6

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> CompanySize { get; set; }
        public FederalGazetteModelElementInfo SelectedCompanySize { get; set; } //small,midsize,big

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> PublicationCategory { get; set; }
        public FederalGazetteModelElementInfo SelectedPublicationCategory { get; set; } //small,midsize,big

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> CompanyRegisterationType { get; set; }
        public FederalGazetteModelElementInfo SelectedCompanyRegisterationType { get; set; }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> States { get; set; }
        public FederalGazetteModelElementInfo SelectedState { get; set; }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> CompanyLegalForm { get; set; }
        public FederalGazetteModelElementInfo SelectedCompanyLegalForm { get; set; }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> RegisteredCourt { get; set; }
        public FederalGazetteModelElementInfo SelectedRegisteredCourt { get; set; }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> RegisterationType { get; set; }
        public FederalGazetteModelElementInfo SelectedRegisterationType { get; set; }
        
        public ObservableCollectionAsync<FederalGazetteModelElementInfo> Countries { get; set; }

        public ObservableCollectionAsync<FederalGazetteModelElementInfo> BalanceSheetStandard { get; set; } 
        public FederalGazetteModelElementInfo SelectedBalanceSheetStandard { get; set; } //1,2,3,4,5,6,99  


        public string SubmitterType { get; set; } //parent, subsidiary
        public Document Document { get; set; }
        //Bilanz - bs
        public bool ExportBalanceSheet { get; set; }
        //Gewinn- Verlustrechnung - is
        public bool ExportIncomeStatement { get; set; }
        //Anhang - nt
        public bool ExportNotes { get; set; }
        //Anlagenspiegel - net.ass
        public bool ExportFixedAssets { get; set; }
        //Lagebericht -mgmtRep
        public bool ExportManagementReport { get; set; }
        //ergebnisverwendung - incomeUse
        public bool ExportNetProfit { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
          
        public DateTime? BlockingDate { get; set; }
        public string YourReference { get; set; }
        public bool? OldRegisterData { get; set; }
        public DateTime? AcquisitionPeriodStart { get; set; }
        public DateTime? AcquisitionPeriodEnd { get; set; }
        public bool? BB264 { get; set; }
        public bool? VU264 { get; set; }
        public bool? BH264 { get; set; }
        public DateTime? OriginalPublicationDate { get; set; }
        public string OriginalPublicationOrderNumber { get; set; }
        public bool? HasNaturalperson { get; set; }
        public string CompanyNameAddress { get; set; }
        public string CompanyDomicile { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string CompanyStreet { get; set; }
        public string CompanyPostCode { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public FederalGazetteModelElementInfo CompanySelectedCountry { get; set; }
        public string CompanyDevision { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyFax { get; set; }
        public string ContectPersonFirstName { get; set; }
        public string ContactPersonLastName { get; set; }
        public string ContactPersonTitle { get; set; }
        public string ContactPersonEmail { get; set; }
        public string ContactPersonSalutation { get; set; }
        public string ContactPersonPhone { get; set; }
        public string ContactPersonCell { get; set; }
        public List<ReferencedCompany> ReferencedCompanyList { get; set; }
        public BillingData BillAddress { get; set; }
        public BillingData BillReceiver { get; set; }

        public string CompanyId { get; set; }
        public bool AccountDeleted { get; set; }
        public bool DataSent { get; set; }
        public string EmailRecevingFederalGazetteAsHtml { get; set; }
        public bool? ShouldReceiveEmail { get; set; }
        public string SaveHtmlFilePath { get; set; }
        public bool ShouldSaveHtml { get; set; }
        public string TicketId { get; set; }
        public List<FederalGazetteFiles> Files = new List<FederalGazetteFiles>();
        public string OrderNumber { get; set; }
        public List<FederalGazetteOrder> OrderList { get; set; }
        public bool? HasNaturalPersonSpecified { get; set; }
        public string OrderStatus { get; set; }
        
        private string _companySign;
        public string CompanySign {
            get { return _companySign; }
            set {
                if (_companySign == value) return;
                _companySign = value;
                OnPropertyChanged("CompanySign");
            }
        }
        private string _companyName;
        public string CompanyName {
            get { return _companyName; }
            set {
                if (_companyName == value) return;
                _companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }

        private string _clientNumber;
        public string ClientNumber {
            get { return _clientNumber; }
            set {
                if (_clientNumber == value) return;
                _clientNumber = value;
                OnPropertyChanged("ClientNumber");
            }
        }
        private List<CompanyList> _companyList;
        public List<CompanyList> CompanyList
        {
            get { return _companyList; }
            set {
                if (value == _companyList) return;
                _companyList = value;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private List<ClientsList> _clientsLists = new List<ClientsList>();
        private ObservableCollectionAsync<ClientsList> _filteredClientList = new ObservableCollectionAsync<ClientsList>();
        public IEnumerable<ClientsList> ClientsLists { get { return _filteredClientList; } }
        private string _filter;
        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                
                ApplyFilter();
                OnPropertyChanged("Filter");
            }
        }

        #region methods
        
        private void ApplyFilter() {
            _filteredClientList.Clear();
            var filter = Filter == null ? null : Filter.ToLower();
            if (filter == null) {
                foreach (var clientsList in _clientsLists) {
                    _filteredClientList.Add(clientsList);
                }
                return;
            }

            foreach (var client in _clientsLists) {
                if ((client.ClientId.ToLower().Contains(filter) || client.CompanyName.ToLower().Contains(filter))) 
                    _filteredClientList.Add(client);
            }
            

        }

        public void GetClientsList() {
            var getClient = new FederalGazetteClientOperations(this);
            foreach (var client in getClient.GetClientList()) {
                _clientsLists.Add(client);
            }
            
            ApplyFilter();
            OnPropertyChanged("ClientsLists");
        }

        protected void OnPropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}