using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLParser2.Model
{
    public class SQLParserError
    {

        List<string> _syntaxErrors = new List<string>();

        public string ErrorMessage { get; set; }
        public List<string> SyntaxErrors
        {
            get { return _syntaxErrors; }
        }
        public string CompleteStatement { get; set; }
    }
}
