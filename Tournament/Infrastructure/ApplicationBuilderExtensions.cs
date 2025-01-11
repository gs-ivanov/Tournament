namespace Tournament.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using Tournament.Data;
    using Tournament.Models;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder PrepareDatabase(this IApplicationBuilder app)
        {
            using var scopedservices = app.ApplicationServices.CreateScope();

            var data = scopedservices.ServiceProvider.GetService<TournamentDbContext>();
            
            data.Database.Migrate();

            SeedTeams(data);

            return app;
        }

        private static void SeedTeams(TournamentDbContext data)
        {
            if (data.Teams.Any())
            {
                return;
            }

            data.Teams.AddRange(new[]
            {
                new Team{Name="Ludogorec", Town="Razgrad" },
                new Team{Name="Levski", Town="Sofia" },
                new Team{Name="Cherno More", Town="Varna" },
            });

            data.SaveChanges();
        }
    }
}
