using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using RestSharp;
using Newtonsoft.Json;

namespace rdi
{
    class ApiError
    {
        public string message { get; set; }
        public int status { get; set; }
    }

    class Program
    {
        static NameValueCollection stg;
        static string _errpath;

        static void ErrorMesg(string msg)
        {
            File.AppendAllText(_errpath, DateTime.Now.ToString("yy/MM/dd hh:mm:sss ") + msg + "\n"); 
        }

        static void Main(string[] args)
        {
            try
            {
                stg = ConfigurationManager.AppSettings;

                var source = stg["LogDirectory"];
                _errpath = Path.Combine(source, stg["Errorlog"]);

                var logs = Directory.GetFiles(source);
                var archive = Path.Combine(Path.GetTempPath(), "tmp"+DateTime.Now.ToString("yyyyMMddhhmmsss")+".7z");

                if (logs.Length == 0)
                {
                    return;
                }

                CreateArchive(source, archive);

                var client = new RestClient(stg["Url"]);
                var request = new RestRequest("log", Method.POST);
                request.AddParameter("User.Name", stg["User"]);
                request.AddParameter("User.Password", stg["Password"]);

                request.AddParameter("DeviceId", stg["DeviceId"]);
                request.AddFile("file", archive);

                IRestResponse response = client.Execute(request);
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    ErrorMesg("net error " + response.ErrorMessage + " " + stg["Url"]);
                    return;
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    ErrorMesg("response " + response.StatusCode);
                    var error = JsonConvert.DeserializeObject<ApiError>(response.Content);
                    ErrorMesg("error    " + error.message);
                    return;
                }

                var confirmed = JsonConvert.DeserializeObject<List<string>>(response.Content);
                foreach (string log in confirmed)
                    File.Delete(Path.Combine(source, log));

                File.Delete(archive);
            }
            catch (Exception e)
            {
                ErrorMesg("error " + e.Message);
                ErrorMesg("stack " + e.StackTrace);
            }
        }

        static void CreateArchive(string sourcedir, string archive)
        {
            string sources = sourcedir + "\\*";

            ProcessStartInfo p = new ProcessStartInfo();
            p.UseShellExecute = false;
            p.WindowStyle = ProcessWindowStyle.Hidden;
            p.CreateNoWindow = true;
            p.FileName = stg["7Zip"];
            p.Arguments = "a -t7z \"" + archive + "\" \"" + sources + "\" -mx=9";
            Process x = Process.Start(p);
            x.WaitForExit();
        }
    }
}

