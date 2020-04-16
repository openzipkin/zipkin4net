using System.Collections.Generic;
using System.Linq;
using example.message.common;
using Microsoft.AspNetCore.Mvc;

namespace example.message.center.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private static readonly Stack<Message> Messages = new Stack<Message>();

        [HttpPost]
        [Route("pop")]
        public Message GetOneMessage()
        {
            if (!Messages.Any())
                return null;

            return Messages.Pop();
        }

        [HttpPost]
        [Route("push")]
        public string SaveMessage([FromBody]Message message)
        {
            Messages.Push(message);

            return "Ok";
        }
    }
}
