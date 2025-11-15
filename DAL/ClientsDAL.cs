using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
    public class ClientsDAL
    {
        private string connectionString;

        public ClientsDAL()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["ClientDBConnection"];
            if (connectionStringSettings == null)
            {
                throw new Exception("Connection string 'DefaultConnection' not found in Web.config.");
            }
            connectionString = connectionStringSettings.ConnectionString;
        }

        public List<Clients> GetAllClients()
        {
            List<Clients> clients = new List<Clients>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT Id, Name, Type, Birthdate FROM Clients"; // Removed the extra comma
                    using (SqlCommand selectCmd = new SqlCommand(selectSql, connection))
                    {
                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var client = new Clients
                                {
                                    Id = (int)reader["Id"],
                                    Name = reader["Name"].ToString(),
                                    Type = (ClientsType)(int)reader["Type"],
                                    BirthDate = reader["Birthdate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["Birthdate"]
                                };
                                clients.Add(client);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return clients;
        }
        public Clients AddClient(Clients client)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Clients (Name, Type, Birthdate) 
                      OUTPUT INSERTED.Id 
                      VALUES (@Name, @Type, @BirthDate )";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Type", client.Type);

                    // Proper null handling for all datetime fields
                    command.Parameters.AddWithValue("@Birthdate",
                        client.BirthDate.HasValue ? (object)client.BirthDate.Value : DBNull.Value);

                  
                    connection.Open();
                    client.Id = Convert.ToInt32(command.ExecuteScalar() ?? 0);
                    if (client.Id == 0) throw new Exception("Failed to insert client.");
                }

            }

            return client;
        }

        public bool UpdateClient(Clients client)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"UPDATE Clients 
                      SET Name = @Name, 
                          Type = @Type, 
                          Birthdate = @Birthdate
                      WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Type", client.Type);

                    // Handle nullable BirthDate
                    command.Parameters.AddWithValue("@Birthdate",
                        client.BirthDate.HasValue ? (object)client.BirthDate.Value : DBNull.Value);

                    command.Parameters.AddWithValue("@Id", client.Id);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public bool DeleteClient(int clientId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete from Reserve table
                        string deleteReserveSql = "DELETE FROM Reserve WHERE ClientId = @Id";
                        using (SqlCommand deleteReserveCommand = new SqlCommand(deleteReserveSql, connection, transaction))
                        {
                            deleteReserveCommand.Parameters.AddWithValue("@Id", clientId);
                            deleteReserveCommand.ExecuteNonQuery();
                        }

                        // Delete from Clients table
                        string deleteClientSql = "DELETE FROM Clients WHERE Id = @Id";
                        using (SqlCommand deleteClientCommand = new SqlCommand(deleteClientSql, connection, transaction))
                        {
                            deleteClientCommand.Parameters.AddWithValue("@Id", clientId);
                            int rowsAffected = deleteClientCommand.ExecuteNonQuery();

                            // Commit the transaction if the client was deleted
                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return true; // Client deleted successfully
                            }
                            else
                            {
                                transaction.Rollback();
                                return false; // Client not found or not deleted
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        throw; // Optionally rethrow the exception
                    }
                }
            }
        }

        public ClientDetails GetClientById(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Clients.*, Reserve.Number " +
                             "FROM Clients " +
                             "JOIN Reserve ON Clients.Id = Reserve.ClientId " +
                             "WHERE Clients.Id = @Id AND Reserve.EED IS NULL";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClientDetails
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Type = (ClientsType)reader.GetInt32(reader.GetOrdinal("Type")),
                                BirthDate = reader["Birthdate"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["Birthdate"]) : DateTime.MinValue,
                                PhoneNumber = reader["Number"] != DBNull.Value ?
                                    reader["Number"].ToString() : null // Retrieve PhoneNumber
                            };
                        }
                    }
                }
            }
            return null; // Return null if no client is found
        }


        public bool ReserveClient(int clientId, string number)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Start transaction
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Check if number is already reserved by ANY client
                            string checkSql = @"
                        SELECT  ClientId 
                        FROM Reserve 
                        WHERE Number = @Number AND EED IS NULL";

                            int? existingClientId = null;
                            using (SqlCommand checkCmd = new SqlCommand(checkSql, connection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@Number", number);
                                var result = checkCmd.ExecuteScalar();

                                if (result != null && result != DBNull.Value)
                                {
                                    existingClientId = Convert.ToInt32(result);
                                    return false; // Number already reserved
                                }
                            }

                            // 2. Check if this client already has an active reservation
                            string currentReservationSql = @"
                        SELECT Number 
                        FROM Reserve 
                        WHERE ClientId = @ClientId AND EED IS NULL";

                            string currentNumber = null;
                            using (SqlCommand currentCmd = new SqlCommand(currentReservationSql, connection, transaction))
                            {
                                currentCmd.Parameters.AddWithValue("@ClientId", clientId);
                                var result = currentCmd.ExecuteScalar();

                                if (result != null && result != DBNull.Value)
                                {
                                    currentNumber = result.ToString();
                                }
                            }

                            // 3. End any existing reservation for this client
                            if (!string.IsNullOrEmpty(currentNumber))
                            {
                                string endExistingReservationSql = @"
                            UPDATE Reserve 
                            SET EED = GETDATE() 
                            WHERE ClientId = @ClientId AND Number = @CurrentNumber AND EED IS NULL";

                                using (SqlCommand endCmd = new SqlCommand(endExistingReservationSql, connection, transaction))
                                {
                                    endCmd.Parameters.AddWithValue("@ClientId", clientId);
                                    endCmd.Parameters.AddWithValue("@CurrentNumber", currentNumber);
                                    endCmd.ExecuteNonQuery();
                                }
                            }

                            // 4. Create new reservation
                            string insertReservationSql = @"
                        INSERT INTO Reserve (ClientId, Number, BED, EED) 
                        VALUES (@ClientId, @Number, GETDATE(), NULL)";

                            using (SqlCommand insertCmd = new SqlCommand(insertReservationSql, connection, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@ClientId", clientId);
                                insertCmd.Parameters.AddWithValue("@Number", number);
                                insertCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction Error: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return false;
            }
        }

        // Complementary method to get available phone numbers
        //public List<string> GetAvailablePhoneNumbers()
        //{
        //    List<string> availablePhones = new List<string>();

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            string sql = @"
        //        SELECT DISTINCT Number 
        //        FROM (
        //            SELECT Number 
        //            FROM Reserve 
        //            WHERE EED IS  NULL
        //        ) AS ActiveReservations
        //        WHERE Number NOT IN (
        //            SELECT Number 
        //            FROM Reserve 
        //            WHERE EED IS NULL
        //        )";

        //            using (SqlCommand command = new SqlCommand(sql, connection))
        //            {
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        availablePhones.Add(reader.GetString(0));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error getting available phone numbers: {ex.Message}");
        //    }

        //    return availablePhones;
        //}
        public bool UnreserveClient(int clientId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Only update EED to current date/time for currently reserved clients
                    string sql = @"UPDATE Reserve
                           SET EED = GETDATE()
                           WHERE ClientId = @ClientId 
                           AND EED IS NULL";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        // IMPORTANT: Fixed parameter name from @Id to @ClientId
                        command.Parameters.AddWithValue("@ClientId", clientId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unreserve Error: {ex.Message}");
                return false;
            }
        }


        public List<ClientTypeCount> GetClientCountsByType(string type)
        {
            List<ClientTypeCount> result = new List<ClientTypeCount>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Build the SQL query based on whether we're filtering or not
                string sql;
                if (string.IsNullOrEmpty(type))
                {
                    sql = "SELECT Type, COUNT(*) AS Count FROM Clients GROUP BY Type";
                }
                else
                {
                    sql = "SELECT Type, COUNT(*) AS Count FROM Clients WHERE Type = @type GROUP BY Type";
                }

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Add parameter if we're filtering
                    if (!string.IsNullOrEmpty(type))
                    {
                        command.Parameters.AddWithValue("@type", type);
                    }

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ClientTypeCount
                            {
                                Type = reader["Type"].ToString(),
                                Count = Convert.ToInt32(reader["Count"])
                            });
                        }
                    }
                }
            }

            // Ensure both types are represented if no filter was applied
            if (string.IsNullOrEmpty(type))
            {
                if (!result.Any(r => r.Type == "0"))
                {
                    result.Add(new ClientTypeCount { Type = "0", Count = 0 });
                }
                if (!result.Any(r => r.Type == "1"))
                {
                    result.Add(new ClientTypeCount { Type = "1", Count = 0 });
                }
            }

            return result;
        }

        public class ClientTypeCount
        {
            public string Type { get; set; }
            public int Count { get; set; }
        }
    }
}