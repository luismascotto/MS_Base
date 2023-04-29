using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MS_Base.Data.SQLServer.Contexts;

namespace MS_Base.Controllers;

[ApiController]
[Route("api")]
public class BaseController : Controller
{
    public BaseController()
    {
    }

    [HttpGet]
    public JsonResult GetUP()
    {


        return Json("ok");
    }

}
