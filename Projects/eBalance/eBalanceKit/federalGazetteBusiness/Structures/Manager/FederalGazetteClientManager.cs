// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Structures.DbMapping;
using federalGazetteBusiness.Structures.Parameters;

namespace federalGazetteBusiness.Structures.Manager {
    public class FederalGazetteClientManager : FederalGazetteManagerBase {

        public FederalGazetteClientParameter ClientParameter { get; set; }


        public FederalGazetteClientManager(Document document) : base(document) {
            ClientParameter = new FederalGazetteClientParameter();
            Company = document.Company;
        }

        #region Company
        private Company _company;

        public Company Company {
            get { return _company; }
            set {
                if (_company != value) {
                    _company = value;
                }
            }
        }
        #endregion // Company

        public void CreateClient() {

            var login = GetLogin();
            var communicator = GetBAnzClient();
            External.EbanzClient client = new External.EbanzClient();
            client.company_type = ClientParameter.CompanyType.Value.ToString();
            //client.sign;
            client.company_name = ClientParameter.CompanyName.Value.ToString();
            client.legal_form = ClientParameter.CompanyLegalForm.Value.ToString();
            client.company_domicile = ClientParameter.CompanyDomicile.Value.ToString();
            int companyId;
            client.company_id = (ClientParameter.CompanyId.Value != null && int.TryParse(ClientParameter.CompanyId.Value.ToString(), out  companyId)) ? companyId : (int?)null ;
            client.companyaddress = new External.CompanyAddress();
            client.companyaddress.company_name = ClientParameter.AdressParameter.Name.Value.ToString();
            client.companyaddress.city = ClientParameter.AdressParameter.City.Value.ToString();
            client.companyaddress.country = ClientParameter.AdressParameter.Country.Value.ToString();
            client.companyaddress.devision = ClientParameter.AdressParameter.Devision.Value.ToString();
            client.companyaddress.postcode = ClientParameter.AdressParameter.PostCode.Value.ToString();
            client.companyaddress.state = ClientParameter.AdressParameter.State.Value.ToString();
            client.companyaddress.street = ClientParameter.AdressParameter.Street.Value.ToString();
            if (ClientParameter.ContactPerson != null) {
                client.contactperson.salutation = ClientParameter.ContactPerson.Salutation.Value != null ? (External.salutation_type)System.Enum.Parse(typeof(External.salutation_type),
                    ClientParameter.ContactPerson.Salutation.Value.ToString()) : (External.salutation_type?)null;
                client.contactperson.salutationSpecified = client.contactperson.salutation.HasValue;
                client.contactperson.email = ClientParameter.ContactPerson.EMail.Value.ToString();
                client.contactperson.firstname = ClientParameter.ContactPerson.FirstName.Value.ToString();
                client.contactperson.lastname = ClientParameter.ContactPerson.LastName.Value.ToString();
                client.contactperson.title = ClientParameter.ContactPerson.Title.Value.ToString();
                if (ClientParameter.ContactPerson.Phone != null) {
                    client.contactperson.telephone = new External.Telephone();
                    client.contactperson.telephone.area_code =
                        ClientParameter.ContactPerson.Phone.AreaCode.Value.ToString();
                    client.contactperson.telephone.country_area_code =
                        ClientParameter.ContactPerson.Phone.CountryAreaCode.Value.ToString();
                    client.contactperson.telephone.number = ClientParameter.ContactPerson.Phone.Number.Value.ToString();
                }
            }
            var ticket = GenerateTicketId();
            var response = communicator.createClient(login, client, ticket);
            ErrorAnalyse.AnalyseError(response);

            Company.FederalGazetteClientId = response.clientnumber;
            //return response.clientnumber;
        }

        /// <summary>
        /// Returns a dictionary with CompanyId as Key and CompanyName as Value.
        /// </summary>
        /// <param name="companyName">If the company name is given it tries to find the companyId for only this company.</param>
        /// <param name="returnAllOnNoMatch">Defines if to return all entries if no exact match could be found in the BAnz.</param>
        /// <returns><list type="bullet">
        /// <item>default: List of all Companies / Clients ("Einsenderkunden").</item>
        /// <item><see cref="companyName"/> set, <see cref="returnAllOnNoMatch"/>=false: List of all Companies that are in the BAnz and have the given <see cref="companyName"/>.</item>
        /// <item><see cref="companyName"/> set, <see cref="returnAllOnNoMatch"/>=true: List of all Companies, the leading entries are containing the <see cref="companyName"/>.</item>
        /// </list> 
        /// Throws an exception if something wents wrong (<see cref="ErrorAnalyse.AnalyseError"/>)
        /// </returns>
        public Dictionary<string, string> GetSenderClients(string companyName = null, bool returnAllOnNoMatch = false) {
            var result = new Dictionary<string, string>();
            var company = companyName ?? string.Empty;
            var login = GetLogin();
            var com = GetBAnzClient();
            var queryResult = com.listClients(login, string.Empty, company, string.Empty);

            ErrorAnalyse.AnalyseError(queryResult);
            if (queryResult.result != 0)
                return result;

            if (queryResult.clientList != null) {

                foreach (var client in queryResult.clientList) {
                    result.Add(client.clientnumber, client.company_name);
                }
            }
            else {
                if (string.IsNullOrEmpty(company)) {
                    throw new NoClientsDefinedException();
                }
                queryResult = com.listClients(GetLogin(), string.Empty, string.Empty, string.Empty);
                result.Clear();
                var preferedResult = new Dictionary<string, string>();
                var tmpResult = new Dictionary<string, string>();
                foreach (var client in queryResult.clientList) {
                    if (companyName != null && client.company_name.Contains(companyName)) {
                        preferedResult.Add(client.clientnumber, client.company_name);
                        continue;
                    }
                    tmpResult.Add(client.clientnumber, client.company_name);
                }
                if (preferedResult.Any()) {
                    result = preferedResult;
                    if (returnAllOnNoMatch) {
                        foreach (var tmpEntry in tmpResult) {
                            result.Add(tmpEntry.Key, tmpEntry.Value);
                        }
                    }
                }
                else {
                    result = tmpResult;
                }
            }

            return result;
        }
        
    }
}