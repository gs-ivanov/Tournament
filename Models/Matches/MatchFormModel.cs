namespace Tournament.Models.Matches
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    // ViewModel за създаване и редакция на мач
    public class MatchFormModel
    {
        [Required]
        [Display(Name = "Отбор A")]
        public int TeamAId { get; set; }

        [Required]
        [Display(Name = "Отбор B")]
        public int TeamBId { get; set; }

        [Required]
        [Display(Name = "Дата на мача")]
        public DateTime MatchDate { get; set; }

        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }

        public IEnumerable<SelectListItem> Teams { get; set; }
    }
}