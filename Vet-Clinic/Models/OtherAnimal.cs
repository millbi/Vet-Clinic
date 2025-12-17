namespace Vet_Clinic.Models
{
    public class OtherAnimal : Animal
    {
        public override string GetInfo()
        {
            var breed = string.IsNullOrWhiteSpace(Breed) ? "" : $" ({Breed})";
            return $"{Name} — {Species}{breed}, {GetAge()} years";
        }
    }
}
