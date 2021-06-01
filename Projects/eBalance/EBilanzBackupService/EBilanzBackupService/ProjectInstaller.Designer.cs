namespace EBilanzBackupService {
    partial class ProjectInstaller {
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.EBilanzBackupServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.EBilanzBackupServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // EBilanzBackupServiceProcessInstaller
            // 
            this.EBilanzBackupServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.EBilanzBackupServiceProcessInstaller.Password = null;
            this.EBilanzBackupServiceProcessInstaller.Username = null;
            // 
            // EBilanzBackupServiceInstaller
            // 
            this.EBilanzBackupServiceInstaller.Description = "eBalanceKit Backup Service";
            this.EBilanzBackupServiceInstaller.DisplayName = "eBalanceKit Backup Service";
            this.EBilanzBackupServiceInstaller.ServiceName = "EBilanzBackupService";
            this.EBilanzBackupServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.EBilanzBackupServiceProcessInstaller,
            this.EBilanzBackupServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller EBilanzBackupServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller EBilanzBackupServiceInstaller;
    }
}