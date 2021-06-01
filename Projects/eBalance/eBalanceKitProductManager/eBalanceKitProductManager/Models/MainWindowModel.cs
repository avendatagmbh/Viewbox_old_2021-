using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using eBalanceKitProductManager.Business.Manager;
using eBalanceKitProductManager.Business.Structures.DbMapping;
using System.Windows;
using eBalanceKitProductManager.Windows;

namespace eBalanceKitProductManager.Models {
    
    internal class MainWindowModel {

        public MainWindowModel(Window owner) {
            SelectedVersion = Versions[0];
            this.Owner = owner;
        }

        public ObservableCollection<eBalanceKitProductManager.Business.Structures.DbMapping.Version> Versions {
            get { return VersionManager.Versions; }
        }

        public ObservableCollection<Instance> Instances {
            get { return InstanceManager.Instances; }
        }

        public ObservableCollection<Registration> Registrations {
            get { return RegistrationManager.Registrations; }
        }

        public eBalanceKitProductManager.Business.Structures.DbMapping.Version SelectedVersion { get; set; }

        public Instance SelectedInstance { get; set; }

        public void AddInstance() {
            Instance instance = InstanceManager.AddInstance();
            DlgInstanceData dlg = new DlgInstanceData(instance);
            bool? result = dlg.ShowDialog();
            if (!result.HasValue || result.Value == false) {
                InstanceManager.DeleteInstance(instance);
            }

        }

        public void DeleteInstance(Instance instance) {
            InstanceManager.DeleteInstance(instance);
        }

        public void SaveInstances() {
            InstanceManager.SaveInstances();
        }

        public Window Owner { get; set; }
    }
}
