using System;
using Vet_Clinic.DataAccess;
using Vet_Clinic.Models;

namespace VetClinic.Services
{
    // Сервис для управления медицинскими записями
    public class MedicalRecordService
    {
        private readonly IRepository<MedicalRecord> repo;
        private readonly IRepository<Animal> animalRepo;

        public MedicalRecordService(IRepository<MedicalRecord> repository, IRepository<Animal> animalRepository)
        {
            repo = repository;
            animalRepo = animalRepository;
        }

        public void AddRecord(MedicalRecord record)
        {
            var animal = animalRepo.GetById(record.AnimalId);
            if (animal == null) throw new Exception("Animal not found");

            repo.Add(record);

            var prop = animal.GetType().GetProperty("MedicalRecordIds");
            if (prop != null)
            {
                var list = (System.Collections.Generic.List<Guid>)prop.GetValue(animal);
                list.Add(record.Id);
                animalRepo.Update(animal);
            }
        }
    }
}
