using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ProjectoXappia.Properties;

namespace ProjectoXappia.Class
{
    class DataBase
    {
        static MySqlConnection Contecxt = new MySqlConnection();
        //static MySqlCommand Command = new MySqlCommand();
        public bool conected = false;

        public DataBase()
        {
            Contecxt.ConnectionString = Settings.Default.conectionString;
        }

        public void Connect()
        {
            Contecxt.Open();
        }

        public bool ConnectWithReturnvalue()
        {
            try
            {
                Contecxt.Open();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }
        public void UnConnect()
        {
            Contecxt.Close();
        }
        public bool getClientByDNI(string dni, DataSet enrollClient)
        {
            try
            {
                checkConection();
                var command = new MySqlDataAdapter(Settings.Default.queryTraerUnCliente + dni, Contecxt);
                command.Fill(enrollClient);
            }
            catch (MySqlException)
            {
                return false;
            }
            return true;


        }

        public bool getAllFingerPrint(DataSet clientes)
        {
            try
            {

                checkConection();
                var command = new MySqlDataAdapter(Settings.Default.queryTraerTodosLosClientes, Contecxt);
                command.Fill(clientes);
            }

            catch (MySqlException)
            {
                return false;
            }
            return true;

        }

        public void checkConection()
        {
            if (Contecxt.State == ConnectionState.Closed)
            {
                Connect();
            }
        }


        public bool saveTemplate(string template, DataRow dataRow)
        {
            try
            {
                checkConection();
                using (var cmd = new MySqlCommand(Settings.Default.querySetiarHuellaCliente,
                    Contecxt))
                {
                    cmd.Parameters.Add("@huella", MySqlDbType.MediumText).Value = template;
                    cmd.Parameters.Add("@key", MySqlDbType.UInt64).Value = dataRow["ClienteKey"];
                    cmd.ExecuteNonQuery();
                }
            }


            catch (MySqlException)
            {
                return false;
            }
            return true;

        }

        public bool saveIncomingLog(string dni)
        {
            try
            {
                checkConection();
                using (var cmd = new MySqlCommand(Settings.Default.querySetiarLog,
                    Contecxt))
                {
                    cmd.Parameters.Add("@dni", MySqlDbType.UInt64).Value = dni;
                    cmd.Parameters.Add("@fecha", MySqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                    cmd.ExecuteNonQuery();
                }
            }


            catch (MySqlException)
            {
                return false;
            }
            return true;
        }
    }
}
