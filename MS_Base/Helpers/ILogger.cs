using System;
using System.Collections.Generic;
using System.Text;

namespace MS_Base.Helpers
{
    public interface ILogger
    {
        bool LogController { get; }

        string GetPath();
        void LogPresentation(int _clienteId, string _strSerial, string _msg, string _class, string _method);
        void LogException(int _clienteId, string _strSerial, string _msg, string _class, string _method);
        void LogDbException(int _clienteId, string _strSerial, string _msg, string _class, string _method);
        void LogCriticalException(int _clienteId, string _strSerial, string _msg, string _class, string _method);
        void LogLocalException(int _clienteId, string _strSerial, string _msg, string _class, string _method);
        void LogLocalInformation(string _msg, string _class, string _method);
        void LogLocalInformation(string _strFile, string _msg, string _class, string _method);
        void SetLogRecebido(ref StringBuilder strL, string strRota, object objRecebido);
        void SetLogEnviado(ref StringBuilder strL, object objEnviado);
        void SetLogExcecao(ref StringBuilder strL, string strExcecao);
        void SaveLogs();
    }
}
