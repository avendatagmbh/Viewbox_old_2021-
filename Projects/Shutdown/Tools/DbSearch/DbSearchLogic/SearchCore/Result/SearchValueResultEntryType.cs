﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Result {
    public enum SearchValueResultEntryType {
        Exact,
        InString,
        Rounded,
        DateTimePartial
    };
}