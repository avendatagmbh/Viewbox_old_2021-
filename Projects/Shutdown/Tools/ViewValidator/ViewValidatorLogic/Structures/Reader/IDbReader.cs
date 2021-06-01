using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidatorLogic.Structures.Reader {
    public interface IDbReader : IDisposable{
        bool Read();
        //object GetValue(int index);
        object this[int colIndex] { get; }
        bool IsDBNull(int index);
        void Close();
        void DoPrecomputations(Row row);
    }
}
