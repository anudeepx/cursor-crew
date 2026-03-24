using FluentValidation;
using RetailOrderingWebsite.DTOs.Orders;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Validators;

public class PlaceOrderRequestDtoValidator : AbstractValidator<PlaceOrderRequestDto>
{
    public PlaceOrderRequestDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderItemRequestDtoValidator());
    }
}

public class OrderItemRequestDtoValidator : AbstractValidator<OrderItemRequestDto>
{
    public OrderItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateOrderStatusRequestDtoValidator : AbstractValidator<UpdateOrderStatusRequestDto>
{
    public UpdateOrderStatusRequestDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => OrderStatus.Allowed.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", OrderStatus.Allowed)}");
    }
}
