using RetailOrderingWebsite.Models;

namespace RetailOrderingWebsite.Services;

public class UserRoleService
{
    private readonly IConfiguration _configuration;

    public UserRoleService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetRoleForUser(User user)
    {
        var adminEmails = _configuration.GetSection("Auth:AdminEmails").Get<string[]>() ?? [];
        return adminEmails.Any(email => email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            ? "Admin"
            : "User";
    }
}
