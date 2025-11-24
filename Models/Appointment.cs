using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }

        public int DayOfWeek { get; set; }
        public string Time { get; set; }
        public string Comment { get; set; }

        //0 = pending 1 = accepted, 2 = rejected
        public int Status { get; set; }

        public User Tutor { get; set; }
        public User Student { get; set; }
    }

}
