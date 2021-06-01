using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Xml;
using System.IO;

namespace ABAP_Clusterparser {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private List<Cluster_Structure> Structures { get; set; }
        private const string SEPERATOR = "<<DIV>>";
        private const string ENDOFLINE = "<<EOL>>";
        private const string IMPORTBUFFER = "importbuffer";
        private const string TABLE = "TABLE_NAME";
        private const string COLUMN_NAME_DATA = "COLUMN_NAME_DATA";
        private const string COLUMN_NAME_CLUSTER_STRUCTURE = "COLUMN_NAME_CLUSTER_STRUCTURE";
        private const string EXPORT_PATH = "EXPORT_PATH";
        private const string FIELD_CLUST_ID = "<relid>";
        private const string FIELD_DATA = "<raw>";
        private const string FIELD_WORK = "<wa>";

        public MainWindow() {
            InitializeComponent();
            //tbFile.Text = @"N:\mag\Cluster.xml";
            tbFile.Text = @"N:\mag\Cluster - Test.xml";
            tbFileOutput.Text = @"N:\mag\result.txt";
            Structures = new List<Cluster_Structure>();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(tbFile.Text);

            foreach (XmlNode node in xmlDocument.ChildNodes) {
                if (node.Name == "cluster") {
                    foreach (XmlNode childNode in node.ChildNodes) {
                        if (childNode.Name == "structure") {
                            ReadStructureNode(childNode);
                        } else {
                            throw new NotImplementedException(childNode.Name);
                        }
                    }
                }
            }

            WriteABAP_File(tbFileOutput.Text);

            Test1();

            MessageBox.Show("Finished");
        }

        private void Test1() { 
            var dic = new Dictionary<string, List<Cluster_Sub_Structure>>();
            foreach (var clusterStructure in Structures) {
                foreach (var clusterSubStructure in clusterStructure.SubStructures) {
                    if(!dic.ContainsKey(clusterSubStructure.Name.ToUpper())) dic.Add(clusterSubStructure.Name.ToUpper(), new List<Cluster_Sub_Structure>());
                    dic[clusterSubStructure.Name.ToUpper()].Add(clusterSubStructure);
                }
            }

            var include = string.Empty;
            var addition = string.Empty;
            int occurs = -1;

            int counter;
            foreach (var keyPair in dic) {
                if (keyPair.Value.Count == 1) continue;
                counter = 0;
                foreach (var clusterSubStructure in keyPair.Value) {
                    if(counter == 0) {
                        include = clusterSubStructure.Table;
                        occurs = clusterSubStructure.Occurs;
                        addition = clusterSubStructure.Addition;
                        counter++;
                    } else {
                        if (clusterSubStructure.Table != include) throw new NotImplementedException();
                        if (clusterSubStructure.Occurs != occurs) throw new NotImplementedException();
                        if (clusterSubStructure.Addition != addition) throw new NotImplementedException();
                    }
                }
            }
        }

        private void WriteABAP_File(string p) {
            using (var writer = new StreamWriter(tbFileOutput.Text)) {
                var fileDefinition = GetFileDefinitions();
                var fileDeclaration = GetFileDeclaration();
                var fileOpenDeclaration = GetFileOpenDeclaration();
                var fileCloseDeclaration = GetFileCloseDeclaration();
                var dataDefinition = GetDataDefinition();
                var extractionForms = GetExtractionForms();
                var callHeaderForms = GetCallHeaderForms();
                var headerForms = GetHeaderForms();
                var importExportStatement = GetImportStatementsWithPerformExport();

                writer.WriteLine("*******************");
                writer.WriteLine("* DATA DEFINITION *");
                writer.WriteLine("*******************");
                writer.WriteLine(string.Empty);
                writer.WriteLine(dataDefinition);
                writer.WriteLine(fileDefinition);

                writer.WriteLine("*******************************");
                writer.WriteLine("* DECLARATION OF EXPORT-FILES *");
                writer.WriteLine("*******************************");
                writer.WriteLine(string.Empty);
                writer.WriteLine(fileDeclaration);

                writer.WriteLine("*********************");
                writer.WriteLine("* OPEN EXPORT-FILES *");
                writer.WriteLine("*********************");
                writer.WriteLine(string.Empty);
                writer.WriteLine(fileOpenDeclaration);

                writer.WriteLine("**********************************");
                writer.WriteLine("* WRITING HEADER TO EXPORT-FILES *");
                writer.WriteLine("**********************************");
                writer.WriteLine(string.Empty);
                writer.WriteLine(callHeaderForms);

                writer.WriteLine(importExportStatement);
                writer.WriteLine(fileCloseDeclaration);

                writer.WriteLine("ENDFUNCTION.");

                writer.WriteLine(headerForms);
                writer.WriteLine(extractionForms);
            }
        }

        private string GetCallHeaderForms() {
            var sbCallHeaderForms = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbCallHeaderForms.Append("PERFORM HEADER_" + clusterSubStructure.Name + " USING filename_" + clusterSubStructure.Name + "." + System.Environment.NewLine);
            }
            return sbCallHeaderForms.ToString();
        }

        private string GetFileCloseDeclaration() {
            var sbFileCloseDeclaration = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbFileCloseDeclaration.Append("CLOSE DATASET filename_" + clusterSubStructure.Name + "." + System.Environment.NewLine);
            }
            return sbFileCloseDeclaration.ToString();
        }

        private string GetFileOpenDeclaration() {
            var sbFileOpenDeclaration = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbFileOpenDeclaration.Append("OPEN DATASET filename_" + clusterSubStructure.Name + " FOR OUTPUT IN TEXT MODE ENCODING DEFAULT." + System.Environment.NewLine);
            }
            return sbFileOpenDeclaration.ToString();
        }

        private string GetFileDeclaration() {
            var sbFileDeclaration = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbFileDeclaration.Append("CONCATENATE " + EXPORT_PATH + " " + TABLE + " '_' " + COLUMN_NAME_DATA +  " '___" +
                                         clusterSubStructure.Name + ".CSV' INTO " + "filename_" + clusterSubStructure.Name +
                                         "." + System.Environment.NewLine);
            }
            return sbFileDeclaration.ToString();
        }

        private string GetFileDefinitions() {
            var sbFileDefinitions = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                if (sbFileDefinitions.Length > 0) sbFileDefinitions.Append("," + System.Environment.NewLine);
                sbFileDefinitions.Append("filename_" + clusterSubStructure.Name + " TYPE string");
            }

            return "DATA:" + System.Environment.NewLine + sbFileDefinitions + "." + System.Environment.NewLine;
        }

        private string GetImportStatementsWithPerformExport() {
            var sbDeclaration = new StringBuilder();

            sbDeclaration.Append("* Datentypen" + System.Environment.NewLine);
            sbDeclaration.Append("  TYPES:" + System.Environment.NewLine);
            sbDeclaration.Append("*   Tabellefeld (Position, Wert)" + System.Environment.NewLine);
            sbDeclaration.Append("    BEGIN OF st_values," + System.Environment.NewLine);
            sbDeclaration.Append("      id TYPE i," + System.Environment.NewLine);
            sbDeclaration.Append("      value TYPE string," + System.Environment.NewLine);
            sbDeclaration.Append("    END OF st_values," + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("*   Liste aller Tabellenfelder" + System.Environment.NewLine);
            sbDeclaration.Append("    tt_values TYPE STANDARD TABLE" + System.Environment.NewLine);
            sbDeclaration.Append("      OF st_values" + System.Environment.NewLine);
            sbDeclaration.Append("      WITH KEY id," + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("*   Liste aller Tabellenfeld-Strukturen" + System.Environment.NewLine);
            sbDeclaration.Append("    tt_dfies TYPE STANDARD TABLE" + System.Environment.NewLine);
            sbDeclaration.Append("      OF dfies" + System.Environment.NewLine);
            sbDeclaration.Append("      WITH KEY position," + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("*   Datenbanktabelle" + System.Environment.NewLine);
            sbDeclaration.Append("    BEGIN OF st_tabledefinition," + System.Environment.NewLine);
            sbDeclaration.Append("      name(128) TYPE c," + System.Environment.NewLine);
            sbDeclaration.Append("      type TYPE dd02v-tabclass," + System.Environment.NewLine);
            sbDeclaration.Append("      field TYPE tt_dfies," + System.Environment.NewLine);
            sbDeclaration.Append("      data TYPE tt_values," + System.Environment.NewLine);
            sbDeclaration.Append("    END OF st_tabledefinition," + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("    tt_tabledefinition TYPE STANDARD TABLE" + System.Environment.NewLine);
            sbDeclaration.Append("      OF st_tabledefinition" + System.Environment.NewLine);
            sbDeclaration.Append("      WITH NON-UNIQUE KEY name." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("DATA: BEGIN OF work, BUFFER(65500), END OF work." + System.Environment.NewLine);
            sbDeclaration.Append("DATA: dbcursor TYPE cursor." + System.Environment.NewLine);
            sbDeclaration.Append("FIELD-SYMBOLS: " + FIELD_WORK + " TYPE ANY, " + FIELD_DATA + " TYPE ANY, " + FIELD_CLUST_ID + " TYPE ANY." + System.Environment.NewLine);
            sbDeclaration.Append("ASSIGN work TO " + FIELD_WORK + " CASTING TYPE (" + TABLE + ")." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("DATA: lt_TableDefinition TYPE tt_TableDefinition," + System.Environment.NewLine);
            sbDeclaration.Append("      ls_TableDefinition TYPE st_TableDefinition," + System.Environment.NewLine);
            sbDeclaration.Append("      ls_field TYPE DFIES." + System.Environment.NewLine + System.Environment.NewLine);
            sbDeclaration.Append("DATA: " + IMPORTBUFFER + " TYPE xstring." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("DATA ls_table_structure TYPE DFIES." + System.Environment.NewLine);
            sbDeclaration.Append("DATA table_type TYPE DD02V-TABCLASS." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("* Tabellenstruktur auslesen" + System.Environment.NewLine);
            sbDeclaration.Append("DATA BEGIN OF table_structure OCCURS 10." + System.Environment.NewLine);
            sbDeclaration.Append("  INCLUDE STRUCTURE DFIES." + System.Environment.NewLine);
            sbDeclaration.Append("DATA END OF table_structure." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("CALL FUNCTION 'DDIF_FIELDINFO_GET'" + System.Environment.NewLine);
            sbDeclaration.Append("  EXPORTING" + System.Environment.NewLine);
            sbDeclaration.Append("    TABNAME              = TABLE_NAME" + System.Environment.NewLine);
            sbDeclaration.Append("  IMPORTING" + System.Environment.NewLine);
            sbDeclaration.Append("    DDOBJTYPE            = table_type" + System.Environment.NewLine);
            sbDeclaration.Append("  TABLES" + System.Environment.NewLine);
            sbDeclaration.Append("    DFIES_TAB            = table_structure" + System.Environment.NewLine);
            sbDeclaration.Append("  EXCEPTIONS" + System.Environment.NewLine);
            sbDeclaration.Append("    NOT_FOUND            = 1" + System.Environment.NewLine);
            sbDeclaration.Append("    INTERNAL_ERROR       = 2" + System.Environment.NewLine);
            sbDeclaration.Append("    OTHERS               = 3." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("IF SY-SUBRC <> 0." + System.Environment.NewLine);
            sbDeclaration.Append("  RAISE ERROR_FILLING_TABLEDEFS." + System.Environment.NewLine);
            sbDeclaration.Append("ENDIF." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("CLEAR ls_TableDefinition." + System.Environment.NewLine);
            sbDeclaration.Append("ls_TableDefinition-name = TABLE_NAME." + System.Environment.NewLine);
            sbDeclaration.Append("ls_TableDefinition-type = table_type." + System.Environment.NewLine + System.Environment.NewLine);

            sbDeclaration.Append("* Felder in Tabellenstruktur hinzufügen und Csv-Kopfzeile schreiben" + System.Environment.NewLine);
            sbDeclaration.Append("LOOP AT table_structure INTO ls_table_structure." + System.Environment.NewLine);
            sbDeclaration.Append("  APPEND ls_table_structure TO ls_TableDefinition-field." + System.Environment.NewLine);
            sbDeclaration.Append("ENDLOOP." + System.Environment.NewLine);
            sbDeclaration.Append("APPEND ls_TableDefinition TO lt_TableDefinition." + System.Environment.NewLine);
            sbDeclaration.Append("* check success" + System.Environment.NewLine);
            sbDeclaration.Append("READ TABLE lt_TableDefinition INTO ls_TableDefinition" + System.Environment.NewLine);
            sbDeclaration.Append("  WITH KEY name = " + TABLE + "." + System.Environment.NewLine + System.Environment.NewLine);
            sbDeclaration.Append("IF sy-subrc > 2." + System.Environment.NewLine);
            sbDeclaration.Append("  RAISE ERROR_FILLING_TABLEDEFS." + System.Environment.NewLine);
            sbDeclaration.Append("ENDIF." + System.Environment.NewLine);

            var sbSelect = new StringBuilder();

            sbSelect.Append("* Tabelle öffnen" + System.Environment.NewLine);
            sbSelect.Append("OPEN CURSOR WITH HOLD dbcursor FOR" + System.Environment.NewLine);
            sbSelect.Append("SELECT * FROM (" + TABLE + ")." + System.Environment.NewLine + System.Environment.NewLine);

            sbSelect.Append("  DO." + System.Environment.NewLine);
            sbSelect.Append("  FETCH NEXT CURSOR dbcursor INTO <wa>." + System.Environment.NewLine);
            sbSelect.Append("  IF SY-SUBRC <> 0. EXIT. ENDIF." + System.Environment.NewLine + System.Environment.NewLine);

            sbSelect.Append("  LOOP AT ls_TableDefinition-field INTO ls_field." + System.Environment.NewLine);
            sbSelect.Append("    IF ls_field-FIELDNAME = " + COLUMN_NAME_CLUSTER_STRUCTURE + "." + System.Environment.NewLine);
            sbSelect.Append("      ASSIGN COMPONENT ls_field-FIELDNAME" + System.Environment.NewLine);
            sbSelect.Append("      OF STRUCTURE " + FIELD_WORK + " TO " + FIELD_CLUST_ID + System.Environment.NewLine);
            sbSelect.Append("      TYPE   ls_field-INTTYPE." + System.Environment.NewLine);
            sbSelect.Append("    ENDIF." + System.Environment.NewLine);
            sbSelect.Append("    IF ls_field-FIELDNAME = " + COLUMN_NAME_DATA + "." + System.Environment.NewLine);
            sbSelect.Append("      ASSIGN COMPONENT ls_field-FIELDNAME" + System.Environment.NewLine);
            sbSelect.Append("      OF STRUCTURE " + FIELD_WORK + " TO " + FIELD_DATA + System.Environment.NewLine);
            sbSelect.Append("      TYPE   ls_field-INTTYPE." + System.Environment.NewLine);
            sbSelect.Append("    ENDIF." + System.Environment.NewLine);
            sbSelect.Append("  ENDLOOP." + System.Environment.NewLine + System.Environment.NewLine);

            sbSelect.Append("  CLEAR " + IMPORTBUFFER + "." + System.Environment.NewLine);
            sbSelect.Append("  CONCATENATE " + IMPORTBUFFER + " " + FIELD_DATA + " INTO " + IMPORTBUFFER +
                            " IN BYTE MODE." + System.Environment.NewLine);

            var sbImportStatement = new StringBuilder();

            // writing condition
            for (int i = 0; i < Structures.Count; i++) {
                if(i == 0) {
                    sbImportStatement.Append("IF " + FIELD_CLUST_ID + " = '" + Structures[i].Name + "'." +
                                             System.Environment.NewLine);
                } else {
                    sbImportStatement.Append("ELSEIF " + FIELD_CLUST_ID + " = '" + Structures[i].Name + "'." +
                                             System.Environment.NewLine);
                }

                sbImportStatement.Append(GetImportStatement(Structures[i]));
                sbImportStatement.Append(System.Environment.NewLine);
                sbImportStatement.Append(GetExportStatement(Structures[i]));
                sbImportStatement.Append(System.Environment.NewLine);
            }
            sbImportStatement.Append("ENDIF.");
            sbSelect.Append(sbImportStatement.ToString());

            sbSelect.Append("  ENDDO." + System.Environment.NewLine);
            sbSelect.Append("CLOSE CURSOR dbcursor." + System.Environment.NewLine);

            return sbDeclaration + System.Environment.NewLine + sbSelect.ToString();
        }

        private string GetImportStatement(Cluster_Structure clusterStructure) {
            /*
            IMPORT
            B1_NT1
            NT2
            IFT1
            IFT2
            ERT
            NCT
            QT
            ST
            ITP1
            ITP7
            ITP50
            PDPPM
            FROM DATA BUFFER importBuffer.
            */
            var sbImportStatement = new StringBuilder();

            if(string.IsNullOrEmpty(clusterStructure.Addition)) {
                sbImportStatement.Append("IMPORT" + System.Environment.NewLine);
                foreach (var clusterSubStructure in clusterStructure.SubStructures) {
                    sbImportStatement.Append(clusterSubStructure.Name + System.Environment.NewLine);
                }
                sbImportStatement.Append("FROM DATA BUFFER " + IMPORTBUFFER + ".");
            } else {
                sbImportStatement.Append(clusterStructure.Addition + System.Environment.NewLine);
                sbImportStatement.Append("FROM DATA BUFFER " + IMPORTBUFFER + ".");
            }
            sbImportStatement.Append(System.Environment.NewLine);
            return sbImportStatement.ToString();
        }

        private string GetExportStatement(Cluster_Structure clusterStructure) {
            // PERFORM EXPORT_B1_NT1 USING filename_B1_NT1.
            var sbExportStatement = new StringBuilder();

            foreach (var clusterSubStructure in clusterStructure.SubStructures) {
                sbExportStatement.Append("PERFORM EXPORT_" + clusterSubStructure.Name + " USING filename_" + clusterSubStructure.Name + " " + clusterSubStructure.Name + "[]." + System.Environment.NewLine);
            }
            return sbExportStatement.ToString();
        }

        private string GetDataDefinition() {
            // Data definition
            /*
            DATA BEGIN OF NT1 OCCURS 10.
            INCLUDE STRUCTURE PDCMT.
            DATA END OF NT1.
            */

            var lSubStructures = GetSubStructures();

            var sbDataDefinition = new StringBuilder();
            foreach (var clusterSubStructure in lSubStructures) {
                sbDataDefinition.Append(GetDataDefinitionForSubStructure(clusterSubStructure.Name));
            }

            return sbDataDefinition.ToString();
        }

        private string GetDataDefinitionForSubStructure(string name) {
            var sbDataDefinition = new StringBuilder();
            foreach (var clusterSubStructure in GetSubStructures()) {
                if(name.ToLower() != clusterSubStructure.Name.ToLower()) continue;

                sbDataDefinition.Append("* Data definition for " + clusterSubStructure.Name + System.Environment.NewLine);
                if (string.IsNullOrEmpty(clusterSubStructure.Addition)) {
                    sbDataDefinition.Append("DATA BEGIN OF " + clusterSubStructure.Name);
                    if (clusterSubStructure.Occurs >= 0)
                        sbDataDefinition.Append(" OCCURS " + clusterSubStructure.Occurs);
                    sbDataDefinition.Append('.' + System.Environment.NewLine);
                    sbDataDefinition.Append("INCLUDE STRUCTURE " + clusterSubStructure.Table + "." + System.Environment.NewLine);
                    sbDataDefinition.Append("DATA END OF " + clusterSubStructure.Name + ".");
                } else {
                    sbDataDefinition.Append(clusterSubStructure.Addition);
                    System.Diagnostics.Debug.WriteLine(clusterSubStructure.Addition);
                }
                sbDataDefinition.Append(System.Environment.NewLine + System.Environment.NewLine);
            }
            return sbDataDefinition.ToString();
        }

        private string GetExtractionForms() {
            var sbForms = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbForms.Append("* Form definition for extraction of " + clusterSubStructure.Name + System.Environment.NewLine);
                sbForms.Append("FORM EXPORT_" + clusterSubStructure.Name + " USING filename TYPE string DATA_" + clusterSubStructure.Name + " TYPE TABLE." +
                               System.Environment.NewLine);
                sbForms.Append("FIELD-SYMBOLS <field> TYPE ANY." + System.Environment.NewLine);
                sbForms.Append("DATA: datarow TYPE string." + System.Environment.NewLine);
                sbForms.Append("DATA: value TYPE string." + System.Environment.NewLine);
                sbForms.Append("DATA: inserted(1)." + System.Environment.NewLine);
                sbForms.Append(System.Environment.NewLine);

                sbForms.Append(GetDataDefinitionForSubStructure(clusterSubStructure.Name));

                // loop over all elements in structure
                sbForms.Append("LOOP AT DATA_" + clusterSubStructure.Name + " INTO " + clusterSubStructure.Name + "." + System.Environment.NewLine);
                sbForms.Append("  DO." + System.Environment.NewLine);

                // loop over all clumns
                sbForms.Append("    ASSIGN COMPONENT sy-index OF STRUCTURE " + clusterSubStructure.Name + " TO <field>." +
                               System.Environment.NewLine);

                // exit in case of no data
                sbForms.Append("    IF sy-subrc > 0." + System.Environment.NewLine);
                sbForms.Append("      EXIT." + System.Environment.NewLine);
                sbForms.Append("    ELSE." + System.Environment.NewLine);

                // write value to string variable - needed for decimal values
                sbForms.Append("    value = <field>." + System.Environment.NewLine);

                // write seperator if there were already values inserted
                sbForms.Append("    IF inserted = 'X'." + System.Environment.NewLine);
                sbForms.Append("      CONCATENATE datarow '" + SEPERATOR + "' INTO datarow." + System.Environment.NewLine);
                sbForms.Append("    ENDIF." + System.Environment.NewLine);

                // write value to dataorw
                sbForms.Append("    CONCATENATE datarow value INTO datarow." + System.Environment.NewLine);

                // set inserted-flag for seperator on next itteration of columns
                sbForms.Append("    inserted = 'X'." + System.Environment.NewLine);

                sbForms.Append("    ENDIF." + System.Environment.NewLine);
                sbForms.Append("  ENDDO." + System.Environment.NewLine);

                // writing end of line sign
                sbForms.Append("  CONCATENATE datarow '" + ENDOFLINE + "' INTO datarow." + System.Environment.NewLine);

                // writing valueline into file
                sbForms.Append("  TRANSFER datarow TO filename." + System.Environment.NewLine);

                // reset variables
                sbForms.Append("  CLEAR datarow." + System.Environment.NewLine);
                sbForms.Append("  inserted = ''." + System.Environment.NewLine);
                sbForms.Append("ENDLOOP." + System.Environment.NewLine);
                sbForms.Append("ENDFORM." + System.Environment.NewLine + System.Environment.NewLine);
            }
            return sbForms.ToString();
        }

        private string GetHeaderForms() {
            var sbForms = new StringBuilder();

            foreach (var clusterSubStructure in GetSubStructures()) {
                sbForms.Append("* Form definition for header of " + clusterSubStructure.Name + System.Environment.NewLine);
                sbForms.Append("FORM HEADER_" + clusterSubStructure.Name + " USING filename TYPE string." +
                               System.Environment.NewLine);

                sbForms.Append("DATA: header TYPE string." + System.Environment.NewLine);

                if (string.IsNullOrEmpty(clusterSubStructure.Addition)) {
                    sbForms.Append("DATA ls_table_structure TYPE DFIES." + System.Environment.NewLine);
                    sbForms.Append("DATA table_type TYPE DD02V-TABCLASS." + System.Environment.NewLine);
                    sbForms.Append("DATA: TABLE_NAME TYPE DD02T-TABNAME." + System.Environment.NewLine);
                    sbForms.Append("DATA: inserted(1)." + System.Environment.NewLine);

                    sbForms.Append("Table_name = '" + clusterSubStructure.Table + "'." + System.Environment.NewLine);

                    sbForms.Append("DATA BEGIN OF table_structure OCCURS 10." + System.Environment.NewLine);
                    sbForms.Append("  INCLUDE STRUCTURE DFIES." + System.Environment.NewLine);
                    sbForms.Append("DATA END OF table_structure." + System.Environment.NewLine);

                    sbForms.Append("CALL FUNCTION 'DDIF_FIELDINFO_GET'" + System.Environment.NewLine);
                    sbForms.Append("  EXPORTING" + System.Environment.NewLine);
                    sbForms.Append("    TABNAME              = TABLE_NAME" + System.Environment.NewLine);
                    sbForms.Append("  IMPORTING" + System.Environment.NewLine);
                    sbForms.Append("    DDOBJTYPE            = table_type" + System.Environment.NewLine);
                    sbForms.Append("  TABLES" + System.Environment.NewLine);
                    sbForms.Append("    DFIES_TAB            = table_structure" + System.Environment.NewLine);
                    sbForms.Append("  EXCEPTIONS" + System.Environment.NewLine);
                    sbForms.Append("    NOT_FOUND            = 1" + System.Environment.NewLine);
                    sbForms.Append("    INTERNAL_ERROR       = 2" + System.Environment.NewLine);
                    sbForms.Append("    OTHERS               = 3." + System.Environment.NewLine);

                    sbForms.Append("IF SY-SUBRC <> 0." + System.Environment.NewLine);
                    sbForms.Append("  RAISE ERROR_FILLING_TABLEDEFS." + System.Environment.NewLine);
                    sbForms.Append("ENDIF." + System.Environment.NewLine);

                    sbForms.Append("LOOP AT table_structure INTO ls_table_structure." + System.Environment.NewLine);

                    sbForms.Append("  IF inserted = 'X'." + System.Environment.NewLine);
                    sbForms.Append("    CONCATENATE header '" + SEPERATOR + "' INTO header." + System.Environment.NewLine);
                    sbForms.Append("  ENDIF." + System.Environment.NewLine);

                    sbForms.Append("  CONCATENATE header ls_table_structure-fieldname into header." +
                                   System.Environment.NewLine);
                    sbForms.Append("  inserted = 'X'." + System.Environment.NewLine);
                    sbForms.Append("ENDLOOP." + System.Environment.NewLine);
                } else {
                    var sb = new StringBuilder();
                    foreach (var header in clusterSubStructure.Header.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)) {
                        if (sb.Length > 0) sb.Append(" '" + SEPERATOR  + "' " + System.Environment.NewLine);
                        sb.Append("'" + header + "'");
                    }
                    sbForms.Append("CONCATENATE header " + System.Environment.NewLine + sb.ToString() + " INTO header.");
                }

                sbForms.Append("CONCATENATE header '" + ENDOFLINE + "' INTO header." + System.Environment.NewLine);
                sbForms.Append("TRANSFER header TO filename." + System.Environment.NewLine);

                sbForms.Append("ENDFORM." + System.Environment.NewLine + System.Environment.NewLine);
            }
            return sbForms.ToString();
        }

        private List<Cluster_Sub_Structure> GetSubStructures() {
            var dic = new Dictionary<string, Cluster_Sub_Structure>();
            foreach (var clusterStructure in Structures) {
                foreach (var clusterSubStructure in clusterStructure.SubStructures) {
                    if (!dic.ContainsKey(clusterSubStructure.Name.ToUpper())) dic.Add(clusterSubStructure.Name.ToUpper(), clusterSubStructure);
                }
            }
            var result = new List<Cluster_Sub_Structure>();
            foreach (var keyPair in dic) {
                result.Add(keyPair.Value);
            }

            return result;
        }

        private void ReadStructureNode(XmlNode node) {
            var structure = new Cluster_Structure();

            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "name":
                        structure.Name = childNode.InnerText;
                        break;
                    case "include":
                        ReadSubStructure(childNode, structure);
                        break;
                    case "addition":
                        structure.Addition = childNode.InnerText;
                        break;
                    default:
                        throw new NotImplementedException(childNode.Name);
                }
            }

            Structures.Add(structure);
        }

        private void ReadSubStructure(XmlNode node, Cluster_Structure structure) {
            var subStructure = new Cluster_Sub_Structure();

            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "name":
                        subStructure.Name = childNode.InnerText;
                        break;
                    case "occurs":
                        subStructure.Occurs = int.Parse(childNode.InnerText);
                        break;
                    case "table":
                        subStructure.Table = childNode.InnerText;
                        break;
                    case "addition":
                        subStructure.Addition = childNode.InnerText;
                        break;
                    case "header":
                        subStructure.Header = childNode.InnerText;
                        break;
                    default:
                        throw new NotImplementedException(childNode.Name);
                }
            }

            structure.SubStructures.Add(subStructure);
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.FileOk += (o, args) => tbFile.Text = dlg.FileName;
            dlg.ShowDialog();
        }
    }
}
