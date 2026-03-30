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

    public class UserRoleRequestValidator : AbstractValidator<AssignRoleRequest>
    {
        public UserRoleRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.");

            RuleFor(x => x.Roles)
                .NotNull().WithMessage("Roles list cannot be null.")
                .Must(r => r.Any()).WithMessage("At least one role must be provided.");

            RuleForEach(x => x.Roles)
                .NotEmpty().WithMessage("Role cannot be empty.");
        }
    }

}