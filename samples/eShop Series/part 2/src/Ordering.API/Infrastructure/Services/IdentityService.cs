namespace eShop.Ordering.API.Infrastructure.Services;

public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    public string GetUserIdentity()
        => context.HttpContext?.User.FindFirst("sub")?.Value;

    public string GetUserName()
        => context.HttpContext?.User.Identity?.Name;

    public string GetUserEmail()
        => context.HttpContext?.User.FindFirst("email")?.Value
            ?? context.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
}
