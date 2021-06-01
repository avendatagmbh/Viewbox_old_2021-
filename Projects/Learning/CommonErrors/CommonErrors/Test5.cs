using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonErrors {
    internal static class Test5 {
        internal static void Test() {
            List<int> ints = new List<int>(){5,1,5};
            int number = 0;

            var orderedInts = ints.OrderBy(i => {
                number++;
                return i;
            });

            var allFives = ints.Where((
                j => {
                    number++;
                    return j == 5;
            }));

            Console.WriteLine(allFives.Count());
            Console.WriteLine(number);
        }
    }
}
