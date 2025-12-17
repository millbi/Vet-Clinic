namespace Vet_Clinic.Models
{
    public class Dog : Animal
    {
        public bool IsTrained { get; set; } = false; // дрессированная или нет
        public override string GetInfo()
        {
            var breed = string.IsNullOrWhiteSpace(Breed) ? "" : $" ({Breed})";
            return $"{Name} — Dog{breed}, {GetAge()} years";
        }
    }
}
