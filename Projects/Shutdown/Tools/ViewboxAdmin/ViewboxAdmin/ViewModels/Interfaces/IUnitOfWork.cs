using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels
{
    /// <summary>
    /// collect items which were edited, deleted, created... and make the db call in a single unit by calling commit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<in T> {
        void MarkAsDirty(T item);
        void MarkAsNew(T item);
        void MarkAsDeleted(T item);
        void Commit();
        void RollBack();
    }
}
