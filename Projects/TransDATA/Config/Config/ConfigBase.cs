// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Config.Enums;
using Config.Interfaces.Config;
using Utils;

namespace Config.Config {
    public class ConfigBase : NotifyPropertyChangedBase {

        public event EventHandler ForceDataUpdate;
        internal void OnForceDataUpdate() { if (ForceDataUpdate != null) ForceDataUpdate(this, new EventArgs()); }

        public static IConfig GetInputConfig(InputConfigTypes type, string xmlRepresentation) {
            switch (type) {
                case InputConfigTypes.Database:
                    return new DatabaseInputConfig(xmlRepresentation);

                case InputConfigTypes.Csv:
                    return new CsvInputConfig(xmlRepresentation);

                //case InputConfigTypes.Binary:
                //    return new BinaryConfig(xmlRepresentation);

                default:
                    throw new NotImplementedException();
            }
        }

        public static IConfig GetOutputConfig(OutputConfigTypes type, string xmlRepresentation) {
            switch (type) {
                case OutputConfigTypes.Database:
                    return new DatabaseOutputConfig(xmlRepresentation);

                case OutputConfigTypes.Gdpdu:
                    return new GdpduOutputConfig(xmlRepresentation);

                case OutputConfigTypes.Csv:
                    return new CsvOutputConfig(xmlRepresentation);

                //case OutputConfigTypes.Sql:
                //    return new SqlExportConfig(xmlRepresentation);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}