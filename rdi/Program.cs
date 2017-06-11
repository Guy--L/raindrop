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

        static void Main(string[] args)
        {
            try
            {
                stg = ConfigurationManager.AppSettings;

                var logs = Directory.GetFiles(stg["LogDirectory"]);
                var archive = Path.Combine(Path.GetTempPath(), "tmp"+DateTime.Now.ToString("yyyyMMddhhmmsss")+".7z");

                if (logs.Length == 0)
                {
                    Console.WriteLine("No logs found");
                    Console.ReadKey();
                    return;
                }

                CreateArchive(stg["LogDirectory"], archive);

                var client = new RestClient(stg["Url"]);
                var request = new RestRequest("log", Method.POST);
                request.AddParameter("User.Name", stg["User"]);
                request.AddParameter("User.Password", stg["Password"]);

                request.AddParameter("DeviceId", stg["DeviceId"]);
                request.AddFile("file", archive);
                //var bytes = File.ReadAllBytes(archive);
                //request.AddFile("file", bytes, Path.GetFileName(archive), "application/octet-stream");
                
                //request.AddHeader("Content-type", "application/json");
                //request.AddHeader("Accept", "application/json");
                //request.RequestFormat = DataFormat.Json;

                Console.WriteLine("request to start for "+logs.Length+" log(s)");
                IRestResponse response = client.Execute(request);
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    Console.WriteLine("net error " + response.ErrorMessage + " " + stg["Url"]);
                    Console.ReadKey();
                    return;
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("response " + response.StatusCode);
                    var error = JsonConvert.DeserializeObject<ApiError>(response.Content);
                    Console.WriteLine("error    " + error.message);
                    Console.ReadKey();
                    return;
                }

                foreach (var log in logs)
                    File.Delete(log);

                File.Delete(archive);
                Console.WriteLine("uploaded "+response.Content+" log(s)");
            }
            catch (Exception e)
            {
                Console.WriteLine("error " + e.Message);
                Console.WriteLine("stack " + e.StackTrace);
            }
            Console.ReadKey();
        }

        static void CreateArchive(string sourcedir, string archive)
        {
            string sources = Path.Combine(sourcedir, "*.*");
            ProcessStartInfo p = new ProcessStartInfo();
            p.UseShellExecute = false;
            p.RedirectStandardError = true;
            p.RedirectStandardOutput = true;
            p.FileName = stg["7Zip"];
            p.Arguments = "a -t7z \"" + archive + "\" \"" + sources + "\" -mx=9";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            Process x = new Process();
            x.StartInfo = p;
            x.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process
            x.Start();
            //* Read one element asynchronously
            x.BeginErrorReadLine();
            //* Read the other one synchronously
            string output = x.StandardOutput.ReadToEnd();
            Console.WriteLine("stdout: "+output);
            x.WaitForExit();
            var code = x.ExitCode;
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
        }
    }
}

