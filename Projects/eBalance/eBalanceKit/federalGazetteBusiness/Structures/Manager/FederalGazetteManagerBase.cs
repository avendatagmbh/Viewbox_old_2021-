// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Taxonomy;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using federalGazetteBusiness.External;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Manager {
    public abstract class FederalGazetteManagerBase {
        //public Parameters Parameters { get { return Parameters.Instance; } }
        //private Parameters.FederalGazetteParameterBase SendParameter { get; set; }
        protected Document _document;

        private FederalGazetteManagerBase() {
            LoginData = new Connection();
            ElementsSmall = new List<IElement>();
            ElementsMidsize = new List<IElement>();
            ElementsBig = new List<IElement>();
            TaxonomyElementsByCompanySize = new Dictionary<CompanySize, List<IElement>> {
                {CompanySize.Small, ElementsSmall},
                {CompanySize.Midsize, ElementsMidsize},
                {CompanySize.Big, ElementsBig}
            };
        }

        protected FederalGazetteManagerBase(Document document) : this() {
            //string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            //foreach (string name in names)
            //    System.Console.WriteLine(name);

            _document = document;

            LoadTaxonomyLevelStuff();
        }


        /// <summary>
        /// Generates a unique ticket ID based on the DateTime.Now.Ticks, DocumentManager.Instance.CurrentDocument.Id and DocumentManager.Instance.CurrentDocument.CompanyId
        /// </summary>
        /// <returns></returns>
        protected string GenerateTicketId() {
            return "BAnz-Ticket_ebk_" + DateTime.Now.Ticks + "_" + DocumentManager.Instance.CurrentDocument.Id + "_" + DocumentManager.Instance.CurrentDocument.CompanyId;
        }


        protected BAnzServicePortTypeClient GetBAnzClient() {
            var c = new BAnzServicePortTypeClient();
            c.Endpoint.Behaviors.Add(new InspectorBehavior());
            return c;
        }

        protected LoginData GetLogin() { return GetLogin(LoginData.Username, LoginData.Password); }

        protected LoginData GetLogin(string username, string password) {

            var login = new LoginData { username = username, password = password };

            return login;
        }
        
        public Connection LoginData { get; set; }
        
        public List<IElement> ElementsSmall { get; set; }
        public List<IElement> ElementsMidsize { get; set; }
        public List<IElement> ElementsBig { get; set; }

        public Dictionary<CompanySize, List<Taxonomy.IElement>> TaxonomyElementsByCompanySize;

        public static SoapCommunicationData LastCommunicationData;

        #region Helper

        /// <summary>
        /// Loads the information about which TaxonomyIds are relevant for which company size from the taxonomyPositionsFederalGazette.txt (included as resource).
        /// </summary>
        private void LoadTaxonomyLevelStuff() {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("federalGazetteBusiness.Resources.taxonomyPositionsFederalGazette.txt");
            Debug.Assert(stream != null);
            if (stream == null) {
                return;
            }

            EnhancedCsvReader reader = new EnhancedCsvReader("xx");
            reader.CreateReader = (filename, encoding) => new StreamReader(stream);

            reader.Separator = ";";

            var data = reader.GetCsvData(0, null);
            IElement element;
            foreach (DataRow row in data.Rows) {

                try {
                    element = GetElement(new List<string> { row[0].ToString(), row[1].ToString() });
                }
                catch (Exception e) {
                    ExceptionLogging.LogException(e);
                    continue;
                }

                // Debug.Assert(element != null);

                if (element == null) {
                    continue;
                }

                bool setter;
                // Is it true for small companies?
                if (Boolean.TryParse(row[2].ToString(), out setter)) {
                    if (setter) {
                        ElementsSmall.Add(element);
                    }
                }

                // Is it true for midsized companies?
                if (Boolean.TryParse(row[3].ToString(), out setter)) {
                    if (setter) {
                        ElementsMidsize.Add(element);
                    }
                }

                // Is it true for big companies?
                if (Boolean.TryParse(row[4].ToString(), out setter)) {
                    if (setter) {
                        ElementsBig.Add(element);
                    }
                }
            }
            //try {
            //    while (!reader.EndOfStream) {
            //        var line = reader.ReadLine();
            //        if (string.IsNullOrEmpty(line)) continue;
            //        var parts = line.Split(';');
            //        if (parts.Length != 2) continue;
            //        _idNumberDict.Add(parts[0], parts[1]);
            //        _inverseIdNumberDict.Add(parts[1], parts[0]);

            //        string partialNumber = parts[1].Substring(0, 3);
            //        if (_idNumberCount.ContainsKey(partialNumber)) _idNumberCount[partialNumber]++;
            //        else _idNumberCount[partialNumber] = 0;
            //    }
            //}
            //finally
            //{
            //    reader.Close();
            //}
        }

        private IElement GetElement(IEnumerable<string> ids) {
            IElement result = null;

            var valueTrees = new List<ValueTree> { _document.ValueTreeMainFg, _document.ValueTreeMain, _document.ValueTreeGcd };

            foreach (var id in ids) {

                string elementId = GetElementId(id);

                // Could not be converted to a valid ElementId so we continue
                if (String.IsNullOrEmpty(elementId))
                    continue;

                foreach (var valueTree in valueTrees) {
                    if (valueTree.Root.Elements.TryGetValue(elementId, out result)) {
                        return result;
                    }
                }

                //_document.ValueTreeMainFg.GetValue(elementId)
                IValueTreeEntry valueTreeEntry = GetTaxonomyValue(elementId);
                if (valueTreeEntry != null) {
                    result = valueTreeEntry.Element;
                }

                if (result != null) {
                    break;
                }
            }

            return result;
        }

        private string GetElementId(string maybeId) {
            string elementId = null;
            //var taxonomies = new List<ITaxonomy> {_document.MainTaxonomy, _document.GcdTaxonomy};
            var taxonomies = new List<ITaxonomy> { _document.FederalGazetteTaxonomy, _document.GcdTaxonomy };

            foreach (var taxonomy in taxonomies) {
                if (String.IsNullOrEmpty(elementId)) {
                    elementId = TaxonomyIdManager.GetTaxonomyId(maybeId, taxonomy);
                }
                else {
                    break;
                }
            }
            return elementId;
        }

        #region Helper for getting Values

        private IValueTreeEntry GetTaxonomyValue(string taxonomyId) {

            IValueTreeEntry valueTreeEntry = null;
            var valueTrees = new List<ValueTree> { _document.ValueTreeMainFg, _document.ValueTreeMain, _document.ValueTreeGcd };
            foreach (var valueTree in valueTrees) {

                if (!_document.ValueTreeMainFg.Root.Values.TryGetValue(taxonomyId, out valueTreeEntry)) {
                    valueTreeEntry = valueTree.GetValue(taxonomyId);
                    if (valueTreeEntry == null)
                        valueTreeEntry =
                            valueTree.Root.Values.Values.FirstOrDefault(
                                val => val.Element.Name.Equals(taxonomyId));
                }

                if (valueTreeEntry != null) {
                    break;
                }
            }

            return valueTreeEntry;
        }

        //protected bool LoadValue(string elementName) {
        //    return LoadValue(Parameters.GetType().GetProperty(elementName).GetValue(Parameters, null) as IFederalGazetteElementInfo);
        //}

        protected bool LoadValue(IFederalGazetteElementInfo elementInfo) {
            if (elementInfo == null) {
                return false;
            }
            if (elementInfo is FederalGazetteElementSelectionBase) {
                return LoadValue(elementInfo as FederalGazetteElementSelectionBase);
            }
            var value = GetValue(elementInfo);
            if (value != null) {
                elementInfo.Value = value;
                return true;
            }
            return false;
        }

        protected bool LoadValue(FederalGazetteElementSelectionBase options) {
            if (String.IsNullOrEmpty(options.TaxonomyElement)) {
                return false;
            }
            var value = GetTaxonomyValue(options.TaxonomyElement);
            var val = value as XbrlElementValue_SingleChoice;

            if (val == null || val.SelectedValue == null) {
                return false;
            }

            var result =
                options.Options.FirstOrDefault(option => option.TaxonomyElements != null && option.TaxonomyElements.Contains(val.SelectedValue.Name));
            if (result != null) {
                options.SelectedOption = result;
                return true;
            }
            return false;
        }

        protected object GetValue(IFederalGazetteElementInfo elementInfo,
                                ValueTree valueTree = null) {
            object result = null;

            foreach (var taxonomyElement in elementInfo.TaxonomyElements) {
                var taxonomyId = GetElementId(taxonomyElement);
                var value = valueTree != null
                                ? valueTree.GetValue(taxonomyId)
                                : (_document.ValueTreeMainFg.GetValue(taxonomyId) ??
                                   _document.ValueTreeGcd.GetValue(taxonomyId) ??
                                   _document.ValueTreeMain.GetValue(taxonomyId));

                if (value != null) {
                    result = value.Value;
                }
            }
            return result;
        }

        protected T GetValue<T>(IFederalGazetteElementInfo elementInfo,
                              ValueTree valueTree = null) {
            var value = GetValue(elementInfo, valueTree);
            Debug.Assert(value is T);
            var result = (T)value;
            return result;
        }

        #endregion

        #endregion

        #region HelperClasses


        public class Connection {
            public string Username { get; set; }
            public string Password { get; set; }
        }


        public class InspectorBehavior : IEndpointBehavior {
            public void Validate(ServiceEndpoint endpoint) { }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { clientRuntime.MessageInspectors.Add(new MyMessageInspector()); }
        }

        public class MyMessageInspector : IClientMessageInspector {
            private SoapCommunicationData _communicationData;

            public object BeforeSendRequest(ref Message request, IClientChannel channel) {
                _communicationData = new SoapCommunicationData();
                _communicationData.Request = request;
                LastCommunicationData = _communicationData;
#if WriteCommunicationIntoFiles
                File.WriteAllText(System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "request.soap"), request.ToString());
                
                // if you want to open the folder where the request was written down to.
                //ert: commented
                //System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
#endif
                return null;
            }

            public void AfterReceiveReply(ref Message reply, object correlationState) {
                if (_communicationData == null) {
                    Debug.Fail("_communicationData is null inside the request :(");
                    _communicationData = new SoapCommunicationData();
                }
                _communicationData.Response = reply;
                LastCommunicationData = _communicationData;
            }
        }

        public class SoapCommunicationData {

            #region Request
            private Message _request;

            public Message Request {
                get { return _request; }
                set {
                    if (_request != value) {
                        _request = value;
                        RequestTime = DateTime.Now;
                    }
                }
            }
            #endregion // Request

            #region Response
            private Message _response;

            public Message Response {
                get { return _response; }
                set {
                    if (_response != value) {
                        _response = value;
                        ResponseTime = DateTime.Now;
                    }
                }
            }
            #endregion // Response

            public DateTime RequestTime { get; private set; }
            public DateTime ResponseTime { get; private set; }

        }
        #endregion
    }
}