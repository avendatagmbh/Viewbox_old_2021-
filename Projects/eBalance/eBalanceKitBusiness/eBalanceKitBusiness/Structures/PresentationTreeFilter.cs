using System.Collections.Generic;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.Structures {
    public class PresentationTreeFilter : Utils.NotifyPropertyChangedBase {

        internal PresentationTreeFilter(IEnumerable<ITaxonomyTreeNode> rootEntries) {
            _rootEntries = rootEntries;
            //ShowOnlyPositionsForSelectedLegalStatus = true;
        }

        private readonly IEnumerable<ITaxonomyTreeNode> _rootEntries;

        #region Filter
        private string _filter;

        public string Filter {
            set {
                if (_filter == value)
                    return;

                _filter = value;
                //Do not expand nodes when the filter is cleared
                bool updateIsExpanded = !string.IsNullOrEmpty(_filter);
                foreach (var pnode in _rootEntries)
                    pnode.UpdateVisibility(this, updateIsExpanded);
            }

            get {
                return _filter;
            }
        }
        #endregion

        #region ShowOnlyMandatoryPostions
        private bool _showOnlyMandatoryPosition;

        public bool ShowOnlyMandatoryPostions { 
            
            set {
                if (_showOnlyMandatoryPosition == value)
                    return;
                _showOnlyMandatoryPosition = value;

                foreach (var pnode in _rootEntries)
                    pnode.UpdateVisibility(this);

                OnPropertyChanged("ShowOnlyMandatoryPostions");
            }

            get { return _showOnlyMandatoryPosition;
            }
        }
        #endregion ShowOnlyMandatoryPostions

        #region HideEmptyPositions
        private bool _hideEmptyPositions;

        public bool HideEmptyPositions {
            set {
                if (_hideEmptyPositions == value)
                    return;
                _hideEmptyPositions = value;
                foreach (var pnode in _rootEntries)
                    pnode.UpdateVisibility(this);
            }
            get { return _hideEmptyPositions; }
        }
        #endregion HideEmptyPositions

        #region HideNonManualFields
        private bool _hideNonManualFields;
        
        public bool HideNonManualFields {
            set {
                if (_hideNonManualFields == value)
                    return;
                _hideNonManualFields = value;
                foreach (var pnode in _rootEntries)
                    pnode.UpdateVisibility(this);
            }
            get { return _hideNonManualFields; }
        }
        #endregion HideNonManualFields

        #region ShowOnlyPositionsForSelectedLegalStatus
        //private bool _showOnlyPositionsForSelectedLegalStatus;

        public bool ShowOnlyPositionsForSelectedLegalStatus {

            //set {
            //    if (_showOnlyPositionsForSelectedLegalStatus == value)
            //        return;
            //    _showOnlyPositionsForSelectedLegalStatus = value;

            //    foreach (var pnode in _rootEntries)
            //        pnode.UpdateVisibility(this);
            //}

            get {
                //return _showOnlyPositionsForSelectedLegalStatus;
                return Options.GlobalUserOptions.UserOptions.ShowSelectedLegalForm;
            }
        }
        #endregion

        
        public bool GetVisiblility(Document document, IElement element, IValueTreeEntry value) {
            bool isVisible = GetVisibilityForGlobalOptions(document, element, false);

            if (ShowOnlyMandatoryPostions && !element.IsMandatoryField) {
                // hide non mandatory positions
                isVisible = false;
            } 
            else if (ShowOnlyMandatoryPostions && element.IsMandatoryField) {
                // hide non mandatory positions
                isVisible = true;
            } 
            
            if (HideEmptyPositions && value != null && !value.HasValue) {
                // hide empty positions
                isVisible = false;
            } 
            else if (HideEmptyPositions && value != null && value.HasValue) {
                // hide empty positions
                isVisible = true;
            } 
            

            if (HideNonManualFields &&
                !(value != null && element.ValueType == XbrlElementValueTypes.Monetary && value.HasManualValue)) {
                // show only manual fields
                isVisible = false;
            }


            return isVisible;
        }

        /// <summary>
        /// Checks the visibility of the specified element.
        /// </summary>
        /// <param name="document">The document / current report.</param>
        /// <param name="element">The element of that the visibility should be checked.</param>
        /// <param name="directCall">Identifies if the call was directly for this function. Requires because global ShowOnlyMandatoryPostions can be different in current presentation tree and in global option.</param>
        /// <returns></returns>
        public bool GetVisibilityForGlobalOptions(Document document, IElement element, bool directCall = true) {
            bool isVisible = true;
            if (document != null) {

                if (Options.GlobalUserOptions.UserOptions.ShowOnlyMandatoryPostions && !element.IsMandatoryField && directCall) {
                    // hide non mandatory positions
                    isVisible = false;
                }

                if (Options.GlobalUserOptions.UserOptions.ShowTypeOperatingResult) {
                    // Show only fields for the selected TypeOperatingResult
                    isVisible &= (StaticCompanyMethods.IsTypeOperatingResultGKV(document) &&
                                  element.TypeOperatingResultGKV) ||
                                 (StaticCompanyMethods.IsTypeOperatingResultUKV(document) &&
                                  element.TypeOperatingResultUKV) ||
                                 (StaticCompanyMethods.IsTypeOperatingResultNotSelected(document) ||
                                  element.TypeOperatingResultAll);

                }

                // "Ausblenden aller nicht zur gewählten Branche gehörenden Positionen" 
                if (Options.GlobalUserOptions.UserOptions.ShowSelectedTaxonomy) {
                    switch (document.MainTaxonomyKind) {
                        case TaxonomySubType.EBV:
                            isVisible &= element.TaxonomieEBV;
                            break;
                        case TaxonomySubType.VUV:
                            isVisible &= element.TaxonomieVUV;
                            break;
                        case TaxonomySubType.KHBV:
                            isVisible &= element.TaxonomieKHBV;
                            break;
                        case TaxonomySubType.LUF:
                            isVisible &= element.TaxonomieLUF;
                            break;
                        case TaxonomySubType.PBV:
                            isVisible &= element.TaxonomiePBV;
                            break;
                        case TaxonomySubType.WUV:
                            isVisible &= element.TaxonomieWUV;
                            break;
                        default:
                            isVisible &= element.TaxonomieNEIN;
                            break;
                    }
                }


                if (ShowOnlyPositionsForSelectedLegalStatus) {
                    // hide positions, which do not fit to the selected legal status
                    
                    // Dirty fix because the taxonomy for Insurance tells the Income statement is NOT visible for PG, EU and KSt. It is only visible for SEAG, VVaG, OerV, BNaU
                    if (document.MainTaxonomyInfo.Type == TaxonomyType.Insurance) {
                        isVisible = true;
                    }
                    else if (StaticCompanyMethods.IsPersonenGesellschaft(document) && !element.LegalFormPG) {
                        isVisible = false;
                    }
                    else if (StaticCompanyMethods.IsEinzelunternehmer(document) && !element.LegalFormEU) {
                        isVisible = false;
                    }
                    else if (StaticCompanyMethods.IsKoerperschaft(document) && !element.LegalFormKSt) {
                        isVisible = false;
                    }
                    /*
                    var value1 =
                        document.ValueTreeGcd.GetValue("de-gcd_genInfo.company.id.legalStatus") as XbrlElementValue_SingleChoice;
                
                    if (value1 != null && value1.SelectedValue != null) {
                        switch (value1.SelectedValue.Id) {
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.GMBH":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.AG":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.SE":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.KOG":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.OHG":
                                if (!element.LegalFormPG) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.KG":
                                if (!element.LegalFormPG) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.KGAA":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.GKG":
                                if (!element.LegalFormPG) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.AGKG":
                                if (!element.LegalFormPG) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.EG":
                                if (!element.LegalFormEU) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.VVAG":
                                if (!element.LegalFormVVaG) isVisible = false;
                                break;

                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.BNVA":
                                //if (E!lement.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.EWI":
                                if (!element.LegalFormPG) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.EU":
                                if (!element.LegalFormPG) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.EUN":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.GBR":
                                if (!element.LegalFormPG) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.EV":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.PA":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.PG":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.ST":
                                if (!element.LegalFormPG) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.HAJP":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.SJPR":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.BG":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.SG":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.VOER":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.KOER":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.LTD":
                                if (!element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.RHRA":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.RHRB":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.RAG":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.RAP":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.FB43":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.G43":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.ASG":
                                if (!element.LegalFormPG) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.Delta.EUN":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.Delta.KOER":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.Delta.MUN":
                                //if (!Element.LegalFormKSt) isVisible = false;
                                break;
                            case "de-gcd_genInfo.company.id.legalStatus.legalStatus.SR":
                                break;
                        }
                    
                    }*/
                }

            }
            return isVisible;
        }
    }
}
