using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfControlsSample.Structures.ListDemo {
    class PersonProvider {
        public static List<Person> GetPersons() {
            return new List<Person>() {
                                          new Person("Karl", 18, false),
                                          new Person("Louis", 35, true),
                                          new Person("Jesus", 2012, false),
                                          new Person("Mareike", 27, true),
                                          new Person("Sarah", 33, false),
                                      };
        }
    }
}
