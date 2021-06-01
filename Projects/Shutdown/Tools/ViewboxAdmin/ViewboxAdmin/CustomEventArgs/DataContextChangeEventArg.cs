using System;

namespace ViewboxAdmin.CustomEventArgs
{
    /// <summary>
    /// this eventargs subclass can report a ViewModel for a new window, control etc...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataContextChangeEventArg<T> : EventArgs
    {
        public DataContextChangeEventArg(T ViewModel) { this.ViewModel = ViewModel; }

        public T ViewModel { get; private set; }
    }
}
