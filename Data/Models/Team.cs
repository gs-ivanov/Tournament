using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tournament.Data.Models
{
    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string CoachName { get; set; }
        public string LogoUrl { get; set; }
        //public string ContactEmail { get; set; }
        public bool FeePaid { get; set; }

        // ✅ Навигационни свойства за мачовете
        public ICollection<Match> MatchesAsTeamA { get; set; } = new List<Match>();
        public ICollection<Match> MatchesAsTeamB { get; set; } = new List<Match>();

        // ✅ Навигационно свойство за заявки
        public ICollection<ManagerRequest> ManagerRequests { get; set; } = new List<ManagerRequest>();
        
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
