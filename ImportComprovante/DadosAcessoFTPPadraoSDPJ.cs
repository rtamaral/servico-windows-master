using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComprovante
{
    public class DadosAcessoFTPPadraoSDPJ
    {
        public static string LOGIN { get { return ""; } }
        public static string SENHA { get { return ""; } }

        public const string CAMINHO_FTP_RAIZ = "";

#if DESENVOLVIMENTO
        public const string CAMINHO_FTP_RAIZ_ARQUIVO = "";
#endif
#if HOMOLOGACAO
         public const string CAMINHO_FTP_RAIZ_ARQUIVO = "";
#endif
#if PRODUCAO
        public const string CAMINHO_FTP_RAIZ_ARQUIVO = "";
#endif
       
    }
}
