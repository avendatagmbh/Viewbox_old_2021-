// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.AuditCorrections {
    internal class AuditCorrectionSet : IAuditCorrectionSet {

        private readonly Dictionary<long, DbEntityAuditCorrectionSetEntry> _setEntries =
            new Dictionary<long, DbEntityAuditCorrectionSetEntry>();

        private readonly long _setId;

        public AuditCorrectionSet(long setId) { _setId = setId; }

        public AuditCorrectionSet(IEnumerable<DbEntityAuditCorrectionSetEntry> entities) {
            foreach (var entity in entities) {
                _setEntries[entity.CorrectionId] = entity;
            }
        }

        public void AddCorrection(IAuditCorrection correction) {
            var dbEntity = new DbEntityAuditCorrectionSetEntry
                           {CorrectionId = ((AuditCorrection) correction).DbEntity.Id, SetId = _setId};

            _setEntries[dbEntity.CorrectionId] = dbEntity;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(dbEntity);
                    // LogManager.Instance.TODO
                } catch (Exception ex) {
                    // TODO: add meaningful exception message
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        public void RemoveCorrection(IAuditCorrection correction) {
            var id = ((AuditCorrection) correction).DbEntity.Id;
            var dbEntity = _setEntries[id];
            _setEntries.Remove(id);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(dbEntity);
                    // LogManager.Instance.TODO                
                } catch (Exception ex) {
                    // TODO: add meaningful exception message
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        public void SetName(string newName) {
            foreach (var correction in _setEntries) {
                // TODO
            }
        }

        public void SetComment(string newComment) {
            foreach (var correction in _setEntries) {
                // TODO
            }
        }
    }
}