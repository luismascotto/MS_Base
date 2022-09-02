using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS_Base.Helpers {
    public static class Comunicacao {

        [Obsolete("Alterar para URL e PATH separados")]
        public static T ComunicaWebService<T>(string strURL, Object obj)
        {
            string result = ComunicaWebService(Utils.NormalizeURL(strURL), "", "", "", obj);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public static T ComunicaWebService<T>(string strServer, string strPath, Object obj)
        {
            string result = ComunicaWebService(strServer, strPath, "", "", obj);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public static string ComunicaWebService(string strServer, string strPath, string user, string pass, Object obj)
        {
            string strFinalUrl = Utils.NormalizeURL(strServer, strPath);
            using var client = new HttpClient();
            Task<HttpResponseMessage> task;
            HttpResponseMessage response;

            if (user != string.Empty && pass != string.Empty)
            {
                var bytePass = Encoding.ASCII.GetBytes($"{user}:{pass}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytePass));
            }


            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            task = client.PostAsync(new Uri(strFinalUrl), content);
            response = task.Result;

            string result = response.Content.ReadAsStringAsync().Result;

            return result;
        }
    }
}
