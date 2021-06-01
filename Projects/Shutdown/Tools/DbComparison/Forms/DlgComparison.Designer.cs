namespace DbComparison.Forms {
    partial class DlgComparison {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtCsvOutputDir = new System.Windows.Forms.TextBox();
            this.btnSelectCsvDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStartComparison = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dbConfigControl2 = new AvdControls.DbConfig();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dbConfigControl1 = new AvdControls.DbConfig();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(952, 460);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(944, 434);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Datenbanken";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.progressBar);
            this.groupBox3.Controls.Add(this.txtCsvOutputDir);
            this.groupBox3.Controls.Add(this.btnSelectCsvDir);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.btnStartComparison);
            this.groupBox3.Location = new System.Drawing.Point(6, 319);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(921, 100);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Export";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(13, 71);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(744, 23);
            this.progressBar.Step = 1;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 134;
            this.progressBar.Visible = false;
            // 
            // txtCsvOutputDir
            // 
            this.txtCsvOutputDir.Location = new System.Drawing.Point(13, 36);
            this.txtCsvOutputDir.Name = "txtCsvOutputDir";
            this.txtCsvOutputDir.Size = new System.Drawing.Size(799, 20);
            this.txtCsvOutputDir.TabIndex = 133;
            // 
            // btnSelectCsvDir
            // 
            this.btnSelectCsvDir.Location = new System.Drawing.Point(818, 34);
            this.btnSelectCsvDir.Name = "btnSelectCsvDir";
            this.btnSelectCsvDir.Size = new System.Drawing.Size(89, 22);
            this.btnSelectCsvDir.TabIndex = 132;
            this.btnSelectCsvDir.Text = "Durchsuchen";
            this.btnSelectCsvDir.UseVisualStyleBackColor = true;
            this.btnSelectCsvDir.Click += new System.EventHandler(this.btnSelectCsvDir_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Pdf Ausgabe Verzeichnis";
            // 
            // btnStartComparison
            // 
            this.btnStartComparison.Location = new System.Drawing.Point(797, 71);
            this.btnStartComparison.Name = "btnStartComparison";
            this.btnStartComparison.Size = new System.Drawing.Size(124, 23);
            this.btnStartComparison.TabIndex = 3;
            this.btnStartComparison.Text = "Starte Vergleich";
            this.btnStartComparison.UseVisualStyleBackColor = true;
            this.btnStartComparison.Click += new System.EventHandler(this.btnStartComparison_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dbConfigControl2);
            this.groupBox2.Location = new System.Drawing.Point(468, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(459, 309);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Datenbank 2";
            // 
            // dbConfigControl2
            // 
            this.dbConfigControl2.DbName = "";
            this.dbConfigControl2.DbType = DbAccess.DatabaseTypes.MySQL;
            this.dbConfigControl2.Dsn = "Xtreme Sample Database 2008";
            this.dbConfigControl2.Host = "";
            this.dbConfigControl2.Location = new System.Drawing.Point(6, 19);
            this.dbConfigControl2.Name = "dbConfigControl2";
            this.dbConfigControl2.Password = "";
            this.dbConfigControl2.Port = "3306";
            this.dbConfigControl2.ShowDbName = false;
            this.dbConfigControl2.Size = new System.Drawing.Size(453, 290);
            this.dbConfigControl2.TabIndex = 0;
            this.dbConfigControl2.User = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dbConfigControl1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(459, 309);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Datenbank 1";
            // 
            // dbConfigControl1
            // 
            this.dbConfigControl1.DbName = "";
            this.dbConfigControl1.DbType = DbAccess.DatabaseTypes.MySQL;
            this.dbConfigControl1.Dsn = "Xtreme Sample Database 2008";
            this.dbConfigControl1.Host = "";
            this.dbConfigControl1.Location = new System.Drawing.Point(3, 19);
            this.dbConfigControl1.Name = "dbConfigControl1";
            this.dbConfigControl1.Password = "";
            this.dbConfigControl1.Port = "3306";
            this.dbConfigControl1.ShowDbName = false;
            this.dbConfigControl1.Size = new System.Drawing.Size(453, 290);
            this.dbConfigControl1.TabIndex = 0;
            this.dbConfigControl1.User = "";
            // 
            // DlgComparison
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 460);
            this.Controls.Add(this.tabControl1);
            this.Name = "DlgComparison";
            this.Text = "Datenbank-Vergleich";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox2;
        private AvdControls.DbConfig dbConfigControl2;
        private System.Windows.Forms.GroupBox groupBox1;
        private AvdControls.DbConfig dbConfigControl1;
        private System.Windows.Forms.Button btnStartComparison;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelectCsvDir;
        private System.Windows.Forms.TextBox txtCsvOutputDir;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}