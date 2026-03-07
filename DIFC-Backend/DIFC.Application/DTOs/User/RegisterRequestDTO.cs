using System;
using System.Collections.Generic;
using System.Text;

namespace DIFC.Application.DTOs.User
{
    public class RegisterRequestDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
