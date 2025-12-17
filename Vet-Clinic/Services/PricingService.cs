using System;
using System.Collections.Generic;
using System.Linq;
using Vet_Clinic.DataAccess;
using Vet_Clinic.Models;

namespace Vet_Clinic.Services
{
    // Сервис для расчета стоимости услуг
    public class PricingService
    {
        private readonly IRepository<Service> serviceRepo;

        public PricingService(IRepository<Service> repo)
        {
            serviceRepo = repo;
        }

        public decimal CalculatePrice(IEnumerable<Guid> serviceIds)
        {
            var ids = serviceIds?.ToList() ?? new List<Guid>();
            return serviceRepo.GetAll().Where(s => ids.Contains(s.Id)).Sum(s => s.Price);
        }
    }
}
