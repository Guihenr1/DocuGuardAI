using DocuGuardAI.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    [HttpGet("user-role")]
    public IActionResult GetStatusValues()
    {
        var values = Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .Select(e => new
            {
                Name = e.ToString(),
                Value = (int)e
            })
            .ToList();

        return Ok(values);
    }
}