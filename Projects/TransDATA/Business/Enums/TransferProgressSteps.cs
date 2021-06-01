// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Business.Enums {
    public enum TransferProgressSteps {
        ExportingTables,
        GeneratingIndexXml,
        ExportingErrorTables,
        CreateDocumentation,
        WaitForConnection
    }
}