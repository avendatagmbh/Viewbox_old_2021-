using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    abstract class TypeAnalyser : IDisposable
    {
        protected List<Regex> expressions;

        // Flag: Has Dispose already been called? 
        protected bool disposed = false;

        protected TypeAnalyser()
        {
            expressions = new List<Regex>();
        }

        public abstract string AnalyzeInput(string input);        

        public void AddExpression(Regex exp)
        {
            expressions.Add(exp);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                expressions = null;
            }

            disposed = true;
        }
    }
}
