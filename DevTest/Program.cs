using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        string jsonFilePath = @"C:\Users\matth\Downloads\CurrentOnTapList.txt";
        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\matth\\OneDrive\\Desktop\\C#\\DevTest\\DevTest\\onTap.mdf;Integrated Security=True";

        string jsonContent = File.ReadAllText(jsonFilePath);
        var data = JsonConvert.DeserializeObject<EverythingOnTap>(jsonContent);

        Console.WriteLine("IPourIt: Dev Test\n");
        Console.WriteLine("Find the location for your favorite drink below!");
        Console.WriteLine("Please allow the database some time to load data.\n");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            connection.Open();

            CreateTable(connection);

            foreach (var onTapData in data.onTap)
            {
                InsertOnTap(connection, onTapData.BarName, onTapData.BarCity);
                foreach (var beerontap in onTapData.Beers)
                {
                    InsertBeer(connection, onTapData.BarName, onTapData.BarCity, beerontap.BeerName, beerontap.BrewerName, beerontap.StyleDesc);
                }
            }

            SearchResult(connection);

            Console.WriteLine("\n");
            Console.WriteLine("Search completed!");

            connection.Close();

            Console.ReadLine();

        }

    }

    static void CreateTable(SqlConnection connection)
    {
        string createOnTapTableQuery = @"IF OBJECT_ID('onTap') IS NULL
                                CREATE TABLE onTap (
                                    BarId INT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
                                    BarName VARCHAR(255) NOT NULL,
                                    BarCity VARCHAR(255) NOT NULL
                                )";
        string createBeersTableQuery = @"IF OBJECT_ID('beers') IS NULL
                                CREATE TABLE beers (
                                    BeerName VARCHAR(255) NOT NULL,
                                    BrewerName VARCHAR(255) NOT NULL,
                                    StyleDesc VARCHAR(255) NOT NULL,
                                    BarId INT,
                                    FOREIGN KEY (BarId) REFERENCES onTap(BarId)
                                )";
        using (SqlCommand command = new SqlCommand(createOnTapTableQuery, connection))
        {
            command.ExecuteNonQuery();
        }
        using (SqlCommand command = new SqlCommand(createBeersTableQuery, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    static void InsertOnTap(SqlConnection connection, string barName, string barCity)
    {
        string insertOnTapQuery = "INSERT INTO onTap (BarName, BarCity) VALUES (@BarName, @BarCity)";
        using (SqlCommand command = new SqlCommand(insertOnTapQuery, connection))
        {
            command.Parameters.AddWithValue("@BarName", barName);
            command.Parameters.AddWithValue("@BarCity", barCity);
            command.ExecuteNonQuery();
        }
    }

    static void InsertBeer(SqlConnection connection, string barName, string barCity, string beerName, string brewerName, string styleDesc)
    {
        string selectOnTapQuery = "SELECT BarId FROM onTap WHERE BarName = @BarName AND BarCity = @BarCity";

        using (SqlCommand command = new SqlCommand(selectOnTapQuery, connection))
        {
            command.Parameters.AddWithValue("@BarName", barName);
            command.Parameters.AddWithValue("@BarCity", barCity);

            int barId = Convert.ToInt32(command.ExecuteScalar());

            string insertBeerQuery = "INSERT INTO beers (BeerName, BrewerName, StyleDesc, BarId) VALUES (@BeerName, @BrewerName, @StyleDesc, @BarId)";

            using (SqlCommand insertCommand = new SqlCommand(insertBeerQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@BeerName", beerName);
                insertCommand.Parameters.AddWithValue("@BrewerName", brewerName);
                insertCommand.Parameters.AddWithValue("@StyleDesc", styleDesc);
                insertCommand.Parameters.AddWithValue("@BarId", barId);
                insertCommand.ExecuteNonQuery();
            }
        }

    }


    static void SearchResult(SqlConnection connection)
    {
        string userInput = "";
        Console.Write("Please enter a query for a drink: ");
        userInput = Console.ReadLine();

        string selectQuery = $"SELECT DISTINCT onTap.BarName, onTap.BarCity FROM onTap INNER JOIN beers ON onTap.BarId = beers.BarId WHERE beers.BeerName LIKE '%{userInput}%'";

        using (SqlCommand command = new SqlCommand(selectQuery, connection))
        {
            Console.WriteLine("\n");
            Console.WriteLine($"Retrieving data from the following input: {userInput}");
            Console.WriteLine("\n");

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string barName = (string)reader["BarName"];
                        string barCity = (string)reader["BarCity"];

                        Console.WriteLine($"Barname: {barName}      -       BarCity: {barCity}");
                    }
                } else
                {
                    Console.WriteLine("Your input did not have any results.");
                }
            }
        }

    }

    private class EverythingOnTap
    {
        public List<OnTap> onTap { get; set; }
    }

    private class OnTap
    {
        public string BarName { get; set; }
        public string BarCity { get; set; }
        public List<BeerOnTap> Beers { get; set; }
    }

    private class BeerOnTap
    {
        public string BeerName { get; set; }
        public string BrewerName { get; set; }
        public string StyleDesc { get; set; }
    }

}


