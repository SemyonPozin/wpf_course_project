using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.Models
{
    public class TutorInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string? Description { get; set; }
        public string? Subject { get; set; } 
        public float PricePerHour { get; set; }
        public User User { get; set; }
    }

}
