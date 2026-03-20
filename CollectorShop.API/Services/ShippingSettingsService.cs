namespace CollectorShop.API.Services;

public class ShippingSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly object _lock = new();
    private ShippingSettings? _override;

    public ShippingSettingsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ShippingSettings GetSettings()
    {
        lock (_lock)
        {
            if (_override != null)
                return _override;
        }

        return new ShippingSettings
        {
            DefaultShippingCost = _configuration.GetValue<decimal>("OrderSettings:DefaultShippingCost", 25m),
            TaxRate = _configuration.GetValue<decimal>("OrderSettings:TaxRate", 0.20m),
            FreeShippingThreshold = _configuration.GetValue<decimal>("OrderSettings:FreeShippingThreshold", 0m)
        };
    }

    public void UpdateSettings(ShippingSettings settings)
    {
        lock (_lock)
        {
            _override = settings;
        }
    }
}

public class ShippingSettings
{
    public decimal DefaultShippingCost { get; set; }
    public decimal TaxRate { get; set; }
    public decimal FreeShippingThreshold { get; set; }
}
