using Amqp;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace MS_Base.Helpers;

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

    public Queue arrLogs = new();


    public bool LogController => bool.TrueString == _configuration["ControllerLog"];

    private readonly IConfiguration _configuration;

    public Logger(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    readonly JsonSerializerOptions jsonLogSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };


    /// <summary>
    /// Returns Current Binary's Path.
    /// </summary>
    /// <returns></returns>
    public string GetPath()
    {
        var appPath = System.Reflection.Assembly.GetEntryAssembly()?.Location;

        if (appPath != null)
        {
            appPath = appPath.Substring(0, appPath.LastIndexOf(Path.DirectorySeparatorChar));

            if (!appPath.EndsWith(Path.DirectorySeparatorChar))
            {
                appPath = $"{appPath}{Path.DirectorySeparatorChar}";
            }
        }

        return appPath;
    }

    /// <summary>
    /// Save log messages
    /// </summary>
    /// <param name="_clienteId"></param>
    /// <param name="_strSerial">Message to save</param>
    /// <param name="msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    /// <param name="_type"></param>
    private string LogMessage(int _clienteId, string _strSerial, string msg, string _class, string _method, int _type)
    {
        return LogMessage(_clienteId, _strSerial, msg, _class, _method, _type, LOG_LEVEL_INFORMATION, "");
    }

    private string LogMessage(int _clienteId, string _strSerial, string _msg, string _class, string _method, int _type, int _level, string _logId)
    {
        Models.Log objLog = new(_logId)
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
        objLog.isException = _type is LOG_TYPE_EXCEPTION or LOG_TYPE_BD_EXCEPTION;

        lock (arrLogs)
        {
            arrLogs.Enqueue(objLog);
        }
        return objLog._id;
    }

    /// <summary>
    /// Save presentation messages.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogPresentation(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_PRESENTATION);
    }

    /// <summary>
    /// Save exception messages.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_EXCEPTION);
    }

    /// <summary>
    /// Save exception messages with provided id.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    /// <param name="_logId"></param>
    public string LogExceptionId(int _clienteId, string _strSerial, string _msg, string _class, string _method, string _logId)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_EXCEPTION, LOG_LEVEL_WARNING, _logId);
    }

    /// <summary>
    /// Save database exception messages.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogDbException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_BD_EXCEPTION, LOG_LEVEL_WARNING, "");
    }

    /// <summary>
    /// Save local exception messages into a log file.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogLocalException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_LOCAL_EXCEPTION);
    }

    /// <summary>
    /// Save local information messages into a log file.
    /// </summary>
    /// <param name="_msg">Message to save</param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogLocalInformation(string _msg, string _class, string _method)
    {
        return LogMessage(0, "", _msg, _class, _method, LOG_TYPE_LOCAL_INFORMATION);
    }

    /// <summary>
    /// Save local information messages into a specific log file.
    /// </summary>
    /// <param name="_strFile">Message to save</param>
    /// <param name="_msg">Arquivo_MS.txt to save</param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogLocalInformation(string _strFile, string _msg, string _class, string _method)
    {
        return LogMessage(0, _strFile, _msg, _class, _method, LOG_TYPE_LOCAL_INFORMATION);
    }

    /// <summary>
    /// Save CRITICAL exception messages.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogCriticalException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_EXCEPTION, LOG_LEVEL_CRITICAL, "");
    }

    /// <summary>
    /// Save CRITICAL database exception messages.
    /// </summary>
    /// <param name="_clienteId">Message to save</param>
    /// <param name="_strSerial"></param>
    /// <param name="_msg"></param>
    /// <param name="_class"></param>
    /// <param name="_method"></param>
    public string LogCriticalDbException(int _clienteId, string _strSerial, string _msg, string _class, string _method)
    {
        return LogMessage(_clienteId, _strSerial, _msg, _class, _method, LOG_TYPE_BD_EXCEPTION, LOG_LEVEL_CRITICAL, "");
    }

    /// <summary>
    /// Métodos para auxiliar gravação de logs de chamada e retorno do controller
    /// </summary>
    /// <param name="strL"></param>
    /// <param name="strController"></param>
    /// <param name="objRecebido"></param>
    public void SetLogRecebido(ref StringBuilder strL, string strController, string strAcrion, object objRecebido)
    {
        try
        {
            strL.AppendLine();
            strL.Append("==================== ").Append(strController).Append('/').Append(strAcrion).AppendLine(" ====================");
            strL.Append("Recebido: ").AppendFormat("{0:yyyy-MM-dd HH:mm:ss:fff}", DateTime.Now).AppendLine();
            if (objRecebido != null)
            {
                strL.AppendLine(JsonSerializer.Serialize(objRecebido, jsonLogSerializerOptions));
                strL.AppendLine();
            }
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
            strL.Append("Enviado: ").AppendFormat("{0:yyyy-MM-dd HH:mm:ss:fff}", DateTime.Now).AppendLine();
            strL.AppendLine(JsonSerializer.Serialize(objEnviado, jsonLogSerializerOptions));
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
            strL.Append("Exceção: ").AppendFormat("{0:yyyy-MM-dd HH:mm:ss:fff}", DateTime.Now).AppendLine();
            strL.AppendLine(strExcecao);
            strL.AppendLine();
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
        Models.Log objLog;
        bool blnTemosLog;
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
                Address address = new(_configuration["LogAMQP_url"]);
                connection = new Connection(address);
                session = new Session(connection);
                senderLink = new SenderLink(session, "MSBASELOG", _configuration["LogAMQP_fila"]);
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
                            senderLink.Send(new Message(JsonSerializer.Serialize(objLog)),
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
                senderLink?.Close();
                session?.Close();
                connection?.Close();
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
        try
        {
            // using StreamWriter strW = new StreamWriter(,);
            var strFilePath = $"{GetPath()}Log{Path.DirectorySeparatorChar}{DateTime.Now:yyyyMMdd}{Path.DirectorySeparatorChar}";
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }


            strFilePath = objLog.intLogType switch
            {
                LOG_TYPE_PRESENTATION => $"{strFilePath}{objLog.vchFunctionName.Replace(Path.GetInvalidFileNameChars().ToString() ?? string.Empty, "_")}-{DateTime.Now.Hour:00}.txt",
                LOG_TYPE_EXCEPTION => $"{strFilePath}Exception.txt",
                LOG_TYPE_BD_EXCEPTION => $"{strFilePath}DB_Exception.txt",
                LOG_TYPE_LOCAL_EXCEPTION => $"{strFilePath}Local_Exception.txt",
                LOG_TYPE_LOCAL_INFORMATION => $"{strFilePath}Local.txt",
                _ => $"{strFilePath}{objLog.vchSerialTerminal}_Desconhecido.txt",
            };
            using var strW = File.AppendText(strFilePath);
            strW.Write($"{objLog.dtmReceived:yyyy-MM-dd HH:mm:ss.fff} - ");
            if (objLog.vchFunctionName.Length > 0)
            {
                strW.Write($"{objLog.vchFunctionName}() - ");
            }
            strW.WriteLine($"{objLog.vchMessage}{Environment.NewLine}");
        }
        catch { /*ignored*/ }
    }
}