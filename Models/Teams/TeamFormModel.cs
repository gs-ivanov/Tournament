namespace Tournament.Models.Teams
{
    using System.ComponentModel.DataAnnotations;

    using static Data.DataConstants.Team;

    public class TeamFormModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(NameMaxLength, MinimumLength = NameMinLength)]
        public string Name { get; init; }

        [Display(Name="Брой на отборите в новия списък.")]
        public int TeamCount { get; init; }

    }
}