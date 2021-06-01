using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Utils;
using eBalanceKitBusiness.Manager;
using System.Windows;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures;
using eBalanceKitBase;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitManagement.Models {
    public class MainWindowModel : INotifyPropertyChanged {

        public MainWindowModel(Window owner) {
            SendLogConfig = new SendLogConfig();
            //AdminLogConfig = new AdminLogConfig(((owner as eBalanceKitManagement.Windows.MainWindow).tabLogging.Content as Controls.CtlLogging).tab1.;
            AdminLogConfig = new AdminLogConfig();
            //AdminLogConfig = new AdminLogConfig();
            SystemManager.Init();

            CompanyManager.Init(false);

            DocumentManager.Instance.Init();
            TemplateManager.Init();
            RoleManager.Init();
            RightManager.Init();
            //Inits log manager
            //LogManager tempLogManager = LogManager.Instance;

            RightManager.InitAllowedDetails();

            this.Owner = owner;
            ReportConfig = new ReportConfig();
            ReportLogConfig = new ReportLogConfig(ReportConfig);

        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region ReportConfig
        private ReportConfig _reportConfig;

        public ReportConfig ReportConfig {
            get { return _reportConfig; }
            set { 
                _reportConfig = value;
                OnPropertyChanged("ReportConfig");
            }
        }
        #endregion

        #region AdminLogConfig
        public LogConfig AdminLogConfig { get; set; }
        #endregion

        #region ReportLogConfig
        public LogConfig ReportLogConfig { get; set; }
        #endregion

        #region SendLogConfig
        public LogConfig SendLogConfig { get; set; }
        #endregion

        public User CurrentUser { get { return UserManager.Instance.CurrentUser; } }
        
        internal void Init() {
            ReportConfig.Init();
            
            // No document visible for the user so we throw an Exception that will be shown in a MessageBox and the app will close
            if (!(ReportConfig.Items as ObservableCollectionAsync<Document>).Any()) {
                throw new Exception("No documents visible for you");
            }


            bool sendTabVisible = false;
            // Hide the tab3 (ResourcesLogging.SendCaption) if there are no documents the user is allowed to send. (only see what you are allowed to do)
            foreach (var document in ReportConfig.Items as ObservableCollectionAsync<Document>) {
                sendTabVisible |= RightManager.SendAllowed(document);
            }

            if (!sendTabVisible) {
                ((Owner as Windows.MainWindow).tabLogging.Content as
                 Controls.CtlLogging).tab3.Visibility = Visibility.Collapsed;
            }
        }

        public Window Owner { get; set; }

    }
}
