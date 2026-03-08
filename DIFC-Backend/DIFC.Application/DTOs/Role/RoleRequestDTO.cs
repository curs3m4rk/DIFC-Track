using System;
using System.Collections.Generic;
using System.Text;

namespace DIFC.Application.DTOs.Role
{
    public class RoleRequestDTO
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateRoleDTO
    {
        public string RoleName { get; set; } = string.Empty;

        public string NewRoleName { get; set; } = string.Empty;
    }
}
