using System;

namespace Vet_Clinic.Models
{
    // Статусы записи
    public enum AppointmentStatus{Scheduled, Completed, Cancelled}

    // Запись на прием
    public class Appointment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AnimalId { get; set; }
        public Guid VeterinarianId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public DateTime EndTime => StartTime + Duration;

        public bool OverlapsWith(Appointment other)
        {
            return StartTime < other.EndTime && other.StartTime < EndTime;
        }
    }
}
