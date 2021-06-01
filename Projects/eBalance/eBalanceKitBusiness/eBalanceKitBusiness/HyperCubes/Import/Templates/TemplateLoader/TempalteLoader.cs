using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    /// <summary>
    /// Base class for loading the xml configuration for assignments CSV - HyperCube
    /// </summary>
    public abstract class TempalteLoader : TemplateBase
    {
        /// <summary>
        /// Methode that loads the XML to the XmlAssignmentDoc.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Loads the Dictionary with assignments csv column - HyperCube column id.
        /// </summary>
        /// <returns>Dictionary with assignments (columnId, dimensionId).</returns>
        public Dictionary<int, long> LoadColumnDict()
        {
            long dimensionId;
            int columnId;
            // load the nodes with the information for columns
            var e = XmlAssignmentDoc.GetElementsByTagName(XmlColumnRoot.Name);
            Dictionary<int, long> columnDict = new Dictionary<int, long>();
            // there should be only one "column" entry
            foreach (XmlNode node in e)
            {
                // for each column entry
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    // remove all non numeric symbols
                    var dimID = Regex.Replace(childNode.Name, @"[^\d]", string.Empty);
                    var colId = childNode.Attributes[XmlAttrName].InnerText;

                    if (long.TryParse(dimID, out dimensionId) && int.TryParse(colId, out columnId))
                    {
                        columnDict.Add(columnId, dimensionId);
                    }
                }
            }
            return columnDict;
        }


        /// <summary>
        /// Loads the Dictionary with assignments csv row - HyperCube row id.
        /// </summary>
        /// <returns>Dictionary with assignments (rowId, dimensionId).</returns>
        public Dictionary<int, long> LoadRowDict()
        {
            long dimensionId;
            int rowId;
            // load the nodes for Rows
            var e = XmlAssignmentDoc.GetElementsByTagName(XmlRowRoot.Name);
            Dictionary<int, long> rowDict = new Dictionary<int, long>();
            //
            foreach (XmlNode node in e)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    // remove all non numeric symbols
                    var dimID = Regex.Replace(childNode.Name, @"[^\d]", string.Empty); // actually it should be the same like childNode.Name.Replace("id", string.Empty);
                    if (childNode.Attributes.Count > 0)
                    {
                        var readRowId = childNode.Attributes[XmlAttrName].InnerText;
                        if (long.TryParse(dimID, out dimensionId) && int.TryParse(readRowId, out rowId))
                        {
                            rowDict.Add(rowId, dimensionId);
                        }
                    }
                }
            }
            return rowDict;
        }
    }
}
