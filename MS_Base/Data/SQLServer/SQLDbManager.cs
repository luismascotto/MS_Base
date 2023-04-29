using MS_Base.Helpers;

namespace MS_Base.Data.SQLServer;

public static class SQLDbManager
{
    private static string CONN_STRING = "";


    public static void SetConnectionString(string strConn)
    {
        CONN_STRING = strConn;
    }


    public static string GetConnectionString()
    {
        return CONN_STRING;
    }

}
