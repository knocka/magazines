using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace RestClient
{
    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }

    static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private static HttpWebResponse MultipartFormPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, string headerkey, string headervalue)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;
 
            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData, headerkey, headervalue);
        }

        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData, string headerkey, string headervalue)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.  
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:  
            // request.PreAuthenticate = true;  
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;  

            //Add header if needed  
            request.Headers.Add(headerkey, headervalue);

            // Send the form data to the request.  
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {

                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter) // to check if parameter if of file type   
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream  
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.  
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline  
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]  
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public static class PostJSON
        {
            public static void Answer(string token, string mmm)
            {
                string URL = "http://magazinestore.azurewebsites.net/api/answer/" + token;
                Console.WriteLine(URL);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"subscribers\": [ " + mmm + " ] }";
                    Console.WriteLine("json: " + json);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (
                    
                    var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine("result: " + result.ToString());
                }
            }
        }


        public class MagazineStore
        {
            string GetToken()
            {
                string tk = "";
                string URL = "http://magazinestore.azurewebsites.net/api/token";
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

            List<Object> GetSubscribers(string token)
            {
                List<Object> mag = null;
                string URL = "http://magazinestore.azurewebsites.net/api/subscribers/" + token;
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

            void PostAnswer(string token, string mmm)
            {
                string URL = "http://magazinestore.azurewebsites.net/api/answer/" + token;
                string urlParameters = "";
                try
                {
                    string requestURL = URL + urlParameters;
                    Dictionary<string, object> postParameters = new Dictionary<string, object>();

                    // Add your parameters here  
                    postParameters.Add("answer", mmm);

                    string userAgent = "Someone";
                    string headerkey = "key";
                    string headervalue = "value";
                    HttpWebResponse webResponse = FormUpload.MultipartFormPost(requestURL, userAgent, postParameters, headerkey, headervalue);

                    // Process response  
                    StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                    string returnResponseText = responseReader.ReadToEnd();
                    webResponse.Close();
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }

            static void Main(string[] args)
            {
                string mmm = "";
                MagazineStore p = new MagazineStore();
                string tk = p.GetToken();
                List<string> cat = p.GetCategories(tk);
                foreach (string c in cat)
                {
                    Console.WriteLine("cat: " + c);
                    List<Object> mag = p.GetMagazines(tk, c);
                    foreach (Object m in mag)
                    {
                        Console.WriteLine("mag: " + m.ToString());
                        mmm = mmm + m.ToString();
                    }
                }
                mmm = "";
                List<Object> subs = p.GetSubscribers(tk);
                foreach (Object sub in subs)
                {
                    Console.WriteLine("sub: " + sub.ToString());
                    mmm = mmm + sub.ToString() + ", ";
                }
                Console.WriteLine("-------------------------------");
                PostJSON.Answer(tk, mmm);
                Console.WriteLine("Press any key to exit!");
                Console.ReadLine();
            }
        }
    }
}

