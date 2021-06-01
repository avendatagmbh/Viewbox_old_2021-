using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using iTextSharp.text;
using Document = eBalanceKitBusiness.Structures.DbMapping.Document;

namespace eBalanceKitBusiness.HyperCubes.Import {
    //public class ChangesEquityStatementImporter {

    //    /*
    //     * 0. Check number of rows + columns
    //     * 1. check if columns contain header
    //     * 2. check if rows contain header
    //     * 3. check if value types are valid
    //     * 4. check logical errors (value where not allowed because of value in other cell)
    //     */

    //    public ChangesEquityStatementImporter(string filename, Document document) {
    //        CsvReader = new CsvReader(filename);
    //        Doc = document;
    //    }

    //    private DataRow headerRow;
    //    private DataColumn headerColumn;

    //    /// <summary>
    //    /// Checks if the number of rows and columns is valid.
    //    /// If necessary it removes the header row and column and delivers a value only DataTable.
    //    /// If necessary it adds empty rows and columns.
    //    /// </summary>
    //    /// <param name="data">The DataTable that should be checked.</param>
    //    /// <returns>Is the DataTable valid?</returns>
    //    public bool CheckCount(DataTable data) {
    //        //bool res = true;

    //        if (data.Rows.Count > RowCounter + 1 ||
    //            Math.BigMul(data.Columns.Count, DimensionCounter) > ColumnCounter + 1 ||
    //            data.Rows.Count <= 0 ||
    //            data.Columns.Count <= 0) {
    //            return false;
    //        }

    //        if (data.Rows.Count > RowCounter) {
    //            headerRow = data.Rows[0];
    //            data.Rows.RemoveAt(0);
    //        }
            
    //        if (Math.BigMul(data.Columns.Count, DimensionCounter) > ColumnCounter) {
    //            headerColumn = data.Columns[0];
    //            data.Columns.RemoveAt(0);
    //        }
            

    //        while (data.Rows.Count < RowCounter) {
    //            data.Rows.Add(new object());
    //        }

    //        while (data.Columns.Count < ColumnCounter) {
    //            data.Columns.Add();
    //        }

    //        return true;
    //    }

    //    /// <summary>
    //    /// Contains Row|Column|Value in CSV|auto calculated Value for "Value in not allowed cell" warnings
    //    /// </summary>
    //    private HashSet<String> _warningCollection = new HashSet<string>();

    //    public HashSet<String> ImportWarnings { get { return _warningCollection; } }


    //    public void DoItReverse(IPresentationTreeNode node, DataTable csvData) {

    //        var cube = Doc.GetHyperCube(node.Element.Id);
            
    //        if (cube != null) {

    //            var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());
                
    //            RowCounter = table.AllRows.Count();
    //            ColumnCounter = table.AllColumns.Count();
    //            //ColumnCounter = Convert.ToInt32(Math.BigMul(table.AllColumns.Count(), cube.Dimensions.AllDimensionItems.Count())); // cube.Dimensions.DimensionItems.Count()));
                
    //            if (!CheckCount(csvData)) {
    //                throw new Exception("oops, die Datei scheint nicht konform zu sein");
    //            }

    //            var reverseItems = cube.Items.Items.Reverse();
    //            //reverseItems.ToList()[]
    //            int limit = (cube.Dimensions.AllDimensionItems.Count() > 2)
    //                            ? cube.Dimensions.AllDimensionItems.First().Values.Count()
    //                            : 2;
    //            // works for 2D cubes
    //            int counter = cube.Items.Items.Count() - table.AllColumns.Count();
    //            //int counter = cube.Items.Items.Count() / cube.Dimensions.AllDimensionItems.Count() - (csvData.Columns.Count);
    //            int skiper = Convert.ToInt32(Math.BigMul(table.AllRows.Count(), table.AllColumns.Count()));
    //            bool skipFirstDimensionElement = true;
    //            int dataRowCounter = csvData.Rows.Count - 1;

    //            //foreach (var row in table.AllRows) {
    //            while (dataRowCounter >= 0) {
    //                for (int columnIterater = (csvData.Columns.Count - 1); columnIterater >= 0; columnIterater--) {

    //                    for (int k = limit - 1; k >= 0; k--) {
                            
    //                        if (skipFirstDimensionElement && k == 1) {
    //                            continue;
    //                        }
                            
    //                        if (columnIterater + counter + Math.BigMul(skiper, k) < 0)
    //                            break;

    //                        //item.Value = csvData.Rows[dataRowCounter][columnIterater].ToString();
    //                        //_csvContent.Append(";" + cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).Value);
    //                        //if (cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).IsEditable) {
    //                            var val = csvData.Rows[dataRowCounter][columnIterater].ToString();
    //                            //csvData.[dataRowCounter][columnIterater]
    //                            var oldVal =
    //                                cube.Items.GetItemByID(
    //                                    Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).Value;
    //                            bool stringEqual = false;
    //                            if (oldVal != null) stringEqual = val.ToString().Equals(oldVal.ToString());
    //                        if (cube.Items.GetItemByID(
    //                                Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).IsEditable) {
    //                            if (val != null && !stringEqual) {
    //                                cube.Items.GetItemByID(
    //                                    Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k)))
    //                                    .Value = val;
    //                            }
    //                        } else {
    //                            //if (oldVal != null && val != String.Empty) {
    //                            if(val != null && val != String.Empty && !val.Equals(oldVal)) {
    //                                // Warning: Value in not allowed cell
    //                                _warningCollection.Add(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k)) + "|" + 
    //                                    //cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).IsLocked.ToString() +  "|" +  
    //                                    //cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).IsComputed.ToString() +  "|" +
    //                                    table.AllRows.ToList()[dataRowCounter].Header + "|" + 
    //                                    table.AllColumns.ToList()[columnIterater].Header + "|" + 
    //                                    //dataRowCounter + "|" + 
    //                                    //columnIterater + "|" + 
    //                                    val);
                                    
    //                            }
                                
    //                        }
    //                    }
    //                }

    //                counter -= table.AllColumns.Count();
    //                if(counter < 0) counter = 0;
    //                dataRowCounter--;

    //                /*
    //                if (item.Value.GetType().Equals(Taxonomy.Enums.XbrlElementValueTypes.Monetary)) {
    //                    item.Value = "test";
    //                }
    //                */
    //            }

    //            // Check Warinings again
    //            HashSet<String> temp = new HashSet<String>();
    //            cube = Doc.GetHyperCube(node.Element.Id);
    //            foreach (var warnMsg in _warningCollection) {
    //                var wrn = warnMsg.Split('|');
    //                var id = Convert.ToInt32(wrn[0]);
    //                var newVal = cube.Items.GetItemByID(id).Value.ToString();
    //                newVal = newVal.EndsWith(",00") ? newVal.Substring(0, newVal.Length - 3) : newVal;
    //                var autoVal = wrn[wrn.Count() - 1];

    //                if (!newVal.Equals(autoVal))
    //                    temp.Add(wrn[1] + "|" + wrn[2] + "," + newVal + "," + autoVal);
    //                    //_warningCollection.Remove(warnMsg);
    //            }
    //            _warningCollection = temp;

    //        }
    //    }

    //    public  void DoIt(IPresentationTreeNode node, DataTable csvData) {

    //        var cube = Doc.GetHyperCube(node.Element.Id);


    //        if (cube != null) {
    //            var table = cube.GetTable(cube.Dimensions.Primary, cube.Dimensions.DimensionItems.Last());

    //            RowCounter = table.AllRows.Count();
    //            ColumnCounter = table.AllColumns.Count();


    //            if (!CheckCount(csvData)) {
    //                throw new Exception("oops");
    //            }


    //            HashSet<Type> col;
    //            var tmpNode = node.Children;
    //            /*
    //            while (tmpNode.) {
                    
    //            }
    //            */

    //            // IEnumerable<IHyperCubeItem> reverseCubeItems = cube.Items.Items.Reverse();

    //            //foreach (var item in  cube.Items.Items.Reverse()) {

    //            //node.Element.ValueType
    //            //node.PresentationTree.

    //            /*
    //            //item.Value = "test";
    //            object typ = item.Value.GetType();
                
                    

    //            switch (typ) {
    //                case Taxonomy.Enums.XbrlElementValueTypes.String:
    //                    item.Value = "test";
    //                    break;
    //                case Taxonomy.Enums.XbrlElementValueTypes.Monetary:
    //                    item.Value = "0.00";
    //                    break;
    //            }
    //            */

    //            //item.Value = 

    //            int limit = (cube.Dimensions.AllDimensionItems.Count() > 2) ? cube.Dimensions.AllDimensionItems.First().Values.Count() : 2;
    //            int counter = 0;
    //            int skiper = Convert.ToInt32(Math.BigMul(table.AllRows.Count(), table.AllColumns.Count()));
    //            bool skipFirstDimensionElement = true;
    //            int dataRowCounter = 0;

    //            foreach (var row in table.AllRows) {
    //                var row1 = csvData.Rows[dataRowCounter];

    //                for (int columnIterater = 0; columnIterater < (table.AllColumns.Count()); columnIterater++) {
    //                    /*
    //                    if (i == 0) {
    //                        _csvContent.AppendLine();

    //                        _csvContent.Append(row.Header);
    //                    }
    //                    */
    //                    for (int k = 0; k < limit; k++) {

    //                        if (skipFirstDimensionElement && k == 1) {
    //                            continue;
    //                        }
    //                        //item.Value = csvData.Rows[dataRowCounter][columnIterater].ToString();
    //                        //_csvContent.Append(";" + cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).Value);
    //                        if (cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k))).IsEditable) {
                              
    //                        cube.Items.GetItemByID(Convert.ToInt32(columnIterater + counter + Math.BigMul(skiper, k)))
    //                            .Value = csvData.Rows[dataRowCounter][columnIterater].ToString();
  
    //                        }
    //                    }
    //                }

    //                counter += table.AllColumns.Count();
    //                dataRowCounter++;

    //                /*
    //                if (item.Value.GetType().Equals(Taxonomy.Enums.XbrlElementValueTypes.Monetary)) {
    //                    item.Value = "test";
    //                }
    //                */
    //            }


    //            //cube.Items.Items.

    //        }

    //    }

    //    public void Import(string elementId) {
    //        CsvReader.Separator = ';';
    //        DataTable csvData = CsvReader.GetCsvData(0, Encoding.Default);
    //        //csvData.

    //        //IPresentationTreeNode node = new PresentationTreeNode(new PresentationTree(), new PresentationTreeEntry());
    //        //node.Element
            

    //        //IElement elem = new 
    //        //IPresentationTreeNode d = new PresentationTreeNode();
    //        //IPresentationTree tree = null;
    //        //PresentationTree t = new PresentationTree();
    //        //t.n

    //        //tree.
    //        //csvData.Rows.
            
    //        foreach (IPresentationTree ptree in Doc.GaapPresentationTrees.Values) {
    //            if (ptree.HasNode(elementId)) {
    //                IPresentationTreeNode node = ptree.GetNode(elementId) as IPresentationTreeNode;
    //                DoItReverse(node, csvData);
    //                break;
    //            }
    //        }

    //        if (ImportWarnings.Count > 0) {
    //            throw new Exception(String.Join(Environment.NewLine, ImportWarnings.ToList()));
    //        }
            
    //        //PresentationTreeEntry entry = new PresentationTreeEntry();
    //        //entry.Element.
    //        //entry.Element = 
    //        //Taxonomy.PresentationTree.PresentationTreeEntry element = new PresentationTreeEntry().Element

    //        //node.AddChildren(entry);

    //        //HyperCube cube = new HyperCube(Doc, node);

    //        //HyperCubeItemCollection col = new HyperCubeItemCollection(cube);
            
            

    //        //cube.Items.Items

    //        //cube.

    //    }


    //    public CsvReader CsvReader { get; set; }
    //    public Document Doc { get; set; }
    //    private const int DimensionCounter = 1;
    //    private int RowCounter = 15;
    //    private int ColumnCounter = 10;

    //}
}
