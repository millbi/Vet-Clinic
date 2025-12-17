using System;
using System.Collections.Generic;
using System.Linq;
using Vet_Clinic.DataAccess;
using Vet_Clinic.Models;

namespace Vet_Clinic.Services
{
    // Сервис для управления визитами
    public class VisitService
    {
        private readonly IRepository<Visit> visitRepo;
        private readonly IRepository<Appointment> appointmentRepo;
        private readonly IRepository<Animal> animalRepo;
        private readonly IRepository<Veterinarian> vetRepo;
        private readonly IRepository<Service> serviceRepo;
        private readonly PricingService pricing;

        public VisitService(IRepository<Visit> visitRepo,
                            IRepository<Appointment> appointmentRepo,
                            IRepository<Animal> animalRepo,
                            IRepository<Veterinarian> vetRepo,
                            IRepository<Service> serviceRepo,
                            PricingService pricing)
        {
            this.visitRepo = visitRepo;
            this.appointmentRepo = appointmentRepo;
            this.animalRepo = animalRepo;
            this.vetRepo = vetRepo;
            this.serviceRepo = serviceRepo;
            this.pricing = pricing;
        }

        public Visit CompleteVisit(Guid appointmentId, IEnumerable<Guid> serviceIds, string diagnosis)
        {
            var appt = appointmentRepo.GetById(appointmentId);
            if (appt == null) throw new Exception("Appointment not found");

            var visit = new Visit
            {
                AppointmentId = appointmentId,
                AnimalId = appt.AnimalId,
                VeterinarianId = appt.VeterinarianId,
                ServiceIds = serviceIds.ToList(),
                Diagnosis = diagnosis,
                Date = DateTime.Now
            };

            visit.TotalPrice = pricing.CalculatePrice(visit.ServiceIds);
            visitRepo.Add(visit);

            appt.Status = AppointmentStatus.Completed;
            appointmentRepo.Update(appt);

            return visit;
        }
    }
}
