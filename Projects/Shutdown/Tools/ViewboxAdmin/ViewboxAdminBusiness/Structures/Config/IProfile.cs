using System.ComponentModel;
using SystemDb;
using SystemDb.Upgrader;
using DbAccess.Structures;

namespace ViewboxAdmin_ViewModel.Structures.Config {
    public interface IProfile : INotifyPropertyChanged {
        string Name { get; set; }
        string Description { get; set; }
        IDbConfig DbConfig { get; set; }
        ISystemDb SystemDb { get;}
        string DisplayString { get; }
        bool Loaded { get; set; }
        bool IsLoading { get; set; }
        IDatabaseOutOfDateInformation DatabaseOutOfDateInformation { get; set; }
        void Load();
        void UpgradeDatabase();
    }
}