using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
    public class DevicesDAL
    {
        private string connectionString;

        public DevicesDAL()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["ClientDBConnection"];
            if (connectionStringSettings == null)
            {
                throw new Exception("Connection string 'ClientDBConnection' not found in Web.config.");
            }
            connectionString = connectionStringSettings.ConnectionString;
        }

        public List<Devices> GetAllDevices()
        {
            List<Devices> devices = new List<Devices>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ID, Device FROM Phone";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devices.Add(new Devices
                            {
                                Id = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                Name = reader["Device"] != DBNull.Value ? reader["Device"].ToString() : string.Empty
                                // Add other properties if your Devices class has more fields
                            });
                        }
                    }
                }
            }
            return devices;
        }
        public void AddDevice(Devices device)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Phone (Device) VALUES (@Device)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Device", device.Name);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDevice(Devices device)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "UPDATE Phone SET Device = @Device WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Device", device.Name);
                    command.Parameters.AddWithValue("@ID", device.Id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteDevice(int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Phone WHERE ID = @Id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", deviceId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
       
    }
}