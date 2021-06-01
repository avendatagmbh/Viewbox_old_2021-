using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommonErrors {
    internal static class Test2 {
        static string[] Names = new string[]{"Mirko", "Marcus"};

        internal static void Test() {
            for (int i = 0; i < 4; ++i) {
                Task.Factory.StartNew(() => { printer(i); }, TaskCreationOptions.PreferFairness);
            }
        }

        private static void printer(int i) {
            Console.WriteLine(i);
            Console.WriteLine(Names[i%2]);
        }
    }
}
