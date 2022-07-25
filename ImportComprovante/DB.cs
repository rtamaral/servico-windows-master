using System;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;


namespace ImportComprovante
{
    public class DB : IDisposable
    {
        private readonly OracleConnection conexao;

        public DB()
        {
#if DESENVOLVIMENTO
            string conn = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = )(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = DAPP)));User Id=;Password=; Connection Timeout=120";
#endif
#if PRODUCAO
           string   conn = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = )(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = APP)));User Id=;Password=; Connection Timeout=120";
#endif

#if HOMOLOGACAO
           string  conn = @"Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = )(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = HAPP)));User Id=;Password=; Connection Timeout=120";
#endif

            conexao = new OracleConnection(conn);
            conexao.Open();
        }

        public void ExecutaComando(string strQuery)
        {
            var cmdComando = new OracleCommand
            {

                CommandText = strQuery,
                CommandType = CommandType.Text,
                Connection = conexao
            };

            cmdComando.ExecuteNonQuery();
        }

        public OracleDataReader ExecutaComandoComRetorno(string strQuery)
        {
            var cmdComando = new OracleCommand
            {
                CommandText = strQuery,
                CommandType = CommandType.Text,
                Connection = conexao
            };

            return cmdComando.ExecuteReader();
        }

        public void Dispose()
        {
            if (conexao.State == System.Data.ConnectionState.Open)
                conexao.Close();
        }
    }
}
