using coach_search.DB.coach_search.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.Models
{
    public static class ApplicationContext
    {
        public static UnitOfWork unitofwork = new(new DB.Context());

        public static User? CurrentUser { get; set; }
        public static int? CurrentTutorId { get; set; }
    }
}
