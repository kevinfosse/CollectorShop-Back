using CollectorShop.API.DTOs.Brands;
using FluentValidation;

namespace CollectorShop.API.Validators.Brands;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters")
            .Must(BeAValidUrl).WithMessage("Invalid logo URL format")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500).WithMessage("Website URL must not exceed 500 characters")
            .Must(BeAValidUrl).WithMessage("Invalid website URL format")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
