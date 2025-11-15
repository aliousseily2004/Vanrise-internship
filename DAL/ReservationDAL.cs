using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
    public class ReservationDAL
    {
        private string connectionString;

        public ReservationDAL()
        {
            // Retrieve the connection string from Web.config
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["ClientDBConnection"];
            if (connectionStringSettings == null)
            {
                throw new Exception("Connection string 'ClientDBConnection' not found in Web.config.");
            }
            connectionString = connectionStringSettings.ConnectionString;
        }

        public List<Reservation> GetAllReservations()
        {
            List<Reservation> reservations = new List<Reservation>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT
    r.Id,
    r.ClientId,
    r.Number,
    r.BED,
    r.EED,
    c.Name
FROM
    Reserve r
JOIN
    Clients c ON r.ClientId = c.Id
WHERE
    r.EED IS NULL";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Reservation reservation = new Reservation
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                ClientId = Convert.ToInt32(reader["ClientId"]),
                                Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                                PhoneNumber = reader["Number"] != DBNull.Value ?
                                              reader["Number"].ToString() : null,
                                BED = reader["BED"] != DBNull.Value ?
                                      Convert.ToDateTime(reader["BED"]) : DateTime.MinValue,
                                EED = reader["EED"] != DBNull.Value ?
      (DateTime?)Convert.ToDateTime(reader["EED"]) :
      new DateTime(1, 1, 1) // Default value: "0001-01-01T00:00:00"
                            };
                            reservations.Add(reservation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
            return reservations;
        }
    public List<Reservation> GetReservations()
{
    List<Reservation> reservations = new List<Reservation>();
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"SELECT
    r.Id,
    r.ClientId,
    r.Number,
    r.BED,
    r.EED,
    c.Name
FROM
    Reserve r
JOIN
    Clients c ON r.ClientId = c.Id
";
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Reservation reservation = new Reservation
                    {
                        ID = Convert.ToInt32(reader["Id"]),
                        ClientId = Convert.ToInt32(reader["ClientId"]),
                        Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                        PhoneNumber = reader["Number"] != DBNull.Value ?
                                      reader["Number"].ToString() : null,
                        BED = reader["BED"] != DBNull.Value ?
                              Convert.ToDateTime(reader["BED"]) : DateTime.MinValue,
                        EED = reader["EED"] != DBNull.Value ?
(DateTime?)Convert.ToDateTime(reader["EED"]) :
new DateTime(1, 1, 1) // Default value: "0001-01-01T00:00:00"
                    };
                    reservations.Add(reservation);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
        throw;
    }
    return reservations;
}
    } }