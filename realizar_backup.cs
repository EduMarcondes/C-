using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

public void realizarBackup(string databaseName, string backupPath, ProgressBar progressBar, Label label)
        {
            try
            {
                Database database = connection.Databases[databaseName];

                Backup backup = new Backup();
                backup.Action = BackupActionType.Database;
                backup.Database = database.Name;
                backup.Devices.AddDevice(backupPath, DeviceType.File);
                backup.BackupSetName = databaseName + " Backup";
                backup.BackupSetDescription = databaseName + " Backup";
                backup.Initialize = true;

                backup.PercentComplete += (sender, e) =>
                {
                    progressBar.Invoke((MethodInvoker)delegate
                    {
                        progressBar.Value = e.Percent;
                        progressBar.Update();
                    });

                    label.Invoke((MethodInvoker)delegate
                    {
                        label.Text = e.Percent.ToString() + "%";
                        label.Update();
                    });
                };

                backup.Checksum = true;
                backup.ContinueAfterError = true;
                backup.Incremental = false;
                backup.LogTruncation = BackupTruncateLogType.Truncate;

                backup.SqlBackup(connection);

            }
            catch (Exception e)
            {
                MessageBox.Show($"Erro ao realizar o backup: {e.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
