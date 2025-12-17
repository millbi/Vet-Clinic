using System;
using System.Collections.Generic;

namespace Vet_Clinic.DataAccess
{
    // Репозиторий для управления данными
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(Guid id);
        void Add(T item);
        void Update(T item);
        void Remove(Guid id);
        void Save();
    }
}
