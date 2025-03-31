namespace Tournament.Models.Teams
{
    using System.Collections.Generic;

    public class AllTeamsQueryModel
    {
        public const int TeamsPerPage = 3;

        public string Name { get; init; }

        public int CurrentPage { get; init; } = 1;

        public IEnumerable<TeamFormModel> Teams { get; set; }
    }
}
