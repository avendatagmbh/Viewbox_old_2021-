// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-02-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader {
    public interface ILoader {
        Dictionary<int, long> LoadColumnDict();
        Dictionary<int, long> LoadRowDict();
        void Load();
    }
}