// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Text;
using eFederalGazette;

namespace eBalanceKitBusiness.FederalGazette {
    public class FederalGazetteErrorAnalysis {
        #region AnalyseError

        public void AnalyseError(object response) {

            if (response is ebanzListOrdersResponseType) {
                var tmp = response as ebanzListOrdersResponseType;
                ErrorID(tmp.result);
            } else if (response is ebanzGetReceiptResponseType) {
                var tmp = response as ebanzGetReceiptResponseType;
                ErrorID(tmp.result);
            } else if (response is ebanzGetOrderStatusResponseType) {
                var tmp = response as ebanzGetOrderStatusResponseType;
                ErrorID(tmp.result);
            } else if (response is ebanzGetOrderResponseType) {
                var tmp = response as ebanzGetOrderResponseType;
                ErrorID(tmp.result);
            } else if (response is getTicketStatusResponseType) {
                var tmp = response as getTicketStatusResponseType;
                ErrorID(tmp.result);

            } else if (response is confirmDeletedRegisteredDataResponseType) {
                var tmp = response as confirmDeletedRegisteredDataResponseType;
                ErrorID(tmp.result);

            } else if (response is ebanzCancelOrderResponseType) {
                var tmp = response as ebanzCancelOrderResponseType;
                ErrorID(tmp.result);

            } else if (response is ebanzChangeOrderResponseType) {
                var tmp = response as ebanzChangeOrderResponseType;

                var strBuilder = new StringBuilder();
                if(tmp.validationErrorList!=null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result, strBuilder);
            } else if (response is ebanzNewOrderResponseType) {
                var tmp = response as ebanzNewOrderResponseType;

                var strBuilder = new StringBuilder();
                if (tmp.validationErrorList != null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result, strBuilder);
            } else if (response is ebanzCheckOrderResponseType) {
                var tmp = response as ebanzCheckOrderResponseType;
                var strBuilder = new StringBuilder();
                if (tmp.validationErrorList != null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result, strBuilder);
            } else if (response is queryCompanyIndexResponseType) {
                var tmp = response as queryCompanyIndexResponseType;
                var strBuilder = new StringBuilder();
                if (tmp.validationErrorList != null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result, strBuilder);
            } else if (response is deleteClientResponseType) {
                ErrorID((response as deleteClientResponseType).result);
            } else if (response is createClientResponseType) {
                var tmp = response as createClientResponseType;
                var strBuilder = new StringBuilder();
                if (tmp.validationErrorList != null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result, strBuilder);
            } else if (response is changeClientResponseType) {
                var tmp = response as changeClientResponseType;
                var strBuilder = new StringBuilder();
                if (tmp.validationErrorList != null)
                foreach (var err in tmp.validationErrorList) {
                    strBuilder.Append(err.error + " = " + err.description + "\n");
                }
                ErrorID(tmp.result);
            } else if (response is listClientsResponseType) {
                ErrorID((response as listClientsResponseType).result);
            } else if (response is getClientResponseType) {
                ErrorID((response as getClientResponseType).result);
            } else if (response is updateRegisteredDataResponseType) {
                ErrorID((response as updateRegisteredDataResponseType).result);
            } else if (response is int) {
                ErrorID((response as int?));
            }
        }
        #endregion

        #region ErrorID

        private void ErrorID(int? errId, StringBuilder stringBuilder = null) {
            switch (errId.ToString()) {
                case "20":
                    throw new Exception("Validierungsfehler:\n" + stringBuilder);
                case "21":
                    throw new Exception("TicketID existiert.\n" + stringBuilder);
                case "22":
                    throw new Exception("TicketID ist in Bearbeitung.\n" + stringBuilder);
                case "50":
                    throw new Exception("Client konnte nicht gelöscht werden.\nFehler:Authentifizierungsfehler!\n" +
                                        stringBuilder);
                case "51":
                    throw new Exception("Client existiert nicht.\n" + stringBuilder);
                case "53":
                    throw new Exception(
                        "Die Registrierungsdaten des Veröffentlichungspflichtigen müssen aktualisiert werden.\n" +
                        stringBuilder);
                case "54":
                    throw new Exception("Der angegebene Kunde passt nicht zur vorgegebenen Agentur.\n" + stringBuilder);
                case "55":
                    throw new Exception("Die angegebene Auftragsnummer ist nicht korrekt.\n" + stringBuilder);
                case "56":
                    throw new Exception("Der Veröffentlichungspflichtgen muss sich dem Firmenindex zuordnen.\n" +
                                        stringBuilder);
                case "99":
                    throw new Exception(
                        "eBundesanziger: System wird gewartet - bitte versuchen sie es später noch einmal.\n" +
                        stringBuilder);
                case "100":
                    throw new Exception("eBundesanziger: Interner Fehler.\nBitt versuchen Sie später erneut.\n" +
                                        stringBuilder);
            }


        }
        #endregion
    }
}