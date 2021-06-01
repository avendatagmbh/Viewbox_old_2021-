using System.Collections.Generic;
using System.Linq;
using System.Xml;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public class TemplateGeneratorDb : TemplateGeneratorBase {

        public DbEntityHyperCubeImport Entry { get; set; }

        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        public string Name { get { return Entry.TemplateName; } set { Entry.TemplateName = value; } }

        /// <summary>
        /// Get or set the comment for the template.
        /// </summary>
        public string Comment { get { return Entry.Comment; } set { Entry.Comment = value; } }

        /// <summary>
        /// Get or set the content of the template (inside the root "xml").
        /// </summary>
        public string Content { get { return XmlRoot.InnerXml; } set { XmlRoot.InnerXml = value; } }

        /// <summary>
        /// Initialize an instance of the Generator.
        /// </summary>
        /// <param name="cube"></param>
        /// <param name="template">A DbEntityHyperCubeImport with at least an CubeElementId.</param>
        public TemplateGeneratorDb(IHyperCube cube, DbEntityHyperCubeImport template) { 
            Cube = cube;
            //Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            if (Cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyCu3d = Cube.Get3DCube(Cube.Dimensions.AllDimensionItems.ToList()[0],
                                             Cube.Dimensions.AllDimensionItems.ToList()[1],
                                             Cube.Dimensions.AllDimensionItems.ToList()[2]);
                Table = hyCu3d.Tables.First();
            }
            else {
                Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            }
            Entry = template; 
        }


        /// <summary>
        /// Initialize an instance of the Generator. 
        /// </summary>
        /// <param name="cube">The current HyperCube to get the CubeElementId.</param>
        /// <param name="templateName">Name for this template.</param>
        /// <param name="comment">A comment for this template.</param>
        public TemplateGeneratorDb(IHyperCube cube, string templateName = null, string comment = null) {
            Cube = cube;
            //Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            if (Cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyCu3d = Cube.Get3DCube(Cube.Dimensions.AllDimensionItems.ToList()[0],
                                             Cube.Dimensions.AllDimensionItems.ToList()[1],
                                             Cube.Dimensions.AllDimensionItems.ToList()[2]);
                Table = hyCu3d.Tables.First();
            }
            else {
                Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            }
            var cubeId = cube.Document.TaxonomyIdManager.GetId(cube.Root.Element.Id);
            Entry = new DbEntityHyperCubeImport {
                TaxonomytId = cube.TaxonomyId,
                CubeElementId = cubeId,
                TemplateName = templateName,
                Comment = comment,
                Delimiter = "\"",
                Encoding = System.Text.Encoding.Default,
                Seperator = ";"
            };

            InitHyperCubeHeaderDicts();
        }

        /// <summary>
        /// Initialize an instance of the Generator. 
        /// </summary>
        /// <param name="cube">The current HyperCube to get the CubeElementId.</param>
        /// <param name="csvFileName">Name of the csv file that worked as source for this template.</param>
        /// <param name="templateName">Name for this template.</param>
        /// <param name="comment">A comment for this template.</param>
        public TemplateGeneratorDb(IHyperCube cube, string csvFileName, string templateName = null, string comment = null) {
            Cube = cube;
            //Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            if (Cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyCu3d = Cube.Get3DCube(Cube.Dimensions.AllDimensionItems.ToList()[0],
                                             Cube.Dimensions.AllDimensionItems.ToList()[1],
                                             Cube.Dimensions.AllDimensionItems.ToList()[2]);
                Table = hyCu3d.Tables.First();
            }
            else {
                Table = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
            }
            var cubeId = cube.Document.TaxonomyIdManager.GetId(cube.Root.Element.Id);
            Entry = new DbEntityHyperCubeImport {
                TaxonomytId = cube.TaxonomyId,
                CubeElementId = cubeId,
                TemplateName = templateName,
                Comment = comment,
                Delimiter = "\"",
                Encoding = System.Text.Encoding.Default,
                Seperator = ";",
                FileName = csvFileName
            };

            InitHyperCubeHeaderDicts();
        }

        /// <summary>
        /// Initialize an instance of the Generator. 
        /// </summary>
        /// <param name="taxonomyId"></param>
        /// <param name="cubeElementId">The current CubeElementId.</param>
        /// <param name="templateName">Name for this template.</param>
        /// <param name="comment">A comment for this template.</param>
        public TemplateGeneratorDb(int taxonomyId, int cubeElementId, string templateName = null, string comment = null)
        {
            Entry = new DbEntityHyperCubeImport {
                TaxonomytId = taxonomyId,
                CubeElementId = cubeElementId,
                TemplateName = templateName,
                Comment = comment
            };
        }
        

        /// <summary>
        /// Saves the template with the given content to the database.
        /// </summary>
        /// <param name="content">The template content (xml assignment).</param>
        public void SaveXml(string content) { 
            Content = content;
            SaveXml();
        }

        /// <summary>
        /// Stores the template on the database.
        /// </summary>
        public override void SaveXml()
        {
            //XmlAssignmentDoc.RemoveAll();
            //InitXml();
            XmlRoot.RemoveAll();
            XmlRoot.InnerXml = string.Empty;
            XmlRowRoot.InnerXml = string.Empty;
            XmlColumnRoot.InnerXml = string.Empty;
            InitXmlWriting();
            foreach (var node in CsvRowAssignment.OrderBy(x => int.Parse(x.Value.Name.Substring(XmlIdPrefix.Length)))) {
                XmlRowRoot.AppendChild(node.Value);
            }

            foreach (var node in CsvColumnAssignment.OrderBy(x => int.Parse(x.Value.Name.Substring(XmlIdPrefix.Length)))) {
                XmlColumnRoot.AppendChild(node.Value);
            }


            using (var conn = eBalanceKitBusiness.Structures.AppConfig.ConnectionManager.GetConnection())
            {
                Entry.XmlAssignment = XmlAssignmentDoc.InnerXml;
                // ToDo Check if entries are valid
                
                var ex = conn.DbMapping.Load<DbEntityHyperCubeImport>(
                    conn.Enquote("template_id") + " = " + Entry.TemplateId);

                if (ex.Count == 0) {
                    Entry.Creator = eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser;
                    Entry.CreationDate = System.DateTime.UtcNow;
                } else {
                    Entry.LastModifier = eBalanceKitBusiness.Manager.UserManager.Instance.CurrentUser;
                    Entry.LastModified = System.DateTime.UtcNow;
                }

                conn.DbMapping.Save(Entry);
            }
        }
    }
}
