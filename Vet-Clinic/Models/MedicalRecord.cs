using System;
using System.Collections.Generic;

namespace Vet_Clinic.Models
{
    // Медицинская карта животного
    public class MedicalRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AnimalId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid DoctorId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public List<string> Prescriptions { get; set; } = new List<string>();
    }
}