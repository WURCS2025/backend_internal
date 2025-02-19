using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SqlConnectionTest
    {
        [TestMethod]
        public void Connect_to_Sql_server()
        {
            string connectionString = "Server=TingZeng-dell-laptop\\MSSQLSERVER_22;Database=DevDB;User Id=sa;Password=Newlife04!$;TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    Console.WriteLine("Attempting to connect to SQL Server...");
                    connection.Open();
                    Console.WriteLine("Connection successful!");
                    Assert.IsTrue(1==1);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Connection failed: " + ex.Message);
                    Assert.IsTrue(2 == 1);
                }
            }
        }
    }
}
