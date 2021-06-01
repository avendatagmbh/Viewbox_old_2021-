using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SAP_BilSchemaImport {
    public partial class ResultsForm : Form {
        public ResultsForm(List<FileResult> results ) {
            InitializeComponent();

            dataGridView1.Columns.Add("filename", "Dateiname");
            dataGridView1.Columns.Add("accounts", "Ausgelesene Accountstrukturen");
            dataGridView1.Columns.Add("error", "Fehler");

            foreach (var result in results) {
                dataGridView1.Rows.Add(new object[] {result.Filename, string.Join(",",result.AccountStructures.ToArray()), result.Error});    
            }
        }
    }
}
