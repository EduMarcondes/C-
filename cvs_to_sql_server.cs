using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;

namespace ImportCsvLote
{
    internal class Class1
    {
        public class Rules
        {
            private string server;
            private string instance;
            private string database;
            private string username;
            private string password;
            private string folderPath = "";

            public Rules(string server, string instance, string database, string username, string password)
            {
                this.server = server;
                this.instance = instance;
                this.database = database;
                this.username = username;
                this.password = password;
            }

            public bool TestConnection()
            {
                using (SqlConnection connection = new SqlConnection($"Server={server}\\{instance};Database={database};User Id={username};Password={password};"))
                {
                    try
                    {
                        connection.Open();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            public void SelectFolder()
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        folderPath = dialog.SelectedPath;
                    }
                }
            }

            public void ImportData(ProgressBar progressBar)
            {
                using (SqlConnection connection = new SqlConnection($"Server={server}\\{instance};Database={database};User Id={username};Password={password};"))
                {
                    try
                    {
                        connection.Open();

                        string[] filePaths = Directory.GetFiles(folderPath, "*.csv");
                        int count = filePaths.Length;

                        for (int i = 0; i < count; i++)
                        {
                            string filePath = filePaths[i];
                            string tableName = Path.GetFileNameWithoutExtension(filePath);

                            // Verifica se a tabela já existe no banco
                            bool tableExists = CheckTableExists(connection, tableName);

                            // Se a tabela não existir, cria com as colunas presentes no arquivo CSV
                            if (!tableExists)
                            {
                                CreateTableFromCSV(connection, filePath, tableName);
                            }

                            string bulkInsertQuery = $"BULK INSERT {tableName} FROM '{filePath}' WITH (FIELDTERMINATOR = ',', ROWTERMINATOR = '\n', FIRSTROW = 2, CODEPAGE = 'ACP', DATAFILETYPE = 'char');";

                            using (SqlCommand command = new SqlCommand(bulkInsertQuery, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            progressBar.Value = (int)((i + 1) * 100.0 / count);
                        }

                        MessageBox.Show("Importação concluída!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar.Value = 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            private bool CheckTableExists(SqlConnection connection, string tableName)
            {
                string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{tableName}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }

            private void CreateTableFromCSV(SqlConnection connection, string filePath, string tableName)
            {
                string[] lines = File.ReadAllLines(filePath);
                string[] columnNames = lines[0].Split(',');
                int columnCount = columnNames.Length;

                // Remove as aspas de cada coluna
                for (int i = 0; i < columnCount; i++)
                {
                    columnNames[i] = columnNames[i].Trim('"');
                }

                string createTableQuery = $"CREATE TABLE {tableName} (";

                for (int i = 0; i < columnCount; i++)
                {
                    createTableQuery += $"{columnNames[i]} NVARCHAR(MAX)";
                    if (i < columnCount - 1)
                    {
                        createTableQuery += ", ";
                    }
                }
                
                createTableQuery += ")";

                using (SqlCommand command = new SqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

        }
    }
}
