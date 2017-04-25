using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using raindrop.Models;
using NPoco;

namespace raindrop.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        IDatabase _db;

        public UserController(IDatabase db)
        {
            _db = db;
        }

        // GET api/user
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return _db.Fetch<User>();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public User Get(int id)
        {
            return _db.SingleById<User>(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]User newuser)
        {
            if (ModelState.IsValid)
                _db.Insert(newuser);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]User olduser)
        {
            if (ModelState.IsValid)
                _db.Update(olduser);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _db.Delete<User>(id);
        }
    }
}
