using System;

namespace Vet_Clinic.Models
{
    // Услуга клиники
    public class Service
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0m;
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(15);
        public string Description { get; set; } = string.Empty;
    }
}
