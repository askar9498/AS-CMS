using AS_CMS.Domain.Entities;

namespace AS_CMS.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
} 