using System;
using System.Collections.Generic;

namespace Vet_Clinic.Models
{
    // Визит животного к ветеринару
    public class Visit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppointmentId { get; set; }
        public Guid AnimalId { get; set; }
        public Guid VeterinarianId { get; set; }
        public List<Guid> ServiceIds { get; set; } = new List<Guid>();
        public string Diagnosis { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
