// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbAccess.Structures;
using DbSearchDatabase.Interfaces;

namespace DbSearchDatabase.TableRelated {
    internal class DbRowEntry : IDbRowEntry {
        public DbRowEntry(IDbColumn column, DbRow dbRow) {
            Column = column;
            DbRow = dbRow;
            Value = string.Empty;
        }


        #region Properties
        public object Value { get; set; }
        public string EditedValue { get; set; }
        private IDbColumn Column { get; set; }
        private DbRow DbRow { get; set; }
        #endregion

        #region Methods
        public void ToXml(XmlWriter writer) {
            writer.WriteStartElement("RE");
                if(Value != null) writer.WriteAttributeString("V", Value.ToString());
                if(EditedValue != null) writer.WriteAttributeString("EV", EditedValue);
                writer.WriteAttributeString("R",DbRow.RowNumber.ToString());
            writer.WriteEndElement();
        }

        public void FromXml(XmlReader reader, IDbColumn column, IDbRow dbRow) {
            Column = column;
            DbRow = (DbRow) dbRow;

            if (reader.GetAttribute("V") != null) Value = reader.GetAttribute("V");
            else Value = null;
            if (reader.GetAttribute("EV") != null) EditedValue = reader.GetAttribute("EV");
            if (reader.GetAttribute("R") != null) DbRow.RowNumber = Convert.ToInt64(reader.GetAttribute("R"));
        }
        #endregion Methods
    }
}
