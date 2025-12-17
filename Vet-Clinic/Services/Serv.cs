using System;
using System.Collections.Generic;
using Vet_Clinic.Models;
using Vet_Clinic.DataAccess;

namespace Vet_Clinic.Services
{
    // Сервис для управления услугами клиники
    public class Serv
    {
        private readonly IRepository<Service> repo;
        public Serv(IRepository<Service> repository) => repo = repository;

        public IEnumerable<Service> GetAll() => repo.GetAll();

        public void AddService(Service s)
        {
            repo.Add(s);
        }

        public void UpdateService(Service s)
        {
            repo.Update(s);
        }

        public void RemoveService(Guid id)
        {
            repo.Remove(id);
        }
    }
}
