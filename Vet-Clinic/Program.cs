using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vet_Clinic.DataAccess;
using Vet_Clinic.Models;
using Vet_Clinic.Services;
using VetClinic.Services;

namespace Vet_Clinic
{
    /// <summary>
    /// Консольное приложение VetClinic:
    /// - 0 = отмена на любом шаге;
    /// - выбор вида: Dog / Cat / Parrot / Other;
    /// - валидация вводимых данных.
    /// </summary>
    internal static class Program
    {
        // Репозитории и сервисы
        public static IRepository<Owner> OwnerRepo;
        public static IRepository<Animal> AnimalRepo;
        public static IRepository<Veterinarian> VeterinarianRepo;
        public static IRepository<Appointment> AppointmentRepo;
        public static IRepository<Service> ServiceRepo;
        public static IRepository<MedicalRecord> MedicalRecordRepo;
        public static IRepository<Visit> VisitRepo;

        public static AppointmentService AppointmentService;
        public static PricingService PricingService;
        public static VisitService VisitService;
        public static Serv Service;
        public static MedicalRecordService MedicalRecordService;

        static void Main()
        {
            Console.Title = "Ветеринарная клиника";
            EnsureDataFiles();

            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            OwnerRepo = new JsonRepository<Owner>(Path.Combine(dataPath, "owners.json"));
            AnimalRepo = new JsonRepository<Animal>(Path.Combine(dataPath, "animals.json"));
            VeterinarianRepo = new JsonRepository<Veterinarian>(Path.Combine(dataPath, "veterinarians.json"));
            AppointmentRepo = new JsonRepository<Appointment>(Path.Combine(dataPath, "appointments.json"));
            ServiceRepo = new JsonRepository<Service>(Path.Combine(dataPath, "services.json"));
            MedicalRecordRepo = new JsonRepository<MedicalRecord>(Path.Combine(dataPath, "medicalrecords.json"));
            VisitRepo = new JsonRepository<Visit>(Path.Combine(dataPath, "visits.json"));

            AppointmentService = new AppointmentService(AppointmentRepo, AnimalRepo, VeterinarianRepo);
            PricingService = new PricingService(ServiceRepo);
            VisitService = new VisitService(VisitRepo, AppointmentRepo, AnimalRepo, VeterinarianRepo, ServiceRepo, PricingService);
            Service = new Serv(ServiceRepo);
            MedicalRecordService = new MedicalRecordService(MedicalRecordRepo, AnimalRepo);

            Console.WriteLine("Ветеринарная клиника");
            MainMenu();
        }

        /// <summary>
        /// Создаёт папку data и необходимые json-файлы, если их нет.
        /// Не создаёт logs.txt и receipts.
        /// </summary>
        static void EnsureDataFiles()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dataDir = Path.Combine(baseDir, "data");
                Directory.CreateDirectory(dataDir);

                string[] requiredFiles = {
                    "owners.json",
                    "animals.json",
                    "veterinarians.json",
                    "appointments.json",
                    "services.json",
                    "medicalrecords.json",
                    "visits.json"
                };

                foreach (var file in requiredFiles)
                {
                    string path = Path.Combine(dataDir, file);
                    if (!File.Exists(path)) File.WriteAllText(path, "[]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при создании файлов данных: " + ex.Message);
            }
        }

        static void MainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Главное меню ===");
                Console.WriteLine("1) Владельцы");
                Console.WriteLine("2) Животные");
                Console.WriteLine("3) Ветеринары");
                Console.WriteLine("4) Услуги");
                Console.WriteLine("5) Приёмы");
                Console.WriteLine("6) Медкарты");
                Console.WriteLine("7) Визиты / Завершить визит");
                Console.WriteLine("0) Выход");
                Console.Write("Выберите пункт: ");

                string choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": OwnersMenu(); break;
                    case "2": AnimalsMenu(); break;
                    case "3": VeterinariansMenu(); break;
                    case "4": ServicesMenu(); break;
                    case "5": AppointmentsMenu(); break;
                    case "6": MedicalRecordsMenu(); break;
                    case "7": VisitsMenu(); break;
                    case "0": Console.WriteLine("Выход. Пока."); return;
                    default: Console.WriteLine("Неизвестный пункт."); break;
                }
            }
        }

        #region Owners
        static void OwnersMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Владельцы ---");
                Console.WriteLine("1) Показать всех владельцев");
                Console.WriteLine("2) Добавить владельца");
                Console.WriteLine("3) Редактировать владельца");
                Console.WriteLine("4) Удалить владельца");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListOwners(); break;
                    case "2":
                        try { AddOwner(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "3":
                        try { EditOwner(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "4":
                        try { DeleteOwner(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListOwners()
        {
            var list = OwnerRepo.GetAll().ToList();
            if (!list.Any()) { Console.WriteLine("(владелецов нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var o = list[i];
                Console.WriteLine($"{i + 1}) {o.FullName} | Тел: {o.Phone} | Email: {o.Email} | Id: {o.Id}");
            }
        }

        static void AddOwner()
        {
            // Все Read* методы поддерживают ввод "0" — отмена
            string name = ReadNonEmptyStringOrCancel("ФИО: ");
            string phone = ReadStringOrCancel("Телефон: ");
            string email = ReadStringOrCancel("Email: ");
            string address = ReadStringOrCancel("Адрес: ");

            var owner = new Owner
            {
                FullName = name,
                Phone = phone,
                Email = email,
                Address = address
            };
            OwnerRepo.Add(owner);
            Console.WriteLine("Владелец добавлен. Id: " + owner.Id);
        }

        static void EditOwner()
        {
            var list = OwnerRepo.GetAll().ToList();
            var owner = SelectFromList(list, o => $"{o.FullName} ({o.Phone})");
            if (owner == null) return;

            string name = ReadStringOrCancel($"ФИО [{owner.FullName}]: ");
            string phone = ReadStringOrCancel($"Телефон [{owner.Phone}]: ");
            string email = ReadStringOrCancel($"Email [{owner.Email}]: ");
            string addr = ReadStringOrCancel($"Адрес [{owner.Address}]: ");

            if (!string.IsNullOrWhiteSpace(name)) owner.FullName = name;
            if (!string.IsNullOrWhiteSpace(phone)) owner.Phone = phone;
            if (!string.IsNullOrWhiteSpace(email)) owner.Email = email;
            if (!string.IsNullOrWhiteSpace(addr)) owner.Address = addr;

            OwnerRepo.Update(owner);
            Console.WriteLine("Владелец обновлён.");
        }

        static void DeleteOwner()
        {
            var list = OwnerRepo.GetAll().ToList();
            var owner = SelectFromList(list, o => $"{o.FullName} ({o.Phone})");
            if (owner == null) return;

            Console.Write($"Удалить {owner.FullName}? (y/N, 0 - отмена): ");
            var c = Console.ReadLine();
            if (c == "0") throw new OperationCanceledException();
            if (c?.ToLower() == "y")
            {
                var animals = AnimalRepo.GetAll().Where(a => a.OwnerId == owner.Id).ToList();
                foreach (var a in animals)
                {
                    a.OwnerId = Guid.Empty;
                    AnimalRepo.Update(a);
                }

                OwnerRepo.Remove(owner.Id);
                Console.WriteLine("Удалено.");
            }
            else Console.WriteLine("Отменено.");
        }
        #endregion

        #region Animals
        static void AnimalsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Животные ---");
                Console.WriteLine("1) Показать всех животных");
                Console.WriteLine("2) Добавить животное");
                Console.WriteLine("3) Редактировать животное");
                Console.WriteLine("4) Удалить животное");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListAnimals(); break;
                    case "2":
                        try { AddAnimal(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "3":
                        try { EditAnimal(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "4":
                        try { DeleteAnimal(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListAnimals()
        {
            var list = AnimalRepo.GetAll().ToList();
            if (!list.Any()) { Console.WriteLine("(животных нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                var owner = OwnerRepo.GetById(a.OwnerId);
                string specExtra = "";

                if (a is Dog d) specExtra = d.IsTrained ? "дрессирован" : "не дрессирован";
                else if (a is Cat c) specExtra = c.IsIndoor ? "домашняя" : "уличная";
                else if (a is Parrot p) specExtra = p.CanTalk ? "умеет говорить" : "не умеет говорить";
                // OtherAnimal не имеет доп. полей

                var extraPart = string.IsNullOrWhiteSpace(specExtra) ? "" : $" | {specExtra}";

                Console.WriteLine($"{i + 1}) {a.GetInfo()}{extraPart} | Владелец: {(owner != null ? owner.FullName : "(нет)")} | Id: {a.Id}");
            }
        }

        static void AddAnimal()
        {
            // Выбор вида. 0 = отмена
            string species = null;
            while (true)
            {
                Console.WriteLine("Выберите вид животного (0 - отмена):");
                Console.WriteLine("1) Собака (Dog)");
                Console.WriteLine("2) Кошка (Cat)");
                Console.WriteLine("3) Попугай (Parrot)");
                Console.WriteLine("4) Другой (Other) — введите название вида");
                Console.Write("Введите 1,2,3,4 или 0: ");
                var sel = Console.ReadLine()?.Trim();
                if (sel == "0") throw new OperationCanceledException();
                if (sel == "1") { species = "Dog"; break; }
                if (sel == "2") { species = "Cat"; break; }
                if (sel == "3") { species = "Parrot"; break; }
                if (sel == "4")
                {
                    string custom = ReadNonEmptyStringOrCancel("Введите название вида (например: Rabbit, Hamster и т.д.): ");
                    species = custom;
                    break;
                }
                Console.WriteLine("Некорректный ввод — попробуйте ещё.");
            }

            Animal animal;
            if (species == "Dog") animal = new Dog { Species = "Dog" };
            else if (species == "Cat") animal = new Cat { Species = "Cat" };
            else if (species == "Parrot") animal = new Parrot { Species = "Parrot" };
            else animal = new OtherAnimal { Species = species }; // универсальный класс

            // Дальше все шаги могут быть отменены вводом 0
            animal.Name = ReadNonEmptyStringOrCancel("Имя: ");
            animal.Breed = ReadStringOrCancel("Порода: ");
            animal.DateOfBirth = ReadDateOrDefaultOrCancel("Дата рождения (yyyy-MM-dd) [Enter = год назад] (0 - отмена): ", DateTime.Now.AddYears(-1));

            // Выбор владельца (можно пропустить): Enter = пропустить, 0 = отмена
            var owners = OwnerRepo.GetAll().ToList();
            if (!owners.Any())
            {
                Console.WriteLine("В системе нет владельцев — животное не будет назначено владельцу.");
            }
            else
            {
                var owner = SelectFromListAllowEmptyCancellable(owners, o => o.FullName);
                if (owner != null) animal.OwnerId = owner.Id;
            }

            // Специфичные поля: обязательно вводятся пользователем (0 - отмена)
            if (animal is Dog dog)
            {
                dog.IsTrained = ReadYesNoOrCancel("Животное дрессировано? (y/N, 0 - отмена): ");
            }
            else if (animal is Cat cat)
            {
                cat.IsIndoor = ReadYesNoOrCancel("Домашняя кошка? (y/N, 0 - отмена): ");
            }
            else if (animal is Parrot parrot)
            {
                parrot.CanTalk = ReadYesNoOrCancel("Попугай умеет говорить? (y/N, 0 - отмена): ");
            }

            AnimalRepo.Add(animal);
            Console.WriteLine("Животное добавлено. Id: " + animal.Id);
        }

        static void EditAnimal()
        {
            var list = AnimalRepo.GetAll().ToList();
            var animal = SelectFromList(list, a => a.GetInfo());
            if (animal == null) return;

            Console.WriteLine($"Вид (species): {animal.Species} — смена вида не поддерживается. Если нужно, удалите и добавьте заново.");

            string name = ReadStringOrCancel($"Имя [{animal.Name}]: ");
            string breed = ReadStringOrCancel($"Порода [{animal.Breed}]: ");
            DateTime? dob = ReadDateNullableOrCancel($"Дата рождения [{animal.DateOfBirth:yyyy-MM-dd}] (введите дата или Enter оставить): ");

            if (!string.IsNullOrWhiteSpace(name)) animal.Name = name;
            if (!string.IsNullOrWhiteSpace(breed)) animal.Breed = breed;
            if (dob.HasValue) animal.DateOfBirth = dob.Value;

            // Изменение владельца (опционно). 0 = отмена
            if (ReadYesNoOrCancel("Изменить владельца? (y/N, 0 - отмена): "))
            {
                var owners = OwnerRepo.GetAll().ToList();
                if (owners.Any())
                {
                    var owner = SelectFromListAllowEmptyCancellable(owners, o => o.FullName);
                    if (owner != null) animal.OwnerId = owner.Id;
                }
                else Console.WriteLine("В системе нет владельцев.");
            }

            // Специфичное поле — редактирование (0 = оставить как есть, ввод 0 — отмена операции)
            if (animal is Dog dog)
            {
                bool? val = ReadYesNoNullableCancelAllowed($"Дрессирован? [{(dog.IsTrained ? "y" : "N")}] (y/N, пусто = не менять, 0 - отмена): ");
                if (val.HasValue) dog.IsTrained = val.Value;
            }
            else if (animal is Cat cat)
            {
                bool? val = ReadYesNoNullableCancelAllowed($"Домашняя? [{(cat.IsIndoor ? "y" : "N")}] (y/N, пусто = не менять, 0 - отмена): ");
                if (val.HasValue) cat.IsIndoor = val.Value;
            }
            else if (animal is Parrot parrot)
            {
                bool? val = ReadYesNoNullableCancelAllowed($"Умеет говорить? [{(parrot.CanTalk ? "y" : "N")}] (y/N, пусто = не менять, 0 - отмена): ");
                if (val.HasValue) parrot.CanTalk = val.Value;
            }

            AnimalRepo.Update(animal);
            Console.WriteLine("Обновлено.");
        }

        static void DeleteAnimal()
        {
            var list = AnimalRepo.GetAll().ToList();
            var animal = SelectFromList(list, a => a.GetInfo());
            if (animal == null) return;
            Console.Write($"Удалить {animal.Name}? (y/N, 0 - отмена): ");
            var c = Console.ReadLine();
            if (c == "0") throw new OperationCanceledException();
            if (c?.ToLower() == "y")
            {
                AnimalRepo.Remove(animal.Id);
                Console.WriteLine("Удалено.");
            }
            else Console.WriteLine("Отменено.");
        }
        #endregion

        #region Veterinarians
        static void VeterinariansMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Ветеринары ---");
                Console.WriteLine("1) Показать всех ветеринаров");
                Console.WriteLine("2) Добавить ветеринара");
                Console.WriteLine("3) Редактировать ветеринара");
                Console.WriteLine("4) Удалить ветеринара");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListVets(); break;
                    case "2":
                        try { AddVet(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "3":
                        try { EditVet(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "4":
                        try { DeleteVet(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListVets()
        {
            var list = VeterinarianRepo.GetAll().ToList();
            if (!list.Any()) { Console.WriteLine("(ветеринаров нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                Console.WriteLine($"{i + 1}) {v.FullName} | Спец: {v.Specialization} | Тел: {v.Phone} | Id: {v.Id}");
            }
        }

        static void AddVet()
        {
            string name = ReadNonEmptyStringOrCancel("ФИО: ");
            string spec = ReadStringOrCancel("Специализация: ");
            string phone = ReadStringOrCancel("Телефон: ");

            var v = new Veterinarian { FullName = name, Specialization = spec, Phone = phone };
            VeterinarianRepo.Add(v);
            Console.WriteLine("Ветеринар добавлен. Id: " + v.Id);
        }

        static void EditVet()
        {
            var list = VeterinarianRepo.GetAll().ToList();
            var v = SelectFromList(list, x => $"{x.FullName} ({x.Specialization})");
            if (v == null) return;
            string name = ReadStringOrCancel($"ФИО [{v.FullName}]: ");
            string spec = ReadStringOrCancel($"Спец [{v.Specialization}]: ");
            string phone = ReadStringOrCancel($"Тел [{v.Phone}]: ");

            if (!string.IsNullOrWhiteSpace(name)) v.FullName = name;
            if (!string.IsNullOrWhiteSpace(spec)) v.Specialization = spec;
            if (!string.IsNullOrWhiteSpace(phone)) v.Phone = phone;

            VeterinarianRepo.Update(v);
            Console.WriteLine("Обновлено.");
        }

        static void DeleteVet()
        {
            var list = VeterinarianRepo.GetAll().ToList();
            var v = SelectFromList(list, x => x.FullName);
            if (v == null) return;
            Console.Write($"Удалить {v.FullName}? (y/N, 0 - отмена): ");
            var c = Console.ReadLine();
            if (c == "0") throw new OperationCanceledException();
            if (c?.ToLower() == "y")
            {
                VeterinarianRepo.Remove(v.Id);
                Console.WriteLine("Удалено.");
            }
            else Console.WriteLine("Отменено.");
        }
        #endregion

        #region Services
        static void ServicesMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Услуги ---");
                Console.WriteLine("1) Показать услуги");
                Console.WriteLine("2) Добавить услугу");
                Console.WriteLine("3) Редактировать услугу");
                Console.WriteLine("4) Удалить услугу");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListServices(); break;
                    case "2":
                        try { AddService(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "3":
                        try { EditService(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "4":
                        try { DeleteService(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListServices()
        {
            var list = ServiceRepo.GetAll().ToList();
            if (!list.Any()) { Console.WriteLine("(услуг нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                Console.WriteLine($"{i + 1}) {s.Name} | Цена: {s.Price} | Длительность: {s.Duration.TotalMinutes} мин | Id: {s.Id}");
            }
        }

        static void AddService()
        {
            string name = ReadNonEmptyStringOrCancel("Название: ");
            decimal price = ReadDecimalOrCancel("Цена: ");
            int minutes = ReadIntOrCancel("Длительность в минутах: ", 1, 1440);
            string desc = ReadStringOrCancel("Описание: ");

            var svc = new Service { Name = name, Price = price, Duration = TimeSpan.FromMinutes(minutes), Description = desc };
            ServiceRepo.Add(svc);
            Console.WriteLine("Услуга добавлена. Id: " + svc.Id);
        }

        static void EditService()
        {
            var list = ServiceRepo.GetAll().ToList();
            var s = SelectFromList(list, x => $"{x.Name} ({x.Price})");
            if (s == null) return;
            string name = ReadStringOrCancel($"Название [{s.Name}]: ");
            string priceS = ReadStringOrCancel($"Цена [{s.Price}]: ");
            string minsS = ReadStringOrCancel($"Длительность (мин) [{s.Duration.TotalMinutes}]: ");
            string desc = ReadStringOrCancel($"Описание [{s.Description}]: ");

            if (!string.IsNullOrWhiteSpace(name)) s.Name = name;
            if (decimal.TryParse(priceS, out var p)) s.Price = p;
            if (double.TryParse(minsS, out var m)) s.Duration = TimeSpan.FromMinutes(m);
            if (!string.IsNullOrWhiteSpace(desc)) s.Description = desc;

            ServiceRepo.Update(s);
            Console.WriteLine("Обновлено.");
        }

        static void DeleteService()
        {
            var list = ServiceRepo.GetAll().ToList();
            var s = SelectFromList(list, x => x.Name);
            if (s == null) return;
            Console.Write($"Удалить услугу {s.Name}? (y/N, 0 - отмена): ");
            var c = Console.ReadLine();
            if (c == "0") throw new OperationCanceledException();
            if (c?.ToLower() == "y")
            {
                ServiceRepo.Remove(s.Id);
                Console.WriteLine("Удалено.");
            }
            else Console.WriteLine("Отменено.");
        }
        #endregion

        #region Appointments / MedicalRecords / Visits
        static void AppointmentsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Приёмы ---");
                Console.WriteLine("1) Показать приёмы по дате");
                Console.WriteLine("2) Запланировать приём");
                Console.WriteLine("3) Отменить приём");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListAppointmentsByDate(); break;
                    case "2":
                        try { ScheduleAppointment(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "3":
                        try { CancelAppointment(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListAppointmentsByDate()
        {
            DateTime date = ReadDateOrDefaultOrCancel("Дата (yyyy-MM-dd) [Enter = сегодня] (0 - отмена): ", DateTime.Today);
            var list = AppointmentRepo.GetAll().Where(a => a.StartTime.Date == date.Date).OrderBy(a => a.StartTime).ToList();
            if (!list.Any()) { Console.WriteLine("(приёмов нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                var animal = AnimalRepo.GetById(a.AnimalId);
                var vet = VeterinarianRepo.GetById(a.VeterinarianId);
                Console.WriteLine($"{i + 1}) {a.StartTime:g} - {(animal?.Name ?? "(нет)")} с {(vet?.FullName ?? "(нет)")} | Причина: {a.Reason} | Статус: {a.Status} | Id: {a.Id}");
            }
        }

        static void ScheduleAppointment()
        {
            var animals = AnimalRepo.GetAll().ToList();
            var vets = VeterinarianRepo.GetAll().ToList();
            if (!animals.Any()) { Console.WriteLine("Нет животных. Добавьте животное первым."); return; }
            if (!vets.Any()) { Console.WriteLine("Нет ветеринаров. Добавьте ветеринара первым."); return; }

            var animal = SelectFromList(animals, a => a.GetInfo());
            if (animal == null) return;
            var vet = SelectFromList(vets, v => v.FullName);
            if (vet == null) return;

            DateTime date = ReadDateOrDefaultOrCancel("Дата (yyyy-MM-dd) [Enter = сегодня]: ", DateTime.Today);
            TimeSpan time = ReadTimeOrDefaultOrCancel("Время (HH:mm) [Enter = сейчас+1ч]: ", DateTime.Now.AddHours(1).TimeOfDay);
            int minutes = ReadIntOrCancel("Длительность (мин) [например 30]: ", 1, 1440);
            string reason = ReadStringOrCancel("Причина (0 - отмена): ");

            var appt = new Appointment
            {
                AnimalId = animal.Id,
                VeterinarianId = vet.Id,
                StartTime = date.Date + time,
                Duration = TimeSpan.FromMinutes(minutes),
                Reason = reason
            };

            try
            {
                AppointmentService.ScheduleAppointment(appt);
                Console.WriteLine("Приём запланирован. Id: " + appt.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при планировании: " + ex.Message);
            }
        }

        static void CancelAppointment()
        {
            var list = AppointmentRepo.GetAll().Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
            var appt = SelectFromList(list, a => $"{a.StartTime:g} - {AnimalRepo.GetById(a.AnimalId)?.Name ?? "(нет)"}");
            if (appt == null) return;
            Console.Write($"Отменить приём {appt.Id}? (y/N, 0 - отмена): ");
            var c = Console.ReadLine();
            if (c == "0") throw new OperationCanceledException();
            if (c?.ToLower() == "y")
            {
                AppointmentService.CancelAppointment(appt.Id);
                Console.WriteLine("Отменено.");
            }
        }

        static void MedicalRecordsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Медкарты ---");
                Console.WriteLine("1) Показать записи по животному");
                Console.WriteLine("2) Добавить запись");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListRecordsForAnimal(); break;
                    case "2":
                        try { AddMedicalRecord(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListRecordsForAnimal()
        {
            var animal = SelectFromList(AnimalRepo.GetAll().ToList(), a => a.GetInfo());
            if (animal == null) return;
            var recs = MedicalRecordRepo.GetAll().Where(r => r.AnimalId == animal.Id).OrderByDescending(r => r.Date).ToList();
            if (!recs.Any()) { Console.WriteLine("(записей нет)"); return; }
            for (int i = 0; i < recs.Count; i++)
            {
                var r = recs[i];
                var doc = VeterinarianRepo.GetById(r.DoctorId);
                Console.WriteLine($"{i + 1}) {r.Date:g} | Врач: {doc?.FullName ?? "(нет)"} | Диагноз: {r.Diagnosis} | Id: {r.Id}");
            }
        }

        static void AddMedicalRecord()
        {
            var animal = SelectFromList(AnimalRepo.GetAll().ToList(), a => a.GetInfo());
            if (animal == null) return;
            var vet = SelectFromList(VeterinarianRepo.GetAll().ToList(), v => v.FullName);
            if (vet == null) return;
            string diag = ReadNonEmptyStringOrCancel("Диагноз: ");
            string desc = ReadStringOrCancel("Описание: ");
            string pres = ReadStringOrCancel("Препараты (через запятую): ");

            var rec = new MedicalRecord
            {
                AnimalId = animal.Id,
                DoctorId = vet.Id,
                Diagnosis = diag,
                Description = desc,
                Prescriptions = (pres ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()
            };

            MedicalRecordService.AddRecord(rec);
            Console.WriteLine("Запись добавлена. Id: " + rec.Id);
        }

        static void VisitsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Визиты ---");
                Console.WriteLine("1) Показать визиты");
                Console.WriteLine("2) Завершить приём (создать визит)");
                Console.WriteLine("0) Назад");
                Console.Write("Выберите: ");
                var s = Console.ReadLine();
                switch (s)
                {
                    case "1": ListVisits(); break;
                    case "2":
                        try { CompleteVisitInteractive(); } catch (OperationCanceledException) { Console.WriteLine("Операция отменена."); }
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
        }

        static void ListVisits()
        {
            var list = VisitRepo.GetAll().ToList();
            if (!list.Any()) { Console.WriteLine("(визитов нет)"); return; }
            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                var animal = AnimalRepo.GetById(v.AnimalId);
                var vet = VeterinarianRepo.GetById(v.VeterinarianId);
                Console.WriteLine($"{i + 1}) {v.Date:g} | Животное: {animal?.Name ?? "(нет)"} | Врач: {vet?.FullName ?? "(нет)"} | Итого: {v.TotalPrice} | Id: {v.Id}");
            }
        }

        static void CompleteVisitInteractive()
        {
            var appts = AppointmentRepo.GetAll().Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
            if (!appts.Any()) { Console.WriteLine("Нет запланированных приёмов."); return; }
            var appt = SelectFromList(appts, a => $"{a.StartTime:g} - {AnimalRepo.GetById(a.AnimalId)?.Name ?? "(нет)"}");
            if (appt == null) return;

            var services = ServiceRepo.GetAll().ToList();
            var chosenIds = new List<Guid>();
            if (services.Any())
            {
                Console.WriteLine("Выберите услуги (введите номера через запятую) или Enter, чтобы пропустить (0 - отмена):");
                for (int i = 0; i < services.Count; i++) Console.WriteLine($"{i + 1}) {services[i].Name} - {services[i].Price}");
                Console.Write("Выбрано: ");
                var sel = Console.ReadLine();
                if (sel == "0") throw new OperationCanceledException();
                if (!string.IsNullOrWhiteSpace(sel))
                {
                    var parts = sel.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (int.TryParse(p.Trim(), out var idx) && idx >= 1 && idx <= services.Count)
                            chosenIds.Add(services[idx - 1].Id);
                    }
                }
            }

            string diag = ReadStringOrCancel("Диагноз: ");

            try
            {
                var visit = VisitService.CompleteVisit(appt.Id, chosenIds, diag ?? "");
                Console.WriteLine($"Визит завершён. Итоговая цена: {visit.TotalPrice}. Id визита: {visit.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при завершении визита: " + ex.Message);
            }
        }
        #endregion

        #region Helpers / Input validation (поддерживают ввод "0" как отмену)
        // Читает непустую строку; ввод "0" — бросает OperationCanceledException
        static string ReadNonEmptyStringOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("Поле не может быть пустым. Попробуйте ещё (или введите 0 для отмены).");
            }
        }

        // Читает строку; если введено "0" — отмена; если пусто — возвращает пустую строку
        static string ReadStringOrCancel(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (s == "0") throw new OperationCanceledException();
            return string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
        }

        // Читает дату или возвращает значение по умолчанию; ввод "0" => отмена; пусто => defaultValue
        static DateTime ReadDateOrDefaultOrCancel(string prompt, DateTime defaultValue)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (s == "0") throw new OperationCanceledException();
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            if (DateTime.TryParse(s, out var dt)) return dt;
            Console.WriteLine("Некорректная дата. Попробуйте ещё (или введите 0 для отмены).");
            return ReadDateOrDefaultOrCancel(prompt, defaultValue);
        }

        // Читает дату: пустая строка => null; "0" => отмена
        static DateTime? ReadDateNullableOrCancel(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (s == "0") throw new OperationCanceledException();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            Console.WriteLine("Некорректная дата. Попробуйте ещё (или 0 для отмены).");
            return ReadDateNullableOrCancel(prompt);
        }

        // Читает время или возвращает defaultTime; "0" => отмена; пусто => default
        static TimeSpan ReadTimeOrDefaultOrCancel(string prompt, TimeSpan defaultTime)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (s == "0") throw new OperationCanceledException();
            if (string.IsNullOrWhiteSpace(s)) return defaultTime;
            if (TimeSpan.TryParse(s, out var ts)) return ts;
            Console.WriteLine("Некорректное время. Попробуйте ещё (или 0 для отмены).");
            return ReadTimeOrDefaultOrCancel(prompt, defaultTime);
        }

        // Читает int в диапазоне; "0" => отмена; если пусто — ошибку; (для полей где допускается default, есть отдельный метод)
        static int ReadIntOrCancel(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (int.TryParse(s, out var val))
                {
                    if (val < min || val > max) Console.WriteLine($"Введите число в диапазоне {min}–{max} (или 0 для отмены).");
                    else return val;
                }
                else Console.WriteLine("Некорректный ввод. Введите целое число (или 0 для отмены).");
            }
        }

        // Читает decimal; "0" => отмена
        static decimal ReadDecimalOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (decimal.TryParse(s, out var val)) return val;
                Console.WriteLine("Некорректный ввод. Введите число (например, 250.50) или 0 для отмены.");
            }
        }

        // Да/Нет; "0" => отмена
        static bool ReadYesNoOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (string.IsNullOrWhiteSpace(s)) return false; // default = false
                var t = s.Trim().ToLower();
                if (t == "y") return true;
                if (t == "n") return false;
                Console.WriteLine("Введите y (да) или n (нет), или 0 для отмены.");
            }
        }

        // Да/Нет nullable: пусто = null (не менять), "0" => отмена
        static bool? ReadYesNoNullableCancelAllowed(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (string.IsNullOrWhiteSpace(s)) return null;
                var t = s.Trim().ToLower();
                if (t == "y") return true;
                if (t == "n") return false;
                Console.WriteLine("Введите y, n или Enter для пропуска, 0 для отмены.");
            }
        }

        // Читает да/нет; пусто = false; 0 = отмена (альтернативный вариант)
        static bool? ReadYesNoNullableOrCancel(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (string.IsNullOrWhiteSpace(s)) return null;
                var t = s.Trim().ToLower();
                if (t == "y") return true;
                if (t == "n") return false;
                Console.WriteLine("Введите y, n или 0 для отмены.");
            }
        }

        // Выбор из списка: 0 = отмена (бросает исключение), иначе возвращает выбранный элемент
        static T SelectFromList<T>(List<T> items, Func<T, string> displayFunc) where T : class
        {
            if (items == null || !items.Any()) return null;
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {displayFunc(items[i])}");
            }
            while (true)
            {
                Console.Write("Выберите номер: ");
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (!int.TryParse(s, out var idx)) { Console.WriteLine("Некорректный ввод."); continue; }
                if (idx == 0) throw new OperationCanceledException();
                if (idx < 1 || idx > items.Count) { Console.WriteLine("Номер вне диапазона."); continue; }
                return items[idx - 1];
            }
        }

        // Селектор, позволяющий Enter для пропуска (возвращает null), но 0 = отмена
        static T SelectFromListAllowEmptyCancellable<T>(List<T> items, Func<T, string> displayFunc) where T : class
        {
            if (items == null || !items.Any()) return null;
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {displayFunc(items[i])}");
            }
            while (true)
            {
                Console.Write("Выберите номер: ");
                var s = Console.ReadLine();
                if (s == "0") throw new OperationCanceledException();
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (!int.TryParse(s, out var idx)) { Console.WriteLine("Некорректный ввод."); continue; }
                if (idx < 1 || idx > items.Count) { Console.WriteLine("Номер вне диапазона."); continue; }
                return items[idx - 1];
            }
        }
        #endregion
    }
}
