using Microsoft.AspNetCore.Mvc;

namespace example.message.center.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WelcomeController : ControllerBase
    {
        [HttpGet]
        public string Welcome()
        {
            return "Welcome to Message center!";
        }
    }
}
