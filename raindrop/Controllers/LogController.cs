using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using raindrop.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace raindrop.Controllers
{
    [Route("api/[controller]")]
    public class LogController : Controller
    {
        IHostingEnvironment _env;
        IDatabase _db;

        public LogController(IDatabase db, IHostingEnvironment env)
        {
            _env = env;
            _db = db;
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
        public void Post([FromForm]Log header, [FromForm]IFormFile file)
        {
            if (!ModelState.IsValid)
                return;

            User cred = header.User;
            User auth = _db.SingleOrDefault<User>(" where name = @0 and password = @1", cred.Name, cred.Password);

            if (auth == null)
                return;

            header.Stamp = DateTime.UtcNow;
            header.UserId = auth.UserId;
            header.Status = 0;

            var inserted = _db.Insert<Log>(header);
            SaveFile(header.DeviceId, file, inserted);
        }

        private void SaveFile(int deviceid, IFormFile file, object logid)
        {
            var dir = Path.Combine(new string[] { _env.WebRootPath, "uploads", deviceid.ToString() });
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var newname = string.Format($"{Path.GetFileName(file.FileName)}_{logid}");
            
            using (var fileStream = new FileStream(Path.Combine(dir, newname), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
        }
    }
}