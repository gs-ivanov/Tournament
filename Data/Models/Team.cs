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

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
