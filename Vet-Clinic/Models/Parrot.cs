namespace Vet_Clinic.Models
{
    public class Parrot : Animal
    {
        public bool CanTalk { get; set; } = false; // умеет говорить или нет

        public override string GetInfo()
        {
            var breed = string.IsNullOrWhiteSpace(Breed) ? "" : $" ({Breed})";
            return $"{Name} — Parrot{breed}, {GetAge()} years";
        }
    }
}
