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
    }
}
