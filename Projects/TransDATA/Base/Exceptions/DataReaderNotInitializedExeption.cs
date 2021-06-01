using System;
using Base.Localisation;

namespace Base.Exceptions {
    public class DataReaderNotInitializedExeption : Exception {
        public DataReaderNotInitializedExeption(string detailMessage, Exception ex) :
            base(string.Format(ExceptionMessages.DataReaderNotInitializedExeption, detailMessage), ex) { }
    }
}
