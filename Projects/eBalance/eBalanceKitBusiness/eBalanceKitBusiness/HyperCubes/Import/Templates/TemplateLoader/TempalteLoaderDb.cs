// --------------------------------------------------------------------------------
// author: $Author: $
// since: 2012-02-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.HyperCubes.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    public class TempalteLoaderDb : TempalteLoaderString
    {

        public TempalteLoaderDb(DbEntityHyperCubeImport dbEntity)
            : base(dbEntity.XmlAssignment)
        {
        }


        private static string GetAssignment(DbEntityHyperCubeImport dbEntity)
        {
            return dbEntity.XmlAssignment;
        }

        
    }
}
