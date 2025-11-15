using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
    public class ProductDAL
    {
        private string connectionString;

        public ProductDAL()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (connectionStringSettings == null)
            {
                throw new Exception("Connection string 'DefaultConnection' not found in Web.config.");
            }
            connectionString = connectionStringSettings.ConnectionString;
        }

        // Method to get all products
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("GetProductDetails", connection);
                command.CommandType = CommandType.StoredProcedure; // Set command type to stored procedure

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            return products;
        }

        // Method to add a new product
        public void AddProduct(Product product)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Products (Name) VALUES (@Name)";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", product.Name);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Method to update an existing product
        public void UpdateProduct(Product product)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "UPDATE Products SET Name = @Name WHERE Id = @Id";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Id", product.Id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Method to delete a product
        public void DeleteProduct(int productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Products WHERE Id = @Id";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", productId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}