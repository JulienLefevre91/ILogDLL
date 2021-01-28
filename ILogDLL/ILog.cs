using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ILogDLL
{
    public class ILog
    {
        internal static Token GetToken()
        {
            Token token = null;

            try
            {
                string baseAddress = ConfigurationManager.AppSettings["url_token"].ToString();

                string grant_type = ConfigurationManager.AppSettings["grant_type_ilog"].ToString();
                string client_id = ConfigurationManager.AppSettings["client_id_ilog"].ToString();
                string client_secret = ConfigurationManager.AppSettings["client_secret_ilog"].ToString();
                string scope = ConfigurationManager.AppSettings["scope_ilog"].ToString();

                Dictionary<string, string> formData = new Dictionary<string, string>() {
                    { "grant_type", grant_type },
                    { "client_id", client_id },
                    { "client_secret", client_secret },
                    { "scope", scope }
                };

                token = Task.Run(async () => await PostFormUrlEncoded(baseAddress, formData)).Result;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("[GetToken] Problème de récupération du Token : " + ex.Message + " | " + ex.StackTrace);
            }

            return token;
        }

        internal static async Task<Token> PostFormUrlEncoded(string url, IEnumerable<KeyValuePair<string, string>> postData)
        {
            Token t = null;

            using (var httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    t = await response.Content.ReadAsAsync<Token>();
                }
            }

            return t;
        }

        public static bool SaveLogToILog(string comment, string applicationName, string version, string category, string subject, string data)
        {
            bool success = false;

            try
            {
                Token t = GetToken();

                if (t == null)
                    throw new Exception("Erreur lors de la récupération du Token");

                dynamic x = new
                {
                    Comment = comment,
                    ApplicationName = applicationName,
                    Version = version,
                    Category = category,
                    Subject = subject,
                    Data = data
                };

                string json = Task.Run(async () =>
                    await Post(t, x, ConfigurationManager.AppSettings["url_api_ilog"].ToString())
                ).Result;

                success = true;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        internal static async Task<string> Post(Token token, dynamic parameters, string url)
        {
            string responseBody = string.Empty;

            try
            {
                string json = JsonConvert.SerializeObject(parameters);

                using (HttpClient httpClient = new HttpClient())
                {
                    //specify to use TLS 1.2 as default connection
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    if (token != null)
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    HttpContent httpContent = new StringContent(json, Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.PostAsync(url, httpContent);

                    if (response.IsSuccessStatusCode)
                        responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PostLog] Problème lors de l'appel à l'API ILogging : " + ex.Message + " | " + ex.StackTrace);
            }

            return responseBody;
        }
    }
}