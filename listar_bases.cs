using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

        //Lista todas as base de dados do sql server
        public void listaBases(ComboBox comboBox)
        {
            try
            {
                string query = "SELECT name FROM master.dbo.sysdatabases ORDER BY name";

                // conectarDB.ConnectionString contém a conexão aberta com o sqlserver incluindo a instância
                using (SqlConnection sqlConnection = new SqlConnection(conectarDB.ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        comboBox.Items.Add(reader["name"]);
                    }

                    reader.Close();
                }
            }
