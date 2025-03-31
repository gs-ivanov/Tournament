namespace Tournament.Data
{
    using Tournament.Data.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TurnirDbContext : IdentityDbContext<User>
    {
        public TurnirDbContext(DbContextOptions<TurnirDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; init; }

        public DbSet<Match> Matches { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Връзка между Team и Match (Домакин)
            builder
                .Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Връзка между Team и Match (Гост)
            builder
                .Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}
