using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        string GetToken()
        {
            string tk = "";
            string URL = "http://magazinestore.azurewebsites.net/api/token";
            string urlParameters = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("Application/JSON"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string jsonString = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                if (jsonString.Length > 0)
                {
                    JObject r = JObject.Parse(jsonString);
                    JToken jt = r.GetValue("token");
                    tk = jt.ToObject<string>();
                    Console.WriteLine(tk);
                }
            }
            client.Dispose();
            return tk;
        }

        List<string> GetCategories(string token)
        {
            List<string> cat = null;
            string URL = "http://magazinestore.azurewebsites.net/api/categories/" + token;
            string urlParameters = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("Application/JSON"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string jsonString = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                if (jsonString.Length > 0)
                {
                    JObject r = JObject.Parse(jsonString);
                    JToken jt = r.GetValue("data");
                    cat = jt.ToObject<List<string>>();
                    //Console.WriteLine(cat);
                    
                }
            }
            client.Dispose();
            return cat;
        }

        List<Object> GetMagazines(string token, string cat)
        {
            List<Object> mag = null;
            string URL = "http://magazinestore.azurewebsites.net/api/magazines/" + token + "/" + cat;
            string urlParameters = "";
            HttpClient client = new HttpClient();
            Console.WriteLine(URL);
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("Application/JSON"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string jsonString = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                if (jsonString.Length > 0)
                {
                    JObject r = JObject.Parse(jsonString);
                    JToken jt = r.GetValue("data");
                    mag = jt.ToObject<List<Object>>();
                    //Console.WriteLine(cat);

                }
            }
            client.Dispose();
            return mag;
        }


        static void Main(string[] args)
        {
            Program p = new Program();
            string tk = p.GetToken();
            List<string> cat = p.GetCategories(tk);
            foreach (string c in cat)
            {
                Console.WriteLine("cat: " + c);
                List<Object> mag = p.GetMagazines(tk, c);
                foreach (Object m in mag)
                    Console.WriteLine("mag: " + m.ToString());
            }
            Console.ReadLine();
        }
    }
}
