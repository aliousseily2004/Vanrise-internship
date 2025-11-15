using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
    public class PhonesDAL
    {

        private string connectionString;

        public PhonesDAL()
        {
            // Retrieve the connection string from Web.config
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["ClientDBConnection"];
            if (connectionStringSettings == null)
            {
                throw new Exception("Connection string 'ClientDBConnection' not found in Web.config.");
            }
            connectionString = connectionStringSettings.ConnectionString;
        }

        public List<Phones> GetAllPhoneNumbers()
        {
            List<Phones> phoneNumbers = new List<Phones>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        ID,
                        Number,
                        Device
                    FROM
                        Phone
"; // Adjust table name as needed

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Phones phoneNumber = new Phones
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Number = reader.IsDBNull(reader.GetOrdinal("Number")) ? string.Empty : reader.GetString(reader.GetOrdinal("Number")),
                                Device = reader.GetString(reader.GetOrdinal("Device"))
                            };

                            phoneNumbers.Add(phoneNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    // In a real application, use proper logging
                    Console.WriteLine($"Error retrieving phone numbers: {ex.Message}");
                }
            }

            return phoneNumbers;
        }
        public List<Phones> GetAllAvailablePhoneNumbers()
        {
            List<Phones> phoneNumbers = new List<Phones>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT DISTINCT Number 
FROM (
    SELECT Number 
    FROM Reserve 
    WHERE EED IS Not NULL
) AS ActiveReservations
WHERE Number NOT IN (
    SELECT Number 
    FROM Reserve 
    WHERE EED IS NULL
)
"; // Adjust table name as needed

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Phones phoneNumber = new Phones
                            {
                               
                             Number = reader.IsDBNull(reader.GetOrdinal("Number")) ? string.Empty : reader.GetString(reader.GetOrdinal("Number"))
                           
                            };

                            phoneNumbers.Add(phoneNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    // In a real application, use proper logging
                    Console.WriteLine($"Error retrieving phone numbers: {ex.Message}");
                }
            }

            return phoneNumbers;
        }


        public int AddPhone(Phones phone)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Phone (Number, Device) VALUES (@Number, @Device); SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Number", phone.Number);
                    command.Parameters.AddWithValue("@Device", phone.Device);

                    try
                    {
                        connection.Open();
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding phone: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public bool UpdatePhone(Phones phone)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Phone SET Number = @Number, Device = @Device WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", phone.Id);
                    command.Parameters.AddWithValue("@Number", phone.Number);
                    command.Parameters.AddWithValue("@Device", phone.Device);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating phone: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public bool DeletePhone(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Phone WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting phone: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        public Phones GetPhoneById(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT ID, Number, Device 
            FROM Phone 
            WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Phones
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                    Number = reader.IsDBNull(reader.GetOrdinal("Number"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Number")),
                                    Device = reader.GetString(reader.GetOrdinal("Device"))
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving phone: {ex.Message}");
                        throw;
                    }
                }
            }
            return null;
        }
        

        // Updated DAL method
        public List<PhoneReportItem> GetPhoneInfo()
        {
            List<PhoneReportItem> reportData = new List<PhoneReportItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT 
                p.Device,
                CASE WHEN r.EED IS NULL THEN 'Reserved' ELSE 'Unreserved' END AS Status,
                COUNT(p.Number) AS NumberCount
            FROM Phone p
            JOIN Reserve r ON p.Number = r.Number
            GROUP BY p.Device, CASE WHEN r.EED IS NULL THEN 'Reserved' ELSE 'Unreserved' END";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportData.Add(new PhoneReportItem
                            {
                                Device = reader["Device"].ToString(),
                                Status = reader["Status"].ToString(),
                                NumberCount = Convert.ToInt32(reader["NumberCount"])
                            });
                        }
                    }
                }
            }
            return reportData;
        }
    }
}




