// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using Utils;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    /// <summary>
    /// Class that contains all Parameter requiered for getting informations about the <see cref="federalGazetteBusiness.External.EbanzClient"/> or creating new one.
    /// </summary>
    public class FederalGazetteClientParameter : FederalGazetteParameterBase {

        public FederalGazetteClientParameter() {

            #region CompanyRegisterationType
            CompanyRegisterationType = new FederalGazetteElementSelectionOption("CompanyRegisterationType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("german_registered",
                                                   "Firma/Institution mit Sitz in Deutschland, eingetragen in ein Register"),
                new FederalGazetteElementInfoString("german_not_registered",
                                                   "Firma/Institution mit Sitz in Deutschland, nicht eingetragen in ein Register"),
                new FederalGazetteElementInfoString("foreign", "Firma mit Sitz im Ausland")
            });
            #endregion CompanyRegisterationType

            CompanyType = new FederalGazetteElementSelectionList("CompanyType", OCompanyTypes, OCompanyTypes.Last(), taxonomyElement: "genInfo.report.id.specialAccountingStandard");


            #region CompanyLegalForm
            CompanyLegalForm = new FederalGazetteElementSelectionList("CompanyLegalForm", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("AG", "Aktiengesellschaft"),
                new FederalGazetteElementInfoString("eGen", "eingetragene Genossenschaft"),
                new FederalGazetteElementInfoString("EK", "Einzelkaufmann"),
                new FederalGazetteElementInfoString("Ekf", "Einzelkauffrau"),
                new FederalGazetteElementInfoString("Ewl", "Europäische wirtschaftliche Interessenvereinigung"),
                new FederalGazetteElementInfoString("GmbH", "Gesellschaft mit beschränkter Haftung"),
                new FederalGazetteElementInfoString("KG", "Kommanditgesellschaft (inkl. GmbH & Co.KG)"),
                new FederalGazetteElementInfoString("KGaA", "Kommanditgesellschaft auf Aktien"),
                new FederalGazetteElementInfoString("OHG", "Offene Handelsgesellschaft (inkl. GmbH Co. OHG)"),
                new FederalGazetteElementInfoString("Part", "Partnerschaft"),
                new FederalGazetteElementInfoString("SE", "Europäische Aktiengesellschaft (SE)"),
                new FederalGazetteElementInfoString("Ver", "eingetragener Verein"),
                new FederalGazetteElementInfoString("VvaG", "Versicherungsverein auf Gegenseitigkeit"),
                new FederalGazetteElementInfoString("ARGnR", "Rechtsform ausländischen Rechts GnR"),
                new FederalGazetteElementInfoString("ARHRA", "Rechtsform ausländischen Rechts HRA"),
                new FederalGazetteElementInfoString("ARHRB", "Rechtsform ausländischen Rechts HRB (z.B. Limited)"),
                new FederalGazetteElementInfoString("ARPR", "Rechtsform ausländischen Rechts PR"),
                new FederalGazetteElementInfoString("HRAJP", "HRA Juristische Person"),
                new FederalGazetteElementInfoString("SG", "Seerechtliche Gesellschaft"),
            });
            #endregion CompanyLegalForm

            #region RegisteredCourt
            RegisteredCourt = new FederalGazetteElementSelectionList("RegisteredCourt", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("R3101", "Aachen"),
                new FederalGazetteElementInfoString("D3101", "Amberg"),
                new FederalGazetteElementInfoString("D3201", "Ansbach"),
                new FederalGazetteElementInfoString("R1901", "Arnsberg"),
                new FederalGazetteElementInfoString("D4102", "Aschaffenburg"),
                new FederalGazetteElementInfoString("D2102", "Augsburg"),
                new FederalGazetteElementInfoString("P3101", "Aurich"),
                new FederalGazetteElementInfoString("M1305", "Bad Hersfeld"),
                new FederalGazetteElementInfoString("M1202", "Bad Homburg v.d.H."),
                new FederalGazetteElementInfoString("T2101", "Bad Kreuznach"),
                new FederalGazetteElementInfoString("R2108", "Bad Oeynhausen"),
                new FederalGazetteElementInfoString("D4201", "Bamberg"),
                new FederalGazetteElementInfoString("D4301", "Bayreuth"),
                new FederalGazetteElementInfoString("F1103", "Berlin (Charlottenburg)"),
                new FederalGazetteElementInfoString("R2101", "Bielefeld"),
                new FederalGazetteElementInfoString("R2201", "Bochum"),
                new FederalGazetteElementInfoString("R3201", "Bonn"),
                new FederalGazetteElementInfoString("P1103", "Braunschweig"),
                new FederalGazetteElementInfoString("H1101", "Bremen"),
                new FederalGazetteElementInfoString("H1102", "Bremerhaven"),
                new FederalGazetteElementInfoString("U1206", "Chemnitz"),
                new FederalGazetteElementInfoString("D4401", "Coburg"),
                new FederalGazetteElementInfoString("R2707", "Coesfeld"),
                new FederalGazetteElementInfoString("G1103", "Cottbus"),
                new FederalGazetteElementInfoString("M1103", "Darmstadt"),
                new FederalGazetteElementInfoString("D2201", "Deggendorf"),
                new FederalGazetteElementInfoString("R2402", "Dortmund"),
                new FederalGazetteElementInfoString("U1104", "Dresden"),
                new FederalGazetteElementInfoString("R1202", "Duisburg"),
                new FederalGazetteElementInfoString("R3103", "Düren"),
                new FederalGazetteElementInfoString("R1101", "Düsseldorf"),
                new FederalGazetteElementInfoString("M1602", "Eschwege"),
                new FederalGazetteElementInfoString("R2503", "Essen"),
                new FederalGazetteElementInfoString("X1112", "Flensburg"),
                new FederalGazetteElementInfoString("M1201", "Frankfurt am Main"),
                new FederalGazetteElementInfoString("G1207", "Frankfurt/Oder"),
                new FederalGazetteElementInfoString("B1204", "Freiburg"),
                new FederalGazetteElementInfoString("M1405", "Friedberg"),
                new FederalGazetteElementInfoString("M1603", "Fritzlar"),
                new FederalGazetteElementInfoString("M1301", "Fulda"),
                new FederalGazetteElementInfoString("D3304", "Fürth"),
                new FederalGazetteElementInfoString("R2507", "Gelsenkirchen"),
                new FederalGazetteElementInfoString("M1406", "Gießen"),
                new FederalGazetteElementInfoString("P2204", "Göttingen"),
                new FederalGazetteElementInfoString("R2103", "Gütersloh"),
                new FederalGazetteElementInfoString("R2602", "Hagen"),
                new FederalGazetteElementInfoString("K1101", "Hamburg"),
                new FederalGazetteElementInfoString("R2404", "Hamm"),
                new FederalGazetteElementInfoString("M1502", "Hanau"),
                new FederalGazetteElementInfoString("P2305", "Hannover"),
                new FederalGazetteElementInfoString("P2408", "Hildesheim"),
                new FederalGazetteElementInfoString("D4501", "Hof"),
                new FederalGazetteElementInfoString("V1102", "Homburg"),
                new FederalGazetteElementInfoString("D5701", "Ingolstadt"),
                new FederalGazetteElementInfoString("R2604", "Iserlohn"),
                new FederalGazetteElementInfoString("Y1206", "Jena"),
                new FederalGazetteElementInfoString("T3201", "Kaiserslautern"),
                new FederalGazetteElementInfoString("M1607", "Kassel"),
                new FederalGazetteElementInfoString("D2304", "Kempten (Allgäu)"),
                new FederalGazetteElementInfoString("R3305", "Kerpen"),
                new FederalGazetteElementInfoString("X1517", "Kiel"),
                new FederalGazetteElementInfoString("R1304", "Kleve"),
                new FederalGazetteElementInfoString("T2210", "Koblenz"),
                new FederalGazetteElementInfoString("R3306", "Köln"),
                new FederalGazetteElementInfoString("M1203", "Königstein"),
                new FederalGazetteElementInfoString("M1608", "Korbach"),
                new FederalGazetteElementInfoString("R1402", "Krefeld"),
                new FederalGazetteElementInfoString("T3304", "Landau"),
                new FederalGazetteElementInfoString("D2404", "Landshut"),
                new FederalGazetteElementInfoString("R1105", "Langenfeld"),
                new FederalGazetteElementInfoString("V1103", "Lebach"),
                new FederalGazetteElementInfoString("U1308", "Leipzig"),
                new FederalGazetteElementInfoString("R2307", "Lemgo"),
                new FederalGazetteElementInfoString("R3311", "Leverkusen"),
                new FederalGazetteElementInfoString("M1706", "Limburg"),
                new FederalGazetteElementInfoString("X1721", "Lübeck"),
                new FederalGazetteElementInfoString("T3104", "Ludwigshafen a.Rhein (Ludwigshafen)"),
                new FederalGazetteElementInfoString("P2507", "Lüneburg"),
                new FederalGazetteElementInfoString("T2304", "Mainz"),
                new FederalGazetteElementInfoString("B1601", "Mannheim"),
                new FederalGazetteElementInfoString("M1809", "Marburg"),
                new FederalGazetteElementInfoString("D2505", "Memmingen"),
                new FederalGazetteElementInfoString("V1104", "Merzig"),
                new FederalGazetteElementInfoString("R1504", "Mönchengladbach"),
                new FederalGazetteElementInfoString("T2214", "Montabaur"),
                new FederalGazetteElementInfoString("D2601", "München"),
                new FederalGazetteElementInfoString("R2713", "Münster"),
                new FederalGazetteElementInfoString("N1105", "Neubrandenburg"),
                new FederalGazetteElementInfoString("V1105", "Neunkirchen"),
                new FederalGazetteElementInfoString("G1309", "Neuruppin"),
                new FederalGazetteElementInfoString("R1102", "Neuss"),
                new FederalGazetteElementInfoString("D3310", "Nürnberg"),
                new FederalGazetteElementInfoString("M1114", "Offenbach am Main"),
                new FederalGazetteElementInfoString("P3210", "Oldenburg (Oldenburg)"),
                new FederalGazetteElementInfoString("P3313", "Osnabrück"),
                new FederalGazetteElementInfoString("V1107", "Ottweiler"),
                new FederalGazetteElementInfoString("R2809", "Paderborn"),
                new FederalGazetteElementInfoString("D2803", "Passau"),
                new FederalGazetteElementInfoString("X1321", "Pinneberg"),
                new FederalGazetteElementInfoString("G1312", "Potsdam"),
                new FederalGazetteElementInfoString("R2204", "Recklinghausen"),
                new FederalGazetteElementInfoString("D3410", "Regensburg"),
                new FederalGazetteElementInfoString("N1206", "Rostock"),
                new FederalGazetteElementInfoString("V1109", "Saarbrücken"),
                new FederalGazetteElementInfoString("V1110", "Saarlouis"),
                new FederalGazetteElementInfoString("R3106", "Schleiden"),
                new FederalGazetteElementInfoString("D4608", "Schweinfurt"),
                new FederalGazetteElementInfoString("N1308", "Schwerin"),
                new FederalGazetteElementInfoString("R3208", "Siegburg"),
                new FederalGazetteElementInfoString("R2909", "Siegen"),
                new FederalGazetteElementInfoString("V1111", "St. Ingbert (St Ingbert)"),
                new FederalGazetteElementInfoString("V1112", "St. Wendel (St Wendel)"),
                new FederalGazetteElementInfoString("P2106", "Stadthagen"),
                new FederalGazetteElementInfoString("R2706", "Steinfurt"),
                new FederalGazetteElementInfoString("W1215", "Stendal"),
                new FederalGazetteElementInfoString("N1209", "Stralsund"),
                new FederalGazetteElementInfoString("D3413", "Straubing"),
                new FederalGazetteElementInfoString("B2609", "Stuttgart"),
                new FederalGazetteElementInfoString("P2613", "Tostedt"),
                new FederalGazetteElementInfoString("D2910", "Traunstein"),
                new FederalGazetteElementInfoString("B2805", "Ulm"),
                new FederalGazetteElementInfoString("V1115", "Völklingen"),
                new FederalGazetteElementInfoString("P2716", "Walsrode"),
                new FederalGazetteElementInfoString("D3508", "Weiden i. d. OPf."),
                new FederalGazetteElementInfoString("M1710", "Wetzlar"),
                new FederalGazetteElementInfoString("M1906", "Wiesbaden"),
                new FederalGazetteElementInfoString("T2408", "Wittlich"),
                new FederalGazetteElementInfoString("R1608", "Wuppertal"),
                new FederalGazetteElementInfoString("D4708", "Würzburg"),
                new FederalGazetteElementInfoString("T3403", "Zweibrücken"),

            });
            #endregion RegisteredCourt

            #region RegisterationType
            RegisterationType = new FederalGazetteElementSelectionList("RegisterationType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("HRA", "Personengesellschaften (HRA)"),
                new FederalGazetteElementInfoString("HRB", "Kapitalgesellschaften (HRB)"),
                new FederalGazetteElementInfoString("VR", "Vereinsregister (VR)"),
                new FederalGazetteElementInfoString("GnR", "Genossenschaftsregister (GnR)"),
                new FederalGazetteElementInfoString("PR", "Partnerschaftsregister (PR)"),

            });
            #endregion


            CompanyName = new FederalGazetteElementText("CompanyName", "de-gcd_genInfo.company.id.name");

            CompanyDomicile = new FederalGazetteElementText("CompanyDomicile", "de-gcd_genInfo.company.id.location");

            CompanyId = new FederalGazetteElementText("CompanyId");

            AdressParameter = new FederalGazetteAdressParameter(ParameterType);


            LoadEverything();
        }

        public void AddContactPerson() { ContactPerson = new FederalGazettePersonParameter(ParameterType); }

        public IFederalGazetteElementInfo CompanyRegisterationType { get; set; }

        public IFederalGazetteElementInfo CompanyType { get; set; }

        public IFederalGazetteElementInfo CompanyName { get; set; }

        public IFederalGazetteElementInfo CompanyLegalForm { get; set; }

        public IFederalGazetteElementInfo CompanyDomicile { get; set; }

        public IFederalGazetteElementInfo CompanyId { get; set; }

        public IFederalGazetteElementInfo RegisteredCourt { get; set; }

        public IFederalGazetteElementInfo RegisterationType { get; set; }

        public IFederalGazetteElementInfo Countries { get; set; }

        public FederalGazetteAdressParameter AdressParameter { get; set; }

        public FederalGazettePersonParameter ContactPerson { get; set; }

        #region Overrides of FederalGazetteParameterBase
        public override ParameterArea ParameterType { get { return ParameterArea.Client; } }
        protected override void Validate() { }
        #endregion
    }
}