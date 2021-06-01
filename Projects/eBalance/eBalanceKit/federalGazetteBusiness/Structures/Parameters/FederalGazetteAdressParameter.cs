// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Parameters {
    public class FederalGazetteAdressParameter : FederalGazetteParameterBase {

        public FederalGazetteAdressParameter(ParameterArea type) {
            Name = new FederalGazetteElementText("Name");
            Devision = new FederalGazetteElementText("Devision");
            Street = new FederalGazetteElementText("Street", "de-gcd_genInfo.company.id.street");
            PostCode = new FederalGazetteElementText("PostCode", "de-gcd_genInfo.company.id.location.zipCode");
            City = new FederalGazetteElementText("City", "de-gcd_genInfo.company.id.location.city");

            #region countries
            Country = new FederalGazetteElementSelectionList("Country", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("de", "Deutschland"),
                new FederalGazetteElementInfoString("at", "Austria"),
                new FederalGazetteElementInfoString("ch", "Switzerland"),
                new FederalGazetteElementInfoString("nl", "Netherlands"),
                new FederalGazetteElementInfoString("fr", "France")
            });
            #endregion countries

            #region States
            State = new FederalGazetteElementSelectionList("State", new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                new FederalGazetteElementInfoString("BW", "Baden-Württemberg"),
                new FederalGazetteElementInfoString("BY", "Bayern"),
                new FederalGazetteElementInfoString("BE", "Berlin"),
                new FederalGazetteElementInfoString("BR", "Brandenburg"),
                new FederalGazetteElementInfoString("HB", "Bremen"),
                new FederalGazetteElementInfoString("HH", "Hamburg"),
                new FederalGazetteElementInfoString("HE", "Hessen"),
                new FederalGazetteElementInfoString("MV", "Mecklenburg-Vorpommern"),
                new FederalGazetteElementInfoString("NI", "Niedersachsen"),
                new FederalGazetteElementInfoString("NW", "Nordrhein-Westfalen"),
                new FederalGazetteElementInfoString("RP", "Rheinland-Pfalz"),
                new FederalGazetteElementInfoString("SL", "Saarland"),
                new FederalGazetteElementInfoString("SN", "Sachsen"),
                new FederalGazetteElementInfoString("ST", "Sachsen-Anhalt"),
                new FederalGazetteElementInfoString("SH", "Schleswig-Holstein"),
                new FederalGazetteElementInfoString("TH", "Thüringen")
            }){IsNullable = true};
            #endregion States

            _parameterType = type;

            LoadEverything();
        }

        private ParameterArea _parameterType;

        public IFederalGazetteElementInfo Name { get; set; }
        public IFederalGazetteElementInfo Devision { get; set; }
        public IFederalGazetteElementInfo Street { get; set; }
        public IFederalGazetteElementInfo PostCode { get; set; }
        public IFederalGazetteElementInfo City { get; set; }
        public IFederalGazetteElementInfo State { get; set; }
        public IFederalGazetteElementInfo Country { get; set; }

        #region Overrides of FederalGazetteParameterBase
        public override ParameterArea ParameterType { get { return _parameterType; } }
        protected override void Validate() { }
        #endregion
    }
}