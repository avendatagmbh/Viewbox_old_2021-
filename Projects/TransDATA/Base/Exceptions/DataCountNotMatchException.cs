using System;
using Base.Localisation;

namespace Base.Exceptions {
    public class DataCountNotMatchException : Exception {
        public DataCountNotMatchException(string detailMessage, Exception ex) :
            base(string.Format(ExceptionMessages.DataCountNotMatch, detailMessage), ex) { }
    }
}
