using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("countries")]
    public ActionResult<IEnumerable<string>> GetShippingCountries()
    {
        var countries = _configuration.GetSection("ShippingCountries").Get<string[]>()
            ?? new[] { "US", "CA", "UK", "FR", "DE" };
        return Ok(countries);
    }
}
