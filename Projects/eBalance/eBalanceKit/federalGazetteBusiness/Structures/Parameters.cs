// Not in use anymore!
//// --------------------------------------------------------------------------------
//// author: Sebastian Vetter
//// since: 2012-10-22
//// copyright 2012 AvenDATA GmbH
//// --------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using Utils;
//using federalGazetteBusiness.Structures.ValueTypes;

//namespace federalGazetteBusiness.Structures
//{
//    public class Parameters
//    {
//        public static Parameters Instance { get { return _instance ?? (_instance = new Parameters()); } }
//        private static Parameters _instance;

//        Parameters()
//        {
            
//            #region AcquisitationPeriodStart - "Beginn des Bezugszeitraums"
//            AcquisitationPeriodStart = new FederalGazetteElementDate(string.Empty, "AcquisitationPeriodStart",
//                                                                         new List<string>
//                                                                             {
//                                                                                 "genInfo.report.period.reportPeriodBegin",
//                                                                                 "genInfo.report.period.fiscalYearBegin"
//                                                                             });

//            #endregion // AcquisitationPeriodStart

//            #region AcquisitationPeriodEnd - "Ende des Bezugszeitraums",
//            AcquisitationPeriodEnd = new FederalGazetteElementDate(string.Empty, "AcquisitationPeriodEnd",
//                                                                       new List<string>
//                                                                           {
//                                                                               "genInfo.report.period.reportPeriodEnd",
//                                                                               "genInfo.report.period.fiscalYearEnd"
//                                                                           });
//            #endregion // AcquisitationPeriodEnd

//            #region BlockingDate
//            BlockingDate = new FederalGazetteElementDate(string.Empty, "BlockingDate");
//            #endregion

//            #region OrderTypes
//            OrderTypes = new FederalGazetteElementSelectionOption("OrderTypes", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("1", "Neue Veröffentlichung einstellen"),
//                new FederalGazetteElementInfoString("2", "Berichtigung zu einer bereits erfolgten Veröffentlichung"),
//                new FederalGazetteElementInfoString("3", "Ergänzung zu einer bereits erfolgten Veröffentlichung"),
//            });

            
//            #endregion OrderTypes

//            #region PublicationArea
//            PublicationArea = new FederalGazetteElementSelectionList("PublicationArea", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("22", "Rechnungslegung/Finanzberichte")
//            }, isSelectable: false);
//            #endregion PublicationArea

//            #region PublicationCategory
//            PublicationCategory = new FederalGazetteElementSelectionList("PublicationCategory", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("14", "Jahresabschlüsse/Jahresfinanzberichte"),
//                new FederalGazetteElementInfoString("90", "weitere Finanzberichte")
//            });
//            #endregion PublicationCategory

//            #region PublicationType

//            var publicationTypeList = new ObservableCollectionAsync<IFederalGazetteElementInfo>
//                                          {
//                                              new FederalGazetteElementInfoString("13", "Sontiges"),
//                                              new FederalGazetteElementInfoString("86", "§§ 264 Abs. 3, 264b HGB", false),
//                                              new FederalGazetteElementInfoString("121",
//                                                                                 "Hinweis auf Jahresfinanzbericht, Halbjahresfinanzbericht, Quartalsfinanzbericht, Zwischenmitteilung"),
//                                              new FederalGazetteElementInfoString("135",
//                                                                                 "Jahresabschluss/Jahresfinanzbericht"),
//                                              new FederalGazetteElementInfoString("136", "Halbjahresfinanzbericht"),
//                                              new FederalGazetteElementInfoString("137", "Zwischenmitteilung"),
//                                              new FederalGazetteElementInfoString("138", "Quartalsfinanzbericht"),
//                                              new FederalGazetteElementInfoString("139", "Fehlerbekanntmachung")
//                                          };
//            //PublicationType = new FederalGazetteElementOptions(publicationTypeList, publicationTypeList.FirstOrDefault(entry => entry.Id.Equals("135")));
//            PublicationType = new FederalGazetteElementSelectionList("PublicationType", publicationTypeList, publicationTypeList[3]);
//            #endregion PublicationType

//            #region PublicationSubType
//            PublicationSubType = new FederalGazetteElementSelectionList("PublicationSubType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("3", "Jahresabschluss/Jahresfinanzbericht"),
//                new FederalGazetteElementInfoString("5", "Konzernabschluss"),
//                new FederalGazetteElementInfoString("10", "Hinweis auf Jahresfinanzbericht"),
//                new FederalGazetteElementInfoString("11", "Hinweis auf Halbjahresfinanzbericht"),
//                new FederalGazetteElementInfoString("12", "Hinweis auf Quartalsfinanzbericht"),
//                new FederalGazetteElementInfoString("13", "Hinweis auf Zwischenmitteilung der Geschäftsführung"),
//                new FederalGazetteElementInfoString("14", "Fehlerbekanntmachung für Jahresfinanzbericht"),
//                new FederalGazetteElementInfoString("15", "Fehlerbekanntmachung für Halbjahresfinanzbericht")
//            });
//            #endregion PublicationSubType

//            #region PublicationLanguage
//            PublicationLanguage = new FederalGazetteElementSelectionList("PublicationLanguage", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("de", "Deutsch"),
//                new FederalGazetteElementInfoString("en", "English")
//            }){IsVisible = false};
//            #endregion PublicationLanguage

//            #region CompanyType
//            var types = new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("1", "Kreditinstitut",
//                                                    new List<string>()
//                                                    {"genInfo.report.id.specialAccountingStandard.RKV"}),
//                new FederalGazetteElementInfoString("2", "Pensionsfond"),
//                new FederalGazetteElementInfoString("3", "Finanzierungsdienstleister"),
//                new FederalGazetteElementInfoString("4", "Versicherung",
//                                                    "genInfo.report.id.specialAccountingStandard.RVV"),
//                new FederalGazetteElementInfoString("5", "Rückversicherungsgesellschaft"),
//                new FederalGazetteElementInfoString("6", "keine der genannten Gesellschaftsarten",
//                                                    new List<string>() {
//                                                        "genInfo.report.id.specialAccountingStandard.K",
//                                                        "genInfo.report.id.specialAccountingStandard.PBV",
//                                                        "genInfo.report.id.specialAccountingStandard.KHBV",
//                                                        "genInfo.report.id.specialAccountingStandard.EBV",
//                                                        "genInfo.report.id.specialAccountingStandard.WUV",
//                                                        "genInfo.report.id.specialAccountingStandard.VUV",
//                                                        "genInfo.report.id.specialAccountingStandard.LUF"
//                                                    })
//            };
//            CompanyType = new FederalGazetteElementSelectionList("CompanyType", types, types.Last(), taxonomyElement: "genInfo.report.id.specialAccountingStandard");
//            #endregion CompanyType

//            #region SubmitterType
//            SubmitterType = new FederalGazetteElementSelectionList("SubmitterType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("parent", "Muttergesellschaft"),
//                new FederalGazetteElementInfoString("subsidiary", "Tochtergesellschaft")}){IsNullable = true};
//            #endregion SubmitterType

//            #region CompanySize
//            CompanySize = new FederalGazetteElementSelectionOption("CompanySize", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("small", "Klein", new List<string>(){"genInfo.company.id.sizeClass.sizeClass.NK", "genInfo.company.id.sizeClass.sizeClass.KK"}),
//                new FederalGazetteElementInfoString("midsize", "Mittel", "genInfo.company.id.sizeClass.sizeClass.MK"),
//                new FederalGazetteElementInfoString("big", "Groß", "genInfo.company.id.sizeClass.sizeClass.GK")
//            }, taxonomyElement: "genInfo.company.id.sizeClass");
//            #endregion CompanySize

//            #region CompanyRegisterationType
//            CompanyRegisterationType = new FederalGazetteElementSelectionOption("CompanyRegisterationType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("german_registered",
//                                                   "Firma/Institution mit Sitz in Deutschland, eingetragen in ein Register"),
//                new FederalGazetteElementInfoString("german_not_registered",
//                                                   "Firma/Institution mit Sitz in Deutschland, nicht eingetragen in ein Register"),
//                new FederalGazetteElementInfoString("foreign", "Firma mit Sitz im Ausland")
//            });
//            #endregion CompanyRegisterationType

//            #region States
//            States = new FederalGazetteElementSelectionList("States", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("BW", "Baden-Württemberg"),
//                new FederalGazetteElementInfoString("BY", "Bayern"),
//                new FederalGazetteElementInfoString("BE", "Berlin"),
//                new FederalGazetteElementInfoString("BR", "Brandenburg"),
//                new FederalGazetteElementInfoString("HB", "Bremen"),
//                new FederalGazetteElementInfoString("HH", "Hamburg"),
//                new FederalGazetteElementInfoString("HE", "Hessen"),
//                new FederalGazetteElementInfoString("MV", "Mecklenburg-Vorpommern"),
//                new FederalGazetteElementInfoString("NI", "Niedersachsen"),
//                new FederalGazetteElementInfoString("NW", "Nordrhein-Westfalen"),
//                new FederalGazetteElementInfoString("RP", "Rheinland-Pfalz"),
//                new FederalGazetteElementInfoString("SL", "Saarland"),
//                new FederalGazetteElementInfoString("SN", "Sachsen"),
//                new FederalGazetteElementInfoString("ST", "Sachsen-Anhalt"),
//                new FederalGazetteElementInfoString("SH", "Schleswig-Holstein"),
//                new FederalGazetteElementInfoString("TH", "Thüringen")
//            });
//            #endregion States

//            #region CompanyLegalForm
//            CompanyLegalForm = new FederalGazetteElementSelectionList("CompanyLegalForm", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("AG", "Aktiengesellschaft"),
//                new FederalGazetteElementInfoString("eGen", "eingetragene Genossenschaft"),
//                new FederalGazetteElementInfoString("EK", "Einzelkaufmann"),
//                new FederalGazetteElementInfoString("Ekf", "Einzelkauffrau"),
//                new FederalGazetteElementInfoString("Ewl", "Europäische wirtschaftliche Interessenvereinigung"),
//                new FederalGazetteElementInfoString("GmbH", "Gesellschaft mit beschränkter Haftung"),
//                new FederalGazetteElementInfoString("KG", "Kommanditgesellschaft (inkl. GmbH & Co.KG)"),
//                new FederalGazetteElementInfoString("KGaA", "Kommanditgesellschaft auf Aktien"),
//                new FederalGazetteElementInfoString("OHG", "Offene Handelsgesellschaft (inkl. GmbH Co. OHG)"),
//                new FederalGazetteElementInfoString("Part", "Partnerschaft"),
//                new FederalGazetteElementInfoString("SE", "Europäische Aktiengesellschaft (SE)"),
//                new FederalGazetteElementInfoString("Ver", "eingetragener Verein"),
//                new FederalGazetteElementInfoString("VvaG", "Versicherungsverein auf Gegenseitigkeit"),
//                new FederalGazetteElementInfoString("ARGnR", "Rechtsform ausländischen Rechts GnR"),
//                new FederalGazetteElementInfoString("ARHRA", "Rechtsform ausländischen Rechts HRA"),
//                new FederalGazetteElementInfoString("ARHRB", "Rechtsform ausländischen Rechts HRB (z.B. Limited)"),
//                new FederalGazetteElementInfoString("ARPR", "Rechtsform ausländischen Rechts PR"),
//                new FederalGazetteElementInfoString("HRAJP", "HRA Juristische Person"),
//                new FederalGazetteElementInfoString("SG", "Seerechtliche Gesellschaft"),
//            });
//            #endregion CompanyLegalForm

//            #region RegisteredCourt
//            RegisteredCourt = new FederalGazetteElementSelectionList("RegisteredCourt", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("R3101", "Aachen"),
//                new FederalGazetteElementInfoString("D3101", "Amberg"),
//                new FederalGazetteElementInfoString("D3201", "Ansbach"),
//                new FederalGazetteElementInfoString("R1901", "Arnsberg"),
//                new FederalGazetteElementInfoString("D4102", "Aschaffenburg"),
//                new FederalGazetteElementInfoString("D2102", "Augsburg"),
//                new FederalGazetteElementInfoString("P3101", "Aurich"),
//                new FederalGazetteElementInfoString("M1305", "Bad Hersfeld"),
//                new FederalGazetteElementInfoString("M1202", "Bad Homburg v.d.H."),
//                new FederalGazetteElementInfoString("T2101", "Bad Kreuznach"),
//                new FederalGazetteElementInfoString("R2108", "Bad Oeynhausen"),
//                new FederalGazetteElementInfoString("D4201", "Bamberg"),
//                new FederalGazetteElementInfoString("D4301", "Bayreuth"),
//                new FederalGazetteElementInfoString("F1103", "Berlin (Charlottenburg)"),
//                new FederalGazetteElementInfoString("R2101", "Bielefeld"),
//                new FederalGazetteElementInfoString("R2201", "Bochum"),
//                new FederalGazetteElementInfoString("R3201", "Bonn"),
//                new FederalGazetteElementInfoString("P1103", "Braunschweig"),
//                new FederalGazetteElementInfoString("H1101", "Bremen"),
//                new FederalGazetteElementInfoString("H1102", "Bremerhaven"),
//                new FederalGazetteElementInfoString("U1206", "Chemnitz"),
//                new FederalGazetteElementInfoString("D4401", "Coburg"),
//                new FederalGazetteElementInfoString("R2707", "Coesfeld"),
//                new FederalGazetteElementInfoString("G1103", "Cottbus"),
//                new FederalGazetteElementInfoString("M1103", "Darmstadt"),
//                new FederalGazetteElementInfoString("D2201", "Deggendorf"),
//                new FederalGazetteElementInfoString("R2402", "Dortmund"),
//                new FederalGazetteElementInfoString("U1104", "Dresden"),
//                new FederalGazetteElementInfoString("R1202", "Duisburg"),
//                new FederalGazetteElementInfoString("R3103", "Düren"),
//                new FederalGazetteElementInfoString("R1101", "Düsseldorf"),
//                new FederalGazetteElementInfoString("M1602", "Eschwege"),
//                new FederalGazetteElementInfoString("R2503", "Essen"),
//                new FederalGazetteElementInfoString("X1112", "Flensburg"),
//                new FederalGazetteElementInfoString("M1201", "Frankfurt am Main"),
//                new FederalGazetteElementInfoString("G1207", "Frankfurt/Oder"),
//                new FederalGazetteElementInfoString("B1204", "Freiburg"),
//                new FederalGazetteElementInfoString("M1405", "Friedberg"),
//                new FederalGazetteElementInfoString("M1603", "Fritzlar"),
//                new FederalGazetteElementInfoString("M1301", "Fulda"),
//                new FederalGazetteElementInfoString("D3304", "Fürth"),
//                new FederalGazetteElementInfoString("R2507", "Gelsenkirchen"),
//                new FederalGazetteElementInfoString("M1406", "Gießen"),
//                new FederalGazetteElementInfoString("P2204", "Göttingen"),
//                new FederalGazetteElementInfoString("R2103", "Gütersloh"),
//                new FederalGazetteElementInfoString("R2602", "Hagen"),
//                new FederalGazetteElementInfoString("K1101", "Hamburg"),
//                new FederalGazetteElementInfoString("R2404", "Hamm"),
//                new FederalGazetteElementInfoString("M1502", "Hanau"),
//                new FederalGazetteElementInfoString("P2305", "Hannover"),
//                new FederalGazetteElementInfoString("P2408", "Hildesheim"),
//                new FederalGazetteElementInfoString("D4501", "Hof"),
//                new FederalGazetteElementInfoString("V1102", "Homburg"),
//                new FederalGazetteElementInfoString("D5701", "Ingolstadt"),
//                new FederalGazetteElementInfoString("R2604", "Iserlohn"),
//                new FederalGazetteElementInfoString("Y1206", "Jena"),
//                new FederalGazetteElementInfoString("T3201", "Kaiserslautern"),
//                new FederalGazetteElementInfoString("M1607", "Kassel"),
//                new FederalGazetteElementInfoString("D2304", "Kempten (Allgäu)"),
//                new FederalGazetteElementInfoString("R3305", "Kerpen"),
//                new FederalGazetteElementInfoString("X1517", "Kiel"),
//                new FederalGazetteElementInfoString("R1304", "Kleve"),
//                new FederalGazetteElementInfoString("T2210", "Koblenz"),
//                new FederalGazetteElementInfoString("R3306", "Köln"),
//                new FederalGazetteElementInfoString("M1203", "Königstein"),
//                new FederalGazetteElementInfoString("M1608", "Korbach"),
//                new FederalGazetteElementInfoString("R1402", "Krefeld"),
//                new FederalGazetteElementInfoString("T3304", "Landau"),
//                new FederalGazetteElementInfoString("D2404", "Landshut"),
//                new FederalGazetteElementInfoString("R1105", "Langenfeld"),
//                new FederalGazetteElementInfoString("V1103", "Lebach"),
//                new FederalGazetteElementInfoString("U1308", "Leipzig"),
//                new FederalGazetteElementInfoString("R2307", "Lemgo"),
//                new FederalGazetteElementInfoString("R3311", "Leverkusen"),
//                new FederalGazetteElementInfoString("M1706", "Limburg"),
//                new FederalGazetteElementInfoString("X1721", "Lübeck"),
//                new FederalGazetteElementInfoString("T3104", "Ludwigshafen a.Rhein (Ludwigshafen)"),
//                new FederalGazetteElementInfoString("P2507", "Lüneburg"),
//                new FederalGazetteElementInfoString("T2304", "Mainz"),
//                new FederalGazetteElementInfoString("B1601", "Mannheim"),
//                new FederalGazetteElementInfoString("M1809", "Marburg"),
//                new FederalGazetteElementInfoString("D2505", "Memmingen"),
//                new FederalGazetteElementInfoString("V1104", "Merzig"),
//                new FederalGazetteElementInfoString("R1504", "Mönchengladbach"),
//                new FederalGazetteElementInfoString("T2214", "Montabaur"),
//                new FederalGazetteElementInfoString("D2601", "München"),
//                new FederalGazetteElementInfoString("R2713", "Münster"),
//                new FederalGazetteElementInfoString("N1105", "Neubrandenburg"),
//                new FederalGazetteElementInfoString("V1105", "Neunkirchen"),
//                new FederalGazetteElementInfoString("G1309", "Neuruppin"),
//                new FederalGazetteElementInfoString("R1102", "Neuss"),
//                new FederalGazetteElementInfoString("D3310", "Nürnberg"),
//                new FederalGazetteElementInfoString("M1114", "Offenbach am Main"),
//                new FederalGazetteElementInfoString("P3210", "Oldenburg (Oldenburg)"),
//                new FederalGazetteElementInfoString("P3313", "Osnabrück"),
//                new FederalGazetteElementInfoString("V1107", "Ottweiler"),
//                new FederalGazetteElementInfoString("R2809", "Paderborn"),
//                new FederalGazetteElementInfoString("D2803", "Passau"),
//                new FederalGazetteElementInfoString("X1321", "Pinneberg"),
//                new FederalGazetteElementInfoString("G1312", "Potsdam"),
//                new FederalGazetteElementInfoString("R2204", "Recklinghausen"),
//                new FederalGazetteElementInfoString("D3410", "Regensburg"),
//                new FederalGazetteElementInfoString("N1206", "Rostock"),
//                new FederalGazetteElementInfoString("V1109", "Saarbrücken"),
//                new FederalGazetteElementInfoString("V1110", "Saarlouis"),
//                new FederalGazetteElementInfoString("R3106", "Schleiden"),
//                new FederalGazetteElementInfoString("D4608", "Schweinfurt"),
//                new FederalGazetteElementInfoString("N1308", "Schwerin"),
//                new FederalGazetteElementInfoString("R3208", "Siegburg"),
//                new FederalGazetteElementInfoString("R2909", "Siegen"),
//                new FederalGazetteElementInfoString("V1111", "St. Ingbert (St Ingbert)"),
//                new FederalGazetteElementInfoString("V1112", "St. Wendel (St Wendel)"),
//                new FederalGazetteElementInfoString("P2106", "Stadthagen"),
//                new FederalGazetteElementInfoString("R2706", "Steinfurt"),
//                new FederalGazetteElementInfoString("W1215", "Stendal"),
//                new FederalGazetteElementInfoString("N1209", "Stralsund"),
//                new FederalGazetteElementInfoString("D3413", "Straubing"),
//                new FederalGazetteElementInfoString("B2609", "Stuttgart"),
//                new FederalGazetteElementInfoString("P2613", "Tostedt"),
//                new FederalGazetteElementInfoString("D2910", "Traunstein"),
//                new FederalGazetteElementInfoString("B2805", "Ulm"),
//                new FederalGazetteElementInfoString("V1115", "Völklingen"),
//                new FederalGazetteElementInfoString("P2716", "Walsrode"),
//                new FederalGazetteElementInfoString("D3508", "Weiden i. d. OPf."),
//                new FederalGazetteElementInfoString("M1710", "Wetzlar"),
//                new FederalGazetteElementInfoString("M1906", "Wiesbaden"),
//                new FederalGazetteElementInfoString("T2408", "Wittlich"),
//                new FederalGazetteElementInfoString("R1608", "Wuppertal"),
//                new FederalGazetteElementInfoString("D4708", "Würzburg"),
//                new FederalGazetteElementInfoString("T3403", "Zweibrücken"),

//            });
//            #endregion RegisteredCourt

//            #region RegisterationType
//            RegisterationType = new FederalGazetteElementSelectionList("RegisterationType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("HRA", "Personengesellschaften (HRA)"),
//                new FederalGazetteElementInfoString("HRB", "Kapitalgesellschaften (HRB)"),
//                new FederalGazetteElementInfoString("VR", "Vereinsregister (VR)"),
//                new FederalGazetteElementInfoString("GnR", "Genossenschaftsregister (GnR)"),
//                new FederalGazetteElementInfoString("PR", "Partnerschaftsregister (PR)"),

//            });
//            #endregion

//            #region countries
//            Countries = new FederalGazetteElementSelectionList("Countries", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("de", "Deutschland"),
//                new FederalGazetteElementInfoString("at", "Austria"),
//                new FederalGazetteElementInfoString("ch", "Switzerland"),
//                new FederalGazetteElementInfoString("nl", "Netherlands"),
//                new FederalGazetteElementInfoString("fr", "France")
//            });
//            #endregion countries

//            #region Salutation
//            Salutation = new FederalGazetteElementSelectionList("Salutation", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("Frau", "Frau"),
//                new FederalGazetteElementInfoString("Herr", "Herr")
                
//            });
//            #endregion

//            #region BalanceSheetStandard
//            BalanceSheetStandard = new FederalGazetteElementSelectionList("BalanceSheetStandard", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
//                new FederalGazetteElementInfoString("1", "HGB"),
//                new FederalGazetteElementInfoString("2", "IFRS"),
//                new FederalGazetteElementInfoString("3", "US-GAAP"),
//                new FederalGazetteElementInfoString("4", "Australia"),
//                new FederalGazetteElementInfoString("5", "Canada GAAP"),
//                new FederalGazetteElementInfoString("6", "Japan GAAP"),
//                new FederalGazetteElementInfoString("99", "Sontiges")
//            }, null, false);
//            #endregion

//            #region AnnualContentType

//            var annualContentType = new ObservableCollectionAsync<IFederalGazetteElementInfo>
//                                        {
//                                            new FederalGazetteElementInfoString("0", "Freitext"),
//                                            new FederalGazetteElementInfoString("1",
//                                                                               "Vollständige Rechnungslegungsunterlagen"),
//                                            new FederalGazetteElementInfoString("2", "Lagebericht"),
//                                            new FederalGazetteElementInfoString("3", "Bilanz"),
//                                            new FederalGazetteElementInfoString("4", "GuV"),
//                                            new FederalGazetteElementInfoString("5", "Anhang"),
//                                            new FederalGazetteElementInfoString("6", "Bestätigungsvermerk"),
//                                            new FederalGazetteElementInfoString("7", "Bericht des Aufsichtsrates"),
//                                            new FederalGazetteElementInfoString("8",
//                                                                               "Vorschlag für die Verwendung des Ergebnisses"),
//                                            new FederalGazetteElementInfoString("9",
//                                                                               "Beschluss über die Ergebnisverwendung"),
//                                            new FederalGazetteElementInfoString("10", "Entsprechenserklärung"),
//                                            new FederalGazetteElementInfoString("11", "Kapitalflussrechnung"),
//                                            new FederalGazetteElementInfoString("12", "Eigenkapitalspiegel"),
//                                            new FederalGazetteElementInfoString("13", "Sontiges")
//                                        };
            
//            //AnnualContentType = new FederalGazetteElementOptions(annualContentType, annualContentType.FirstOrDefault(entry => entry.Id.Equals("1")), false);
//            AnnualContentType = new FederalGazetteElementSelectionList("AnnualContentType", annualContentType, annualContentType[1], false);
//            #endregion

//            //var hasNaturalPersonOptions = new ObservableCollectionAsync<IFederalGazetteElementInfo>
//            //                              {
//            //                                  new FederalGazetteElementInfoString(true.ToString(), true.ToString()),
//            //                                  new FederalGazetteElementInfoString(false.ToString(), false.ToString()),
//            //                                  new FederalGazetteElementInfoString(true.ToString(), true.ToString())
//            //                              };
//            HasNaturalPerson = new FederalGazetteElementBool("HasNaturalPerson", true);
//            HasNaturalPerson2 = new FederalGazetteElementBool("HasNaturalPerson2");

//            SendParameters = new FederalGazetteElementList();
//            SendParameters.Add(OrderTypes);
//            SendParameters.Add(PublicationArea);
//            SendParameters.Add(PublicationCategory);
//            SendParameters.Add(PublicationType);
//            SendParameters.Add(PublicationSubType);
//            SendParameters.Add(PublicationLanguage);
//            SendParameters.Add(CompanyType);
//            SendParameters.Add(SubmitterType);
//            SendParameters.Add(BalanceSheetStandard);
//            SendParameters.Add(CompanySize);
//            SendParameters.Add(BlockingDate);
//            SendParameters.Add(HasNaturalPerson);
//            SendParameters.Add(HasNaturalPerson2);

//            LoadValuesFromDatabase();
//            SendParameters.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SendParameterPropertyChanged);

//            FederalGazetteElementInfoBase.DisableSaving = false;
//        }

//        private void LoadValuesFromDatabase() {

//            //IValueMapping mapping = eBalanceKitBusiness.Structures.DbMapping.ValueMappings.ValueMappingBase.Load(conn, typeof(DbElementValue), null, "", (idObject as Document).Id,
//            //                                        idObject);
//            using (DbAccess.IDatabase conn = eBalanceKitBusiness.Structures.AppConfig.ConnectionManager.GetConnection()) {
//                try {
//                    if (eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument == null){
//                        System.Diagnostics.Debug.Fail("No document loaded!!!");
//                        return;
//                    }
//                    var values = conn.DbMapping.Load<DbElementValue>(conn.Enquote("document_id") + "=" + eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.Id);
//                    foreach (var dbElementValue in values) {
//                        SetElementValue(dbElementValue, dbElementValue.ElementValue);
//                    }
//                } catch (Exception exception) {
//                    eBalanceKitBase.Structures.ExceptionLogging.LogException(exception);
//                    //throw;
//                }
//            }
//        }

//        private void SendParameterPropertyChanged(object sender, PropertyChangedEventArgs e) {
//            if (e.PropertyName.Equals("Value")) {
//                UpdateAndValidateSendParameters();
//            }
//        }


//        public IFederalGazetteElementInfo OrderTypes { get; set; }

//        public IFederalGazetteElementInfo PublicationArea { get; set; }

//        public IFederalGazetteElementInfo PublicationType { get; set; }

//        public IFederalGazetteElementInfo PublicationSubType { get; set; }

//        public IFederalGazetteElementInfo PublicationLanguage { get; set; }

//        public IFederalGazetteElementInfo CompanyType { get; set; }

//        public IFederalGazetteElementInfo CompanySize { get; set; }

//        public IFederalGazetteElementInfo PublicationCategory { get; set; }

//        public IFederalGazetteElementInfo SubmitterType { get; set; }

//        public IFederalGazetteElementInfo CompanyRegisterationType { get; set; }

//        public IFederalGazetteElementInfo States { get; set; }

//        public IFederalGazetteElementInfo CompanyLegalForm { get; set; }

//        public IFederalGazetteElementInfo RegisteredCourt { get; set; }

//        public IFederalGazetteElementInfo RegisterationType { get; set; }

//        public IFederalGazetteElementInfo Countries { get; set; }

//        public IFederalGazetteElementInfo BalanceSheetStandard { get; set; }

//        public IFederalGazetteElementInfo AnnualContentType { get; set; }
        
//        public IFederalGazetteElementInfo HasNaturalPerson { get; set; }
//        public IFederalGazetteElementInfo HasNaturalPerson2 { get; set; }

//        public IFederalGazetteElementInfo AcquisitationPeriodStart { get; set; }
//        public IFederalGazetteElementInfo AcquisitationPeriodEnd { get; set; }
//        public IFederalGazetteElementInfo BlockingDate { get; set; }


//        public FederalGazetteElementList SendParameters { get; set; }
//    }
//}