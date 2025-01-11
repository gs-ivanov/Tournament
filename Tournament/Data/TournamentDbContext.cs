namespace Tournament.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Tournament.Models;

    public class TournamentDbContext : IdentityDbContext
    {

        public TournamentDbContext(DbContextOptions<TournamentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; init; }

        public DbSet<Team> Teams { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<Player>()
                .HasOne(c => c.Team)
                .WithMany(c => c.Players)
                .HasForeignKey(c => c.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}
