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


    public class UpdateRoleValidator : AbstractValidator<UpdateRoleDTO>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty()
                .WithMessage("Role name is required")
                .MaximumLength(50);

            RuleFor(x => x.NewRoleName)
                .NotEmpty()
                .WithMessage("New role name is required")
                .MaximumLength(50)
                .NotEqual(x => x.RoleName)
                .WithMessage("New role name must be different from old role name");
        }
    }

}