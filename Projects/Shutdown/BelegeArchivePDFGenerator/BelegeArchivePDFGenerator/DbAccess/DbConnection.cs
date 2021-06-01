using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows;

namespace BelegeArchivePDFGenerator
{
    class DbConnection
    {
        public static string connectionString;

        public static bool TestConnection(string connectionString)
        {
            bool result = false;
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    result = true;
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 0:
                            MessageBox.Show("Cannot connect to server!");
                            break;

                        case 1045:
                            MessageBox.Show("Invalid username/password!");
                            break;

                        case 1049:
                            MessageBox.Show("Unknown database!");
                            break;
                        default:
                            MessageBox.Show(ex.Number + " - " + ex.Message);
                            break;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            return result;
        }
    }
}
