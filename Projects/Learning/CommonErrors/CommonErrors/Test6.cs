using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonErrors {
    class Test6 {
        class EventClass {
            internal event EventHandler MyEvent;
            private void OnMyEvent() {
                EventHandler myEvent = MyEvent;
                if (myEvent != null) myEvent(this, new EventArgs());
            }

            public void FireEvent() {
                OnMyEvent();
            }
        }
        internal static void Test() {
            try {
                EventClass instance = new EventClass();
                for (int i = 0; i < 5; ++i)
                    instance.MyEvent += InstanceMyEvent;

                instance.FireEvent();
            } catch (Exception) {
                Console.WriteLine("Ex");
            }

        }

        private static int _counter = 0;
        static void InstanceMyEvent(object sender, EventArgs e) {
            Console.WriteLine(_counter);
            if(_counter++ == 1) throw new Exception("");
        }

    }
}
