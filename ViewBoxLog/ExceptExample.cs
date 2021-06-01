using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewBoxLog
{
    /// <summary>
    /// set up an inner exception to be caught... just a testclass
    /// </summary>
    internal class ExceptExample
    {
        public void ThrowInner() {
            throw new Exception("ExceptExample inner exception");
        }
        public void CatchInner() {
            try
            {
                this.ThrowInner();
            }
            catch (Exception e)
            {
                throw new Exception("Error caused by trying ThrowInner.", e);
            }
        }
    }
}
