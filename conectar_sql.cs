using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

        //Metodo de conexão com SQL Server
        public bool conectarSQL(string server, string user, string password)
        {
            try
            {
                conectarDB = new ServerConnection(server, user, password);
                connection = new Server(conectarDB);
                conectarDB.Connect();
                return true;
            }
            catch
            {
                return false;
            }
        }
