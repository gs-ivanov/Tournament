﻿namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using global::Tournament.Models.Teams;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    // Модел Manager
    public class Manager
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Name { get; set; }

        public TournamentType TournamentType { get; set; }

        public ICollection<TeamViewModel> Teams { get; set; } = new List<TeamViewModel>();
    }
}
