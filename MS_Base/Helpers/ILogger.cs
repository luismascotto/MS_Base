using System;
using System.Text;

namespace MS_Base.Helpers;

public interface ILogger
{
    bool LogController { get; }

    string GetPath();

    string LogPresentation(int _clienteId, string _strSerial, string _msg, string _class, string method);

    string LogException(int _clienteId, string _strSerial, string _msg, string _class, string method);

    string LogExceptionId(int _clienteId, string _strSerial, string _msg, string _class, string method, string _logId);

    string LogDbException(int _clienteId, string _strSerial, string _msg, string _class, string method);

    string LogCriticalException(int _clienteId, string _strSerial, string _msg, string _class, string method);

    string LogLocalException(int _clienteId, string _strSerial, string _msg, string _class, string method);

    string LogLocalInformation(string _msg, string _class, string _method);

    string LogLocalInformation(string _strFile, string _msg, string _class, string method);

    void SetLogRecebido(ref StringBuilder strL, string strController, string strAcrion, object objRecebido);

    void SetLogEnviado(ref StringBuilder strL, object objEnviado);

    void SetLogExcecao(ref StringBuilder strL, string strExcecao);

    void SaveLogs();
}