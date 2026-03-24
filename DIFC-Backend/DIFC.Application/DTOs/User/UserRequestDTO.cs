using System;
using System.Collections.Generic;
using System.Text;

namespace DIFC.Application.DTOs.Role
{
    public class UserRequestDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }

}
