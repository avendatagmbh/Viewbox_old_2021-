using System.Collections.Generic;
using System.Linq;
using System.Xml;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public abstract class TemplateGeneratorBase : TemplateBase {
        
        /// <summary>
        /// Create default nodes for xml(root), rows, columns
        /// </summary>
        protected TemplateGeneratorBase() {
            InitXmlWriting();
            CsvRowAssignment = new Dictionary<int, XmlNode>();
            CsvColumnAssignment = new Dictionary<int, XmlNode>();

            if (Cube != null) {
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

                InitHyperCubeHeaderDicts();
            }
        }

        
        protected Dictionary<int, XmlNode> CsvRowAssignment { get; set; }
        protected Dictionary<int, XmlNode> CsvColumnAssignment { get; set; }

        /// <summary>
        /// Adds an entry for a row to the xml.
        /// </summary>
        /// <param name="uid">The unique identifier for this row.</param>
        /// <param name="theirRowId">The position in the imported CSV data.</param>
        protected void AddXmlRowEntry(long uid, int theirRowId)
        {
            XmlNode myNode = XmlAssignmentDoc.CreateElement(XmlIdPrefix + uid);

            XmlAttribute myAttribute = XmlAssignmentDoc.CreateAttribute(XmlAttrName);
            myAttribute.InnerText = theirRowId.ToString();
            myNode.Attributes.Append(myAttribute);

            CsvRowAssignment.Add(theirRowId, myNode);
            XmlRowRoot.AppendChild(myNode);
        }

        /// <summary>
        /// Adds an entry for a column to the xml.
        /// </summary>
        /// <param name="uid">The unique identifier for this column.</param>
        /// <param name="theirColumnId">The position in the imported CSV data.</param>
        protected void AddXmlColumnEntry(long uid, int theirColumnId)
        {
            XmlNode myNode = XmlAssignmentDoc.CreateElement(XmlIdPrefix + uid);

            XmlAttribute myAttribute = XmlAssignmentDoc.CreateAttribute(XmlAttrName);
            myAttribute.InnerText = theirColumnId.ToString();
            myNode.Attributes.Append(myAttribute);

            CsvColumnAssignment.Add(theirColumnId, myNode);
            XmlColumnRoot.AppendChild(myNode);
        }


        public void LoadDictsForEntryModification(Dictionary<int, long> columnDict, Dictionary<int, long> rowDict, bool isInverse = false) {
            InitHyperCubeHeaderDicts();
            foreach (KeyValuePair<int, long> keyValuePair in rowDict) {
                if (isInverse) {
                    var entry = ColumnHeaders.First(x => x.Id == keyValuePair.Value);
                    AssignRow(keyValuePair.Key, entry);
                } else {
                    var entry = RowHeaders.First(x => x.Id == keyValuePair.Value);
                    AssignRow(keyValuePair.Key, entry);
                }
            }

            foreach (KeyValuePair<int, long> keyValuePair in columnDict) {
                if (isInverse) {
                    var entry = RowHeaders.First(x => x.Id == keyValuePair.Value);
                    AssignColumn(keyValuePair.Key, entry);
                } else {
                    var entry = ColumnHeaders.First(x => x.Id == keyValuePair.Value);
                    AssignColumn(keyValuePair.Key, entry);
                }
                
            }
        }
        
        /// <summary>
        /// Adds or updates an assignemnt of CSV data row number to HyperCube row header.
        /// </summary>
        /// <param name="theirRowId">Position in the CSV data.</param>
        /// <param name="header">Assignable HyperCube row header.</param>
        public void AssignRow(int theirRowId, HyperCubeHeader header) {

            if (RowHeaders.Count(x => x.CsvPosition == theirRowId) != 0) {
                UpdateRowAssignment(theirRowId, header);
            } else {
                AddXmlRowEntry(header.Id, theirRowId);
            }
                header.CsvPosition = theirRowId;
                header.AssignmentFlag = true;
        }
        

        /// <param name="rowNumber">The row number in CSV data that is allready assigned but has to be changed.</param>
        /// <param name="header">The new assigned HyperCube row header.</param>
        protected void UpdateRowAssignment(int rowNumber, HyperCubeHeader header)
        {
            RemoveRowAssignment(rowNumber);
            header.CsvPosition = rowNumber;
            AddXmlRowEntry(header.Id, rowNumber);
        }

        /// <summary>
        /// Adds or updates an assignemnt of CSV data column number to HyperCube column header.
        /// </summary>
        /// <param name="theirColumnId">Position in the CSV data.</param>
        /// <param name="header">Assignable HyperCube column header.</param>
        public void AssignColumn(int theirColumnId, HyperCubeHeader header) {
            var y = ColumnHeaders.Where(x => x.CsvPosition == theirColumnId);
            if (ColumnHeaders.Count(x => x.CsvPosition == theirColumnId) != 0) {
                UpdateColumnAssignment(theirColumnId, header);
            } else {
                AddXmlColumnEntry(header.Id, theirColumnId);
            }
                header.CsvPosition = theirColumnId;
                header.AssignmentFlag = true;
        }


        public void RemoveColumnAssignment(int theirColumnId) {

            ColumnHeaders.First(x => x.CsvPosition == theirColumnId).AssignmentFlag = false;
            ColumnHeaders.First(x => x.CsvPosition == theirColumnId).CsvPosition = 0;

            CsvColumnAssignment.Remove(theirColumnId);
        }

        public void RemoveRowAssignment(int theirRowNumber) {
            RowHeaders.First(x => x.CsvPosition == theirRowNumber).AssignmentFlag = false;
            CsvRowAssignment.Remove(theirRowNumber);
        }

        

        protected void UpdateColumnAssignment(int columnNumber, HyperCubeHeader header) {
            RemoveColumnAssignment(columnNumber);
            header.CsvPosition = columnNumber;
            AddXmlColumnEntry(header.Id, columnNumber);
        }


        /// <summary>
        /// Initialize the xml writing by adding defaulft nodes to the document
        /// </summary>
        protected void InitXmlWriting()
        {
            XmlAssignmentDoc.AppendChild(XmlRoot);

            XmlRoot.AppendChild(XmlColumnRoot);

            XmlRoot.AppendChild(XmlRowRoot);
        }


        public abstract void SaveXml();

        //XmlAssignmentDoc.Save(XmlSource);


/*
#region Testarea


        private string _s;
        private string uniqueIdentifier { get { return _s; } set { _s = "id" + value; } }
        public void TestGenerationOfXmlDemo()
        {

            InitXmlWriting();
            Config.ElementId = "de-gaap-ci_table.eqCh";

            var elementId = Config.ElementId;

            eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode root = null;
            foreach (eBalanceKitBusiness.IPresentationTree ptree in doc.GaapPresentationTrees.Values)
            {
                if (ptree.HasNode(elementId))
                {
                    root = ptree.GetNode(elementId) as eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;
                    break;
                }
            }

            if (cube != null)
            {
                var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
                int columncounter = 0;
                int rowcounter = 0;

                // get the column identifier
                foreach (var column in table.AllColumns)
                {
                    if (column == table.AllColumns.First())
                    {
                        continue;
                    }
                    uniqueIdentifier = column.DimensionValue.ElementId.ToString();
                    AddXmlColumnEntry(uniqueIdentifier, columncounter.ToString());
                    columncounter++;
                }

                // get the row identifier
                foreach (var row in table.AllRows)
                {
                    if (row == table.AllRows.First())
                    {

                    }
                    uniqueIdentifier = row.Dimension.ElementId.ToString();
                    AddXmlRowEntry(uniqueIdentifier, rowcounter.ToString());
                    rowcounter++;
                }
            }
            XmlAssignmentDoc.Save(XmlSource);
        }
#endregion
*/
    }
}
