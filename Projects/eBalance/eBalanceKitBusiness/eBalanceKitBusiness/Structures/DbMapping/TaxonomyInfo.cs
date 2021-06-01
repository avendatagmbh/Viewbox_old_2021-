// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-10
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using Taxonomy.Enums;
using Taxonomy.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping {

    [DbTable("taxonomy_info", ForceInnoDb = true)]
    public class TaxonomyInfo : ITaxonomyInfo {

        #region TaxonomyInfo
        private readonly Taxonomy.TaxonomyInfo _taxonomyInfo = new Taxonomy.TaxonomyInfo();
        public ITaxonomyInfo BaseTaxonomyInfo { get { return _taxonomyInfo; } }
        #endregion TaxonomyInfo

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal int Id { get; private set; }
        #endregion

        #region Name
        [DbColumn("name", AllowDbNull = false, Length = 128)]
        [DbIndex("idx__taxonomy_info__name")]
        public string Name { get { return _taxonomyInfo.Name; } internal set { _taxonomyInfo.Name = value; } }
        #endregion

        #region Path
        [DbColumn("path", AllowDbNull = false, Length = 4096)]
        public string Path { get { return _taxonomyInfo.Path; } internal set { _taxonomyInfo.Path = value; } }
        #endregion

        #region Filename
        [DbColumn("filename", AllowDbNull = false, Length = 1024)]
        public string Filename { get { return _taxonomyInfo.Filename; } internal set { _taxonomyInfo.Filename = value; } }
        #endregion

        #region TaxonomyType
        [DbColumn("type", AllowDbNull = false)]
        public TaxonomyType Type { get { return _taxonomyInfo.Type; } internal set { _taxonomyInfo.Type = value; } }
        #endregion

        #region Version
        [DbColumn("version", AllowDbNull = false)]
        public string Version { get { return _taxonomyInfo.Version; } internal set { _taxonomyInfo.Version = value; } }
        #endregion

        public override string ToString() { return Type + " (" + Name + ")"; }
    }
}