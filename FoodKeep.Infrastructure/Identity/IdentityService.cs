namespace FoodKeep.Infrastructure.Identity;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<(bool Success, string UserId)> CreateUserAsync(string userName, string password);
    Task<bool> DeleteUserAsync(string userId);
}

public class IdentityService : IIdentityService
{
    public Task<string?> GetUserNameAsync(string userId)
    {
        // TODO: Implémenter avec Identity
        return Task.FromResult<string?>("user");
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        // TODO: Implémenter avec Identity
        return Task.FromResult(false);
    }

    public Task<(bool Success, string UserId)> CreateUserAsync(string userName, string password)
    {
        // TODO: Implémenter avec Identity
        return Task.FromResult((true, Guid.NewGuid().ToString()));
    }

    public Task<bool> DeleteUserAsync(string userId)
    {
        // TODO: Implémenter avec Identity
        return Task.FromResult(true);
    }
}
