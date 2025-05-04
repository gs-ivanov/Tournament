namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using Tournament.Data;
    using Tournament.Data.Models;

    public interface IMatchGeneratorDbl
    {
        List<Round> GenerateRounds(TurnirDbContext dbContext, List<Team> teams, Tournament tournament);
    }
}
