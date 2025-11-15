using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
	public class Reservation
	{
        public int ID { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BED { get; set; }
        public DateTime? EED { get; set; }
    }
}