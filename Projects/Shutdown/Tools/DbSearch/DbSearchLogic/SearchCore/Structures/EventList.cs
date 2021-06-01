using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Structures {
    public class EventListArgs<T> : EventArgs {
        private T _value;

        public EventListArgs(T value) {
            this._value = value;
        }

        public T Value {
            get {
                return this._value;
            }
        }

    }


    public class EventList<T> : List<T> {

        /// <summary>
        /// Triggers if an item is added to the list
        /// </summary>
        public event EventHandler<EventListArgs<T>> ItemAdded;

        /// <summary>
        /// Triggers the ItemAdded-event
        /// </summary>
        /// <param name="value"></param>
        private void OnItemAdded(T value) {
            if (this.ItemAdded != null) {
                this.ItemAdded(this, new EventListArgs<T>(value));
            }
        }

        /// <summary>
        /// Add an item to the list
        /// </summary>
        /// <param name="obj"></param>
        public new void Add(T value) {

            // Call the base method
            base.Add(value);

            // Trigger the event
            this.OnItemAdded(value);
        }

    }
}
