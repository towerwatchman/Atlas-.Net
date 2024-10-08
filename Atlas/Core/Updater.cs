using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlas.Core
{
    public static class Updater
    {
        public static void CheckForUpdates()
        {
            //Check GH releases for updates and download if found
            string url = "https://api.github.com/repos/KJNeko/Atlas/releases";

            RequestJSON(url);
        }
        public static DataTable RequestJSON(string url)
        {
            DataTable dataTable = new DataTable();
            string response = string.Empty;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept" , "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            try
            {
                HttpResponseMessage rspmsg = client.GetAsync(url).Result;
                rspmsg.EnsureSuccessStatusCode();
                response = rspmsg.Content.ReadAsStringAsync().Result;


                if (response != string.Empty)
                {
                    JArray? json = null;

                    //stock
                    JArray jsonArray = JArray.Parse(response);
                    string version = jsonArray[0]["tag_name"].ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return dataTable;
        }
    }
}
