using Microsoft.AspNetCore.Mvc;

namespace Nanoka.Client.Controllers
{
    [ApiController]
    [Route("/")]
    public class DefaultController : ControllerBase
    {
        public class ClientInfo { }

        [HttpGet]
        public ClientInfo Get() => new ClientInfo();
    }
}
