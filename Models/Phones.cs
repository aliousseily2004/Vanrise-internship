using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Phones
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty; // Default to empty string
        public string Device { get; set; }
    }
    public class PhoneReportItem
    {
        public string Device { get; set; }
        public string Status { get; set; }
        public int NumberCount { get; set; }
    }
}