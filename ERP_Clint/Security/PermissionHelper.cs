using System.Security.Claims;

namespace ERP_Clint.Security;

public static class PermissionHelper
{
    public static bool HasPermission(this ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole(PermissionNames.FullAccess))
            return true;

        return user.IsInRole(permission);
    }

    public static bool HasAny(this ClaimsPrincipal? user, params string[] permissions)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole(PermissionNames.FullAccess))
            return true;

        return permissions.Any(user.IsInRole);
    }

    public static string Roles(params string[] permissions)
        => string.Join(",", new[] { PermissionNames.FullAccess }.Concat(permissions).Distinct());
}
