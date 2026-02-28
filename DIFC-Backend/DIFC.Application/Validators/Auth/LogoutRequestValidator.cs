using DIFC.Application.DTOs.Auth;
using FluentValidation;

namespace DIFC.Application.Validators.Auth
{
    public class LogoutRequestValidator : AbstractValidator<LogoutRequestDTO>
    {
        public LogoutRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
