using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.User;
using FluentValidation;

namespace DIFC.Application.Validators.User
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDTO>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
