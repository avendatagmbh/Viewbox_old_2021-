using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CommonErrors {
    internal static class Test3 {
        class EventClass {
            internal event EventHandler MyEvent;
            private void OnMyEvent(){
                EventHandler myEvent = MyEvent;
                if(myEvent != null) myEvent(this, new EventArgs());
            }

            public void FireEvent(){
                OnMyEvent();
            }

            internal void ClearEvents() {
                MyEvent = null;
            }
        }

        internal static void Test() {
            EventClass instance = new EventClass();
            instance.MyEvent += new EventHandler(instance_MyEvent);
            instance.MyEvent += new EventHandler(instance_MyEvent);
            instance.FireEvent();
            instance.MyEvent -= new EventHandler(instance_MyEvent);
            instance.FireEvent();
            instance.ClearEvents();
            instance.FireEvent();
        }

        static void instance_MyEvent(object sender, EventArgs e) {
            Console.WriteLine("HIER");
        }
    }
}
