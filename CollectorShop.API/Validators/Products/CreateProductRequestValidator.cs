using CollectorShop.API.DTOs.Products;
using FluentValidation;

namespace CollectorShop.API.Validators.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .Matches("^[A-Za-z0-9-_]+$").WithMessage("SKU can only contain alphanumeric characters, hyphens, and underscores");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0).WithMessage("Compare at price must be greater than 0")
            .When(x => x.CompareAtPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative");

        RuleFor(x => x.Condition)
            .IsInEnum().WithMessage("Invalid product condition");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be non-negative");

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Dimensions must not exceed 100 characters");

        RuleForEach(x => x.Images)
            .SetValidator(new CreateProductImageRequestValidator());

        RuleForEach(x => x.Attributes)
            .SetValidator(new CreateProductAttributeRequestValidator());
    }
}

public class CreateProductImageRequestValidator : AbstractValidator<CreateProductImageRequest>
{
    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Image URL is required")
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters");

        RuleFor(x => x.AltText)
            .MaximumLength(200).WithMessage("Alt text must not exceed 200 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}

public class CreateProductAttributeRequestValidator : AbstractValidator<CreateProductAttributeRequest>
{
    public CreateProductAttributeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required")
            .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Attribute value is required")
            .MaximumLength(500).WithMessage("Attribute value must not exceed 500 characters");
    }
}
