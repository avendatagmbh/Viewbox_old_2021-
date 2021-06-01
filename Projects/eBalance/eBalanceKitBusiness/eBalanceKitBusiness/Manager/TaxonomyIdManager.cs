// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-19
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using TaxonomyInfo = eBalanceKitBusiness.Structures.DbMapping.TaxonomyInfo;

namespace eBalanceKitBusiness.Manager {
    public class TaxonomyIdManager {

        static TaxonomyIdManager() {
            ReadNumberDict();            
        }

        internal TaxonomyIdManager(ITaxonomyInfo gcdTaxonomyInfo, ITaxonomyInfo mainTaxonomyInfo) {

            lock (_idNumberDict) {

                if (gcdTaxonomyInfo == null && mainTaxonomyInfo == null) throw new Exception("At least one of the taxonomy info objects must be not null.");

                if (gcdTaxonomyInfo != null) {
                    GcdTaxonomyInfo = gcdTaxonomyInfo;
                    GcdTaxonomy = TaxonomyManager.GetTaxonomy(gcdTaxonomyInfo);
                }

                if (mainTaxonomyInfo != null) {
                    MainTaxonomyInfo = mainTaxonomyInfo;
                    MainTaxonomy = TaxonomyManager.GetTaxonomy(mainTaxonomyInfo);
                }

                using (var conn = AppConfig.ConnectionManager.GetConnection()) {

                    // get existing taxonomy elements
                    foreach (var taxonomyIdAssignment in conn.DbMapping.Load<TaxonomyIdAssignment>()) 
                        _taxonomyIdAssignmentDict[taxonomyIdAssignment.XbrlElementId] = taxonomyIdAssignment;

                    // assign missing taxonomy elements and init dictionaries
                    try {
                        conn.BeginTransaction();

                        if (MainTaxonomy != null) foreach (var elem in MainTaxonomy.Elements.Values) InitDictionaries(mainTaxonomyInfo, conn, elem);

                        if (GcdTaxonomy != null) foreach (var elem in GcdTaxonomy.Elements.Values) InitDictionaries(gcdTaxonomyInfo, conn, elem);

                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                }
            }
        }

        private void InitDictionaries(ITaxonomyInfo taxonomyInfo, IDatabase conn, IElement elem) {
            TaxonomyIdAssignment assignment;
            _taxonomyIdAssignmentDict.TryGetValue(elem.Id, out assignment);
            if (assignment == null) {
                assignment = new TaxonomyIdAssignment(((TaxonomyInfo)taxonomyInfo).Id, elem.Id, GetNumber(taxonomyInfo, elem.Id));
                _taxonomyIdAssignmentDict[elem.Id] = assignment;
                conn.DbMapping.Save(assignment);
            }
            elem.PositionNumber = assignment.Number;

            _elementById[assignment.Id] = elem;
            _elementByTaxonomyId[elem.Id] = elem;
            _idByElement[elem] = assignment.Id;
        }

        private ITaxonomyInfo GcdTaxonomyInfo { get; set; }
        private ITaxonomy GcdTaxonomy { get; set; }
        
        private ITaxonomyInfo MainTaxonomyInfo { get; set; }
        private ITaxonomy MainTaxonomy { get; set; }

        private readonly Dictionary<string, TaxonomyIdAssignment> _taxonomyIdAssignmentDict =
            new Dictionary<string, TaxonomyIdAssignment>();

        private readonly Dictionary<int, IElement> _elementById = new Dictionary<int, IElement>();
        private readonly Dictionary<string, IElement> _elementByTaxonomyId = new Dictionary<string, IElement>();
        private readonly Dictionary<IElement, int> _idByElement = new Dictionary<IElement, int>();

        #region ReadNumberDict

        private static void ReadNumberDict() {
            if (!File.Exists("Taxonomy\\TaxonomyPosNumbers.txt")) return;
            var reader = new StreamReader("Taxonomy\\TaxonomyPosNumbers.txt");
            lock (_idNumberDict) {
                try {
                    while (!reader.EndOfStream) {
                        var line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        var parts = line.Split(';');
                        if (parts.Length != 2) continue;
                        _idNumberDict.Add(parts[0], parts[1]);
                        _inverseIdNumberDict.Add(parts[1], parts[0]);

                        string partialNumber = parts[1].Substring(0, 3);
                        if (_idNumberCount.ContainsKey(partialNumber)) _idNumberCount[partialNumber]++;
                        else _idNumberCount[partialNumber] = 0;
                    }
                } finally {
                    reader.Close();
                }
            }
        }
        #endregion

        #region IdNumberDict
        private static readonly Dictionary<string, string> _idNumberDict = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _inverseIdNumberDict = new Dictionary<string, string>();
        #endregion IdNumberDict

        //private string GetNumber(string xbrlElementId) {
        //    if (IdNumberDict.ContainsKey(xbrlElementId)) return IdNumberDict[xbrlElementId];
        //    else return string.Empty;
        //}

        // temp. code for first generation of id number values
        private static readonly Dictionary<string, int> _idNumberCount = new Dictionary<string, int>();

        private string GetNumber(ITaxonomyInfo taxonomyInfo, string xbrlElementId) {

            lock (_idNumberDict) {
                if (_idNumberDict.ContainsKey(xbrlElementId)) return _idNumberDict[xbrlElementId];

                string taxonomyId;
                string partId;
                switch (taxonomyInfo.Type) {
                    case TaxonomyType.Unknown:
                        taxonomyId = "00";
                        partId = "0";
                        break;

                    case TaxonomyType.GCD:
                        taxonomyId = "01";
                        if (xbrlElementId.StartsWith("de-gcd_genInfo.doc")) {
                            partId = "1";
                        } else if (xbrlElementId.StartsWith("de-gcd_genInfo.report")) {
                            partId = "2";
                        } else if (xbrlElementId.StartsWith("de-gcd_genInfo.company")) {
                            partId = "3";
                        } else {
                            partId = "0";
                        }
                        break;

                    case TaxonomyType.GAAP:
                        taxonomyId = "02";
                        if (xbrlElementId.StartsWith("de-gaap-ci_bs.ass")) {
                            // Aktiva
                            partId = "1";
                        } else if (xbrlElementId.StartsWith("de-gaap-ci_bs.eqLiab")) {
                            // Passiva
                            partId = "2";
                        } else if (xbrlElementId.StartsWith("de-gaap-ci_is")) {
                            // GuV
                            partId = "3";
                        } else {
                            partId = "0";
                        }
                        break;

                    case TaxonomyType.OtherBusinessClass:
                        taxonomyId = "03";
                        partId = "0";
                        break;

                    case TaxonomyType.Financial:
                        taxonomyId = "04";
                        partId = "0";
                        break;

                    case TaxonomyType.Insurance:
                        taxonomyId = "05";
                        partId = "0";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string partialNumber = taxonomyId + partId;
                if (_idNumberCount.ContainsKey(partialNumber)) _idNumberCount[partialNumber]++;
                else _idNumberCount[partialNumber] = 0;

                int number = 0;
                var result = taxonomyId + partId + number.ToString().PadLeft(4, '0');
                while (_inverseIdNumberDict.ContainsKey(result)) {
                    number++;
                    result = taxonomyId + partId + number.ToString().PadLeft(4, '0');
                }
                //var result = taxonomyId + partId + (_idNumberCount[partialNumber]).ToString().PadLeft(4, '0');
                _idNumberDict[xbrlElementId] = result;
                _inverseIdNumberDict[result] = xbrlElementId;
                System.Diagnostics.Debug.WriteLine(xbrlElementId + ";" + result);

                return result;
            }
        }

        internal IEnumerable<TaxonomyIdAssignment> GetTaxonomyIdAssignments() {
            return _taxonomyIdAssignmentDict.Values;
        }

        /// <summary>
        /// Gets the id for the specified element id.
        /// </summary>
        public int GetId(string xbrlElementId) {
            var elem = GetElement(xbrlElementId);
            TaxonomyIdAssignment taxonomyIdAssignment;
            _taxonomyIdAssignmentDict.TryGetValue(elem.Id, out taxonomyIdAssignment);
            if (taxonomyIdAssignment != null) return taxonomyIdAssignment.Id;
            throw new Exception("TaxonomyIdManager: The taxonomy id '" + xbrlElementId + "' could not be found.");
        }

        public int GetId(IElement element) {
            TaxonomyIdAssignment taxonomyIdAssignment;
            _taxonomyIdAssignmentDict.TryGetValue(element.Id, out taxonomyIdAssignment);
            if (taxonomyIdAssignment != null) return taxonomyIdAssignment.Id;
            throw new Exception("TaxonomyIdManager: The taxonomy id '" + element.Id + "' could not be found.");
        }

        public bool HasId(string xbrlElementId) { return _taxonomyIdAssignmentDict.ContainsKey(xbrlElementId); }

        /// <summary>
        /// Returns the taxonomy element id for the specified element, which could be an element name, an element id or an element position number.
        /// </summary>
        /// <returns></returns>
        public string TryToConvertToElementId(string element) {
            return _inverseIdNumberDict.ContainsKey(element) ? _inverseIdNumberDict[element] : GetElement(element).Id;
        }

        /// <summary>
        /// Gets the element with the specified taxonomy element id.
        /// </summary>
        /// <returns></returns>
        internal IElement GetElement(string xbrlElementId) {
            IElement elem;
            if (MainTaxonomy == null) {
                if (!GcdTaxonomy.Elements.TryGetValue(xbrlElementId, out elem)) {
                    throw new Exception("TaxonomyIdManager: The taxonomy id '" + xbrlElementId +
                                        "' could not be found.");
                }

            } else if (GcdTaxonomy == null) {
                if (!MainTaxonomy.Elements.TryGetValue(xbrlElementId, out elem)) {
                    throw new Exception("TaxonomyIdManager: The taxonomy id '" + xbrlElementId +
                                        "' could not be found.");
                }

            } else {
                if (!MainTaxonomy.Elements.TryGetValue(xbrlElementId, out elem) &&
                    !GcdTaxonomy.Elements.TryGetValue(xbrlElementId, out elem)) {
                    throw new Exception("TaxonomyIdManager: The taxonomy id '" + xbrlElementId +
                                        "' could not be found.");
                }

            }
            return elem;
        }

        /// <summary>
        /// Gets the element with the specified internal id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IElement GetElement(int id) { return _elementById.ContainsKey(id) ? _elementById[id] : null; }

        /// <summary>
        /// Sets the assigned element object for each item in the items collection, 
        /// if the element for the respective ElementId property could be found.
        /// </summary>
        internal void AssignElements(IEnumerable<ITaxonomyAssignment> items) {
            foreach (var item in items) {
                var elem = GetElement(item.AssignedElementId);
                if (elem != null) item.AssignedElement = elem;
            }
        }

        /// <summary>
        /// Assings the specified element to the specified item.
        /// </summary>
        internal void SetElementAssignment(ITaxonomyAssignment item, IElement element) {
            item.AssignedElement = element;
            item.AssignedElementId = GetId(element.Id);
        }

        /// <summary>
        /// Assings element with the specified id to the specified item.
        /// </summary>
        internal void SetElementAssignment(ITaxonomyAssignment item, string taxonomyId) {
            var element = GetElement(taxonomyId);
            if (element == null) 
                throw new Exception("TaxonomyIdManager: The taxonomy id '" + taxonomyId + "' could not be found.");
            SetElementAssignment(item, element);
        }

        /// <summary>
        /// Assings element with the specified id to the specified item.
        /// </summary>
        internal static void RemoveElementAssignment(ITaxonomyAssignment item) {
            item.AssignedElement = null;
            item.AssignedElementId = 0;
        }
    }
}
 