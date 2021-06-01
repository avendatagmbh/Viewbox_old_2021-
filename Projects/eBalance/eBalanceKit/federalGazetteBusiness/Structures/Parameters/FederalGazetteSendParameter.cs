// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utils;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    public class FederalGazetteSendParameter : FederalGazetteParameterBase {

        public FederalGazetteSendParameter() {
            FederalGazetteElementInfoBase.DisableSaving = true;
            #region AcquisitationPeriodStart - "Beginn des Bezugszeitraums"
            AcquisitationPeriodStart = new FederalGazetteElementDate(string.Empty, "AcquisitationPeriodStart",
                                                                         new List<string>
                                                                             {
                                                                                 "genInfo.report.period.reportPeriodBegin",
                                                                                 "genInfo.report.period.fiscalYearBegin"
                                                                             }, false) {IsVisible = false};

            #endregion // AcquisitationPeriodStart

            #region AcquisitationPeriodEnd - "Ende des Bezugszeitraums",
            AcquisitationPeriodEnd = new FederalGazetteElementDate(string.Empty, "AcquisitationPeriodEnd",
                                                                       new List<string>
                                                                           {
                                                                               "genInfo.report.period.reportPeriodEnd",
                                                                               "genInfo.report.period.fiscalYearEnd"
                                                                           }, false) {IsVisible = false};
            #endregion // AcquisitationPeriodEnd

            #region BlockingDate
            BlockingDate = new FederalGazetteElementDate(string.Empty, "BlockingDate"){IsNullable = true};
            #endregion

            #region OrderTypes
            OrderTypes = new FederalGazetteElementSelectionList("OrderTypes", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("1", "Neue Veröffentlichung einstellen"),
                new FederalGazetteElementInfoString("2", "Berichtigung zu einer bereits erfolgten Veröffentlichung"),
                new FederalGazetteElementInfoString("3", "Ergänzung zu einer bereits erfolgten Veröffentlichung"),
            });


            #endregion OrderTypes

            #region PublicationArea
            PublicationArea = new FederalGazetteElementSelectionList("PublicationArea", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("22", "Rechnungslegung/Finanzberichte")
            }, isSelectable: false) {IsVisible = false};
            #endregion PublicationArea

            #region PublicationCategory
            PublicationCategory = new FederalGazetteElementSelectionList("PublicationCategory", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("14", "Jahresabschlüsse/Jahresfinanzberichte"),
                new FederalGazetteElementInfoString("90", "weitere Finanzberichte")
            });
            #endregion PublicationCategory

            #region PublicationType

            var publicationTypeList = new ObservableCollectionAsync<IFederalGazetteElementInfo>
                                          {
                                              new FederalGazetteElementInfoString("13", "Sontiges"),
                                              new FederalGazetteElementInfoString("86", "§§ 264 Abs. 3, 264b HGB", false),
                                              new FederalGazetteElementInfoString("121",
                                                                                 "Hinweis auf Jahresfinanzbericht, Halbjahresfinanzbericht, Quartalsfinanzbericht, Zwischenmitteilung"),
                                              new FederalGazetteElementInfoString("135",
                                                                                 "Jahresabschluss/Jahresfinanzbericht"),
                                              new FederalGazetteElementInfoString("136", "Halbjahresfinanzbericht"),
                                              new FederalGazetteElementInfoString("137", "Zwischenmitteilung"),
                                              new FederalGazetteElementInfoString("138", "Quartalsfinanzbericht"),
                                              new FederalGazetteElementInfoString("139", "Fehlerbekanntmachung")
                                          };
            //PublicationType = new FederalGazetteElementOptions(publicationTypeList, publicationTypeList.FirstOrDefault(entry => entry.Id.Equals("135")));
            PublicationType = new FederalGazetteElementSelectionList("PublicationType", publicationTypeList, publicationTypeList[3]);
            #endregion PublicationType

            #region PublicationSubType
            PublicationSubType = new FederalGazetteElementSelectionList("PublicationSubType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("3", "Jahresabschluss/Jahresfinanzbericht"),
                new FederalGazetteElementInfoString("5", "Konzernabschluss"),
                new FederalGazetteElementInfoString("10", "Hinweis auf Jahresfinanzbericht"),
                new FederalGazetteElementInfoString("11", "Hinweis auf Halbjahresfinanzbericht"),
                new FederalGazetteElementInfoString("12", "Hinweis auf Quartalsfinanzbericht"),
                new FederalGazetteElementInfoString("13", "Hinweis auf Zwischenmitteilung der Geschäftsführung"),
                new FederalGazetteElementInfoString("14", "Fehlerbekanntmachung für Jahresfinanzbericht"),
                new FederalGazetteElementInfoString("15", "Fehlerbekanntmachung für Halbjahresfinanzbericht")
            });
            #endregion PublicationSubType

            #region PublicationLanguage
            PublicationLanguage = new FederalGazetteElementSelectionList("PublicationLanguage", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("de", "Deutsch"),
                new FederalGazetteElementInfoString("en", "English")
            }) { IsVisible = false };
            #endregion PublicationLanguage

            #region SubmitterType
            SubmitterType = new FederalGazetteElementSelectionList("SubmitterType", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("parent", "Muttergesellschaft"),
                new FederalGazetteElementInfoString("subsidiary", "Tochtergesellschaft")}) { IsNullable = true };
            #endregion SubmitterType

            #region CompanySize
            CompanySize = new FederalGazetteElementSelectionOption("CompanySize", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("small", "Klein", new List<string>(){"genInfo.company.id.sizeClass.sizeClass.NK", "genInfo.company.id.sizeClass.sizeClass.KK"}),
                new FederalGazetteElementInfoString("midsize", "Mittel", "genInfo.company.id.sizeClass.sizeClass.MK"),
                new FederalGazetteElementInfoString("big", "Groß", "genInfo.company.id.sizeClass.sizeClass.GK")
            }, taxonomyElement: "genInfo.company.id.sizeClass"){IsVisible = false};
            #endregion CompanySize


            #region BalanceSheetStandard
            BalanceSheetStandard = new FederalGazetteElementSelectionList("BalanceSheetStandard", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("1", "HGB"),
                new FederalGazetteElementInfoString("2", "IFRS"),
                new FederalGazetteElementInfoString("3", "US-GAAP"),
                new FederalGazetteElementInfoString("4", "Australia"),
                new FederalGazetteElementInfoString("5", "Canada GAAP"),
                new FederalGazetteElementInfoString("6", "Japan GAAP"),
                new FederalGazetteElementInfoString("99", "Sontiges")
            }, null, false);
            #endregion

            #region AnnualContentType

            var annualContentType = new ObservableCollectionAsync<IFederalGazetteElementInfo>
                                        {
                                            new FederalGazetteElementInfoString("0", "Freitext"),
                                            new FederalGazetteElementInfoString("1",
                                                                               "Vollständige Rechnungslegungsunterlagen"),
                                            new FederalGazetteElementInfoString("2", "Lagebericht"),
                                            new FederalGazetteElementInfoString("3", "Bilanz"),
                                            new FederalGazetteElementInfoString("4", "GuV"),
                                            new FederalGazetteElementInfoString("5", "Anhang"),
                                            new FederalGazetteElementInfoString("6", "Bestätigungsvermerk"),
                                            new FederalGazetteElementInfoString("7", "Bericht des Aufsichtsrates"),
                                            new FederalGazetteElementInfoString("8",
                                                                               "Vorschlag für die Verwendung des Ergebnisses"),
                                            new FederalGazetteElementInfoString("9",
                                                                               "Beschluss über die Ergebnisverwendung"),
                                            new FederalGazetteElementInfoString("10", "Entsprechenserklärung"),
                                            new FederalGazetteElementInfoString("11", "Kapitalflussrechnung"),
                                            new FederalGazetteElementInfoString("12", "Eigenkapitalspiegel"),
                                            new FederalGazetteElementInfoString("13", "Sontiges")
                                        };


            #region CompanyType
            CompanyType = new FederalGazetteElementSelectionList("CompanyType", OCompanyTypes, OCompanyTypes.Last(), taxonomyElement: "genInfo.report.id.specialAccountingStandard");
            #endregion CompanyType

            //AnnualContentType = new FederalGazetteElementOptions(annualContentType, annualContentType.FirstOrDefault(entry => entry.Id.Equals("1")), false);
            AnnualContentType = new FederalGazetteElementSelectionList("AnnualContentType", annualContentType, annualContentType[1], false);
            #endregion


            HasNaturalPerson = new FederalGazetteElementBool("HasNaturalPerson", true);

            FederalGazetteElementInfoBase.DisableSaving = false;
            LoadEverything();

            PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { if(args.PropertyName == "Value") Validate(); };
        }


        protected override void Validate() {
            ClearAllErrors();
            // Check required fields
            if (!HasValue(OrderTypes)) {
                AddError(string.Format(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_MustHaveValue, OrderTypes.Caption));
            }
            if (!HasValue(PublicationArea)) {
                AddError(string.Format(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_MustHaveValue, PublicationArea.Caption));
            }
            if (!HasValue(PublicationCategory)) {
                AddError(string.Format(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_MustHaveValue, PublicationCategory.Caption));
            }
            if (!HasValue(PublicationType)) {
                AddError(string.Format(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_MustHaveValue, PublicationType.Caption));
            }
            if (!HasValue(PublicationLanguage)) {
                AddError(string.Format(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_MustHaveValue, PublicationLanguage.Caption));
            }

            if (!IsValid) {
                return;
            }

            bool categoryMatch;
            bool typeMatch;
            bool subTypeMatch;

            // Check publication_sub_type
            if (!HasValue(PublicationSubType)) {
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135, 121, 139 });
                if (categoryMatch && typeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_PublicationSubType);
                    return;
                }
            }

            // Check company_type
            if (!HasValue(CompanyType)) {
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135 });
                subTypeMatch = IsValueSetTo(PublicationSubType.ToString(), new[] { 3, 5 });
                if (categoryMatch && typeMatch && subTypeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_CompanyType);
                    return;
                }
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 90 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 136, 138 });
                if (categoryMatch && typeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_CompanyType);
                    return;
                }
            }

            // Check submitter_type
            if (!HasValue(SubmitterType)) {
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 86 });
                if (categoryMatch && typeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_SubmitterType);
                    return;
                }
            }

            // Check BalanceSheetStandard
            if (!HasValue(BalanceSheetStandard)) {
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135 });
                subTypeMatch = IsValueSetTo(PublicationSubType.ToString(), new[] { 3, 5 });
                if (categoryMatch && typeMatch && subTypeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_BalanceSheetStandard);
                    return;
                }
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135 });
                subTypeMatch = IsValueSetTo(PublicationSubType.ToString(), new[] { 16 });
                if (categoryMatch && typeMatch && subTypeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_BalanceSheetStandard);
                    return;
                }
            }
            if (!HasValue(CompanySize)) {
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135 });
                subTypeMatch = IsValueSetTo(PublicationSubType.ToString(), new[] { 3, 5 });
                if (categoryMatch && typeMatch && subTypeMatch) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_CompanySize);
                    return;
                }
            }

            if (HasValue(BlockingDate)) {
                var failed = true;
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 135 });
                subTypeMatch = IsValueSetTo(PublicationSubType.ToString(), new[] { 3, 5 });
                if (categoryMatch && typeMatch && subTypeMatch) {
                    failed = false;
                    //return;
                }
                categoryMatch = IsValueSetTo(PublicationCategory, new[] { 14 });
                typeMatch = IsValueSetTo(PublicationType.ToString(), new[] { 86 });
                if (categoryMatch && typeMatch && failed) {
                    //SendParameters.AddError("Error_BlockingDate");
                    //return;
                    failed = false;
                }
                if (failed) {
                    AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_BlockingDate);
                    return;
                }
                System.DateTime yearEndDate;
                if (!(HasValue(AcquisitationPeriodEnd) && System.DateTime.TryParse(AcquisitationPeriodEnd.Value.ToString(), out yearEndDate))) {
                    System.Diagnostics.Debug.Fail("AcquisitationPeriodEnd has failed");
                }
                else {
                    System.DateTime blockDate;
                    if (System.DateTime.TryParse(BlockingDate.Value.ToString(), out blockDate)) {
                        if (blockDate < System.DateTime.Now || blockDate > yearEndDate.AddYears(1) || blockDate.DayOfWeek == DayOfWeek.Sunday) {
                            // must be in the future but is not allowed to be later than 1 year after end of the financial year the user wants to send the report for
                            // must be a working day
                            AddError(eBalanceKitResources.Localisation.ResourcesFederalGazetteParameters.Error_BlockingDate);
                        }
                    }
                }
            }

        }



        public IFederalGazetteElementInfo OrderTypes { get; set; }

        public IFederalGazetteElementInfo PublicationArea { get; set; }

        public IFederalGazetteElementInfo PublicationType { get; set; }

        public IFederalGazetteElementInfo PublicationSubType { get; set; }

        public IFederalGazetteElementInfo PublicationLanguage { get; set; }

        public IFederalGazetteElementInfo CompanySize { get; set; }

        public IFederalGazetteElementInfo PublicationCategory { get; set; }

        public IFederalGazetteElementInfo AnnualContentType { get; set; }

        public IFederalGazetteElementInfo CompanyType { get; set; }

        public IFederalGazetteElementInfo SubmitterType { get; set; }

        public IFederalGazetteElementInfo BalanceSheetStandard { get; set; }

        public IFederalGazetteElementInfo HasNaturalPerson { get; set; }

        public IFederalGazetteElementInfo AcquisitationPeriodStart { get; set; }
        public IFederalGazetteElementInfo AcquisitationPeriodEnd { get; set; }
        public IFederalGazetteElementInfo BlockingDate { get; set; }

        public override ParameterArea ParameterType { get { return ParameterArea.Order; } }
    }
}