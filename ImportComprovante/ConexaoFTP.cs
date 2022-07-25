using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace ImportComprovante
{
    public class ConexaoFTP
    {
        private FtpWebRequest _conexaoCriada;
        private Uri _host;
        private string _login;
        private string _senha;

        public ConexaoFTP(Uri host, string login, string senha)
        {
            this._host = host;
            this._login = login;
            this._senha = senha;

            this.NovaConexao();
        }

        public void NovaConexao(string caminhoRemoto = "")
        {
            try
            {
                
                this._conexaoCriada = (FtpWebRequest)WebRequest.Create(this._host + caminhoRemoto);

                this._conexaoCriada.Credentials = new NetworkCredential(this._login, this._senha);
                this._conexaoCriada.UsePassive = true;
                this._conexaoCriada.KeepAlive = false;
                this._conexaoCriada.UseBinary = true;

                //this._conexaoCriada.Proxy = null;
                

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void FecharConexao()
        {
            if (!this.VerificarSeServidorResponde())
                this._conexaoCriada = null;
        }

        public bool VerificarSeServidorResponde()
        {
            if (this._conexaoCriada == null)
                return false;

            this._conexaoCriada.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
            using (FtpWebResponse resp = (FtpWebResponse)this._conexaoCriada.GetResponse())
            {
                return resp.StatusCode == FtpStatusCode.CommandOK;
            }
        }

        public string ListarDiretorioRemoto(string caminhoRemoto = "")
        {
            IList<string> diretorios = new List<string>();

            this.NovaConexao(caminhoRemoto);

            this._conexaoCriada.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            this._conexaoCriada.Timeout = 200000;
            FtpWebResponse resposta = (FtpWebResponse)this._conexaoCriada.GetResponse();
            Stream streamResposta = resposta.GetResponseStream();

            StreamReader leituraDaResposta = new StreamReader(streamResposta);

            string saida = leituraDaResposta.ReadToEnd();

            resposta.Close();
            streamResposta.Close();
            streamResposta.Flush();
            leituraDaResposta.Close();

            return saida ?? "";
        }
        public void MoverArquivo(string NomeArquivo, string pastaFonte, string PastaDestino)
        {
            
            FtpWebResponse ftpResponse = null;
            try
            {
                 NovaConexao(pastaFonte + "/" + NomeArquivo);
                _conexaoCriada.Timeout = 200000;
                _conexaoCriada.Method = WebRequestMethods.Ftp.Rename;
                _conexaoCriada.RenameTo = PastaDestino + "/" + NomeArquivo;               

                ftpResponse = (FtpWebResponse)_conexaoCriada.GetResponse();
               
                ftpResponse.Dispose();               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool Download(string caminhoRemoto, string arquivo, string caminhoDestino = "", string arquivoDestino = "")
        {
            this.NovaConexao(caminhoRemoto + arquivo);
            this._conexaoCriada.Method = WebRequestMethods.Ftp.DownloadFile;
            this._conexaoCriada.Timeout = 200000;
            FtpWebResponse response = (FtpWebResponse)this._conexaoCriada.GetResponse();

            Stream stream = response.GetResponseStream();

            using (FileStream file = new FileStream(arquivoDestino, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.CopyTo(file);
            }

            stream.Close();
            stream.Dispose();

            return true;
        }

        public bool DeletarArquivoRemoto(string caminhoRemoto, string arquivo)
        {
            try
            {
                FtpWebRequest request;
                FtpWebResponse response;
                FtpStatusCode responseStatusCode;
                string responseStatusDescription;

                this.NovaConexao(caminhoRemoto + arquivo);
                this._conexaoCriada.Timeout = 200000;
                this._conexaoCriada.Method = WebRequestMethods.Ftp.DeleteFile;
                request = this._conexaoCriada as FtpWebRequest;

                response = (FtpWebResponse)request.GetResponse();
                responseStatusCode = response.StatusCode;
                responseStatusDescription = response.StatusDescription;

                if (responseStatusCode == FtpStatusCode.ClosingData || responseStatusCode == FtpStatusCode.FileActionOK)
                {
                    return true;
                }
                else
                {
                    throw new Exception(response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Stream Download(string caminhoRemoto, string arquivo)
        {
            this.NovaConexao(caminhoRemoto + arquivo);
            
            this._conexaoCriada.Method = WebRequestMethods.Ftp.DownloadFile;

            this._conexaoCriada.Timeout = 200000;

            FtpWebResponse response = (FtpWebResponse)this._conexaoCriada.GetResponse();


            return response.GetResponseStream();
        }


        public bool Upload(string destinoRemoto, FileInfo arquivo)
        {
            try
            {
                FtpWebRequest requisicaoFTP;
                FtpWebResponse respostaFTP;

                this.NovaConexao(destinoRemoto + arquivo.Name);

                this._conexaoCriada.Method = WebRequestMethods.Ftp.UploadFile;
                requisicaoFTP = this._conexaoCriada as FtpWebRequest;

                using (FileStream fluxoArquivo = File.OpenRead(arquivo.FullName))
                {
                    using (Stream fluxoRequisicao = requisicaoFTP.GetRequestStream())
                    {
                        fluxoArquivo.CopyTo(fluxoRequisicao);
                        fluxoRequisicao.Close();
                    }
                }

                respostaFTP = (FtpWebResponse)requisicaoFTP.GetResponse();

                respostaFTP.Close();

                if (respostaFTP.StatusCode == FtpStatusCode.ClosingData || respostaFTP.StatusCode == FtpStatusCode.FileActionOK)
                {
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("{0} - {1}", respostaFTP.StatusCode, respostaFTP.StatusDescription));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Upload(string destinoRemoto, Stream fluxoArquivo, string nomeArquivo)
        {
            FtpWebRequest requisicaoFTP = null;
            FtpWebResponse respostaFTP = null;

            try
            {
                this.NovaConexao(destinoRemoto + nomeArquivo);

                this._conexaoCriada.Method = WebRequestMethods.Ftp.UploadFile;
                requisicaoFTP = this._conexaoCriada as FtpWebRequest;

                using (Stream fluxoRequisicao = requisicaoFTP.GetRequestStream())
                {
                    fluxoArquivo.CopyTo(fluxoRequisicao);

                    fluxoArquivo.Close();
                    fluxoArquivo.Dispose();
                }

                respostaFTP = (FtpWebResponse)requisicaoFTP.GetResponse();

                respostaFTP.Close();

                if (respostaFTP.StatusCode == FtpStatusCode.ClosingData || respostaFTP.StatusCode == FtpStatusCode.FileActionOK)
                {
                    return true;
                }
                else
                {
                    throw new WebException(string.Format("{0} - {1}", respostaFTP.StatusCode, respostaFTP.StatusDescription));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UploadAppend(string destinoRemoto, Stream fluxoArquivo, string nomeArquivo)
        {
            FtpWebRequest requisicaoFTP = null;
            FtpWebResponse respostaFTP = null;

            try
            {
                int tentativas = 2;
                int tentativaAtual = 1;


                while (tentativaAtual <= tentativas)
                {
                    try
                    {
                        this.NovaConexao(destinoRemoto + nomeArquivo);

                        this._conexaoCriada.Method = WebRequestMethods.Ftp.AppendFile;
                        requisicaoFTP = this._conexaoCriada as FtpWebRequest;
                        requisicaoFTP.Timeout = 600000;

                        using (Stream fluxoRequisicao = requisicaoFTP.GetRequestStream())
                        {
                            fluxoArquivo.CopyTo(fluxoRequisicao);
                            fluxoRequisicao.Close();
                        }

                        respostaFTP = (FtpWebResponse)requisicaoFTP.GetResponse();

                        respostaFTP.Close();
                        fluxoArquivo.Close();
                        fluxoArquivo.Dispose();

                        if (respostaFTP.StatusCode == FtpStatusCode.ClosingData || respostaFTP.StatusCode == FtpStatusCode.FileActionOK)
                        {
                            return true;
                        }
                        else
                        {                            
                            throw new WebException(string.Format("{0} - {1}", respostaFTP.StatusCode, respostaFTP.StatusDescription));
                        }                       
                    }
                    catch(Exception)
                    {
                        tentativaAtual++;
                        Thread.Sleep(5000);                       
                    }                   
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
