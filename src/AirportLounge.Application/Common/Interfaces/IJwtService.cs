namespace AirportLounge.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
