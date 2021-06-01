using System;
using System.Runtime.Serialization;

namespace eBalanceKitBusiness.Exceptions
{
    public class FieldLengthException : Exception
    {
        public FieldLengthException() : base() { }
        public FieldLengthException(string message) : base(message) { }
        public FieldLengthException(string message, System.Exception inner) : base(message, inner) { }

        protected FieldLengthException(SerializationInfo info, StreamingContext context) { }
    }
}
