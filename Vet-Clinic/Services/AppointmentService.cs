using System;
using System.Linq;
using Vet_Clinic.DataAccess;
using Vet_Clinic.Models;

namespace Vet_Clinic.Services
{
    // Сервис для управления записями на прием
    public class AppointmentService
    {
        private readonly IRepository<Appointment> appointmentRepo;
        private readonly IRepository<Animal> animalRepo;
        private readonly IRepository<Veterinarian> vetRepo;

        public AppointmentService(IRepository<Appointment> appointmentRepository,
                                  IRepository<Animal> animalRepository,
                                  IRepository<Veterinarian> veterinarianRepository)
        {
            appointmentRepo = appointmentRepository;
            animalRepo = animalRepository;
            vetRepo = veterinarianRepository;
        }

        public void ScheduleAppointment(Appointment appt)
        {
            if (appt == null) throw new ArgumentNullException(nameof(appt));
            var vet = vetRepo.GetById(appt.VeterinarianId);
            if (vet == null) throw new Exception("Veterinarian not found.");

            var existing = appointmentRepo.GetAll()
                .Where(a => a.VeterinarianId == appt.VeterinarianId && a.Status == AppointmentStatus.Scheduled);

            foreach (var e in existing)
            {
                if (e.OverlapsWith(appt)) throw new InvalidOperationException("Time slot is occupied for the selected veterinarian.");
            }

            appointmentRepo.Add(appt);
        }

        public void CancelAppointment(Guid appointmentId)
        {
            var appt = appointmentRepo.GetById(appointmentId);
            if (appt == null) return;
            appt.Status = AppointmentStatus.Cancelled;
            appointmentRepo.Update(appt);
        }
    }
}
