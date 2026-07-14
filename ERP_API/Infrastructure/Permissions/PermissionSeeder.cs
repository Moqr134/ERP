using ERP_API.Domin.PermartionEntity;
using Infrastructure.ORM;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Infrastructure.Permissions;

public static class PermissionSeeder
{
    public static async Task EnsurePermissionsAsync(IServiceProvider services, ILogger? logger = null)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DBContext>();

        var required = SystemPermissions.All
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (required.Count == 0)
            return;

        var existing = await context.Permissions
            .AsNoTracking()
            .Select(p => p.Name)
            .ToListAsync();

        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var missing = required.Where(name => !existingSet.Contains(name)).ToList();

        if (missing.Count == 0)
        {
            logger?.LogInformation("Permission seed check: all {Count} system permissions are present.", required.Count);
            return;
        }

        foreach (var name in missing)
        {
            context.Permissions.Add(new Permission { Name = name });
        }

        await context.SaveChangesAsync();
        logger?.LogInformation(
            "Permission seed check: added {Added} missing permission(s). Total required: {Total}. Added: {Names}",
            missing.Count,
            required.Count,
            string.Join(", ", missing));
    }
}
