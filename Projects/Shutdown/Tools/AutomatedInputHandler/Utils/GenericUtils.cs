﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils {
    public class GenericUtils {
        public static void Swap<T>(ref T lhs, ref T rhs) {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
