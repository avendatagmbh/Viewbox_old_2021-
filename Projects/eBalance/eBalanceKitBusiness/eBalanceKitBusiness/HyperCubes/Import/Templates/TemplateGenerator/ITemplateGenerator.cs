// --------------------------------------------------------------------------------
// author: ???
// since: 2012-02-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator {
    public interface ITemplateGenerator {

        void AddXmlRowEntry(string uid, int theirRowId);
        void AddXmlColumnEntry(string uid, int theirColumnId);
        void SaveXml();
    }
}