using System;
using System.Data.SqlClient;
using System.IO;

namespace TextFileToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "server=LASHA; database=G14_Products; integrated security=true;"; 
            string filePath = @"C:\Products.txt"; 

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] data = line.Split('\t');

                            string productCode = data[1]; 
                            string productName = data[2];
                            decimal price = decimal.Parse(data[3]);
                            bool isDeleted = (data[4] == "1");
                            string categoryName = data[0];

                            
                            string categoryId;
                            string selectCategorySql = "SELECT ID FROM Categories WHERE Name = @CategoryName";
                            using (SqlCommand selectCategoryCmd = new SqlCommand(selectCategorySql, connection))
                            {
                                selectCategoryCmd.Parameters.AddWithValue("@CategoryName", categoryName);
                                categoryId = selectCategoryCmd.ExecuteScalar()?.ToString();
                            }

                            if (string.IsNullOrEmpty(categoryId))
                            {                               
                                string insertCategorySql = "INSERT INTO Categories (Name) VALUES (@CategoryName); SELECT SCOPE_IDENTITY();";
                                using (SqlCommand insertCategoryCmd = new SqlCommand(insertCategorySql, connection))
                                {
                                    insertCategoryCmd.Parameters.AddWithValue("@CategoryName", categoryName);
                                    categoryId = insertCategoryCmd.ExecuteScalar()?.ToString();
                                }
                            }
                            
                            string selectSql = "SELECT COUNT(*) FROM Products WHERE Code = @ProductCode";
                            using (SqlCommand selectCmd = new SqlCommand(selectSql, connection))
                            {
                                selectCmd.Parameters.AddWithValue("@ProductCode", productCode);
                                int count = Convert.ToInt32(selectCmd.ExecuteScalar());

                                if (count > 0)
                                {                                   
                                    string updateSql = "UPDATE Products SET Name = @ProductName, Price = @Price, IsDeleted = @IsDeleted, CategoryID = @CategoryID WHERE Code = @ProductCode";
                                    using (SqlCommand updateCmd = new SqlCommand(updateSql, connection))
                                    {
                                        updateCmd.Parameters.AddWithValue("@ProductCode", productCode);
                                        updateCmd.Parameters.AddWithValue("@ProductName", productName);
                                        updateCmd.Parameters.AddWithValue("@Price", price);
                                        updateCmd.Parameters.AddWithValue("@IsDeleted", isDeleted);
                                        updateCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    string insertSql = "INSERT INTO Products (Code, Name, Price, IsDeleted, CategoryID) VALUES (@ProductCode, @ProductName, @Price, @IsDeleted, @CategoryID)";
                                    using (SqlCommand insertCmd = new SqlCommand(insertSql, connection))
                                    {
                                        insertCmd.Parameters.AddWithValue("@ProductCode", productCode);
                                        insertCmd.Parameters.AddWithValue("@ProductName", productName);
                                        insertCmd.Parameters.AddWithValue("@Price", price);
                                        insertCmd.Parameters.AddWithValue("@IsDeleted", isDeleted);
                                        insertCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine("Data updated/inserted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
