// --------------------------------------------------------------------------------
// author: Sebastian Vetter / Mirko Dibbert
// since: 2012-05-31
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKit.Models;

namespace eBalanceKit.Controls {
    public partial class CtlExternCall {
        public CtlExternCall() {
            InitializeComponent();
            DataContext = new ExternCallModel();
        }
    }
}