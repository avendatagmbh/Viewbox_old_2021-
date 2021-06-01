// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

namespace Config.Interfaces.DbStructure {
    /// <summary>
    /// Interface for a user.
    /// </summary>
    public interface IUser {
        string UserName { get; set; }
        string FullName { get; set; }
        bool IsAdmin { get; set; }
        string DisplayString { get; }
        bool IsSelected { get; set; }
        bool IsInitialized { get; set; }
    }
}