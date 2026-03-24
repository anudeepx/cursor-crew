using FluentValidation;
using RetailOrderingWebsite.DTOs.Auth;

namespace RetailOrderingWebsite.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    private const string StrongPasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,15}$";

    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(15)
            .Matches(StrongPasswordPattern)
            .WithMessage("Password must be 8-15 chars and include uppercase, lowercase, number, and special character.");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(15);
    }
}
