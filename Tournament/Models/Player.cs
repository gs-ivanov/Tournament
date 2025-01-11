namespace Tournament.Models
{
    public class Player
    {
        public int Id { get; init; }

        public string Name { get; set; }

        public int Age { get; set; }

        public int TeamId { get; set; }

        public Team Team { get; set; }
    }
}
