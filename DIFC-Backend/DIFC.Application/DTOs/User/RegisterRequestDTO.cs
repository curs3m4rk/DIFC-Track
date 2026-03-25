using System;
using System.Collections.Generic;
using System.Text;

namespace DIFC.Application.DTOs.User
{
    public class RegisterRequestDTO
    {
        public string UserName { get; set; } = string.Empty ;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

}
