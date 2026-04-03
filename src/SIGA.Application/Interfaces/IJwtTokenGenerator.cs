using SIGA.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIGA.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        public string GenerateToken(User user, List<string> roles);
    }
}
