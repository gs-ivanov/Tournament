namespace Tournament.Models.Teams
{
    public class TeamsQueryModel
    {
        public int Id { get; set; }

        public string Name { get; init; }

        public string City { get; init; }

        public string Trener { get; init; }

        public int Wins { get; init; }

        public int Losts { get; init; }

        public int Draws { get; init; }

        public int GoalsScored { get; init; }

        public int GoalsConceded { get; init; }

        public int GoalDifference { get; init; }

        public bool IsEditable { get; set; }

    }
}
