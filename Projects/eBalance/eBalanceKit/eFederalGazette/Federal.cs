using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace eFederalGazette {
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [ServiceContract(Namespace = "http://ws.publikations-plattform.de/BAnzService",
        ConfigurationName = "BAnzServicePortType")]
    public interface BAnzServicePortType {
        // CODEGEN: Parameter "ticketID" erfordert zusätzliche Schemainformationen, die nicht mit dem Parametermodus erfasst werden können. Das spezifische Attribut ist "System.Xml.Serialization.XmlElementAttribute".
        [OperationContract(Action = "urn:createClient",
            ReplyAction = "urn:createClientResponse")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        createClientResponse createClient(createClientRequest request);

        // CODEGEN: Parameter "ticketID" erfordert zusätzliche Schemainformationen, die nicht mit dem Parametermodus erfasst werden können. Das spezifische Attribut ist "System.Xml.Serialization.XmlElementAttribute".
        [OperationContract(Action = "urn:changeClient",
            ReplyAction = "urn:changeClientResponse")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        changeClientResponse changeClient(changeClientRequest request);

        [OperationContract(Action = "urn:deleteClient",
            ReplyAction = "urn:deleteClientResponse")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        deleteClientResponseType deleteClient(LoginData loginData, string clientnumber);

        // CODEGEN: Parameter "clientnumber" erfordert zusätzliche Schemainformationen, die nicht mit dem Parametermodus erfasst werden können. Das spezifische Attribut ist "System.Xml.Serialization.XmlElementAttribute".
        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/listClients", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        listClientsResponse listClients(listClientsRequest request);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/getClient", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        getClientResponseType getClient(LoginData loginData, string clientnumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/updateRegisteredData", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        updateRegisteredDataResponseType updateRegisteredData(LoginData loginData, string clientnumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/confirmDeletedRegisteredData", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        confirmDeletedRegisteredDataResponseType confirmDeletedRegisteredData(LoginData loginData, string clientnumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/reassignToIndex", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "result")]
        int reassignToIndex(LoginData loginData, string clientnumber, int company_id);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/getTicketStatus", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        getTicketStatusResponseType getTicketStatus(LoginData loginData, string ticketID);

        // CODEGEN: Parameter "ticketID" erfordert zusätzliche Schemainformationen, die nicht mit dem Parametermodus erfasst werden können. Das spezifische Attribut ist "System.Xml.Serialization.XmlElementAttribute".
        [OperationContract(
            Action = "http://schemas.xmlsoap.org/wsdl/MTOMServicePortType/AttachmentRequest",
            ReplyAction = "http://schemas.xmlsoap.org/wsdl/MTOMServicePortType/AttachmentResponse")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzNewOrderResponse ebanzNewOrder(ebanzNewOrderRequest request);

        [OperationContract(
            Action = "http://schemas.xmlsoap.org/wsdl/MTOMServicePortType/AttachmentRequest",
            ReplyAction = "http://schemas.xmlsoap.org/wsdl/MTOMServicePortType/AttachmentResponse")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzCheckOrderResponseType ebanzCheckOrder(LoginData loginData, ebanzPublicationOrderData orderData,
                                                    string clientnumber, OrderReplyInfo replyInfo);

        // CODEGEN: Parameter "ticketID" erfordert zusätzliche Schemainformationen, die nicht mit dem Parametermodus erfasst werden können. Das spezifische Attribut ist "System.Xml.Serialization.XmlElementAttribute".
        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzChangeOrder", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzChangeOrderResponse ebanzChangeOrder(ebanzChangeOrderRequest request);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzListOrders", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzListOrdersResponseType ebanzListOrders(LoginData loginData, OrderFilter orderFilter);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzGetOrder", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzGetOrderResponseType ebanzGetOrder(LoginData loginData, string ordernumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzGetOrderStatus", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzGetOrderStatusResponseType ebanzGetOrderStatus(LoginData loginData, string ordernumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzGetReceipt", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzGetReceiptResponseType ebanzGetReceipt(LoginData loginData, string ordernumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/ebanzCancelOrder", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        ebanzCancelOrderResponseType ebanzCancelOrder(LoginData loginData, string ordernumber);

        [OperationContract(
            Action = "http://ws.publikations-plattform.de/BAnzService/queryCompanyIndex", ReplyAction = "*")]
        [XmlSerializerFormat]
        [return: MessageParameter(Name = "return")]
        queryCompanyIndexResponseType queryCompanyIndex(LoginData loginData, QueryCompanyIndexDataType queryData);
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class LoginData {
        private string passwordField;
        private string usernameField;


        [XmlElement(Order = 0)]
        public string username { get { return usernameField; } set { usernameField = value; } }


        [XmlElement(Order = 1)]
        public string password { get { return passwordField; } set { passwordField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class QueryBranchIndexResult {
        private string company_nameField;


        [XmlElement(Order = 0)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class QueryCompanyIndexResult {
        private int company_idField;

        private string company_nameField;
        private bool deletedField;

        private string domicileField;

        private legalform_type? legal_formField;
        private QueryBranchIndexResult[] queryBranchResultListField;

        private string register_courtField;

        private string register_numberField;
        private register_type_type register_typeField;


        [XmlElement(Order = 0)]
        public int company_id { get { return company_idField; } set { company_idField = value; } }


        [XmlElement(Order = 1)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string domicile { get { return domicileField; } set { domicileField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public legalform_type? legal_form { get { return legal_formField; } set { legal_formField = value; } }


        [XmlElement(Order = 4)]
        public string register_court { get { return register_courtField; } set { register_courtField = value; } }


        [XmlElement(Order = 5)]
        public register_type_type register_type { get { return register_typeField; } set { register_typeField = value; } }


        [XmlElement(Order = 6)]
        public string register_number { get { return register_numberField; } set { register_numberField = value; } }


        [XmlElement(Order = 7)]
        public bool deleted { get { return deletedField; } set { deletedField = value; } }


        [XmlArray(IsNullable = true, Order = 8)]
        [XmlArrayItem("queryBranchResult", IsNullable = false)]
        public QueryBranchIndexResult[] queryBranchResultList { get { return queryBranchResultListField; } set { queryBranchResultListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum legalform_type {
        AG,


        GmbH,


        eGen,


        KGaA,


        VvaG,


        EwI,


        EK,


        SE,


        OHG,


        KG,


        Ver,


        Part,


        ARGnR,


        ARHRA,


        ARHRB,


        ARPR,


        EKf,


        HRAJP,


        SG,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum register_type_type {
        HRA,


        HRB,


        GnR,


        PR,


        VR,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class queryCompanyIndexResponseType {
        private QueryCompanyIndexResult[] queryResultListField;
        private int resultField;

        private StatusType? statusField;

        private bool statusFieldSpecified;

        private ValidationError[] validationErrorListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public StatusType? status { get { return statusField; } set { statusField = value; } }


        [XmlIgnore]
        public bool statusSpecified { get { return statusFieldSpecified; } set { statusFieldSpecified = value; } }


        [XmlArray(IsNullable = true, Order = 2)]
        [XmlArrayItem("queryResult", IsNullable = false)]
        public QueryCompanyIndexResult[] queryResultList { get { return queryResultListField; } set { queryResultListField = value; } }


        [XmlArray(IsNullable = true, Order = 3)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum StatusType {
        no_match,


        unique_match,


        ambiguous_match,


        unique_no_match,


        no_match_branch,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ValidationError {
        private string descriptionField;
        private int errorField;


        [XmlElement(Order = 0)]
        public int error { get { return errorField; } set { errorField = value; } }


        [XmlElement(Order = 1)]
        public string description { get { return descriptionField; } set { descriptionField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class QueryCompanyIndexDataType {
        private string company_nameField;
        private string register_courtField;
        private string register_numberField;

        private register_type_type? register_typeField;

        private bool register_typeFieldSpecified;


        [XmlElement(Order = 0)]
        public string register_court { get { return register_courtField; } set { register_courtField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public register_type_type? register_type { get { return register_typeField; } set { register_typeField = value; } }


        [XmlIgnore]
        public bool register_typeSpecified { get { return register_typeFieldSpecified; } set { register_typeFieldSpecified = value; } }


        [XmlElement(Order = 2)]
        public string register_number { get { return register_numberField; } set { register_numberField = value; } }


        [XmlElement(Order = 3)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzCancelOrderResponseType {
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ReceiptFile {
        private base64Binary fileField;

        private string filenameField;


        [XmlElement(Order = 0)]
        public base64Binary file { get { return fileField; } set { fileField = value; } }


        [XmlElement(Order = 1)]
        public string filename { get { return filenameField; } set { filenameField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://www.w3.org/2005/05/xmlmime")]
    public class base64Binary {
        private string contentTypeField;

        private byte[] valueField;


        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string contentType { get { return contentTypeField; } set { contentTypeField = value; } }


        [XmlText(DataType = "base64Binary")]
        public byte[] Value { get { return valueField; } set { valueField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzGetReceiptResponseType {
        private ReceiptFile receiptFileField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public ReceiptFile receiptFile { get { return receiptFileField; } set { receiptFileField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzGetOrderStatusResponseType {
        private int resultField;

        private orderStatusType? statusField;

        private bool statusFieldSpecified;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public orderStatusType? status { get { return statusField; } set { statusField = value; } }


        [XmlIgnore]
        public bool statusSpecified { get { return statusFieldSpecified; } set { statusFieldSpecified = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum orderStatusType {
        auftrag_eingegangen,


        in_bearbeitung,


        in_produktion,


        endfreigabe,


        veroeffentlicht,


        storniert_kunde,


        storniert_banz,


        geloescht_banz,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzGetOrderResponseType {
        private ebanzPublicationOrderData ebanzPublicationOrderField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public ebanzPublicationOrderData ebanzPublicationOrder { get { return ebanzPublicationOrderField; } set { ebanzPublicationOrderField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzPublicationOrderData {
        private DateTime? acquisition_period_endField;
        private DateTime? acquisition_period_startField;
        private int? balance_sheet_standardField;
        private bool? bb_264Field;
        private bool? bh_264Field;

        private ebanzBillAddress bill_addressField;

        private ebanzBillAddress bill_receiverField;
        private DateTime? blocking_dateField;
        private string company_sizeField;
        private int? company_typeField;

        private ebanzPublicationFile[] filesField;
        private bool? has_natural_personField;

        private bool has_natural_personFieldSpecified;
        private string languageField;

        private bool? oldregisterdataField;
        private int order_typeField;

        private DateTime? orig_pub_dateField;

        private string orig_pub_ordernumberField;
        private int publication_areaField;

        private int publication_categoryField;
        private int? publication_sub_typeField;
        private int publication_typeField;
        private ReferencedCompany[] referencedCompanyListField;
        private submittertype_type? submitter_typeField;
        private bool? vu_264Field;
        private string your_referenceField;


        [XmlElement(Order = 0)]
        public int order_type { get { return order_typeField; } set { order_typeField = value; } }


        [XmlElement(Order = 1)]
        public int publication_area { get { return publication_areaField; } set { publication_areaField = value; } }


        [XmlElement(Order = 2)]
        public int publication_category { get { return publication_categoryField; } set { publication_categoryField = value; } }


        [XmlElement(Order = 3)]
        public int publication_type { get { return publication_typeField; } set { publication_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public int? publication_sub_type { get { return publication_sub_typeField; } set { publication_sub_typeField = value; } }


        [XmlElement(Order = 5)]
        public string language { get { return languageField; } set { languageField = value; } }


        [XmlElement(IsNullable = true, Order = 6)]
        public int? company_type { get { return company_typeField; } set { company_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 7)]
        public submittertype_type? submitter_type { get { return submitter_typeField; } set { submitter_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 8)]
        public int? balance_sheet_standard { get { return balance_sheet_standardField; } set { balance_sheet_standardField = value; } }


        [XmlElement(IsNullable = true, Order = 9)]
        public string company_size { get { return company_sizeField; } set { company_sizeField = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 10)]
        public DateTime? blocking_date { get { return blocking_dateField; } set { blocking_dateField = value; } }


        [XmlElement(IsNullable = true, Order = 11)]
        public string your_reference { get { return your_referenceField; } set { your_referenceField = value; } }


        [XmlElement(IsNullable = true, Order = 12)]
        public ebanzBillAddress bill_address { get { return bill_addressField; } set { bill_addressField = value; } }


        [XmlElement(IsNullable = true, Order = 13)]
        public ebanzBillAddress bill_receiver { get { return bill_receiverField; } set { bill_receiverField = value; } }


        [XmlArray(IsNullable = true, Order = 14)]
        [XmlArrayItem("file", IsNullable = false)]
        public ebanzPublicationFile[] files { get { return filesField; } set { filesField = value; } }


        [XmlArray(IsNullable = true, Order = 15)]
        [XmlArrayItem("referencedCompany", IsNullable = false)]
        public ReferencedCompany[] referencedCompanyList { get { return referencedCompanyListField; } set { referencedCompanyListField = value; } }


        [XmlElement(IsNullable = true, Order = 16)]
        public bool? oldregisterdata { get { return oldregisterdataField; } set { oldregisterdataField = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 17)]
        public DateTime? acquisition_period_start { get { return acquisition_period_startField; } set { acquisition_period_startField = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 18)]
        public DateTime? acquisition_period_end { get { return acquisition_period_endField; } set { acquisition_period_endField = value; } }


        [XmlElement(IsNullable = true, Order = 19)]
        public bool? bb_264 { get { return bb_264Field; } set { bb_264Field = value; } }


        [XmlElement(IsNullable = true, Order = 20)]
        public bool? vu_264 { get { return vu_264Field; } set { vu_264Field = value; } }


        [XmlElement(IsNullable = true, Order = 21)]
        public bool? bh_264 { get { return bh_264Field; } set { bh_264Field = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 22)]
        public DateTime? orig_pub_date { get { return orig_pub_dateField; } set { orig_pub_dateField = value; } }


        [XmlElement(IsNullable = true, Order = 23)]
        public string orig_pub_ordernumber { get { return orig_pub_ordernumberField; } set { orig_pub_ordernumberField = value; } }


        [XmlElement(IsNullable = true, Order = 24)]
        public bool? has_natural_person { get { return has_natural_personField; } set { has_natural_personField = value; } }


        [XmlIgnore]
        public bool has_natural_personSpecified { get { return has_natural_personFieldSpecified; } set { has_natural_personFieldSpecified = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum submittertype_type {
        parent,


        subsidiary,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzBillAddress {
        private string companyField;
        private string countryField;

        private string devisionField;
        private string emailField;
        private Telephone faxField;

        private string firstnameField;

        private string lastnameField;
        private Telephone mobileField;

        private string postboxField;
        private salutation_type? salutationField;
        private string streetField;

        private Telephone telephoneField;
        private string titleField;
        private string townField;
        private string zipcodeField;


        [XmlElement(Order = 0)]
        public string company { get { return companyField; } set { companyField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string devision { get { return devisionField; } set { devisionField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public salutation_type? salutation { get { return salutationField; } set { salutationField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public string title { get { return titleField; } set { titleField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public string firstname { get { return firstnameField; } set { firstnameField = value; } }


        [XmlElement(IsNullable = true, Order = 5)]
        public string lastname { get { return lastnameField; } set { lastnameField = value; } }


        [XmlElement(IsNullable = true, Order = 6)]
        public string street { get { return streetField; } set { streetField = value; } }


        [XmlElement(IsNullable = true, Order = 7)]
        public string postbox { get { return postboxField; } set { postboxField = value; } }


        [XmlElement(IsNullable = true, Order = 8)]
        public string zipcode { get { return zipcodeField; } set { zipcodeField = value; } }


        [XmlElement(IsNullable = true, Order = 9)]
        public string town { get { return townField; } set { townField = value; } }


        [XmlElement(IsNullable = true, Order = 10)]
        public string country { get { return countryField; } set { countryField = value; } }


        [XmlElement(IsNullable = true, Order = 11)]
        public Telephone telephone { get { return telephoneField; } set { telephoneField = value; } }


        [XmlElement(IsNullable = true, Order = 12)]
        public Telephone fax { get { return faxField; } set { faxField = value; } }


        [XmlElement(IsNullable = true, Order = 13)]
        public Telephone mobile { get { return mobileField; } set { mobileField = value; } }


        [XmlElement(IsNullable = true, Order = 14)]
        public string email { get { return emailField; } set { emailField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public enum salutation_type {
        Herr,


        Frau,
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class Telephone {
        private string area_codeField;
        private string country_area_codeField;

        private string numberField;


        [XmlElement(Order = 0)]
        public string country_area_code { get { return country_area_codeField; } set { country_area_codeField = value; } }


        [XmlElement(Order = 1)]
        public string area_code { get { return area_codeField; } set { area_codeField = value; } }


        [XmlElement(Order = 2)]
        public string number { get { return numberField; } set { numberField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzPublicationFile {
        private int? annual_content_typeField;

        private string annual_content_type_freeField;
        private base64Binary fileField;

        private string file_nameField;


        [XmlElement(Order = 0)]
        public base64Binary file { get { return fileField; } set { fileField = value; } }


        [XmlElement(Order = 1)]
        public string file_name { get { return file_nameField; } set { file_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public int? annual_content_type { get { return annual_content_typeField; } set { annual_content_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public string annual_content_type_free { get { return annual_content_type_freeField; } set { annual_content_type_freeField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ReferencedCompany {
        private int? company_idField;
        private string company_locationField;
        private string company_nameField;
        private string company_typeField;

        private string countryField;
        private string legalform_otherField;


        [XmlElement(Order = 0)]
        public string company_type { get { return company_typeField; } set { company_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string company_location { get { return company_locationField; } set { company_locationField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public string legalform_other { get { return legalform_otherField; } set { legalform_otherField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public string country { get { return countryField; } set { countryField = value; } }


        [XmlElement(IsNullable = true, Order = 5)]
        public int? company_id { get { return company_idField; } set { company_idField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class PublicationDateListEntry {
        private DateTime dateField;
        private int date_typeField;


        [XmlElement(Order = 0)]
        public int date_type { get { return date_typeField; } set { date_typeField = value; } }


        [XmlElement(DataType = "date", Order = 1)]
        public DateTime date { get { return dateField; } set { dateField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class OrderEntry {
        private string clientnumberField;

        private string company_nameField;
        private DateTime creation_dateField;

        private string ordernumberField;

        private int publication_categoryField;
        private PublicationDateListEntry[] publication_date_listField;

        private int publication_typeField;
        private string signField;

        private orderStatusType statusField;


        [XmlElement(Order = 0)]
        public string clientnumber { get { return clientnumberField; } set { clientnumberField = value; } }


        [XmlElement(Order = 1)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string sign { get { return signField; } set { signField = value; } }


        [XmlElement(Order = 3)]
        public string ordernumber { get { return ordernumberField; } set { ordernumberField = value; } }


        [XmlElement(Order = 4)]
        public int publication_category { get { return publication_categoryField; } set { publication_categoryField = value; } }


        [XmlElement(Order = 5)]
        public int publication_type { get { return publication_typeField; } set { publication_typeField = value; } }


        [XmlElement(Order = 6)]
        public orderStatusType status { get { return statusField; } set { statusField = value; } }


        [XmlElement(DataType = "date", Order = 7)]
        public DateTime creation_date { get { return creation_dateField; } set { creation_dateField = value; } }


        [XmlArray(IsNullable = true, Order = 8)]
        [XmlArrayItem("date", IsNullable = false)]
        public PublicationDateListEntry[] publication_date_list { get { return publication_date_listField; } set { publication_date_listField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzListOrdersResponseType {
        private OrderEntry[] orderListField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlArray(IsNullable = true, Order = 1)]
        [XmlArrayItem("orderEntry", IsNullable = false)]
        public OrderEntry[] orderList { get { return orderListField; } set { orderListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class OrderFilter {
        private string clientnumberField;

        private string company_nameField;

        private string[] ordernumber_listField;

        private int? publication_categoryField;

        private bool publication_categoryFieldSpecified;
        private DateTime? publication_date_endField;

        private bool publication_date_endFieldSpecified;
        private DateTime? publication_date_startField;

        private bool publication_date_startFieldSpecified;

        private int? publication_typeField;

        private bool publication_typeFieldSpecified;
        private string signField;

        private orderStatusType? statusField;

        private bool statusFieldSpecified;

        private DateTime? status_changed_sinceField;

        private bool status_changed_sinceFieldSpecified;


        [XmlElement(IsNullable = true, Order = 0)]
        public string clientnumber { get { return clientnumberField; } set { clientnumberField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string sign { get { return signField; } set { signField = value; } }


        [XmlArray(IsNullable = true, Order = 3)]
        [XmlArrayItem("ordernumber", IsNullable = false)]
        public string[] ordernumber_list { get { return ordernumber_listField; } set { ordernumber_listField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public int? publication_category { get { return publication_categoryField; } set { publication_categoryField = value; } }


        [XmlIgnore]
        public bool publication_categorySpecified { get { return publication_categoryFieldSpecified; } set { publication_categoryFieldSpecified = value; } }


        [XmlElement(IsNullable = true, Order = 5)]
        public int? publication_type { get { return publication_typeField; } set { publication_typeField = value; } }


        [XmlIgnore]
        public bool publication_typeSpecified { get { return publication_typeFieldSpecified; } set { publication_typeFieldSpecified = value; } }


        [XmlElement(IsNullable = true, Order = 6)]
        public orderStatusType? status { get { return statusField; } set { statusField = value; } }


        [XmlIgnore]
        public bool statusSpecified { get { return statusFieldSpecified; } set { statusFieldSpecified = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 7)]
        public DateTime? publication_date_start { get { return publication_date_startField; } set { publication_date_startField = value; } }


        [XmlIgnore]
        public bool publication_date_startSpecified { get { return publication_date_startFieldSpecified; } set { publication_date_startFieldSpecified = value; } }


        [XmlElement(DataType = "date", IsNullable = true, Order = 8)]
        public DateTime? publication_date_end { get { return publication_date_endField; } set { publication_date_endField = value; } }


        [XmlIgnore]
        public bool publication_date_endSpecified { get { return publication_date_endFieldSpecified; } set { publication_date_endFieldSpecified = value; } }


        [XmlElement(IsNullable = true, Order = 9)]
        public DateTime? status_changed_since { get { return status_changed_sinceField; } set { status_changed_sinceField = value; } }


        [XmlIgnore]
        public bool status_changed_sinceSpecified { get { return status_changed_sinceFieldSpecified; } set { status_changed_sinceFieldSpecified = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzChangeOrderResponseType {
        private int resultField;

        private ebanzResultOrderData resultOrderDataField;

        private ValidationError[] validationErrorListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public ebanzResultOrderData resultOrderData { get { return resultOrderDataField; } set { resultOrderDataField = value; } }


        [XmlArray(IsNullable = true, Order = 2)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzResultOrderData {
        private base64Binary html_contentField;

        private string ordernumberField;


        [XmlElement(IsNullable = true, Order = 0)]
        public base64Binary html_content { get { return html_contentField; } set { html_contentField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string ordernumber { get { return ordernumberField; } set { ordernumberField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzCheckOrderResponseType {
        private int resultField;

        private ebanzResultOrderData resultOrderDataField;

        private ValidationError[] validationErrorListField;

        private Warning[] warningListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public ebanzResultOrderData resultOrderData { get { return resultOrderDataField; } set { resultOrderDataField = value; } }


        [XmlArray(IsNullable = true, Order = 2)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }


        [XmlArray(IsNullable = true, Order = 3)]
        [XmlArrayItem(IsNullable = false)]
        public Warning[] warningList { get { return warningListField; } set { warningListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class Warning {
        private string descriptionField;


        [XmlElement(Order = 0)]
        public string description { get { return descriptionField; } set { descriptionField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ebanzNewOrderResponseType {
        private int resultField;

        private ebanzResultOrderData resultOrderDataField;

        private ValidationError[] validationErrorListField;

        private Warning[] warningListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public ebanzResultOrderData resultOrderData { get { return resultOrderDataField; } set { resultOrderDataField = value; } }


        [XmlArray(IsNullable = true, Order = 2)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }


        [XmlArray(IsNullable = true, Order = 3)]
        [XmlArrayItem(IsNullable = false)]
        public Warning[] warningList { get { return warningListField; } set { warningListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class OrderReplyInfo {
        private string emailField;
        private bool? html_direct_replyField;


        [XmlElement(IsNullable = true, Order = 0)]
        public bool? html_direct_reply { get { return html_direct_replyField; } set { html_direct_replyField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string email { get { return emailField; } set { emailField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class getTicketStatusResponseType {
        private string dataField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string data { get { return dataField; } set { dataField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class confirmDeletedRegisteredDataResponseType {
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class updateRegisteredDataResponseType {
        private EbanzClient ebanzClientField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public EbanzClient ebanzClient { get { return ebanzClientField; } set { ebanzClientField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class EbanzClient : EbanzChangeClient {
        private int? company_idField;
        private string company_typeField;


        [XmlElement(Order = 0)]
        public string company_type { get { return company_typeField; } set { company_typeField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public int? company_id { get { return company_idField; } set { company_idField = value; } }
    }


    [XmlInclude(typeof (EbanzClient))]
    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class EbanzChangeClient {
        private string company_domicileField;

        private string company_nameField;

        private CompanyAddress companyaddressField;
        private ContactPerson contactpersonField;
        private string legal_formField;
        private string signField;


        [XmlElement(IsNullable = true, Order = 0)]
        public string sign { get { return signField; } set { signField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string legal_form { get { return legal_formField; } set { legal_formField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string company_domicile { get { return company_domicileField; } set { company_domicileField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public ContactPerson contactperson { get { return contactpersonField; } set { contactpersonField = value; } }


        [XmlElement(Order = 5)]
        public CompanyAddress companyaddress { get { return companyaddressField; } set { companyaddressField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ContactPerson {
        private string emailField;
        private Telephone faxField;
        private string firstnameField;

        private string lastnameField;

        private Telephone mobileField;
        private salutation_type? salutationField;

        private bool salutationFieldSpecified;
        private Telephone telephoneField;
        private string titleField;


        [XmlElement(IsNullable = true, Order = 0)]
        public salutation_type? salutation { get { return salutationField; } set { salutationField = value; } }


        [XmlIgnore]
        public bool salutationSpecified { get { return salutationFieldSpecified; } set { salutationFieldSpecified = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string title { get { return titleField; } set { titleField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string firstname { get { return firstnameField; } set { firstnameField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public string lastname { get { return lastnameField; } set { lastnameField = value; } }


        [XmlElement(IsNullable = true, Order = 4)]
        public Telephone telephone { get { return telephoneField; } set { telephoneField = value; } }


        [XmlElement(IsNullable = true, Order = 5)]
        public Telephone mobile { get { return mobileField; } set { mobileField = value; } }


        [XmlElement(IsNullable = true, Order = 6)]
        public Telephone fax { get { return faxField; } set { faxField = value; } }


        [XmlElement(IsNullable = true, Order = 7)]
        public string email { get { return emailField; } set { emailField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class CompanyAddress {
        private string cityField;
        private string company_nameField;
        private string countryField;

        private string devisionField;

        private string postcodeField;

        private string stateField;
        private string streetField;


        [XmlElement(Order = 0)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public string devision { get { return devisionField; } set { devisionField = value; } }


        [XmlElement(Order = 2)]
        public string street { get { return streetField; } set { streetField = value; } }


        [XmlElement(Order = 3)]
        public string postcode { get { return postcodeField; } set { postcodeField = value; } }


        [XmlElement(Order = 4)]
        public string city { get { return cityField; } set { cityField = value; } }


        [XmlElement(IsNullable = true, Order = 5)]
        public string state { get { return stateField; } set { stateField = value; } }


        [XmlElement(Order = 6)]
        public string country { get { return countryField; } set { countryField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class getClientResponseType {
        private EbanzClient ebanzClientField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlElement(IsNullable = true, Order = 1)]
        public EbanzClient ebanzClient { get { return ebanzClientField; } set { ebanzClientField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ClientEntry {
        private string clientnumberField;

        private string company_nameField;

        private string signField;


        [XmlElement(Order = 0)]
        public string clientnumber { get { return clientnumberField; } set { clientnumberField = value; } }


        [XmlElement(Order = 1)]
        public string company_name { get { return company_nameField; } set { company_nameField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string sign { get { return signField; } set { signField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class listClientsResponseType {
        private ClientEntry[] clientListField;
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlArray(IsNullable = true, Order = 1)]
        [XmlArrayItem("client", IsNullable = false)]
        public ClientEntry[] clientList { get { return clientListField; } set { clientListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class deleteClientResponseType {
        private int resultField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class changeClientResponseType {
        private int resultField;

        private ValidationError[] validationErrorListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlArray(IsNullable = true, Order = 1)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class ResultEbanzClient {
        private string company_domicileField;

        private string legal_formField;
        private string register_company_nameField;


        [XmlElement(Order = 0)]
        public string register_company_name { get { return register_company_nameField; } set { register_company_nameField = value; } }


        [XmlElement(Order = 1)]
        public string company_domicile { get { return company_domicileField; } set { company_domicileField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string legal_form { get { return legal_formField; } set { legal_formField = value; } }
    }


    [GeneratedCode("svcutil", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://ws.publikations-plattform.de/BAnzService")]
    public class createClientResponseType {
        private string clientnumberField;

        private ResultEbanzClient resultEbanzClientField;
        private int resultField;

        private ValidationError[] validationErrorListField;


        [XmlElement(Order = 0)]
        public int result { get { return resultField; } set { resultField = value; } }


        [XmlArray(IsNullable = true, Order = 1)]
        [XmlArrayItem("validationError", IsNullable = false)]
        public ValidationError[] validationErrorList { get { return validationErrorListField; } set { validationErrorListField = value; } }


        [XmlElement(IsNullable = true, Order = 2)]
        public string clientnumber { get { return clientnumberField; } set { clientnumberField = value; } }


        [XmlElement(IsNullable = true, Order = 3)]
        public ResultEbanzClient resultEbanzClient { get { return resultEbanzClientField; } set { resultEbanzClientField = value; } }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "createClient", WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService",
        IsWrapped = true)]
    public class createClientRequest {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService", Order = 1)] public EbanzClient
            client;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService", Order = 0)] public LoginData
            loginData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService", Order = 2)] [XmlElement(IsNullable = true)] public string ticketID;

        public createClientRequest() { }

        public createClientRequest(LoginData loginData, EbanzClient client, string ticketID) {
            this.loginData = loginData;
            this.client = client;
            this.ticketID = ticketID;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "createClientResponse",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class createClientResponse {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public createClientResponseType @return;

        public createClientResponse() { }

        public createClientResponse(createClientResponseType @return) { this.@return = @return; }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "changeClient",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class changeClientRequest {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 2)] public EbanzChangeClient client;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 1)] public string clientnumber;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public LoginData loginData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 3)] [XmlElement(IsNullable = true)] public string ticketID;

        public changeClientRequest() { }

        public changeClientRequest(LoginData loginData, string clientnumber, EbanzChangeClient client, string ticketID) {
            this.loginData = loginData;
            this.clientnumber = clientnumber;
            this.client = client;
            this.ticketID = ticketID;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "changeClientResponse",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class changeClientResponse {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public changeClientResponseType @return;

        public changeClientResponse() { }

        public changeClientResponse(changeClientResponseType @return) { this.@return = @return; }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "listClients",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class listClientsRequest {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 1)] [XmlElement(IsNullable = true)] public string clientnumber;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 2)] [XmlElement(IsNullable = true)] public string company_name;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public LoginData loginData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 3)] [XmlElement(IsNullable = true)] public string sign;

        public listClientsRequest() { }

        public listClientsRequest(LoginData loginData, string clientnumber, string company_name, string sign) {
            this.loginData = loginData;
            this.clientnumber = clientnumber;
            this.company_name = company_name;
            this.sign = sign;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "listClientsResponse",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class listClientsResponse {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public listClientsResponseType @return;

        public listClientsResponse() { }

        public listClientsResponse(listClientsResponseType @return) { this.@return = @return; }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "ebanzNewOrder",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class ebanzNewOrderRequest {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 2)] public string clientnumber;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public LoginData loginData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 1)] public ebanzPublicationOrderData orderData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 4)] public OrderReplyInfo replyInfo;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 3)] [XmlElement(IsNullable = true)] public string ticketID;

        public ebanzNewOrderRequest() { }

        public ebanzNewOrderRequest(LoginData loginData, ebanzPublicationOrderData orderData, string clientnumber,
                                    string ticketID, OrderReplyInfo replyInfo) {
            this.loginData = loginData;
            this.orderData = orderData;
            this.clientnumber = clientnumber;
            this.ticketID = ticketID;
            this.replyInfo = replyInfo;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "ebanzNewOrderResponse",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class ebanzNewOrderResponse {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public ebanzNewOrderResponseType @return;

        public ebanzNewOrderResponse() { }

        public ebanzNewOrderResponse(ebanzNewOrderResponseType @return) { this.@return = @return; }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "ebanzChangeOrder",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class ebanzChangeOrderRequest {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public LoginData loginData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 1)] public ebanzPublicationOrderData orderData;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 2)] public string ordernumber;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 4)] public OrderReplyInfo replyInfo;

        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 3)] [XmlElement(IsNullable = true)] public string ticketID;

        public ebanzChangeOrderRequest() { }

        public ebanzChangeOrderRequest(LoginData loginData, ebanzPublicationOrderData orderData, string ordernumber,
                                       string ticketID, OrderReplyInfo replyInfo) {
            this.loginData = loginData;
            this.orderData = orderData;
            this.ordernumber = ordernumber;
            this.ticketID = ticketID;
            this.replyInfo = replyInfo;
        }
    }

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "ebanzChangeOrderResponse",
        WrapperNamespace = "http://ws.publikations-plattform.de/BAnzService", IsWrapped = true)]
    public class ebanzChangeOrderResponse {
        [MessageBodyMember(Namespace = "http://ws.publikations-plattform.de/BAnzService",
            Order = 0)] public ebanzChangeOrderResponseType @return;

        public ebanzChangeOrderResponse() { }

        public ebanzChangeOrderResponse(ebanzChangeOrderResponseType @return) { this.@return = @return; }
    }

    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    public interface BAnzServicePortTypeChannel : BAnzServicePortType, IClientChannel {}

    [DebuggerStepThrough]
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    public class BAnzServicePortTypeClient : ClientBase<BAnzServicePortType>,
                                             BAnzServicePortType {
        public BAnzServicePortTypeClient() { }

        public BAnzServicePortTypeClient(string endpointConfigurationName) :
            base(endpointConfigurationName) { }

        public BAnzServicePortTypeClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress) { }

        public BAnzServicePortTypeClient(string endpointConfigurationName,
                                         EndpointAddress remoteAddress) :
                                             base(endpointConfigurationName, remoteAddress) { }

        public BAnzServicePortTypeClient(Binding binding,
                                         EndpointAddress remoteAddress) :
                                             base(binding, remoteAddress) { }

        #region BAnzServicePortType Members
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        createClientResponse BAnzServicePortType.createClient(createClientRequest request) { return base.Channel.createClient(request); }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        changeClientResponse BAnzServicePortType.changeClient(changeClientRequest request) { return base.Channel.changeClient(request); }

        public deleteClientResponseType deleteClient(LoginData loginData, string clientnumber) { return base.Channel.deleteClient(loginData, clientnumber); }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        listClientsResponse BAnzServicePortType.listClients(listClientsRequest request) { return base.Channel.listClients(request); }

        public getClientResponseType getClient(LoginData loginData, string clientnumber) { return base.Channel.getClient(loginData, clientnumber); }

        public updateRegisteredDataResponseType updateRegisteredData(LoginData loginData, string clientnumber) { return base.Channel.updateRegisteredData(loginData, clientnumber); }

        public confirmDeletedRegisteredDataResponseType confirmDeletedRegisteredData(LoginData loginData,
                                                                                     string clientnumber) { return base.Channel.confirmDeletedRegisteredData(loginData, clientnumber); }

        public int reassignToIndex(LoginData loginData, string clientnumber, int company_id) { return base.Channel.reassignToIndex(loginData, clientnumber, company_id); }

        public getTicketStatusResponseType getTicketStatus(LoginData loginData, string ticketID) { return base.Channel.getTicketStatus(loginData, ticketID); }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ebanzNewOrderResponse BAnzServicePortType.ebanzNewOrder(ebanzNewOrderRequest request) { return base.Channel.ebanzNewOrder(request); }

        public ebanzCheckOrderResponseType ebanzCheckOrder(LoginData loginData, ebanzPublicationOrderData orderData,
                                                           string clientnumber, OrderReplyInfo replyInfo) { return base.Channel.ebanzCheckOrder(loginData, orderData, clientnumber, replyInfo); }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ebanzChangeOrderResponse BAnzServicePortType.ebanzChangeOrder(ebanzChangeOrderRequest request) { return base.Channel.ebanzChangeOrder(request); }

        public ebanzListOrdersResponseType ebanzListOrders(LoginData loginData, OrderFilter orderFilter) { return base.Channel.ebanzListOrders(loginData, orderFilter); }

        public ebanzGetOrderResponseType ebanzGetOrder(LoginData loginData, string ordernumber) { return base.Channel.ebanzGetOrder(loginData, ordernumber); }

        public ebanzGetOrderStatusResponseType ebanzGetOrderStatus(LoginData loginData, string ordernumber) { return base.Channel.ebanzGetOrderStatus(loginData, ordernumber); }

        public ebanzGetReceiptResponseType ebanzGetReceipt(LoginData loginData, string ordernumber) { return base.Channel.ebanzGetReceipt(loginData, ordernumber); }

        public ebanzCancelOrderResponseType ebanzCancelOrder(LoginData loginData, string ordernumber) { return base.Channel.ebanzCancelOrder(loginData, ordernumber); }

        public queryCompanyIndexResponseType queryCompanyIndex(LoginData loginData, QueryCompanyIndexDataType queryData) { return base.Channel.queryCompanyIndex(loginData, queryData); }
        #endregion

        public createClientResponseType createClient(LoginData loginData, EbanzClient client, string ticketID) {
            var inValue = new createClientRequest();
            inValue.loginData = loginData;
            inValue.client = client;
            inValue.ticketID = ticketID;
            createClientResponse retVal = ((BAnzServicePortType) (this)).createClient(inValue);
            return retVal.@return;
            //Xop x;
        }

        public changeClientResponseType changeClient(LoginData loginData, string clientnumber, EbanzChangeClient client,
                                                     string ticketID) {
            var inValue = new changeClientRequest();
            inValue.loginData = loginData;
            inValue.clientnumber = clientnumber;
            inValue.client = client;
            inValue.ticketID = ticketID;
            changeClientResponse retVal = ((BAnzServicePortType) (this)).changeClient(inValue);
            return retVal.@return;
        }

        public listClientsResponseType listClients(LoginData loginData, string clientnumber, string company_name,
                                                   string sign) {
            var inValue = new listClientsRequest();
            inValue.loginData = loginData;
            inValue.clientnumber = clientnumber;
            inValue.company_name = company_name;
            inValue.sign = sign;
            listClientsResponse retVal = ((BAnzServicePortType) (this)).listClients(inValue);
            return retVal.@return;
        }

        public ebanzNewOrderResponseType ebanzNewOrder(LoginData loginData, ebanzPublicationOrderData orderData,
                                                       string clientnumber, string ticketID, OrderReplyInfo replyInfo) {
            var inValue = new ebanzNewOrderRequest();
            inValue.loginData = loginData;
            inValue.orderData = orderData;
            inValue.clientnumber = clientnumber;
            inValue.ticketID = ticketID;
            inValue.replyInfo = replyInfo;
            ebanzNewOrderResponse retVal = ((BAnzServicePortType) (this)).ebanzNewOrder(inValue);
            return retVal.@return;
        }

        public ebanzChangeOrderResponseType ebanzChangeOrder(LoginData loginData, ebanzPublicationOrderData orderData,
                                                             string ordernumber, string ticketID,
                                                             OrderReplyInfo replyInfo) {
            var inValue = new ebanzChangeOrderRequest();
            inValue.loginData = loginData;
            inValue.orderData = orderData;
            inValue.ordernumber = ordernumber;
            inValue.ticketID = ticketID;
            inValue.replyInfo = replyInfo;
            ebanzChangeOrderResponse retVal = ((BAnzServicePortType) (this)).ebanzChangeOrder(inValue);
            return retVal.@return;
        }
                                             }
}