using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MS_Base.Helpers;

public class Cache
{
    public bool bFlagRestarting = false;

    private readonly IConfiguration _configuration;

    public Cache(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public enum CacheLocation
    {
        MEMORY = 1,
        REDIS = 2
    }

    public enum DataBase
    {
        CLUSTER = -1,
        CONFIG = 0,
        EXTRACOES = 1,
        TIMEZONE = 2,
        PONTOOPERADOR = 3,
        TOKEN = 4,
        PROMOCAOSMS = 5,
        COMISSAO = 6,
        QTDDIASPRE = 7,
        TIPOJOGO = 8,
        CONFIGURACAOLOCALIZACAO = 9,
        DEPARA_ME = 10,
        MSGESTAO = 11,
        MSAUTENTICACAO = 12,
        CONCURSO = 13,
        JBWEB = 14,
        MEGASERVICE = 15
    }

    public enum CacheRedisType
    {
        REGULAR = 0,
        CLUSTER = 1
    }

    private ConnectionMultiplexer redis;
    private ISubscriber sub;
    private List<IServer> server;
    private static MemoryCache _memoryCache;

    public int GetCacheLocation()
    {
        int result = 1;

        try
        {
            result = int.Parse(_configuration["CacheLocation"]);
        }
        catch
        {
            //Do nothing
        }

        return result;
    }

    public MemoryCache GetMemoryCache()
    {
        if (_memoryCache == null)
        {
            MemoryCacheOptions opts = new();
            _memoryCache = new MemoryCache(opts);
        }

        return _memoryCache;
    }

    public ConnectionMultiplexer GetRedisConnection()
    {
        string CacheURL = _configuration["CacheURL"];

        if (redis == null)
        {
            ConfigurationOptions opt = new()
            {
                AllowAdmin = true      //Conectar como ADMIN para permitir limpar o DB
            };

            //No caso de Redis Cluster, as URLs de cada Nó vem separadas por ,
            var arrEndpoint = CacheURL.Split(',');
            server = new List<IServer>();
            foreach (var endPoint in arrEndpoint)
            {
                opt.EndPoints.Add(endPoint);
            }

            opt.ConnectRetry = 2;
            opt.ConnectTimeout = 3000;

            try
            {
                redis = ConnectionMultiplexer.Connect(opt);
            }
            catch (Exception)
            {
                throw;
            } 
            
            foreach (var endPoint in arrEndpoint)
            {
                server.Add(redis.GetServer(endPoint));
            }

            sub = redis.GetSubscriber();
            sub.Subscribe("CacheBase", (channel, message) =>
            {
                try
                {
                    var command = message.ToString().Split(':');
                    if (server != null && command[0] == "clear")
                    {
                        var dataBase = int.Parse(command[1]);
                        foreach (var objServer in server)
                        {
                            objServer.FlushDatabase(dataBase);
                        }
                    }
                }
                catch
                {
                    //Do nothing
                }
            });
        }

        return redis;
    }

    public void destroyCacheObjects()
    {
        try
        {
            sub?.UnsubscribeAll();

            if (server != null)
            {
                foreach (var objServer in server)
                {
                    objServer.ClientKill();
                }
            }

            redis?.Dispose();

            sub = null;
            server = null;
            redis = null;
        }
        catch (Exception)
        {
            //Isto foi apenas um teste para averiguarmos problemas de conexão no Redis
            //try
            //{
            //    //Do nothing
            //    File.AppendText("C:/temp/corecache.txt").WriteLine("Falha ao destruir Cache:" + ex.ToString());
            //}
            //catch (Exception)
            //{
            //}
        }
    }

    public bool AddItem(string key, ICache cachedObj, int expiration, int dbID, bool useSeconds = false)
    {
        if (expiration < 0)
        {
            //Calcular até 00:00
            DateTime Agora = DateTime.Now;
            DateTime Amanha = Agora.AddDays(1).Date; //.Date pega só a data (retira info de hora/minuto/segundo)
            expiration = (int)(Amanha - Agora).TotalSeconds;
        }
        else
        {
            if (!useSeconds)
            {
                expiration *= 60;
            }
        }

        //Fallback
        if (expiration <= 0)
        {
            expiration = 10 * 60;
        }

        if (GetCacheLocation() == (int)CacheLocation.REDIS)
        {
            if (int.Parse(_configuration["CacheType"]) == (int)CacheRedisType.CLUSTER)
            {
                dbID = (int)DataBase.CLUSTER;
            }

            var db = GetRedisConnection().GetDatabase(dbID);
            db.StringSet(key, JsonSerializer.Serialize(cachedObj), TimeSpan.FromSeconds(expiration), When.Always, CommandFlags.FireAndForget);
        }
        else //Default Memory
        {
            var opcoesDoCache = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(expiration)
            };

            GetMemoryCache().Set(key, cachedObj, opcoesDoCache);
        }

        return true;
    }

    public T GetItem<T>(string key, int dbID)
    {
        Object ret;

        if (GetCacheLocation() == (int)CacheLocation.REDIS)
        {
            var db = GetRedisConnection().GetDatabase(dbID);

            try
            {
                string strObj = db.StringGetAsync(key).Result;
                if (!string.IsNullOrEmpty(strObj))
                {
                    ret = JsonSerializer.Deserialize<T>(strObj);
                    return (T)ret;
                }
            }
            catch (Exception)
            {

                //Isto foi apenas um teste para averiguarmos problemas de conexão no Redis
                //try
                //{
                //    //Do nothing
                //    File.AppendText("C:/temp/corecache.txt").WriteLine("Falha ao Ler Cache:" + ex.ToString());
                //}
                //catch (Exception)
                //{
                //}

                //Caso aja alguma exceção ao obter um objeto no cache, remontar os objetos de conexão com Redis.
                destroyCacheObjects();
                throw;
            }
        }
        else //Default Memory
        {
            ret = GetMemoryCache().Get<T>(key);
            if (ret != null)
            {
                return (T)Convert.ChangeType(ret, typeof(T));
            }
        }

        return default;
    }

    public void ClearItem(string key, int dbID = (int)DataBase.MEGASERVICE)
    {
        try
        {
            if (GetCacheLocation() == (int)CacheLocation.REDIS)
            {
                if (int.Parse(_configuration["CacheType"]) == (int)CacheRedisType.CLUSTER)
                {
                    dbID = (int)DataBase.CLUSTER;
                }

                var db = GetRedisConnection().GetDatabase(dbID);

                List<RedisKey> keys = new();
                //Para limpeza, passa 0 quando é Cluster (e não -1 como no get e no set)
                if (int.Parse(_configuration["CacheType"]) == (int)CacheRedisType.CLUSTER)
                {
                    foreach (var objServer in server)
                    {
                        keys.AddRange(objServer.Keys(pattern: key).ToArray());
                    }
                }
                else
                {
                    keys.AddRange(server[0].Keys(database: dbID, pattern: key).ToArray());
                }

                db.KeyDelete(keys.ToArray(), CommandFlags.FireAndForget);
            }
            else
            {
                GetMemoryCache().Remove(key);
            }
        }
        catch
        {
            //Do nothing
        }
    }

    public static string GetCacheKeyID(int client)
    {
        //return $"-ID_{client}-[{client}]";
        return $"-ID_{client}-";
    }
}