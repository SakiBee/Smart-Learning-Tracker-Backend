using SLT.Core.Entities;

namespace SLT.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    DateTime GetTokenExpiry();
}