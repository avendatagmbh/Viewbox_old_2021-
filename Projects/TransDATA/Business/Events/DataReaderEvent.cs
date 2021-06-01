// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Business.Interfaces;

namespace Business.Events {
    public class DataReaderEvent : EventArgs {

        public DataReaderEvent(IDataReader dataReader) { DataReader = dataReader; }

        public IDataReader DataReader { get; set; }
    }
}