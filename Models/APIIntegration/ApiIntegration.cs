using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Configuration;

namespace VatsCallsMonitoring.Models
{
    static class ApiIntegration
    {
        static HttpClient Client;
        static string ApiPrepareDownloadUrl = ConfigurationManager.AppSettings.Get("ApiPrepareDownloadUrl");
        static string ApiDownloadUrl = ConfigurationManager.AppSettings.Get("ApiDownloadUrl");
        static string ApiUserInfoUrl = ConfigurationManager.AppSettings.Get("ApiUserInfoUrl"); 
        static string ClientID = ConfigurationManager.AppSettings.Get("ClientID");
        static string UID = ConfigurationManager.AppSettings.Get("UID");
        static string Domain = ConfigurationManager.AppSettings.Get("Domain");

        /// <summary>
        /// Requests vats calls data and waits for it's answer
        /// Returns path to csv file containing the data
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static string DownloadCallsData(DateTime startDate, DateTime endDate)
        {
            if (Client == null)
            {
                Client = new HttpClient();
            }

            object jsonBody = new
            {
                date_start = startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                date_end = endDate.ToString("yyyy-MM-dd HH:mm:ss"),
                direction = 0,
                state = 0,
            };

            HttpResponseMessage response = SendRequest(jsonBody, ApiPrepareDownloadUrl).Result;

            string responseBody = response.Content.ReadAsStringAsync().Result;
            string orderId = Regex.Match(responseBody, "(?<=order_id\": \").*(?=\")").Value;

            jsonBody = new
            {
                order_id = orderId
            };

            do
            {
                Thread.Sleep(10000);
                response = SendRequest(jsonBody, ApiDownloadUrl).Result;
            }
            while (response.StatusCode != HttpStatusCode.OK);

            byte[] gzip = response.Content.ReadAsByteArrayAsync().Result;
            string zipPath = Path.GetTempPath() + "apiVATS.zip";
            string folderPath = zipPath.Replace(".zip", "");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            File.WriteAllBytes(zipPath, gzip);
            ZipFile.ExtractToDirectory(zipPath, folderPath);
            return Directory.GetFiles(folderPath)[0];
        }

        public static List<Group> GetGroups()
        {
            if (Client == null)
            {
                Client = new HttpClient();
            }

            object jsonBody = new
            {
                domain = Domain,
                user_name = "",
                user_pin = ""
            };

            HttpResponseMessage response = SendRequest(jsonBody, ApiUserInfoUrl).Result;

            string responseBody = response.Content.ReadAsStringAsync().Result;
            JSONConvertionUtility converter = JsonConvert.DeserializeObject<JSONConvertionUtility>(responseBody);
            return converter.ConvertToGroups();
        }

        private async static Task<HttpResponseMessage> SendRequest(object jsonBody, string url)
        {
            string json = JsonConvert.SerializeObject(jsonBody);
            string hash = CalculateHash(json);

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
                {
                    Headers =
                    {
                        { "X-Client-ID", ClientID },
                        { "X-Client-Sign", hash },
                    }
                }
            };
            return await Client.SendAsync(request);
        }

        private static string CalculateHash(string requestBody)
        {
            SHA256 hashAlgorithm = SHA256.Create();
            byte[] hashInput = Encoding.UTF8.GetBytes(ClientID + requestBody + UID);
            byte[] hash = hashAlgorithm.ComputeHash(hashInput);

            string hashString = "";
            for (int i = 0; i < hash.Length; i++)
            {
                hashString += hash[i].ToString("x2");
            }
            return hashString;
        }

        private class JSONConvertionUtility
        {
            public List<JSONUserConverter> users { get; set; }
            public List<JSONGroupConverter> groups { get; set; }

            public List<Group> ConvertToGroups()
            {
                Dictionary<string, User> usersDictionary = new Dictionary<string, User>();
                foreach (JSONUserConverter converter in users)
                {
                    User user = converter.ConvertToUser();
                    usersDictionary.Add(user.Pin, user);
                }

                List<Group> groupsList = new List<Group>();
                foreach (JSONGroupConverter converter in groups)
                {
                    Group group = converter.ConvetToGroup();
                    foreach (string pin in converter.users_list)
                    {
                        group.Users.Add(usersDictionary[pin]);
                    }
                    groupsList.Add(group);
                }

                return groupsList;
            }
        }

        private class JSONUserConverter
        {
            public string display_name { get; set; }
            public string name { get; set; }
            public string pin { get; set; }
            public bool is_supervisor { get; set; }
            public bool is_operator { get; set; }
            public string email { get; set; }
            public int recording { get; set; }

            public User ConvertToUser()
            {
                return new User() { DisplayName = display_name, Email = email, IsOperator = is_operator, IsSupervisor = is_supervisor, Name = name, Pin = pin, Recording = recording };
            }
        }

        private class JSONGroupConverter
        {
            public string name { get; set; }
            public string pin { get; set; }

            public string[] users_list;

            public Group ConvetToGroup()
            {
                return new Group() { Pin = pin, Name = name, Number = null, Users = new List<User>(), Calls = new List<Call>() };
            }
        }
    }
}
