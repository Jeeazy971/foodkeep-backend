namespace FoodKeep.Infrastructure.Identity;

public interface ITokenService
{
    string GenerateToken(string userId, string email);
    bool ValidateToken(string token);
}

public class TokenService : ITokenService
{
    public string GenerateToken(string userId, string email)
    {
        // TODO: Implémenter la génération JWT
        return "dummy-token";
    }

    public bool ValidateToken(string token)
    {
        // TODO: Implémenter la validation JWT
        return true;
    }
}
