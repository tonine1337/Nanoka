using Microsoft.AspNetCore.Mvc;

namespace Nanoka.Client.Controllers
{
    [ApiController]
    [Route("/")]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public object Get() => null;
    }
}
