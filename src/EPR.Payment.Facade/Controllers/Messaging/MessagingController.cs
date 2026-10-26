using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Payment.Facade.Controllers.Messaging;

[ExcludeFromCodeCoverage]
[ApiController]
[Route("api/")]
[AllowAnonymous]
public class MessagingController(IServiceBusTopicPublisher publisher) : ControllerBase
{
    [HttpPost("test")]
    public async Task<IActionResult> PublishTestMessage(string data1, string data2)
    {
        var message = new FooMessage(data1, data2);
        await publisher.SendMessage(message);
        return NoContent(); 
    }
}