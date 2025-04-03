namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;

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

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Required]
        public string JsonPayload { get; set; }

        public static string GenerateJson(Team team, TournamentType type)
        {
            var payload = new
            {
                Team = new
                {
                    team.Name,
                    team.CoachName,
                    team.ContactEmail,
                    team.LogoUrl
                },
                TournamentType = type,
                Timestamp = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
