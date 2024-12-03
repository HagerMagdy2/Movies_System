using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movie_System.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int NumberOfSeats { get; set; }
    }
}
