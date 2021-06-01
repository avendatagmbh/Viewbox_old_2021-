// --------------------------------------------------------------------------------
// author: Gábor Bauer
// since: 2012-05-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Structures;
using eBalanceKitBase.Structures;

namespace eBalanceKitBusiness.Reconciliation {
    /// <summary>
    /// Hepler class for converting imported transfer kind string to TransferKinds enum
    /// </summary>
    public class TransferKindCaptionHelper {

        #region [ Members ]

        private List<string> _lngRepReclassification = new List<string>();
        private List<string> _lngRepValueChange = new List<string>();
        private List<string> _lngRepReclassificationWithValueChange = new List<string>();

        /// <summary>
        /// Key in the resource file for ValueChange caption 
        /// </summary>
        public const string ValueChange = "TransferKindChangeVlaue";
        /// <summary>
        /// Key in the resource file for TransferKindReclassification caption 
        /// </summary>
        public const string Reclassification = "TransferKindReclassification";
        /// <summary>
        /// Key in the resource file for TransferKindReclassificationChangeValue caption 
        /// </summary>
        public const string ReclassificationWithValueChange = "TransferKindReclassificationChangeValue";

        private static TransferKindCaptionHelper _instance;
        
        #endregion [ Members ]

        #region [ Properties ]

        public static TransferKindCaptionHelper Instance { get { return _instance ?? (_instance = new TransferKindCaptionHelper()); } }
        /// <summary>
        /// All language representation of the caption of TransferKinds.Reclassification
        /// </summary>
        public List<string> LngRepReclassification { get { return _lngRepReclassification; } }
        /// <summary>
        /// All language representation of the caption of TransferKinds.ValueChange
        /// </summary>
        public List<string> LngRepValueChange { get { return _lngRepValueChange; } }
        /// <summary>
        /// All language representation of the caption of TransferKinds.ReclassificationWithValueChange
        /// </summary>
        public List<string> LngRepReclassificationWithValueChange { get { return _lngRepReclassificationWithValueChange; } }

        #endregion [ Properties ]

        #region [ Constructor ]

        private TransferKindCaptionHelper() {
            string lngRep1 = string.Empty;
            ResourceManager resManager = ResourcesReconciliation.ResourceManager;
            foreach (Language lng in AppConfig.Languages) {
                lngRep1 = resManager.GetString(ValueChange, lng.Culture);
                if (!string.IsNullOrEmpty(lngRep1))
                    _lngRepValueChange.Add(lngRep1);
                lngRep1 = resManager.GetString(Reclassification, lng.Culture);
                if (!string.IsNullOrEmpty(lngRep1))
                    _lngRepReclassification.Add(lngRep1);
                lngRep1 = resManager.GetString(ReclassificationWithValueChange, lng.Culture);
                if (!string.IsNullOrEmpty(lngRep1))
                    _lngRepReclassificationWithValueChange.Add(lngRep1);
            }
        }

        #endregion [ Constructor ]
    }
}
