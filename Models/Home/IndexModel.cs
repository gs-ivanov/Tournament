namespace Tournament.Models.Home
{
    public class IndexModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string Trener { get; set; }

        public int Wins { get; set; } = 0;

        public int Losts { get; set; } = 0;

    }
}
