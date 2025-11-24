using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace coach_search.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? PhotoPath { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int Role { get; set; } // 0 = client, 1 = tutor, 2 = admin
        public string Phone { get; set; }
        public bool IsBlocked { get; set; }

        // Navigation properties
        public TutorInfo TutorInfo { get; set; }
        public ICollection<Review> WrittenReviews { get; set; }
        public ICollection<Appointment> TutorAppointments { get; set; }
        public ICollection<Appointment> StudentAppointments { get; set; }
    }

}
