using System;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public enum ClientsType
    {
        Individual,
        Organization
    }

    public class Clients
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ClientsType Type { get; set; }
        public DateTime? BirthDate { get; set; } // Nullable DateTime

       
    }
}
public class ReserveClientRequest
{
    public int ClientId { get; set; }
    public string PhoneNumber { get; set; }
}
public class ClientDetails
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ClientsType Type { get; set; }
    public DateTime BirthDate { get; set; }
    public string PhoneNumber { get; set; } // Add PhoneNumber property
}
