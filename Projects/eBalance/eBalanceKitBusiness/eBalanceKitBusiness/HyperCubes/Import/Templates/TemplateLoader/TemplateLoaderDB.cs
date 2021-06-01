using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using Utils;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    public class TemplateLoaderDB : TempalteLoader, ILoader
    {

        
        #region properties

        public IEnumerable<string> TemplateNames { get { return (from entry in DbEntities select entry.TemplateName); } }

        private DbEntityHyperCubeImport _selectedTemplate;
        public DbEntityHyperCubeImport SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                if (_selectedTemplate != value) {
                    _selectedTemplate = value;
                    if(value != null)
                    XmlAssignmentDoc.LoadXml(value.XmlAssignment);
                    OnPropertyChanged("SelectedTemplate");
                }
            }
        }
        #endregion

        #region vars

        #region TemplatesAvailable
        private bool _templatesAvailable;

        public bool TemplatesAvailable {
            get { return _templatesAvailable; }
            set {
                if (_templatesAvailable != value) {
                    _templatesAvailable = value;
                    OnPropertyChanged("TemplatesAvailable");
                }
            }
        }
        #endregion TemplatesAvailable

        #region dbEntities
        private ObservableCollectionAsync<DbEntityHyperCubeImport> _dbEntities;

        public ObservableCollectionAsync<DbEntityHyperCubeImport> DbEntities {
            get { return _dbEntities; }
            set {
                if (_dbEntities != value) {
                    _dbEntities = value;
                    OnPropertyChanged("DbEntities");
                }
            }
        }
        #endregion dbEntities

        #endregion
        
        #region constructor

        public TemplateLoaderDB(IHyperCube cube)
        {
            Cube = cube;
            //Table = cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());

            if (cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyCu3d = cube.Get3DCube(cube.Dimensions.AllDimensionItems.ToList()[0],
                                             cube.Dimensions.AllDimensionItems.ToList()[1],
                                             cube.Dimensions.AllDimensionItems.ToList()[2]);
                Table = hyCu3d.Tables.First();
            }
            else {
                Table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
            }

            using (var conn = AppConfig.ConnectionManager.GetConnection())
            {
                conn.DbMapping.CreateTableIfNotExists<DbEntityHyperCubeImport>();
                var cubeId = cube.Document.TaxonomyIdManager.GetId(cube.Root.Element.Id);

                DbEntities = new ObservableCollectionAsync<DbEntityHyperCubeImport>();
                var entries = conn.DbMapping.Load<DbEntityHyperCubeImport>(
                    conn.Enquote("taxonomy_id") + " = " + cube.TaxonomyId + " AND " + conn.Enquote("cube_element_id") + " = " + cubeId);

                foreach (var entry in entries) {
                    DbEntities.Add(entry);
                }
            }
            //TemplatesAvailable = dbEntities.Count == 0
        }
        /*
        public TemplateLoaderDB(Document doc, string elementId)
        {
            using (var conn = AppConfig.ConnectionManager.GetConnection())
            {

                conn.DbMapping.CreateTableIfNotExists<DbEntityHyperCubeImport>();
                var cubeId = doc.TaxonomyIdManager.GetId(elementId);

                DbEntities = conn.DbMapping.Load<DbEntityHyperCubeImport>(
                    conn.Enquote("taxonomy_id") + " = " + ((TaxonomyInfo)doc.MainTaxonomyInfo).Id + " AND " + conn.Enquote("cube_element_id") + " = " + cubeId);

            }
        }
        */
        #endregion

        #region public

        /// <summary>
        /// Loads the xml for the specified template to XmlAssignmentDoc.
        /// </summary>
        /// <param name="id">Position of the template in TemplateNames.</param>
        public void Load(int id) {
            SelectedTemplate = DbEntities[id];
            //XmlAssignmentDoc.LoadXml(DbEntities[id].XmlAssignment);
        }

        /// <summary>
        /// Loads the xml for the specified template to XmlAssignmentDoc.
        /// </summary>
        /// <param name="templateName">Name of the template in TemplateNames.</param>
        public void Load(string templateName) {
            SelectedTemplate = GetAccordingTemplate(templateName);
            //XmlAssignmentDoc.LoadXml(SelectedTemplate.XmlAssignment);
        }

        /// <summary>
        /// Loads the xml of the first template in TemplateNames to XmlAssignmentDoc.
        /// </summary>
        public override void Load() {
            SelectedTemplate = DbEntities.First();
            //XmlAssignmentDoc.LoadXml(SelectedTemplate.XmlAssignment);
        }

        public void Load(DbEntityHyperCubeImport entry) { 
            SelectedTemplate = entry;
            //XmlAssignmentDoc.LoadXml(SelectedTemplate.XmlAssignment);
        }

        /// <summary>
        /// Removes the database entry of the given Template
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        public void DeleteTemplate(string templateName) {
            
            using (var conn = AppConfig.ConnectionManager.GetConnection())
            {
                conn.DbMapping.Delete<DbEntityHyperCubeImport>(
                    conn.Enquote("template_id") + " = " + GetAccordingTemplate(templateName).TemplateId);
            }
        }

        /// <summary>
        /// Removes the database entry of the given Template
        /// </summary>
        /// <param name="entity">The template that should be deleted.</param>
        public void DeleteTemplate(DbEntityHyperCubeImport entity) {
            
            using (var conn = AppConfig.ConnectionManager.GetConnection())
            {
                conn.DbMapping.Delete<DbEntityHyperCubeImport>(
                    conn.Enquote("template_id") + " = " + entity.TemplateId);
            }
        }

        /// <summary>
        /// Removes the database entry of the current selected Template
        /// </summary>
        public void DeleteSelectedTemplate() {
            
            using (var conn = AppConfig.ConnectionManager.GetConnection())
            {
                conn.DbMapping.Delete<DbEntityHyperCubeImport>(
                    conn.Enquote("template_id") + " = " + SelectedTemplate.TemplateId);
            }
            DbEntities.Remove(SelectedTemplate);
        }

        #endregion

        private void LoadOverview() {
            
        }

        #region private

        /// <summary>
        /// Gives the first matching template for the specified template name.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The first matching DbEntity.</returns>
        private DbEntityHyperCubeImport GetAccordingTemplate(string templateName) { return (from entry in DbEntities where entry.TemplateName.Equals(templateName) select entry).First(); }

        #endregion
    }
}
