namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ManagerRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public TournamentType TournamentType { get; set; }
        public string JsonPayload { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public static string GenerateJson(Team team, TournamentType type)
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                team.Id,
                team.Name,
                team.CoachName,
                team.LogoUrl,
                team.ContactEmail,
                TournamentType = type.ToString(),
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
            });
        }
    }
}
