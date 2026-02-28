using DIFC.Application.DTOs.Auth;
using FluentValidation;

namespace DIFC.Application.Validators.Auth
{
    /// <summary>
    /// HOW it works: When a request hits the controller, FluentValidation
    /// automatically runs before your action method. If validation fails,
    /// it returns a 400 Bad Request with error details — your controller
    /// code never even runs.
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequestDTO>
    {
        public LoginRequestValidator() 
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is Required")
                .EmailAddress().WithMessage("Please Enter a Valid Email Address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}
