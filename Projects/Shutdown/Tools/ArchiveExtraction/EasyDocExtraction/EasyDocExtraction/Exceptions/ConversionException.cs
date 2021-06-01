using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Helper;

namespace EasyDocExtraction.Exceptions
{
    public class ConversionException: Exception
    {
        public ConversionException(string message)
            : base(message)
        {
            Logger.WriteError(message, this);
        }
        public ConversionException(string format, params object[] args )
            : this(string.Format(format, args))
        {
        }
    }
}
