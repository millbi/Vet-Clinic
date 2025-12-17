using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vet_Clinic.DataAccess
{
    /// <summary>
    /// JsonRepository с поддержкой полиморфной десериализации для Animal.
    /// Для обычных классов (Owner, Veterinarian, Service и т.д.) работает как минимальный репозиторий.
    /// </summary>
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly List<T> _items = new List<T>();

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public JsonRepository(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                Save(); // создаём пустой файл
                return;
            }

            try
            {
                var json = File.ReadAllText(_filePath);

                // Если это Animal — используем fallback для наследников
                if (typeof(T).IsSubclassOf(typeof(Models.Animal)) || typeof(T) == typeof(Models.Animal))
                {
                    LoadAnimals(json);
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(json, _settings);
                    if (data != null)
                        _items.AddRange(data);
                }
            }
            catch
            {
                // поврежденный файл - перезаписываем пустым
                Save();
            }
        }

        private void LoadAnimals(string json)
        {
            try
            {
                var jarr = JArray.Parse(json);
                foreach (var token in jarr)
                {
                    if (!(token is JObject job)) continue;

                    Models.Animal animal = null;

                    // Если есть $type
                    var typeStr = job.Property("$type")?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(typeStr))
                    {
                        var type = Type.GetType(typeStr, false);
                        if (type != null)
                            animal = (Models.Animal)JsonConvert.DeserializeObject(job.ToString(), type, _settings);
                    }

                    // Если нет $type — определяем по Species
                    if (animal == null)
                    {
                        var species = job.Property("Species")?.Value?.ToString()?.ToLowerInvariant();
                        Type concreteType = typeof(Models.OtherAnimal); // fallback
                        switch (species)
                        {
                            case "dog":
                                concreteType = typeof(Models.Dog);
                                break;
                            case "cat":
                                concreteType = typeof(Models.Cat);
                                break;
                            case "parrot":
                                concreteType = typeof(Models.Parrot);
                                break;
                        }
                        animal = (Models.Animal)JsonConvert.DeserializeObject(job.ToString(), concreteType, _settings);
                    }

                    if (animal != null)
                        _items.Add(animal as T);
                }
            }
            catch
            {
                // при ошибках просто оставляем пустой список
            }
        }

        public IEnumerable<T> GetAll() => _items.ToList();

        public T GetById(Guid id)
        {
            var prop = typeof(T).GetProperty("Id");
            return _items.FirstOrDefault(x => (Guid)prop.GetValue(x) == id);
        }

        public void Add(T item)
        {
            _items.Add(item);
            Save();
        }

        public void Update(T item)
        {
            var prop = typeof(T).GetProperty("Id");
            Guid id = (Guid)prop.GetValue(item);
            var index = _items.FindIndex(x => (Guid)prop.GetValue(x) == id);
            if (index != -1)
            {
                _items[index] = item;
                Save();
            }
        }

        public void Remove(Guid id)
        {
            var prop = typeof(T).GetProperty("Id");
            var found = _items.FirstOrDefault(x => (Guid)prop.GetValue(x) == id);
            if (found != null)
            {
                _items.Remove(found);
                Save();
            }
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_items, _settings));
        }
    }
}