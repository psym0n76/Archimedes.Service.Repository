using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Archimedes.Fx.Service.Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryController : ControllerBase
    {
        // GET: api/Repository
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Repo", "Repo" };
        }

        // GET: api/Repository/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Repository
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Repository/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
