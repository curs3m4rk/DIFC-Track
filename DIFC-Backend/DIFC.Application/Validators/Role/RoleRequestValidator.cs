using DIFC.Application.DTOs.Role;
using FluentValidation;

namespace DIFC.Application.Validators.Role
{
    public class RoleRequestValidator : AbstractValidator<RoleRequestDTO>
    {
        public RoleRequestValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role Name should not be empty")
                .MaximumLength(50).WithMessage("Role Name cannot exceed 50 characters");
        }
    }
}