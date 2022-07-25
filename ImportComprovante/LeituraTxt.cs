using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Linq;


namespace ImportComprovante
{
    public static class LeituraTxt
    {
        static Thread _ThreadVerificacao;


        public static void Ler()
        {
            //criação da thread de verificação e sua execução
            _ThreadVerificacao = new Thread(VerificarHorario);
            _ThreadVerificacao.Start();
        }




        private static void VerificarHorario()
        {
            while (true)
            {
                if (new int[] { 7, 8, 9 }.Contains(DateTime.Now.Hour))
                {
                    //if (DateTime.Now.Hour == 13 || DateTime.Now.Hour == 14)
                    //{ 
                    WriteToEventLog("Processamento de comprovantes", "Controle de fluxo", "INICIO DO CICLO", EventLogEntryType.Information);

                    try
                    {

                        //throw new Exception("erro na minha aplicação");
                        ConexaoFTP conexaoFTP = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ_ARQUIVO), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA);
                        string conn = conexaoFTP.ListarDiretorioRemoto();


                        //montar lista com retorno do ftp de listar diretorio

                        var listaDeArquivosDoFTP = conn?.Split('\n', '\r')
                                .Select(s => Regex.Match(s, @"[A-Za-z0-9]{1,30}\.cpv").Value)
                                .Where(w => !string.IsNullOrEmpty(w)) ?? new string[0];


                        foreach (string nomeDoArquivo in listaDeArquivosDoFTP)
                        {

                            var arqStream = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ_ARQUIVO), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA).Download("", "/" + nomeDoArquivo);

                            InsertLog("");
                            //nsertLog("\n INICIO :: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                            InsertLog("\n INICIO: " + nomeDoArquivo + " " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                            ProcessaArquivo(nomeDoArquivo, arqStream);
                            InsertLog("\n FIM :: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                            Thread.Sleep(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToEventLog("Processamento de comprovantes ", "Erro na aplicacao", ex.GetBaseException().Message);

                        Thread.Sleep(30000);
                        continue;
                    }

                    WriteToEventLog("Processamento de comprovantes ", "Controle de fluxo", "FIM DO CICLO", EventLogEntryType.Information);

                    Thread.Sleep(100000);

                    //finally
                    //{
                    //    InsertLog("\n FIM :: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    //}
                }

                //Thread.Sleep(3600000);
            }
        }

        public static bool CreateLog(string strLogName)
        {
            bool Result = false;

            try
            {
                System.Diagnostics.EventLog.CreateEventSource(strLogName, strLogName);
                System.Diagnostics.EventLog SQLEventLog =
            new System.Diagnostics.EventLog();

                SQLEventLog.Source = strLogName;
                SQLEventLog.Log = strLogName;

                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry("The " + strLogName + " was successfully initialize component.", EventLogEntryType.Information);


                Result = true;
            }
            catch
            {
                Result = false;
            }

            return Result;
        }
        public static void WriteToEventLog(string strLogName, string strSource, string strErrDetail, EventLogEntryType tipo = EventLogEntryType.Error)
        {
            System.Diagnostics.EventLog SQLEventLog = new System.Diagnostics.EventLog();

            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(strLogName))
                    CreateLog(strLogName);


                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry($"{Convert.ToString(strSource)} \n\n {Convert.ToString(strErrDetail)}", tipo);

            }
            catch (Exception ex)
            {
                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(Convert.ToString("Erro ao escrever o log: ")
                                      + Convert.ToString(ex.Message),
                EventLogEntryType.Information);
            }
            finally
            {
                SQLEventLog.Dispose();
                SQLEventLog = null;
            }
        }

        private static void ProcessaArquivo(string nomeDoArquivo, Stream arquivoFTP = null)
        {
            try
            {
                //string DtAgendamento = "";
                string DtPagamento = "";
                string VlrDocumento = "";
                string Protocolo = "";
                string Registro = "";
                string Emissao = "";
                bool Mover = true;

                string arquivoConteudo = "";

                using (StreamReader sr = new StreamReader(arquivoFTP))
                {
                    arquivoConteudo = sr.ReadToEnd();
                }

                string[] StringLinha = arquivoConteudo.Split('\n', '\r');

                string CodBarra = "";

                for (int line = 0; line < StringLinha.Length; line++)
                {


                    if (!StringLinha[line].Contains("Cod. Barras:") && !StringLinha[line].Contains("Dt.Agendamento:") && !StringLinha[line].Contains("Dt.Pagamento:") && !StringLinha[line].Contains("Dt.Pagamento:") && !StringLinha[line].Contains("Vlr.Documento:") && !StringLinha[line].Contains("Protocolo:") && !StringLinha[line].Contains("Registro:") && !StringLinha[line].Contains("Emissao.:"))
                    {
                        continue;

                    }
                    if (StringLinha[line].Contains("Cod. Barras:"))
                    {
                        CodBarra = (StringLinha[line].Replace("Cod. Barras:", "").Trim() + StringLinha[line + 2].Trim()).Replace("\\n", "").Replace(" ", "").Replace("\\r", "");

                        //DtAgendamento = "";
                        DtPagamento = "";
                        VlrDocumento = "";
                        Protocolo = "";
                        Registro = "";
                        Emissao = "";

                    }

                    if (StringLinha[line].Contains("Dt.Agendamento:"))
                    {
                        DtPagamento = StringLinha[line].Replace("Dt.Agendamento:", "").Replace(" ", "");

                    }

                    if (StringLinha[line].Contains("Dt.Pagamento:"))
                    {
                        DtPagamento = StringLinha[line].Replace("Dt.Pagamento:", "").Replace(" ", "");
                    }

                    if (StringLinha[line].Contains("Vlr.Documento:"))
                    {
                        VlrDocumento = StringLinha[line].Replace("Vlr.Documento:", "").Replace(" ", "");
                    }

                    if (StringLinha[line].Contains("Protocolo:"))
                    {
                        Protocolo = StringLinha[line].Replace("Protocolo:", "").Replace(" ", "");
                    }

                    if (StringLinha[line].Contains("Registro:"))
                    {
                        Registro = StringLinha[line].Replace("Registro:", "");
                    }

                    if (StringLinha[line].Contains("Emissao.:"))
                    {
                        Emissao = StringLinha[line].Replace("Emissao.:", "");

                        //Busca Código de Barra 
                        string CodProcInter = GetCodProcInter(CodBarra);

                        if (!string.IsNullOrEmpty(CodProcInter))
                        {

                            if (!CodBarraJaInserido(CodBarra))
                            {
                                //Insere Código de Barra
                                bool inserido = InsereDadosBanco(CodProcInter, CodBarra, DtPagamento, VlrDocumento, Protocolo, Registro, Emissao);
                            }
                            else
                            {
                                //Código de Barra já Inserido
                                InsertLog("\n Já Existe -> " + CodBarra);

                            }

                        }
                        else
                        {

                            //Código de Barra não esta disponível na tabela -> sdpj_proc_inter
                            Mover = false;

                            InsertLog("\n O comprovante não foi importado para pasta de processados, pois o Cód. Barra  -> " + CodBarra + " não está disponivel na tabela processo interessado");



                            try
                            {
                                HelperEmail.Enviar("\n A remessa" + " " + nomeDoArquivo + " " + " foi processado com pendência e arquivo não foi movido para pasta de processados, pois o Código de Barra " + CodBarra + " não está disponivel em processo interessado");

                            }
                            catch (Exception e)
                            {
                                WriteToEventLog("Processamento de comprovantes banestes", "Falha ao enviar email", $"arquivo:{nomeDoArquivo}\n\n{e.GetBaseException().Message}");
                            }
                        }
                    }

                }
                //Limpar Variaveis
                CodBarra = "";


                if (!Mover)
                {
                    ProcessadosPendentes(nomeDoArquivo);
                }
                else
                {
                    MoveFile(nomeDoArquivo);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void ProcessadosPendentes(string nomeDoArquivo)
        {
            ConexaoFTP conexaoFTP = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ_ARQUIVO), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA);
            //Funcionando em produção
            conexaoFTP.MoverArquivo(nomeDoArquivo, "", $"{new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ).AbsolutePath}/Processados com pendencia");

            //Para teste
            //conexaoFTP.MoverArquivo(nomeDoArquivo, "", $"{new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ).AbsolutePath}/Processados com pendenciaTeste");
        }

        private static void MoveFile(string nomeDoArquivo)
        {
            ConexaoFTP conexaoFTP = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ_ARQUIVO), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA);
            //Funcionando em producao
            conexaoFTP.MoverArquivo(nomeDoArquivo, "", $"{new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ).AbsolutePath}/processados");

           
        }

        private static string GetCodProcInter(string CodBarra)
        {
            using (DB db = new DB())
            {
                string SQL = string.Format(@"SELECT T0.cod_proc_inter FROM sdpj_proc_inter T0 WHERE T0.cod_barras_banestes = '{0}'", CodBarra);
                var dados = db.ExecutaComandoComRetorno(SQL);

                if (dados.Read())
                {
                    return dados["cod_proc_inter"].ToString();
                }
            }

            return "";
        }

        private static bool CodBarraJaInserido(string CodBarra)
        {
            using (DB db = new DB())
            {
                string SQL = string.Format(@"SELECT T0.COD_COMP EXISTE FROM SDPJ_COMPROVANTE_BANESTES T0 WHERE T0.COD_COMP_BANESTES LIKE '{0}'", CodBarra);
                var dados = db.ExecutaComandoComRetorno(SQL);

                if (dados.Read())
                {
                    if (dados["Existe"].ToString() == "0")
                    {
                        //Não Inserido
                        return false;
                    }
                    else
                    {
                        //Já Existe
                        return true;
                    }

                }
                return false;
            }
        }

        private static bool InsereDadosBanco(string CodProcInter, string CodBarra, string DtPagamento, string VlrDocumento, string Protocolo, string Registro, string Emissao)
        {
            bool insert = false;

            using (DB db = new DB())
            {
                try
                {

                    VlrDocumento = SomenteNumerosPontosVirgula(VlrDocumento).Replace(".", "");

                    string SQL;

                    DateTime data = Convert.ToDateTime(DtPagamento);
                    DateTime dataEmissao = Convert.ToDateTime(Emissao);


                    SQL = "INSERT INTO sdpj_comprovante_banestes (COD_COMP, COD_PROC, COD_BARRAS, DAT_AGENDAMENTO, VLR_DOCUMENTO, NUM_PROTOCOLO, DSC_REGISTRO,DAT_EMISSAO)";
                    SQL += string.Format(@" VALUES(SDPJ_COD_SEQ.NextVal, '{0}', '{1}', TO_DATE('{2}', 'DD/MM/YYYY'), '{3}', '{4}', '{5}', TO_DATE('{6}', 'DD/MM/YYYY HH24:MI:SS'))", CodProc, CodBarra, DtPagamento.Trim(), VlrDocumento, Protocolo, Registro, (Emissao.TrimStart()).TrimEnd());


                    db.ExecutaComando(SQL);
                    insert = true;

                    InsertLog("\n Inserido -> ProcInter: " + CodProc + " Cód. Barra: " + CodBarra);
                }
                catch (Exception)
                {
                    insert = false;
                }
            }

            return insert;
        }

        private static void InsertLog(string linha)
        {

            ConexaoFTP conexaoFTP = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA);

            //ConexaoFTP conexaoFTP = new ConexaoFTP(new Uri(DadosAcessoFTPPadraoSDPJ.CAMINHO_FTP_RAIZ_ARQUIVO), DadosAcessoFTPPadraoSDPJ.LOGIN, DadosAcessoFTPPadraoSDPJ.SENHA);



            MemoryStream str = new MemoryStream();

            //Escreve no arquivo de LOG
            StreamWriter file = new StreamWriter(str);



            file.WriteLine(linha);

            file.Flush();
            str.Flush();
            str.Seek(0, SeekOrigin.Begin);
            file.BaseStream.Seek(0, SeekOrigin.Begin);


            //Funcionando em produção.
            conexaoFTP.UploadAppend("/Log de remessas processadas/", str, "Banes_" + DateTime.Now.ToString("dd_MM_yy") + ".txt");

            //para teste
            //conexaoFTP.UploadAppend("/LogTeste/", str, "Banes_" + DateTime.Now.ToString("dd_MM_yy") + ".txt");
            str.Close();
            file.Close();
            str.Dispose();
            file.Dispose();


        }

        private static string SomenteNumerosPontosVirgula(string toNormalize)
        {
            List<char> numbers = new List<char>("0123456789,.");
            StringBuilder toReturn = new StringBuilder(toNormalize.Length);
            CharEnumerator enumerator = toNormalize.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (numbers.Contains(enumerator.Current))
                    toReturn.Append(enumerator.Current);
            }

            return toReturn.ToString();
        }

        private class HelperEmail
        {
            public HelperEmail()
            {
            }

            public static void Enviar(string mensagem)
            {

                string host = "";
#if PRODUCAO
                host = "";
#endif

#if HOMOLOGACAO || DEBUG
                host = "";
#endif
                string emailDe = "";

#if DEBUG
                emailDe = "";
#endif
#if HOMOLOGACAO
                emailDe = "";
#endif
#if PRODUCAO
                emailDe = "";
#endif

                SmtpClient cliente = new SmtpClient(host, 25);
                cliente.Timeout = 10000;
                MailMessage msg = new MailMessage
                {
                    BodyEncoding = Encoding.UTF8,
                    From = new MailAddress(emailDe)
                };
                msg.To.Add("");
                msg.To.Add("");
                msg.To.Add("");
                msg.To.Add("");
                msg.To.Add("");
                msg.Subject = "Servico de processamento de retorno de remessas comprovantes";
                msg.Body = mensagem;

                cliente.Send(msg);

            }
        }
    }
}
    


