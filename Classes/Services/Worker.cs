using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace Classes.Services
{
    public class Worker
    {
        private static string Username = "l12345678";
        private static string Password = "p12345678";
        private static string EncodedAuth = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                           .GetBytes(Username + ":" + Password));

        private static HttpClient HttpClient = new HttpClient();


        public async Task<HttpResponseMessage> PlacesCountMethodAsync(string nom_route)
        {
            var allDocs = await GetDocsAsync();
            var filteredDocs = allDocs
                    .Where(r => (string)r["nom_route"] == nom_route)
                    .ToList();

            return await GetSumPlacesAsync(filteredDocs);
        }

        public async Task<HttpResponseMessage> PosNameMethodAsync(string nom_doc)
        {
            var allDocs = await GetDocsAsync();
            return await GetPosNamesAsync(allDocs, nom_doc);
        }

        //метод для получения списка всех документов
        private static async Task<List<JToken>> GetDocsAsync()
        {
            string url = "http://api-test.tdera.ru/api/getdocumentlist";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Basic " + EncodedAuth);
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            var jObj = JObject.Parse(content);
            return jObj["data"]
                    .ToList();
        }

        //метод для подсчета суммы place_count
        private async Task<HttpResponseMessage> GetSumPlacesAsync(List<JToken> docs)
        {
            int count = 0;
            string result = "sum_place_count;\n";

            if(docs == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            foreach (var doc in docs)
            {
                string url = "http://api-test.tdera.ru/api/getdocument?id=" + doc["id_record"].ToString();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Basic " + EncodedAuth);
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();

                var jObj = JObject.Parse(content);
                var jData = jObj["data"]["data1"]
                            .Select(r => r);

                count += (int)jData.First()["place_count"];
            }

            result += count.ToString() + ";";
            return new HttpResponseMessage()
            {
                Content = new StringContent(result, Encoding.UTF8, "text/csv"),
                StatusCode = HttpStatusCode.OK
            };
        }

        //метод для вывода списка pos_names
        private async Task<HttpResponseMessage> GetPosNamesAsync(List<JToken> docs, string nom_doc)
        {
            JToken selectedDoc = null;
            StringBuilder result = new StringBuilder("all_pos_names;\n");

            foreach (var doc in docs)
            {
                string url = "http://api-test.tdera.ru/api/getdocument?id=" + doc["id_record"].ToString();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Basic " + EncodedAuth);
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();

                var jObj = JObject.Parse(content);
                var jData = jObj["data"]["data1"]
                            .Select(r => r);

                if (nom_doc == (string)jData.First()["nom_doc"])
                {
                    selectedDoc = jObj["data"]["data2"];
                    break;
                }
            }

            if(selectedDoc != null)
            {
                foreach (var j in selectedDoc.Children())
                {
                    if (j.SelectToken("pos_name") != null)
                        result.Append((string)j["pos_name"]).Append(";\n");
                }

                return new HttpResponseMessage()
                {
                    Content = new StringContent(result.ToString(), Encoding.UTF8, "text/csv"),
                    StatusCode = HttpStatusCode.OK
                };
            }
            else 
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        }
    }
}
