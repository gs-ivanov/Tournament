namespace Tournament.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;
    using Tournament.Data.Models;

    public class TurnirDbContext : IdentityDbContext<User>
    {
        public TurnirDbContext(DbContextOptions<TurnirDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<ManagerRequest> ManagerRequests { get; set; }
        public DbSet<MatchSubscription> MatchSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Ranking>().HasNoKey();

            //builder.Entity<Tournament>().HasData(
            //    new Tournament { Id = 1, Name = "Пролетен", Type = "Knockout", StartDate = DateTime.Now.AddDays(10), IsOpenForApplications = true },
            //    new Tournament { Id = 2, Name = "Летен", Type = "RoundRobin", StartDate = DateTime.Now.AddMonths(1), IsOpenForApplications = true }
            //);


            builder.Entity<ManagerRequest>()
                .HasOne(m => m.Team)
                .WithMany()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tournament>().HasData(
               new Tournament
               {
                   Id = 1,
                   Name = "Пролетен турнир",
                   Type = "Елиминации",
                   StartDate = new DateTime(2025, 5, 10),
                   IsOpenForApplications = true
               },
               new Tournament
               {
                   Id = 2,
                   Name = "Летен шампионат",
                   Type = "Групова фаза",
                   StartDate = new DateTime(2025, 7, 1),
                   IsOpenForApplications = true
               },
               new Tournament
               {
                   Id = 3,
                   Name = "Зимна купа",
                   Type = "Елиминации",
                   StartDate = new DateTime(2025, 12, 5),
                   IsOpenForApplications = false
               }
           );


            base.OnModelCreating(builder);
        }
    }
}
