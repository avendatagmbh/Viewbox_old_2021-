using System;

namespace DataAnalyze.Model {
    public class DimensionMismatchException : ArithmeticException {
        public DimensionMismatchException() { }
        public DimensionMismatchException(string message) : base(message) { }
        public DimensionMismatchException(string message, Exception innerException) : base(message, innerException) { }
    }
}