namespace Tournament.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
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

            builder.Entity<ManagerRequest>()
                .HasOne(m => m.Team)
                .WithMany()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}
