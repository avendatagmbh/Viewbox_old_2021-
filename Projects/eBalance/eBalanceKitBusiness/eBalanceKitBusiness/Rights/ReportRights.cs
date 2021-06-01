// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitBusiness.Rights {
    public class ReportRights {
        public ReportRights(Document document) {
            var transferValuesRight = RightManager.RightDeducer.GetSpecialRight(document,
                                                                                DbRight.ContentTypes.
                                                                                    DocumentSpecialRight_TransferValueCalculation);
            _readTransferValuesAllowed = transferValuesRight.IsReadAllowed;
            _writeTransferValuesAllowed = transferValuesRight.IsWriteAllowed;

            var restRight = RightManager.RightDeducer.GetSpecialRight(document,
                                                                      DbRight.ContentTypes.DocumentSpecialRight_Rest);
            _readRestAllowed = restRight.IsReadAllowed;
            _writeRestAllowed = restRight.IsWriteAllowed;

            var docRight = RightManager.RightDeducer.GetRight(document);
            _readAllowed = docRight.IsReadAllowed;
            _writeAllowed = docRight.IsWriteAllowed;
            _exportAllowed = docRight.IsExportAllowed;

        }

        #region ReadTransferValuesAllowed
        private readonly bool _readTransferValuesAllowed;

        public bool ReadTransferValuesAllowed { get { return _readTransferValuesAllowed; } }
        #endregion

        #region WriteTransferValuesAllowed
        private readonly bool _writeTransferValuesAllowed;

        public bool WriteTransferValuesAllowed { get { return _writeTransferValuesAllowed; } }
        #endregion

        #region ReadRestAllowed
        private readonly bool _readRestAllowed;

        public bool ReadRestAllowed { get { return _readRestAllowed; } }
        #endregion

        #region WriteRestAllowed
        private readonly bool _writeRestAllowed;

        public bool WriteRestAllowed { get { return _writeRestAllowed; } }
        #endregion

        #region ExportAllowed
        private readonly bool _exportAllowed;

        public bool ExportAllowed { get { return _exportAllowed; } }
        #endregion

        #region ReadAllowed
        private readonly bool _readAllowed;

        public bool ReadAllowed { get { return _readAllowed; } }
        #endregion

        #region WriteAllowed
        private readonly bool _writeAllowed;

        public bool WriteAllowed { get { return _writeAllowed; } }
        #endregion

        /// <summary>
        /// joins WriteAllowed && WriteRestAllowed && WriteTransferValuesAllowed
        /// </summary>
        public bool FullWriteRights {
            get { return WriteAllowed && WriteRestAllowed && WriteTransferValuesAllowed;}
        }

    }
}