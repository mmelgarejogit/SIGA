using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIGA.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string JwtToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleClaims {  get; set; } = string.Empty;
    }
}
