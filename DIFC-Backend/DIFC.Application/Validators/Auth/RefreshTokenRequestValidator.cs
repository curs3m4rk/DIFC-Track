using DIFC.Application.DTOs.Auth;
using FluentValidation;

namespace DIFC.Application.Validators.Auth
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDTO>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
