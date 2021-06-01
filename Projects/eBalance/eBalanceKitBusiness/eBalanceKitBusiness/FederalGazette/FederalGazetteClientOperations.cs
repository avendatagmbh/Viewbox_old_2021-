// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2011-12-19
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Utils;
using eBalanceKitBusiness.FederalGazette.Model;
using eFederalGazette;

namespace eBalanceKitBusiness.FederalGazette {
    public class FederalGazetteClientOperations {
        public FederalGazetteClientOperations(FederalGazetteMainModel model) {
            Model = model;
            _login = new LoginData {password = Model.Password, username = Model.Username};
            _federalGazetteErrorAnalysis = new FederalGazetteErrorAnalysis();
        }

        #region Locals
        private FederalGazetteMainModel Model { get; set; }
        private LoginData _login;
        private FederalGazetteErrorAnalysis _federalGazetteErrorAnalysis;
        #endregion

        #region CreateClient
        public bool CreateClient() {
            var federalGazetteService = new BAnzServicePortTypeClient();
            var company = CreateCompany();
            try {
                var response = federalGazetteService.createClient(_login, company, Model.TicketId);
                _federalGazetteErrorAnalysis.AnalyseError(response);
                if (response.result == 0)             
                    return true;
                
            } catch (Exception ex) {

                throw new Exception(ex.Message);
            }
            return false;
        }
        #endregion

        #region CreateCompany
        public EbanzClient CreateCompany() {
            var client = new EbanzClient();
            client.company_type = Model.SelectedCompanyRegisterationType.Id;
            client.sign = Model.CompanySign;

            if (!Model.SelectedCompanyRegisterationType.Id.Equals("german_registered")) {
                client.company_name = Model.CompanyName;
                client.company_domicile = Model.CompanyDomicile;
            }

            if (Model.SelectedCompanyRegisterationType.Id.Equals("foreign")) {
                client.legal_form = Model.SelectedCompanyLegalForm.Id;
            }

            if (Model.SelectedCompanyRegisterationType.Id.Equals("german_registered")) client.company_id = Int32.Parse(Model.CompanyRegistrationNumber);


            client.contactperson = ContactPerson();
            client.companyaddress = CompanyAddress();

            return client;
        }
        #endregion

        #region GetCompanyId
        public QueryCompanyIndexResult[] GetCompanyId() {
            var federalGazetteService = new BAnzServicePortTypeClient();

            var companyIndexData = new QueryCompanyIndexDataType();

            register_type_type registerType;
                Enum.TryParse(Model.SelectedRegisterationType.Id, out registerType);
            companyIndexData.register_type = registerType;

            companyIndexData.register_court = Model.SelectedRegisteredCourt.Id;
            companyIndexData.company_name = Model.CompanyName;
            companyIndexData.register_number = Model.CompanyRegistrationNumber;
            companyIndexData.register_typeSpecified = true;

            try {
                var response = federalGazetteService.queryCompanyIndex(_login, companyIndexData);
                _federalGazetteErrorAnalysis.AnalyseError(response);
                if (response.result == 0) return response.queryResultList;

            } catch (NullReferenceException ne) {
                throw new Exception("Es könnte keine Firmen-ID abgefragt werden.\n" + ne.Message);
            }
            return null;
        }
        #endregion

        #region CompanyList
        public List<CompanyList> GetCompanyListQueryId() {
            var companyInfo = GetCompanyId();
            var companyList = new List<CompanyList>();

            foreach (var result in companyInfo) {
                var tmp = new CompanyList();
                tmp.CompanyId = result.company_id;
                tmp.CompanyName = result.company_name;
                tmp.Delete = result.deleted;
                tmp.Domicile = result.domicile;
                tmp.LegalForm = result.legal_form.HasValue ? result.legal_form.Value.ToString() : null;
                tmp.RegisterCourt = result.register_court;
                tmp.RegisterType = result.register_type.ToString();
                companyList.Add(tmp);
            }
            return companyList;
        }
        #endregion

        #region DeleteClient
        public bool DeleteClient(string clientId) {
            var federalGazetteService = new BAnzServicePortTypeClient();
            try {
                var response = federalGazetteService.deleteClient(_login, clientId);
                if (response.result == 0) return true;

            } catch (Exception invalid) {
                throw new Exception(invalid.Message);
            }
            return false;
        }
        #endregion

        #region ChangingClient
        private EbanzChangeClient ChangingClient() {
            var changeingClient = new EbanzChangeClient {
                company_domicile = Model.CompanyDomicile,
                company_name = Model.CompanyName,
                companyaddress = CompanyAddress(),
                contactperson = ContactPerson(),
                legal_form = Model.SelectedCompanyLegalForm.Id,
                sign = Model.CompanySign
            };
            return changeingClient;
        }
        #endregion

        #region ChangeClient
        public void ChangeClient() {
            var federalGazetteService = new BAnzServicePortTypeClient();

            try {
                var response = federalGazetteService.changeClient(_login, Model.ClientNumber, ChangingClient(), null);

                if (response.result == 0) return;
                else _federalGazetteErrorAnalysis.AnalyseError(response);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }

        }
        #endregion

        #region ContactPerson
        public ContactPerson ContactPerson() {
            var contact = new ContactPerson {
                firstname = Model.ContectPersonFirstName,
                lastname = Model.ContactPersonLastName,
                email = Model.ContactPersonEmail,
                title = Model.ContactPersonTitle
            };

            if (Model.ContactPersonSalutation != null)
                contact.salutation = Model.ContactPersonSalutation.ToLower().Equals("herr")
                                         ? salutation_type.Herr
                                         : salutation_type.Frau;

            return contact;
        }
        #endregion

        #region CompanyAddress
        public CompanyAddress CompanyAddress() {
            var companyAddress = new CompanyAddress {
                city = Model.CompanyCity,
                company_name = Model.CompanyName,
                country = Model.CompanySelectedCountry.Id,
                postcode = Model.CompanyPostCode,
                state = Model.CompanyState,
                street = Model.CompanyStreet
            };
            return companyAddress;
        }
        #endregion

        #region GetClientList
        public ObservableCollectionAsync<ClientsList> GetClientList() {
            var federalGazetteService = new BAnzServicePortTypeClient();
            try {

                var response = federalGazetteService.listClients(_login, null, null, null);
                _federalGazetteErrorAnalysis.AnalyseError(response);

                if (response.result == 0) return ConvertClientList(response.clientList);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return null;
        }
        #endregion

        #region GetClientData
        public EbanzClient GetClientData() {
            var federalGazetteService = new BAnzServicePortTypeClient();
            try {
                var response = federalGazetteService.getClient(_login, Model.ClientNumber);
                _federalGazetteErrorAnalysis.AnalyseError(response);
                if (response.result == 0) return response.ebanzClient;
               
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return null;
        }
        #endregion

        #region UpdateClientDataFromRegCentral
        public EbanzClient UpdateClientDataFromRegCentral() {
            var federalGazetteService = new BAnzServicePortTypeClient();

            try {
                var response = federalGazetteService.updateRegisteredData(_login, Model.ClientNumber);

                _federalGazetteErrorAnalysis.AnalyseError(response);
                if (response.result == 0) return response.ebanzClient;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return null;
        }
        #endregion

        #region ReassignCompanyIndexNumberFromRegCentral

        public bool ReassignCompanyIndexNumberFromRegCentral() {
            var federalGazetteService = new BAnzServicePortTypeClient();
            try {
                var response = federalGazetteService.reassignToIndex(_login, Model.ClientNumber,
                                                                     Int32.Parse(Model.CompanyId));
                _federalGazetteErrorAnalysis.AnalyseError(response);
                if (response == 0) return true;
            } catch (Exception ex) {

                throw new Exception(ex.Message);
            }
            return false;
        }
        #endregion

        #region ConverClientList
        private ObservableCollectionAsync<ClientsList> ConvertClientList(IEnumerable<ClientEntry> clientEntry) {
            var clientList = new ObservableCollectionAsync<ClientsList>();
            foreach (var entry in clientEntry) {
                var tmp = new ClientsList();
                tmp.ClientId = entry.clientnumber;
                tmp.CompanyName = entry.company_name;
                tmp.CompanySign = entry.sign;
                clientList.Add(tmp);
            }
            return clientList;
        }
        #endregion

        #region ConfirmDeletedRgisteredData
        public bool ConfirmDeletedRegisteredData() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                var response = federalGazetteService.confirmDeletedRegisteredData(_login, Model.ClientNumber);
                _federalGazetteErrorAnalysis.AnalyseError(response);
                if(response.result==0) return true;
            }
            return false;
        }
        #endregion
    }
}