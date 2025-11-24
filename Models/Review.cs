using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int TutorId { get; set; }
        public int AuthorId { get; set; }

        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Tutor { get; set; }
        public User Author { get; set; }
    }

}
