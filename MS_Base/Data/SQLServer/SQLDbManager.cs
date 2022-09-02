using MS_Base.Helpers;

namespace MS_Base.Data.SQLServer
{
    public static class SQLDbManager
    {
        private static string CONN_STRING = "";


        public static void SetConnectionString(string strConn, bool Encrypted = false)
        {
            if (!Encrypted)
            {
                CONN_STRING = strConn;
            }
            else
            {
                try
                {
                    CONN_STRING = Criptografia.Decrypt(strConn, Criptografia.strDBKey);
                }
                catch (System.Exception)
                {
                    CONN_STRING = Criptografia.Decrypt(strConn, Criptografia.strKey);
                }
            }
        }

        public static string GetConnectionString()
        {
            return CONN_STRING;
        }

    }
}
