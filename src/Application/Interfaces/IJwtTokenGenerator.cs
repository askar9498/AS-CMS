using System.Security.Claims;
using AS_CMS.Domain.Entities;

namespace AS_CMS.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(Guid userId);
    Guid ValidateRefreshToken(string refreshToken);
    bool ValidateAccessToken(string accessToken);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
} 