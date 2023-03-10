 private void btnRestaurar_Click(object sender, EventArgs e)
        {
            // Informações de conexão com o servidor
            ServerConnection conn = new ServerConnection("localhost\\tew_sqlexpress");
            conn.LoginSecure = false;
            conn.Login = "sa";
            conn.Password = "123456";

            // Instância do servidor
            Server srv = new Server(conn);
            Database db = srv.Databases["db"];

            // Caminhos dos arquivos .mdf e .ldf fisicos da base de dados "db"
            string dataFilePath = db.FileGroups[0].Files[0].FileName;
            string logFilePath = db.LogFiles[0].FileName;

            // Caminho do backup que será restaurado
            string backupFilePath = @"C:\dados\base.bak";

            // Derruba todas as conexões da base de dados "db"
            srv.KillAllProcesses("db");
            
            // Volta a base de dados "db" para online
            db.SetOnline();
            srv.Databases["db"].Alter(TerminationClause.RollbackTransactionsImmediately);

            // Seta a base de dados "db" para Singl User
            //db.UserAccess = DatabaseUserAccess.Single;

            // Objeto de restauração de banco de dados
            Restore rst = new Restore();
            rst.Database = "db"; // Base que será substituida pelo backup
            rst.Action = RestoreActionType.Database;
            rst.Devices.AddDevice(backupFilePath, DeviceType.File);
            rst.ReplaceDatabase = true;
            
            // Declara um objeto do tipo datatable
            DataTable fileTable = rst.ReadFileList(srv);

            // Obtem os nomes lógicos dos arquivos .mdf e .ldf do backup
            string mdfFilePath = 
                Path.GetFileNameWithoutExtension(fileTable.Rows.OfType<DataRow>().FirstOrDefault(row => row["Type"].ToString() == "D")?["LogicalName"].ToString());
            string ldfFilePath = 
                Path.GetFileNameWithoutExtension(fileTable.Rows.OfType<DataRow>().FirstOrDefault(row => row["Type"].ToString() == "L")?["LogicalName"].ToString());

            // Especificando os novos caminhos para os arquivos de dados e log do backup
            rst.RelocateFiles.Add(new RelocateFile(mdfFilePath, dataFilePath));
            rst.RelocateFiles.Add(new RelocateFile(ldfFilePath, logFilePath));

            // Executa a restauração do backup
            rst.SqlRestore(srv);

            // Seta a base de dados para Multi User
            //db.UserAccess = DatabaseUserAccess.Multiple;
            //db.SetOnline();

            MessageBox.Show("Backup restaurado com sucesso!");
}
