using FluentValidation;
using RetailOrderingWebsite.DTOs.Products;

namespace RetailOrderingWebsite.Validators;

public class UpsertProductRequestDtoValidator : AbstractValidator<UpsertProductRequestDto>
{
    public UpsertProductRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.SellerId).GreaterThan(0);
    }
}

public class UpsertCategoryRequestDtoValidator : AbstractValidator<UpsertCategoryRequestDto>
{
    public UpsertCategoryRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
