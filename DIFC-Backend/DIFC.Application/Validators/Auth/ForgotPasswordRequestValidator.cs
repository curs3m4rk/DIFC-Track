using DIFC.Application.DTOs.Auth;
using FluentValidation;

namespace DIFC.Application.Validators.Auth
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDTO>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is Required.")
                .EmailAddress().WithMessage("Please enter a valid email address");
        }
    }
}
