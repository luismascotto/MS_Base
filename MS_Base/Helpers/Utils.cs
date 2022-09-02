using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MS_Base.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Retorna o IPv4 da máquina onde o serviço está sendo executado.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            throw new Exception("Não há adaptadores de rede IPV4 nessa máquina!");
        }

        /// <summary>
        /// Retorna o endereço para comunicação com webservices, removendo possívels erros de // duplicado
        /// </summary>
        /// <returns></returns>
        public static string NormalizeURL(string strURL)
        {
            if (string.IsNullOrEmpty(strURL))
            {
                return "";
            }

            try
            {
                if (strURL.Contains("http") && strURL.Contains("://") && strURL.IndexOf("//") == strURL.LastIndexOf("//"))
                {
                    return strURL;
                }

                StringBuilder sbUrl = new();
                if (strURL.Contains("http://"))
                {
                    sbUrl.Append("http://");
                }
                else if (strURL.Contains("https://"))
                {
                    sbUrl.Append("https://");
                }
                sbUrl.Append(strURL.Replace("http://", "").Replace("httpa://", "").Replace("//", "/"));



                return sbUrl.ToString();
            }
            catch (Exception)
            {
                return strURL;
            }
        }

        /// <summary>
        /// Retorna o endereço completo para comunicação com webservices, removendo possívels erros de barra duplicada ou sem barra
        /// </summary>
        /// <returns></returns>
        public static string NormalizeURL(string strAddress, string strPath)
        {
            if (string.IsNullOrEmpty(strAddress) || string.IsNullOrEmpty(strPath))
            {
                return "";
            }

            try
            {
                StringBuilder sbUrl = new();

                //Sempre colocar o endereço sem / no final
                if (strAddress.EndsWith("/"))
                {
                    sbUrl.Append(strAddress.Substring(0, strAddress.Length - 1));
                }
                else
                {
                    sbUrl.Append(strAddress);
                }

                //Se Path não começar com /, adicionar /

                if (!strPath.StartsWith("/"))
                {
                    sbUrl.Append('/');
                }

                sbUrl.Append(strPath);



                return sbUrl.ToString();
            }
            catch (Exception)
            {
                return strAddress + strPath;
            }
        }

        /// <summary>
        /// Retornar valores com formato 0,00
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static string ToBRLDecimal(this double valor)
        {
            return valor.ToString("n2", CultureInfo.GetCultureInfo("Pt-Br"));
        }
        public static string ToBRLDecimal(this float valor)
        {
            return valor.ToString("n2", CultureInfo.GetCultureInfo("Pt-Br"));
        }
        public static string ToBRLDecimal(this decimal valor)
        {
            return valor.ToString("n2", CultureInfo.GetCultureInfo("Pt-Br"));
        }

    }
}
