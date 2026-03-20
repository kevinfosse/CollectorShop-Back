using CollectorShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollectorShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ShippingSettingsService _shippingSettings;

    public ConfigController(IConfiguration configuration, ShippingSettingsService shippingSettings)
    {
        _configuration = configuration;
        _shippingSettings = shippingSettings;
    }

    [HttpGet("countries")]
    public ActionResult<IEnumerable<string>> GetShippingCountries()
    {
        var countries = _configuration.GetSection("ShippingCountries").Get<string[]>()
            ?? new[] { "US", "CA", "UK", "FR", "DE" };
        return Ok(countries);
    }

    [HttpGet("shipping")]
    public ActionResult<ShippingSettings> GetShippingSettings()
    {
        return Ok(_shippingSettings.GetSettings());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("shipping")]
    public ActionResult<ShippingSettings> UpdateShippingSettings([FromBody] ShippingSettings request)
    {
        if (request.DefaultShippingCost < 0)
            return BadRequest(new { Message = "Shipping cost cannot be negative" });
        if (request.TaxRate < 0 || request.TaxRate > 1)
            return BadRequest(new { Message = "Tax rate must be between 0 and 1" });
        if (request.FreeShippingThreshold < 0)
            return BadRequest(new { Message = "Free shipping threshold cannot be negative" });

        _shippingSettings.UpdateSettings(request);
        return Ok(request);
    }
}
