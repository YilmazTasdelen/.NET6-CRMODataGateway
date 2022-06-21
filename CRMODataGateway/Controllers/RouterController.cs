using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CRMODataGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Consumes("application/json")]
    public class RouterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Route("/[controller]/[action]")]
        public JsonResult GetRequestRouter(GetModels model)
        {

            return new JsonResult(null);
        }

       
    }
}
