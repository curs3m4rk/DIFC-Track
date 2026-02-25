using DIFC.Application.DTOs.Auth;
using FluentValidation;

namespace DIFC.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDTO>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
