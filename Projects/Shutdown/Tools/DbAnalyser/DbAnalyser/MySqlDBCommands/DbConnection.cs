using System.Windows;
using MySql.Data.MySqlClient;

namespace DbAnalyser.MySqlDBCommands
{
    /**
     * This class contains every connection string required for the process
     * It also has a TestConnection and a SetConnectionString method, which will be used later on in the development process.
     */
    public class DbConnection
    {
        /**
         * Limitation settings
         */
        public static bool IsPaused { get; set; }
        public static int Treshold { get; set; }
        public static int AllowedThreads { get; set; }
        public static long InsertStepSize { get; set; }
        public static long FromRowCount { get; set; }
        public static long ToRowCount { get; set; }

        /**
         * Database names
         */ 
        public static string SourceDatabase { get; set; }
        public static string AnalysticDatabase { get; set; }
        public static string FinalDatabase { get; set; }
        public static string FinalSystemDatabase { get; set; }

        /**
         * Connection Strings
         */
        public static string SourceConnectionString { get; set; }
        public static string DestinationConnectionString { get; set; }

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
