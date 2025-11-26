using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
        
        private static readonly string[] DaysOfWeek = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
        private string _dayofweektext;
        [NotMapped]
        public string DayOfWeekText {
            get=> DayOfWeek >= 0 && DayOfWeek < 7 ? DaysOfWeek[DayOfWeek] : "Неизвестно";
            set => _dayofweektext = value;
        }
        public User Tutor { get; set; }
        public User Student { get; set; }
    }

}
