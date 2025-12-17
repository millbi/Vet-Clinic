namespace Vet_Clinic.Models
{
    public class Cat : Animal
    {
        public bool IsIndoor { get; set; } = false; // домашняя или уличная кошка
        public override string GetInfo()
        {
            var breed = string.IsNullOrWhiteSpace(Breed) ? "" : $" ({Breed})";
            return $"{Name} — Cat{breed}, {GetAge()} years";
        }
    }
}
