// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    public class FederalGazettePersonParameter : FederalGazetteParameterBase {

        public FederalGazettePersonParameter(ParameterArea type) {
            #region Salutation
            Salutation = new FederalGazetteElementSelectionList("Salutation", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("Frau", External.salutation_type.Frau.ToString()),
                new FederalGazetteElementInfoString("Herr", External.salutation_type.Herr.ToString())
            });
            #endregion

            Title = new FederalGazetteElementText("Title");

            Phone = new FederalGazetteTelephoneParameter(type);

            _parameterType = type;

            LoadEverything();
        }


        private ParameterArea _parameterType;

        public IFederalGazetteElementInfo Salutation { get; set; }
        public IFederalGazetteElementInfo Title { get; set; }
        public IFederalGazetteElementInfo FirstName { get; set; }
        public IFederalGazetteElementInfo LastName { get; set; }
        public IFederalGazetteElementInfo EMail { get; set; }
        public FederalGazetteTelephoneParameter Phone { get; set; }


        public override ParameterArea ParameterType { get { return _parameterType; } }
        protected override void Validate() {
            //ToDo implement
        }
    }
}