using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections;
using Amqp;
using Microsoft.Extensions.Configuration;

namespace MS_Base.Helpers
{
    public class Logger : ILogger
    {
        public const int LOG_TYPE_PRESENTATION = 1;
        public const int LOG_TYPE_EXCEPTION = 2;
        public const int LOG_TYPE_BD_EXCEPTION = 3;
        public const int LOG_TYPE_LOCAL_EXCEPTION = 4;
        public const int LOG_TYPE_LOCAL_INFORMATION = 5;

        public const int LOG_LEVEL_INFORMATION = 1;
        public const int LOG_LEVEL_WARNING = 2;
        public const int LOG_LEVEL_CRITICAL = 3;

        public Queue arrLogs = new Queue();

        readonly IConfiguration _configuration;

        public Logger(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Returns Current Binary's Path.
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            string appPath = System.Reflection.Assembly.GetEntryAssembly().Location;

            appPath = appPath.Substring(0, appPath.LastIndexOf("\\"));

            if (!appPath.EndsWith("\\"))
            {
                appPath = $"{appPath}\\";
            }

            return appPath;
        }

        /// <summary>
        /// Save log messages
        /// </summary>
        /// <param name="_msg">Message to save</param>
        private void LogMessage(int _clienteId, string _strSerial, string _msg, string _class, string _method, int _type)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, _type, 0);
        }
        private void LogMessage(int _clienteId, string _strSerial, string _msg, string _class, string _method, int _type, int _level)
        {

            Models.Log objLog = new Models.Log
            {
                Client_ID = _clienteId,
                Application_ID = _configuration["ApplicationID"],
                vchSerialTerminal = _strSerial
            };
            if (_clienteId > 0)
            {
                objLog.vchMessage = $"[CLIENTE: {_clienteId}] {_msg} ";
            }
            else
            {
                objLog.vchMessage = $"{_msg} ";
            }

            objLog.vchFunctionName = $"{_class}.{_method}";

            objLog.intLevel = _level;
            objLog.intLogType = _type;
            objLog.dtmReceived = DateTime.Now;
            objLog.isException = _type == LOG_TYPE_EXCEPTION || _type == LOG_TYPE_BD_EXCEPTION;


            lock (arrLogs)
            {
                arrLogs.Enqueue(objLog);
            }


        }

        /// <summary>
        /// Save presentation messages.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogPresentation(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_PRESENTATION);
        }

        /// <summary>
        /// Save exception messages.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_EXCEPTION);
        }

        /// <summary>
        /// Save database exception messages.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogDbException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_BD_EXCEPTION, LOG_LEVEL_WARNING);
        }

        /// <summary>
        /// Save local exception messages into a log file.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogLocalException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_LOCAL_EXCEPTION);
        }

        /// <summary>
        /// Save local information messages into a log file.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogLocalInformation(string _msg, string _class, string _method)
        {
            LogMessage(0, "", _msg, _class, _method, LOG_TYPE_LOCAL_INFORMATION);
        }


        /// <summary>
        /// Save local information messages into a specific log file.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        /// <param name="_strFile">Arquivo_MS.txt to save</param>
        public void LogLocalInformation(string _strFile, string _msg, string _class, string _method)
        {
            LogMessage(0, _strFile, _msg, _class, _method, LOG_TYPE_LOCAL_INFORMATION);
        }



        /// <summary>
        /// Save CRITICAL exception messages.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogCriticalException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_EXCEPTION, LOG_LEVEL_CRITICAL);
        }

        /// <summary>
        /// Save CRITICAL database exception messages.
        /// </summary>
        /// <param name="_msg">Message to save</param>
        public void LogCriticalDbException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
        {
            LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_BD_EXCEPTION, LOG_LEVEL_CRITICAL);
        }


        /// <summary>
        /// Métodos para auxiliar gravação de logs de chamada e retorno do controller
        /// </summary>
        /// <param name="strL"></param>
        /// <param name="strRota"></param>
        /// <param name="objRecebido"></param>
        public void SetLogRecebido(ref StringBuilder strL, string strRota, object objRecebido)
        {
            try
            {
                strL.AppendLine();
                strL.AppendLine($"==================== {strRota} ====================");
                strL.AppendLine($"Recebido: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}");
                strL.AppendLine(JsonConvert.SerializeObject(objRecebido, Formatting.Indented));
                strL.AppendLine();
            }
            catch
            {
                //Nada a fazer
            }

        }
        public void SetLogEnviado(ref StringBuilder strL, object objEnviado)
        {
            try
            {
                strL.AppendLine($"Enviado: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}");
                strL.AppendLine(JsonConvert.SerializeObject(objEnviado, Formatting.Indented));
                strL.AppendLine("================================================================================");
            }
            catch
            {
                //Nada a fazer
            }
        }
        public void SetLogExcecao(ref StringBuilder strL, string strExcecao)
        {
            try
            {
                strL.AppendLine($"Exceção: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}");
                strL.AppendLine(strExcecao);
                strL.AppendLine("================================================================================");
            }
            catch
            {
                //Nada a fazer
            }
        }

        /// <summary>
        /// Método invocado pelo timerLog para salvar Logs
        /// </summary>
        public void SaveLogs()
        {
            Models.Log objLog = null;
            bool blnTemosLog = false;
            lock (arrLogs)
            {
                blnTemosLog = arrLogs.Count > 0;
            }
            if (!blnTemosLog)
            {
                return;
            }

            if (_configuration["LogAMQP_enabled"] == "1")
            {
                Connection connection = null;
                Session session = null;
                SenderLink senderLink = null;
                try
                {
                    Address address = new Address(_configuration["LogAMQP_url"]);
                    connection = new Connection(address);
                    session = new Session(connection);
                    senderLink = new SenderLink(session, "MSIS", _configuration["LogAMQP_fila"]);
                    while (blnTemosLog)
                    {
                        lock (arrLogs)
                        {
                            blnTemosLog = arrLogs.Count > 0;
                            if (blnTemosLog)
                            {
                                objLog = (Models.Log)arrLogs.Dequeue();
                            }
                            else
                            {
                                objLog = null;
                            }
                        }
                        if (objLog != null)
                        {
                            //Forçar que seja em texto caso seja informação local
                            if (objLog.intLogType == LOG_TYPE_LOCAL_EXCEPTION || objLog.intLogType == LOG_TYPE_LOCAL_INFORMATION)
                            {
                                //Console.WriteLine($"Saving Local");
                                SaveLogArquivo(objLog);
                            }
                            else
                            {
                                //Console.WriteLine($"Saving AMQP");
                                senderLink.Send(new Message(JsonConvert.SerializeObject(objLog)),
                                    TimeSpan.FromMilliseconds(int.Parse(_configuration["LogAMQP_timeout"])));
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogLocalException(0, "", ex.ToString(), "Logger", "SaveLogs");
                    //Console.WriteLine($"Log Exception - {ex.Message}");
                }
                finally
                {
                    if (senderLink != null)
                    {
                        senderLink.Close();
                    }
                    if (session != null)
                    {
                        session.Close();
                    }
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }

            }
            else
            {
                while (blnTemosLog)
                {
                    lock (arrLogs)
                    {
                        blnTemosLog = arrLogs.Count > 0;
                        if (blnTemosLog)
                        {
                            objLog = (Models.Log)arrLogs.Dequeue();
                        }
                        else
                        {
                            objLog = null;
                        }
                    }
                    if (objLog != null)
                    {
                        SaveLogArquivo(objLog);
                    }
                }
            }
        }

        /// <summary>
        /// Salvar logs em arquivo local
        /// </summary>
        /// <param name="objLog"></param>
        private void SaveLogArquivo(Models.Log objLog)
        {
            if (objLog == null)
            {
                return;
            }
            StreamWriter strW = null;
            try
            {
                string strDIR = $"{GetPath()}Log\\{DateTime.Now.ToString("yyyyMMdd")}\\";

                if (!Directory.Exists(strDIR))
                {
                    //Create a Directory if nos exists
                    Directory.CreateDirectory(strDIR);
                }


                if (objLog.intLogType == LOG_TYPE_PRESENTATION) //Troca de Mensagem
                {
                    strW = File.AppendText($"{strDIR}{objLog.vchSerialTerminal}.txt");
                }
                else if (objLog.intLogType == LOG_TYPE_EXCEPTION) //Exceção
                {
                    strW = File.AppendText($"{strDIR}{objLog.vchSerialTerminal}_Exception.txt");
                }
                else if (objLog.intLogType == LOG_TYPE_BD_EXCEPTION) //Exceção BD
                {
                    strW = File.AppendText($"{strDIR}MS_BD_Exception.txt");
                }
                else if (objLog.intLogType == LOG_TYPE_LOCAL_EXCEPTION) //Exceção na busca de Configurações
                {
                    strW = File.AppendText($"{strDIR}MS_Local_Exception.txt");
                }
                else if (objLog.intLogType == LOG_TYPE_LOCAL_INFORMATION) //Informativos do Serviço
                {
                    strW = File.AppendText($"{strDIR}{objLog.vchSerialTerminal}_MS.txt");
                }
                else
                {
                    strW = File.AppendText($"{strDIR}{objLog.vchSerialTerminal}_Desconhecido.txt");
                }


                strW.Write($"{objLog.dtmReceived.ToString("yyyyMMdd HH:mm:ss.fff")} - ");
                if (objLog.vchFunctionName.Length > 0)
                {
                    strW.Write($"{objLog.vchFunctionName}() - ");
                }
                strW.WriteLine($"{objLog.vchMessage}{Environment.NewLine}");
            }
            catch (Exception)
            {
                //Abafar exceção
            }
            finally
            {
                if (strW != null)
                {
                    strW.Dispose();
                }
            }
        }

    }
}
