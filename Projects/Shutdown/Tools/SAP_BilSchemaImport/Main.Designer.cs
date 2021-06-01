using DbAccess.Structures;

namespace SAP_BilSchemaImport {
    partial class Main {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.avdLogo1 = new AvdControls.AvdLogo();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblSourceFile = new System.Windows.Forms.Label();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.txtAccountsStructure = new System.Windows.Forms.TextBox();
            this.lblAccountsStructure = new System.Windows.Forms.Label();
            this.btnSelectSourcefile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // avdLogo1
            // 
            this.avdLogo1.Location = new System.Drawing.Point(12, 12);
            this.avdLogo1.Name = "avdLogo1";
            this.avdLogo1.Size = new System.Drawing.Size(527, 60);
            this.avdLogo1.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(468, 423);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblSourceFile
            // 
            this.lblSourceFile.AutoSize = true;
            this.lblSourceFile.Location = new System.Drawing.Point(12, 349);
            this.lblSourceFile.Name = "lblSourceFile";
            this.lblSourceFile.Size = new System.Drawing.Size(54, 13);
            this.lblSourceFile.TabIndex = 3;
            this.lblSourceFile.Text = "Quelldatei";
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtSourceFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtSourceFile.Location = new System.Drawing.Point(15, 365);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.Size = new System.Drawing.Size(501, 20);
            this.txtSourceFile.TabIndex = 4;
            // 
            // txtAccountsStructure
            // 
            this.txtAccountsStructure.Location = new System.Drawing.Point(15, 409);
            this.txtAccountsStructure.Name = "txtAccountsStructure";
            this.txtAccountsStructure.Size = new System.Drawing.Size(135, 20);
            this.txtAccountsStructure.TabIndex = 4;
            // 
            // lblAccountsStructure
            // 
            this.lblAccountsStructure.AutoSize = true;
            this.lblAccountsStructure.Location = new System.Drawing.Point(12, 393);
            this.lblAccountsStructure.Name = "lblAccountsStructure";
            this.lblAccountsStructure.Size = new System.Drawing.Size(61, 13);
            this.lblAccountsStructure.TabIndex = 3;
            this.lblAccountsStructure.Text = "Kontenplan";
            // 
            // btnSelectSourcefile
            // 
            this.btnSelectSourcefile.Location = new System.Drawing.Point(521, 365);
            this.btnSelectSourcefile.Name = "btnSelectSourcefile";
            this.btnSelectSourcefile.Size = new System.Drawing.Size(22, 19);
            this.btnSelectSourcefile.TabIndex = 5;
            this.btnSelectSourcefile.Text = "...";
            this.btnSelectSourcefile.UseVisualStyleBackColor = true;
            this.btnSelectSourcefile.Click += new System.EventHandler(this.btnSelectSourcefile_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(156, 393);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 53);
            this.label1.TabIndex = 3;
            this.label1.Text = "(Kürzel, welches bei Kontogruppen vor jeder Gruppe steht - mehrere Kürzel mit ; t" +
    "rennen)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Host";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(18, 140);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(501, 20);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = "dbdsv";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(18, 183);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(501, 20);
            this.textBox2.TabIndex = 9;
            this.textBox2.Text = "root";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 167);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Username";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(18, 226);
            this.textBox3.Name = "textBox3";
            this.textBox3.PasswordChar = '*';
            this.textBox3.Size = new System.Drawing.Size(501, 20);
            this.textBox3.TabIndex = 11;
            this.textBox3.Text = "avendata";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 210);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Password";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 253);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "DbName";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 297);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "Verbindung testen";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(18, 270);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(166, 21);
            this.comboBox1.TabIndex = 15;
            this.comboBox1.Click += new System.EventHandler(this.comboBox1_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 458);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectSourcefile);
            this.Controls.Add(this.txtAccountsStructure);
            this.Controls.Add(this.txtSourceFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAccountsStructure);
            this.Controls.Add(this.lblSourceFile);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.avdLogo1);
            this.Name = "Main";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AvdControls.AvdLogo avdLogo1;
        private DbConfig dbConfig1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblSourceFile;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.TextBox txtAccountsStructure;
        private System.Windows.Forms.Label lblAccountsStructure;
        private System.Windows.Forms.Button btnSelectSourcefile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

