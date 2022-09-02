using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MS_Base.Data.SQLServer.Contexts;

namespace MS_Base.Controllers
{
    [ApiController]
    [Route("api")]
    public class BaseController : Controller
    {
        public IConfiguration _configuration;
        public DatabaseDbContext _dbContext;
        public Helpers.ILogger _logger;

        public BaseController(IConfiguration configuration, Helpers.ILogger logger, DatabaseDbContext dbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public JsonResult GetUP()
        {


            return Json("ok");
        }

    }
}
