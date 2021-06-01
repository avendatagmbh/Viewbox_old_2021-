using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonErrors {
    internal static class Test4 {
        class Demo {
            internal Demo(int age) {
                Age = age;
            }

            public override bool Equals(object obj) {
                Demo otherDemo = obj as Demo;
                if (otherDemo == null)
                    return false;
                return Age == otherDemo.Age;
            }
            public override int GetHashCode() {
                return Age;
            }

            int Age { get; set; }
        }

        internal static void Test() {
            Demo d1 = new Demo(1);
            HashSet<Demo> demos = new HashSet<Demo> { d1 };
            Console.WriteLine(demos.Contains(d1));
            Console.WriteLine(demos.Contains(new Demo(1)));
        }
    }
}
