using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.Models
{
    public enum BookingStatus
    {
        None = 0,
        Pending = 1,   // серый
        Accepted = 2,  // зелёный
        Rejected = 3   // красный
    }

}
