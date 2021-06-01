// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;

namespace federalGazetteBusiness.Structures.ValueTypes {
    public interface IFederalGazetteElementInfo : INotifyPropertyChanged
    {
        string Id { get; set; }
        string Caption { get; }
        bool IsAllowed { get; set; }
        object Value { get; set; }
        IEnumerable<string> TaxonomyElements { get; set; }
        FederalGazetteElementType Type { get; }
        //void SetDbValue();
        string ElementName { get; }
        Enum.ParameterArea ParameterArea { get; set; }
    }
}