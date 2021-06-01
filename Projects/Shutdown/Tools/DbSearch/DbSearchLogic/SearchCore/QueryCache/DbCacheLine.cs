using System;
using System.Collections;

namespace DbSearchLogic.SearchCore.QueryCache {

    internal class DbCacheLine<T> : IDbCacheLine where T : System.IComparable<T> {

        private static int BLOCK_SIZE = 65536;


        private bool _finalized;

        private ArrayList _values;
        private ArrayList _ids;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCacheLine"/> class.
        /// </summary>
        public DbCacheLine() {
            _count = 0;
            _finalized = false;
            _values = new ArrayList();
            _ids = new ArrayList();
        }

        private uint _count;
        public uint Count {
            get { return _count; }
        }

        public void Add(UInt32 id, T value) {            

            if (_finalized) {
                throw new Exception("Das Einfügen weiterer Elemente nach einem Aufruf von FinalizeInsertion() ist nicht möglich!");
            }
            
            try {
                if (_count % BLOCK_SIZE == 0) {
                    _ids.Add(new UInt32[BLOCK_SIZE]);
                    _values.Add(new T[BLOCK_SIZE]);
                }

                ((UInt32[])_ids[_ids.Count - 1])[_count % BLOCK_SIZE] = id;
                ((T[])_values[_values.Count - 1])[_count % BLOCK_SIZE] = value;

                _count++;

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Finializes the insertion.
        /// </summary>
        public void FinializeInsertion() {

            try {
                if (_finalized) return;

                _finalized = true;

                // trim arrays

                Array oldValueObjArr;
                Array newValueObjArr;

                Array oldIdObjArr;
                Array newIdObjArr;
                
                if (_count == 0)  return;

                oldValueObjArr = (T[])_values[_values.Count - 1];
                newValueObjArr = new T[_count % BLOCK_SIZE];

                oldIdObjArr = (UInt32[])_ids[_ids.Count - 1];
                newIdObjArr = new UInt32[_count % BLOCK_SIZE];

                Array.Copy(oldValueObjArr, newValueObjArr, newValueObjArr.Length);
                Array.Clear(oldValueObjArr, 0, oldValueObjArr.Length);
                _values[_values.Count - 1] = newValueObjArr;

                Array.Copy(oldIdObjArr, newIdObjArr, newIdObjArr.Length);
                Array.Clear(oldIdObjArr, 0, oldIdObjArr.Length);
                _ids[_ids.Count - 1] = newIdObjArr;

                System.GC.Collect();

            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() {

            foreach (object obj in _ids) {
                T[] oArr = (T[])obj;
                Array.Clear(oArr, 0, oArr.Length);
            }

            foreach (object obj in _values) {
                T[] oArr = (T[])obj;
                Array.Clear(oArr, 0, oArr.Length);
            }

            _ids.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public object GetValue(int index) {
            try {
                return ((T[])_values[(index / BLOCK_SIZE)])[index % BLOCK_SIZE];

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public UInt32 GetId(int index) {
            return ((UInt32[])_ids[(index / BLOCK_SIZE)])[index % BLOCK_SIZE];
        }

        /// <summary>
        /// Finds the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public int Find(object value) {

            try {
                for (int i = 0; i < _values.Count; i++) {
                    int r = Array.BinarySearch((T[])_values[i], value);
                    if (r >= 0) return (BLOCK_SIZE * i) + r;
                }                

            } catch (Exception ex) {
                throw ex;
            }

            return -1;
        }
    }
}
