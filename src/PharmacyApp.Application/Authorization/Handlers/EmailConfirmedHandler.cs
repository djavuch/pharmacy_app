using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PharmacyApp.Application.Authorization.Requirements;
using PharmacyApp.Domain.Entities;
using System.Security.Claims;

namespace PharmacyApp.Application.Authorization.Handlers;

public class EmailConfirmedHandler : AuthorizationHandler<EmailConfirmedRequirement>
{
    private readonly UserManager<User> _userManager;

    public EmailConfirmedHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, EmailConfirmedRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user != null && user.EmailConfirmed)
        {
            context.Succeed(requirement);
            return;
        }
        context.Fail();
    }
}
