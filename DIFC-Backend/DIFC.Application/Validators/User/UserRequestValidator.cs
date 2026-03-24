using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.User;
using FluentValidation;

namespace DIFC.Application.Validators.User
{
    public class GetUserByIdValidator : AbstractValidator<string>
    {
        public GetUserByIdValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage("UserId is required")
                .MinimumLength(3).WithMessage("Invalid UserId");
        }
    }
}
