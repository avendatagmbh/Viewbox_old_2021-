using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewBuilderCommon
{
    public class ObjectType
    {
        #region [ Constructor ]

        public ObjectType(string name)
        {
            LangToDescription = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Name = name;
        }

        #endregion [ Constructor ]

        #region [ Properties ]

        public Dictionary<string, string> LangToDescription { get; set; }
        public string Name { get; set; }

        #endregion [ Properties ]

        #region [ Methods ]

        public string GetDescription()
        {
            if (LangToDescription.Count == 0) return Name;
            return LangToDescription.Values.FirstOrDefault();
        }

        #endregion [ Methods ]
    }
}