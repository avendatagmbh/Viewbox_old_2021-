// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    public class FederalGazetteTelephoneParameter : FederalGazetteParameterBase {

        public FederalGazetteTelephoneParameter(ParameterArea type) {

            _parameterType = type;
            AreaCode = new FederalGazetteElementText("AreaCode");
            CountryAreaCode = new FederalGazetteElementText("CountryAreaCode");
            Number = new FederalGazetteElementText("Number");
        }
        private ParameterArea _parameterType;


        public IFederalGazetteElementInfo CountryAreaCode { get; set; }
        public IFederalGazetteElementInfo AreaCode { get; set; }
        public IFederalGazetteElementInfo Number { get; set; }

        #region Overrides of FederalGazetteParameterBase
        public override ParameterArea ParameterType { get { return _parameterType; } }
        #endregion

        protected override void Validate() {
            if (HasValue(CountryAreaCode)) {
                AddError(@"Pflichtfeld bei den Ländern: Deutschland, Estland, Frankreich, Griechenland, Italien,
Norwegen, Polen, Schweiz, Spanien, Tschechien, USA, Großbritannien, Bahamas,
Kanada, Dominikanische Republik, Jamaika, St. Vincent u. die Grenadinen,
Caiman Islands, British Virgin, Islands, Barbados, Trinidad Tobago, Dominica,
Grenada und St. Lucia.
Bei folgenden Ländern darf dieses Feld nicht angegeben werden: Liechtenstein und
Luxemburg. Optional bei allen anderen Ländern.");
            }
        }
    }
}