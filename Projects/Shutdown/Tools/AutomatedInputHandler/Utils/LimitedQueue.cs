// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Utils {

    /// <summary>
    /// Class for a circular FIFO, that have a maximum capacity. The capacity can be set with the Limit property.
    /// Limitation of the code that the Enqueue is not virtual. After casting to Queue any number of elements can be added.
    /// Any inner method in Queue calls the base Enqueue(T item) method. That can cause problems.
    /// </summary>
    public class LimitedQueue<T> : Queue<T> {
        private int _limit = -1;

        public int Limit {
            get { return _limit; }
            set { _limit = value;
                while (Count > value) {
                    Dequeue();
                }
            }
        }

        public LimitedQueue(int limit)
            : base(limit) {
            Limit = limit;
        }

        public new void Enqueue(T item) {
            while (Count >= Limit) {
                Dequeue();
            }
            base.Enqueue(item);
        }

    }

}