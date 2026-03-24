using FluentValidation;
using RetailOrderingWebsite.DTOs.Cart;
using RetailOrderingWebsite.DTOs.Inventory;
using RetailOrderingWebsite.DTOs.Payments;
using RetailOrderingWebsite.DTOs.Users;
using RetailOrderingWebsite.DTOs.Brands;

namespace RetailOrderingWebsite.Validators;

public class AddCartItemRequestDtoValidator : AbstractValidator<AddCartItemRequestDto>
{
    public AddCartItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateCartItemRequestDtoValidator : AbstractValidator<UpdateCartItemRequestDto>
{
    public UpdateCartItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}

public class UpdateInventoryRequestDtoValidator : AbstractValidator<UpdateInventoryRequestDto>
{
    public UpdateInventoryRequestDtoValidator()
    {
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class PaymentCreateRequestDtoValidator : AbstractValidator<PaymentCreateRequestDto>
{
    public PaymentCreateRequestDtoValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
    }
}

public class PaymentVerifyRequestDtoValidator : AbstractValidator<PaymentVerifyRequestDto>
{
    public PaymentVerifyRequestDtoValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.TransactionId).NotEmpty().MaximumLength(100);
    }
}

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

public class UpsertBrandRequestDtoValidator : AbstractValidator<UpsertBrandRequestDto>
{
    public UpsertBrandRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
    }
}
