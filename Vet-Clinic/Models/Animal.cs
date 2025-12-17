using System;
using System.Collections.Generic;

namespace Vet_Clinic.Models
{
    // Абстрактный класс животного
    public abstract class Animal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public Guid OwnerId { get; set; }
        public List<Guid> MedicalRecordIds { get; set; } = new List<Guid>();

        public int GetAge()
        {
            var years = (int)((DateTime.Now - DateOfBirth).TotalDays / 365.25);
            return years < 0 ? 0 : years;
        }

        public virtual string GetInfo()
        {
            var breed = string.IsNullOrWhiteSpace(Breed) ? "" : $" ({Breed})";
            return $"{Name} — {Species}{breed}, {GetAge()} years";
        }

    }
}
 