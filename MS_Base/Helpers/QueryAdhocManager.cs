using System;
using System.Collections.Generic;
using System.IO;

namespace MS_Base.Helpers
{
    public static class QueryAdhocManager
    {
        public static FileSystemWatcher watcher = null;

        private static Dictionary<string, QueryModel> _arrQuery;

        public static Dictionary<string, QueryModel> ArrQuery()
        {
            if (_arrQuery == null)
            {
                _arrQuery = new Dictionary<string, QueryModel>();
            }

            return _arrQuery;
        }

        public static string GetQuery(string queryName, string queryPath)
        {
            QueryModel objQuery;

            if (!ArrQuery().ContainsKey(queryName))
            {
                string strFilePath = $"{queryPath}/{queryName}";
                FileInfo info = new FileInfo(strFilePath);

                objQuery = new QueryModel
                {
                    fileName = info.Name,
                    filePath = info.FullName,
                    modifiedDate = info.LastWriteTime,
                    queryContent = File.ReadAllText(strFilePath, System.Text.Encoding.UTF8)
                };
                ArrQuery().Add(objQuery.fileName, objQuery);
            }
            else
            {
                objQuery = ArrQuery()[queryName];
            }

            return objQuery.queryContent;
        }

        public static void LoadAllQuery(string strFilePath, bool startsMonitor = false)
        {

            try
            {

                foreach (string fl in Directory.GetFiles(strFilePath))
                {
                    FileInfo info = new FileInfo(fl);

                    QueryModel objQuery;

                    if (!ArrQuery().ContainsKey(info.Name))
                    {
                        objQuery = new QueryModel
                        {
                            fileName = info.Name,
                            filePath = info.FullName,
                            modifiedDate = info.LastWriteTime,
                            queryContent = File.ReadAllText(fl, System.Text.Encoding.UTF8)
                        };


                        ArrQuery().Add(objQuery.fileName, objQuery);

                    }
                    else
                    {
                        objQuery = ArrQuery()[info.Name];
                        if (objQuery.modifiedDate != info.LastWriteTime)
                        {
                            objQuery.modifiedDate = info.LastWriteTime;
                            objQuery.queryContent = File.ReadAllText(fl, System.Text.Encoding.UTF8);
                            ArrQuery()[info.Name] = objQuery;
                        }
                    }

                }

                if (startsMonitor)
                {
                    //Chamar o Watcher
                    CreateWatcher(strFilePath);
                }
            }
            catch (Exception ex)
            {
                //Do nothing   
                Console.Write(ex.ToString());
            }

        }

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void CreateWatcher(string path)
        {
            try
            {
                if (watcher == null)
                {
                    watcher = new FileSystemWatcher(path)
                    {
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,

                        // Only watch SQL Files
                        Filter = "*.sql"
                    };

                    watcher.Changed += Watcher_Changed;
                    watcher.Created += Watcher_Changed;
                    watcher.Deleted += Watcher_Changed;
                    watcher.Renamed += Watcher_Renamed;

                    //Start Watcher
                    watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception)
            {
                //Do nothing
            }

        }

        //Rebuild Cache
        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            reloadAll(sender);
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            reloadAll(sender);
        }

        private static void reloadAll(object sender)
        {
            try
            {
                //Stop Watcher
                watcher.EnableRaisingEvents = false;
                System.Threading.Thread.Sleep(5000);

                _arrQuery = new Dictionary<string, QueryModel>();
                LoadAllQuery(((FileSystemWatcher)sender).Path);
            }
            finally
            {
                //Start Watcher
                watcher.EnableRaisingEvents = true;
            }
        }

    }
}
