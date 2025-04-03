using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sample.Models
{
    public class MySqlDataAccess
    {
        private string _connectionString = "Server=localhost;Database=practice;User ID=root;Password=root;";
        public string ConnectionString
        {
            get { return _connectionString; }
        }
        private MySqlConnection _connection;

        public void OpenDb()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection(_connectionString);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }
        public void CloseDb()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
        public List<Dictionary<string, object>> ExecuteQuery(string query)
        {
            var rows = new List<Dictionary<string, object>>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader[i];
                        }
                        rows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while fetching data: " + ex.Message);
                }
            }

            return rows;
        }
        public bool ExecuteAddEdit(string query)
        {
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    OpenDb();
                }
                using (MySqlCommand command = new MySqlCommand(query, _connection))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing query: " + ex.Message);
            }
        }
       

        public DataSet getDs(string query, string tableName)
        {
            DataSet ds = new DataSet();
            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, _connection);
                dataAdapter.Fill(ds, tableName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data: " + ex.Message);
            }
            return ds;
        }

        public int ExecuteNonQuery(string query)
        {
            int rowsAffected = 0;
            using (_connection = new MySqlConnection(_connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, _connection))
            {
                try
                {
                    _connection.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing query: " + ex.Message);
                }
            }
            return rowsAffected;
        }
    }
}