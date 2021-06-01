// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-02
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Logs;

namespace eBalanceKitBusiness.Interfaces {
    /// <summary>
    /// Interface for all objects which could create new log entries, e.g. Document, Company, System
    /// </summary>
    public interface ILoggableObject {
        event LogHandler NewLog;
        int Id { get; }
    }
}