using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Options;

namespace Archimedes.Fx.Service.Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryController : ControllerBase
    {
        private readonly Config _config;
        // GET: api/Repository

        public RepositoryController(IOptions<Config> config)
        {
            _config = config.Value;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Repo", "Repo" , "version: "+ _config.AppVersion };
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
