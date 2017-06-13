using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using raindrop.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace raindrop.Controllers
{
    [Route("api/[controller]")]
    public class LogController : Controller
    {
        IHostingEnvironment _env;
        IDatabase _db;
        IOptions<LogCentralSettings> _cfg;
        string _path7zip;

        public LogController(IDatabase db, IHostingEnvironment env, IOptions<LogCentralSettings> cfg)
        {
            _env = env;
            _db = db;
            _cfg = cfg;
            _path7zip = _cfg.Value.path7zip;
        }

        // GET api/log
        [HttpGet]
        public IEnumerable<Log> Get()
        {
            return _db.Fetch<Log>();
        }

        // POST api/values
        // overly simplistic and entirely cleartext authorization as a placeholder for certs later
        [HttpPost]
        public IActionResult Post([FromForm]Log header, [FromForm]IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                var error = new
                {
                    message = "Post data invalid",
                    status = StatusCodes.Status500InternalServerError
                };
                Response.StatusCode = error.status;
                return new ObjectResult(error);
            }
            
            User cred = header.User;
            User auth = _db.SingleOrDefault<User>(" where name = @0 and password = @1", cred.Name, cred.Password);

            if (auth == null)
            {
                var error = new
                {
                    message = "User not authenticated",
                    status = StatusCodes.Status401Unauthorized
                };
                Response.StatusCode = error.status;
                return new ObjectResult(error);
            }

            header.UserId = auth.UserId;
            header.Status = 0;

            var dir = Path.Combine(new string[] { _env.ContentRootPath, "uploads", header.DeviceId.ToString() });
            var stage = Path.Combine(dir, "stage");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(stage);
            }

            var archive = Path.GetTempFileName();
            using (var fileStream = new FileStream(archive, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            var exit = ExtractArchive(archive, stage);
            
            if (exit != 0)
            {
                var error = new
                {
                    message = "Shell exit with code of "+exit,
                    status = StatusCodes.Status501NotImplemented
                };
                Response.StatusCode = error.status;
                return new ObjectResult(error);
            }

            System.IO.File.Delete(archive);
            var logs = Directory.GetFiles(stage);
            var roll = new List<string>(logs.Length);
            foreach (var log in logs)
            {
                header.Stamp = DateTime.UtcNow;

                var inserted = _db.Insert(header);
                var oldname = Path.GetFileName(log);
                roll.Add(oldname);
                var newname = string.Format($"{oldname}_{inserted}");
                System.IO.File.Move(log, Path.Combine(dir, newname));
            }
            return new OkObjectResult(roll);
        }

        private int ExtractArchive(string source, string destination)
        {
            ProcessStartInfo pro = new ProcessStartInfo();
            pro.CreateNoWindow = true;
            pro.FileName = _path7zip;
            pro.Arguments = "x \"" + source + "\" -o" + destination;
            Process x = Process.Start(pro);
            x.WaitForExit();
            return x.ExitCode;
        }
    }
}
