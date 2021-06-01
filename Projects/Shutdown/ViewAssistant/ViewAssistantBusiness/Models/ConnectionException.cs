using System;

namespace ViewAssistantBusiness.Models
{
    public class ConnectionException : Exception
    {
        public ConnectionException(String message) : base(message) { }
    }
}
