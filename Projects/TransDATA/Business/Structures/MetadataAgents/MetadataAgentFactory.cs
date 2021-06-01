using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Interfaces;
using Config.Interfaces.Config;

namespace Business.Structures.MetadataAgents {
    internal static class MetadataAgentFactory {
        #region Methods
        public static IMetadataAgent CreateAgent(IConfig config, int taskCount) {
            if(config is IDatabaseInputConfig)
                return new MetadataAgentDatabase((IDatabaseInputConfig) config, taskCount);
            if(config is ICsvInputConfig)
                return new MetadataAgentCsv((ICsvInputConfig)config);
            throw new NotSupportedException("Configtyp wird nicht unterstützt");
        }
        #endregion Methods
    }
}
